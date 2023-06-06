using cs20_final_library.Packets;
using cs20_final_library;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Text.Json.Serialization.Metadata;
using System.Threading.Tasks;

namespace cs20_final_client_test
{
    public class StaticClient
    {
        static System.Net.Sockets.TcpClient clientSocket = new System.Net.Sockets.TcpClient();
        public static HandshakeState handshakeState = HandshakeState.Disonnected;
        static byte[] bytesFrom = new byte[Packet.MaxSizePreset];
        public static ClientPlayer? Player;
        static Thread ChatThread = new(ClientConsoleHandler.HandleChat);
        public static string Host = "127.0.0.1";
        public static int Port = 8888;
        public static string PlayerName = "Player";

        public static void Init()
        {
            Thread pingThread = new(SendPingPacket);
            try
            {
                clientSocket.Connect(Host, Port);
                Utility.WriteLineColor("Connected.", ConsoleColor.Green);
                handshakeState = HandshakeState.Connected;
                Console.WriteLine("Checking version...");
                Send(new VersionPacket(Program.Version));
                ChatThread.Start();
            }
            catch (Exception e)
            {
                Console.WriteLine("Failed to connect to server. " + e.Message, ConsoleColor.Red);
                return;
            }
            try
            {
                //pingThread.Start();
                while (true)
                {
                    NetworkStream networkStream = clientSocket.GetStream();
                    int count = networkStream.Read(bytesFrom, 0, Packet.MaxSizePreset);
                    uint ID = BitConverter.ToUInt32(bytesFrom, 0);
                    int IDint = Convert.ToInt32(ID);
                    HandlePacket(ID, bytesFrom);
                }
            }
            catch (Exception ex)
            {
                pingThread.Interrupt();
                Console.WriteLine(ex.ToString());
            }
        }

        public static void Send(Packet p)
        {
            if (!IsConnected())
            {
                Console.WriteLine("Can't Send packet, not connected!");
            }
            else
            {
                NetworkStream serverStream = clientSocket.GetStream();
                byte[] packet = p.GetAsBytes();
                serverStream.Write(packet, 0, Packet.MaxSizePreset);
                serverStream.Flush();
            }
        }

        public static bool IsConnected()
        {
            return clientSocket.Connected;
        }

        public static void HandlePacket(uint ID, byte[] data)
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
                    DisconnectPacket disconnectPacket = DisconnectPacket.GetFromBytes(data);
                    if (disconnectPacket != null)
                    {
                        Utility.WriteLineColor($"Disconnected from Server: {Utility.GetReason(disconnectPacket.DisconnectReason)}", ConsoleColor.Red);
                        Disconnect();
                    }
                    break;
                case 3:
                    HandleHandshake(ID, data); break;
                case 5:
                    HandleHandshake(ID, data); break;
                case 6:
                    ChatPacket chatPacket = ChatPacket.GetFromBytes(data);
                    Log.Info($"[CHAT] {chatPacket.Name}: {chatPacket.Message}");
                    break;
                default:
                    Send(new DisconnectPacket(DisconnectReason.BadPacket));
                    Console.WriteLine("Bad packet recieved, disconnecting. (Is your version mismatched?)");
                    Disconnect();
                    break;
            }
        }

        public static void HandleHandshake(uint ID, byte[] data)
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
                            Disconnect();
                        }
                        else
                        {
                            //Send(new VersionPacket(Version));
                            Utility.WriteLineColor("Version check passed.", ConsoleColor.Green);
                            handshakeState = HandshakeState.SentVersion;
                            Send(new PlayerDataPacket()
                            {
                                PlayerName = PlayerName,
                                PermissionState = new(),
                                PlayerID = 0
                            });
                            Log.Info("Sending player data...");
                            handshakeState = HandshakeState.SentPlayerData;
                        }
                    }
                    break;
                case 5:
                    handshakeState = HandshakeState.GotPlayerData;
                    PlayerDataPacket playerDataPacket = PlayerDataPacket.GetFromBytes(data);
                    if (Player is null)
                    {
                        Player = new();
                    }
                    Player.Name = playerDataPacket.PlayerName;
                    Player.UserFlags = playerDataPacket.PermissionState;
                    Player.ClientID = playerDataPacket.PlayerID;
                    Log.Success("Player data exchange complete.");
                    break;
            }
        }


        public static void Disconnect()
        {
            if (IsConnected())
            {
                Console.WriteLine("Disconnecting!");
                clientSocket.Close();
            }
            else
            {
                Console.WriteLine("Socket already closed. Disonnect Complete.");
            }
        }


        private static void SendPingPacket()
        {
            while (true)
            {
                NetworkStream serverStream = clientSocket.GetStream();
                PingPacket p = new();
                p.CompileTime = Utility.GetUnixTimestamp();
                byte[] packet = p.GetAsBytes();
                serverStream.Write(packet, 0, Packet.MaxSizePreset);
                serverStream.Flush();
                Thread.Sleep(1000);
            }
        }
    }
}