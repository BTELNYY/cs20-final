using cs20_final_library.Packets;
using cs20_final_library;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace cs20_final
{
    public class Client
    {
        public static EventHandler<ServerPlayer>? PlayerConnected;
        public static EventHandler<DisconnectReason>? PlayerDisconnected;
        public static EventHandler<string>? PlayerDisconnectedCustom;



        public HandshakeState handshakeState = HandshakeState.Connected;
        public uint clientID = 0;
        public ServerPlayer? player;
        TcpClient clientSocket = new();
        Thread? clientThread;
        CancellationToken threadToken;
        CancellationTokenSource? tokenSource;

        public void StartClient(TcpClient inClientSocket, uint clientNum, CancellationToken token, CancellationTokenSource source)
        {
            clientSocket = inClientSocket;
            clientID = clientNum;
            threadToken = token;
            tokenSource = source;
            clientThread = new(HandleClient);
            clientThread.Start();
        }

        public string GetName()
        {
            if(player is null)
            {
                return "null";
            }
            else
            {
                return player.Name;
            }
        }

        public NetworkStream GetStream()
        {
            return clientSocket.GetStream();
        }

        public void Send(Packet p)
        {
            if (!IsConnected())
            {
                Utility.WriteLineColor("Cannot send packet: Socket not connected!", ConsoleColor.Red);
                return;
            }
            byte[] packet = p.GetAsBytes();
            //Console.WriteLine($"Sending packet. Length: {packet.Length}");
            NetworkStream networkStream = GetStream();
            networkStream.Write(packet, 0, Packet.MaxSizePreset);
            networkStream.Flush();
        }

        public bool IsConnected()
        {
            if (clientSocket is null)
            {
                return false;
            }
            return clientSocket.Connected;
        }

        public void Kick(DisconnectReason reason)
        {
            Send(new DisconnectPacket(reason));
            Console.WriteLine($"Disconnecting client {clientID}. With Reason: {reason.ToString()}");
            DestroyClient();
            if(PlayerDisconnected is null)
            {
                return;
            }
            else
            {
                PlayerDisconnected.Invoke(this, reason);
            }
        }

        public void Kick(string reason)
        {
            Send(new DisconnectPacket(reason));
            Console.WriteLine($"Disconnecting client {clientID}. With reason: {reason}");
            DestroyClient();
            if(PlayerDisconnectedCustom is null)
            {
                return;
            }
            else
            {
                PlayerDisconnectedCustom.Invoke(this, reason);
            }
        }

        public void DestroyClient()
        {
            if (IsConnected())
            {
                clientSocket.Close();
            }
            Server.clients.Remove(clientID);
            clientSocket = null;
            tokenSource.Cancel();
            clientThread.Join();
            tokenSource.Cancel();
        }

        private void HandlePacket(uint ID, byte[] data)
        {
            switch (ID)
            {
                case 1:
                    //reply to client
                    PingPacket p = PingPacket.GetFromBytes(data);
                    if (p != null && p.Reply)
                    {
                        Send(new PingPacket() { CompileTime = Utility.GetUnixTimestamp(), Reply = false });
                    }
                    break;
                case 2:
                    if (IsConnected())
                    {
                        Console.WriteLine($"Client with ID {clientID} has requested disconnect.");
                        clientSocket.Close();
                        DestroyClient();
                    }
                    else
                    {
                        Console.WriteLine($"Client with ID {clientID} requested disconnected but socket is already closed.");
                        DestroyClient();
                    }
                    break;
                case 3:
                    HandleHandshake(ID, data);
                    break;
                case 5:
                    HandleHandshake(ID, data);
                    break;
                case 6:
                    if(player is null)
                    {
                        Send(new DisconnectPacket("Sent chat before player data handshake!"));
                        Log.Info($"Disconnecting client {clientID} for attemping to send chat before handshake.");
                        DestroyClient();
                        break;
                    }
                    if(!player.UserPermissions.HasFlag(UserFlag.SendChat, out byte state) || state == 0)
                    {
                        Log.Info($"Client {clientID} has no permission to send chat messages.");
                        break;
                    }
                    ChatPacket chatPacket = ChatPacket.GetFromBytes(data);
                    //Sanitize message
                    string fixedMessage = chatPacket.Message;
                    if(player.Name != chatPacket.Name)
                    {
                        Log.Error("Client attempted to forge player name when sending chat!");
                        chatPacket = new(player.Name, fixedMessage);
                    }
                    chatPacket = new(chatPacket.Name, fixedMessage);
                    Log.Info($"[CHAT] {chatPacket.Name}: {fixedMessage}");
                    foreach(var client in Server.clients.Values)
                    {
                        client.Send(chatPacket);
                    }
                    break;
                default:
                    Kick(DisconnectReason.BadPacket);
                    Console.WriteLine($"Disconnecting client {clientID} for bad packets.");
                    DestroyClient();
                    break;
            }
        }

        private void HandleHandshake(uint ID, byte[] data)
        {
            switch (ID)
            {
                case 3:
                    VersionPacket p = VersionPacket.GetFromBytes(data);
                    if (handshakeState == HandshakeState.Connected)
                    {
                        handshakeState = HandshakeState.GotVersion;
                        ushort[] ver = Utility.GetUshortsFromVersionString(Program.Version);
                        if (ver[0] != p.VersionMajor || ver[1] != p.VersionMinor || ver[2] != p.VersionPatch)
                        {
                            Kick(DisconnectReason.VersionMismatch);
                        }
                        else
                        {
                            Send(new VersionPacket(Program.Version));
                            handshakeState = HandshakeState.SentVersion;
                        }
                    }
                    break;
                case 5:
                    handshakeState = HandshakeState.GotPlayerData;
                    PlayerDataPacket playerDataPacket = PlayerDataPacket.GetFromBytes(data);
                    if(player is null)
                    {
                        //player first connects
                        player = new(this, playerDataPacket.PlayerName);
                        playerDataPacket = SanitizePlayerData(playerDataPacket);
                        playerDataPacket.PermissionState = new();
                        playerDataPacket.PlayerID = clientID;
                        player.Name = playerDataPacket.PlayerName;
                        Log.Info($"Client {clientID} registered as player {player.Name}");
                        if(PlayerConnected is null)
                        {
                            
                        }
                        else
                        {
                            PlayerConnected.Invoke(this, player);
                        }
                    }
                    else
                    {
                        playerDataPacket = SanitizePlayerData(playerDataPacket);
                        //player modify self
                        if (player.UserPermissions.HasFlag(UserFlag.ChangeName, out byte result) && result == 1)
                        {
                            Log.Info($"Player {player.Name} changed name to {playerDataPacket.PlayerName}");
                            player.UpdateOnServer(playerDataPacket);
                        }
                    }
                    Send(playerDataPacket);
                    handshakeState = HandshakeState.SentPlayerData;
                    break;
            }
        }

        public static PlayerDataPacket SanitizePlayerData(PlayerDataPacket packet)
        {
            return packet;
        }



        private void HandleClient()
        {
            byte[] bytesFrom = new byte[Packet.MaxSizePreset];
            int requestCount = 0;

            while (tokenSource != null && !tokenSource.IsCancellationRequested)
            {
                try
                {
                    if (!IsConnected())
                    {
                        Console.WriteLine("Client Disconnected: Socket Closed.", ConsoleColor.Red);
                        DestroyClient();
                        break;
                    }
                    else
                    {
                        requestCount = requestCount + 1;
                        NetworkStream networkStream = GetStream();
                        int count = networkStream.Read(bytesFrom, 0, Packet.MaxSizePreset);
                        uint ID = BitConverter.ToUInt32(bytesFrom, 0);
                        int IDint = Convert.ToInt32(ID);
                        //Console.WriteLine($"Read data! Length: {count}, ID: {ID}");
                        HandlePacket(ID, bytesFrom);
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Disconnecting client({clientID}) due to error. Error: " + ex.Message);
                    Kick(DisconnectReason.GeneralError);
                }
            }
            return;
        }
    }
}
