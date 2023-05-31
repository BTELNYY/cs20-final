using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using cs20_final_library;
using cs20_final_library.Packets;

namespace cs20_final;

public class Program
{
    //http://csharp.net-informations.com/communications/csharp-multi-threaded-server-socket.htm
    public static string Version { get; } = "1.0.0";


    public static Dictionary<uint, Client> clients = new Dictionary<uint, Client>();

    static Thread ConsoleThread = new(ConsoleHandler.HandleCommands);

    public static void Main(string[] args)
    {
        Utility.DefinePackets();
        TcpListener serverSocket = new TcpListener(IPAddress.Parse("127.0.0.1"), 8888);
        TcpClient clientSocket = default;
        uint counter = 0;

        serverSocket.Start();
        Console.WriteLine("Server Started");

        ConsoleThread.Start();

        counter = 0;
        while (true)
        {
            counter += 1;
            clientSocket = serverSocket.AcceptTcpClient();
            Console.WriteLine("Client connected. ID: " + counter.ToString());
            Client client = new Client();
            client.clientID = counter;
            var source = new CancellationTokenSource();
            client.StartClient(clientSocket, counter, source.Token, source);
            clients.Add(counter, client);
        }

        clientSocket.Close();
        serverSocket.Stop();
        Console.WriteLine(" >> " + "exit");
        Console.ReadLine();
    }

    public class Client
    {
        public bool UsingEncryption { get; private set; } = false;
        TcpClient clientSocket = new();
        public HandshakeState handshakeState = HandshakeState.Connected;
        public uint clientID = 0;
        public Encryption encryption { get; private set; } = new();
        Thread clientThread;
        CancellationToken threadToken;
        CancellationTokenSource tokenSource;
        public void StartClient(TcpClient inClientSocket, uint clientNum, CancellationToken token, CancellationTokenSource source)
        {
            this.clientSocket = inClientSocket;
            this.clientID = clientNum;
            threadToken = token;
            tokenSource = source;
            clientThread = new(HandleClient);
            clientThread.Start();
        }

        public NetworkStream GetStream()
        {
            return clientSocket.GetStream();
        }

        public void Send(Packet p)
        {
            if (!IsConnected())
            {
                Log.Error("Can't send packet. Socket not connected.");
                return;
            }
            byte[] packet = p.GetAsBytes();
            if (UsingEncryption)
            {
                packet = Utility.GetStringAsBytes(encryption.EncryptToOther(packet));
            }
            Console.WriteLine($"Sending packet. Length: {packet.Length}");
            NetworkStream networkStream = GetStream();
            networkStream.Write(packet, 0, Packet.MaxSizePreset);
            networkStream.Flush();
        }

        public bool IsConnected()
        {
            if(clientSocket is null)
            {
                return false;
            }
            return clientSocket.Connected;
        }

        public void Kick(DisconnectReason reason)
        {
            Send(new DisconnectPacket(reason));
            Log.Info($"Disconnecting client {clientID}. With Reason: {reason}");
            DestroyClient();
        }

        public void Kick(string reason)
        {
            Send(new DisconnectPacket(reason));
            Log.Info($"Disconnecting client {clientID}. With reason: {reason}");
        }

        public void DestroyClient()
        {
            if(IsConnected())
            {
                clientSocket.Close();
            }
            clients.Remove(clientID);
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
                        Send(new PingPacket() { CompileTime = Utility.GetUnixTimestamp(), Reply = false});
                    }
                    break;
                case 2:
                    if (IsConnected())
                    {
                        Log.Info($"Client with ID {clientID} has requested disconnect.");
                        clientSocket.Close();
                        DestroyClient();
                    }
                    else
                    {
                        Log.Info($"Client with ID {clientID} requested disconnected but socket is already closed.");
                        DestroyClient();
                    }
                    break;
                case 3:
                    HandleHandshake(ID, data); 
                    break;
                case 4:
                    HandleHandshake(ID, data);
                    break;
                default:
                    Send(new DisconnectPacket(DisconnectReason.BadPacket));
                    Log.Warning($"Disconnecting client {clientID} for bad packets.");
                    DestroyClient();
                    break;
            }
        }

        private void HandleHandshake(uint ID, byte[] data)
        {
            switch(ID) 
            { 
                case 3:
                    VersionPacket p = VersionPacket.GetFromBytes(data);
                    if(handshakeState == HandshakeState.Connected)
                    {
                        handshakeState = HandshakeState.GotVersion;
                        ushort[] ver = Utility.GetUshortsFromVersionString(Version);
                        if (ver[0] != p.VersionMajor || ver[1] != p.VersionMinor || ver[2] != p.VersionPatch)
                        {
                            Kick(DisconnectReason.VersionMismatch);
                        }
                        else
                        {
                            Send(new VersionPacket(Version));
                            handshakeState = HandshakeState.SentVersion;
                        }
                    }
                    break;
                case 4:
                    EncryptionPacket encryptionPacket = EncryptionPacket.GetFromBytes(data);
                    if(encryptionPacket.KeyLength > 0 )
                    {
                        Log.Info($"Client {clientID} requested Encryption!");
                        encryption.PublicKeyOfOther = encryptionPacket.PublicKey;
                        handshakeState = HandshakeState.GotEncryptionRequest;
                        EncryptionPacket packet = new();
                        packet.PublicKey = encryption._publicKey;
                        packet.EncryptionType = 1;
                        Send(packet);
                        handshakeState = HandshakeState.SentEncryptionRequest;
                        UsingEncryption = true;
                        Log.Info($"Client {clientID} encrypted.");
                        Log.Debug(encryption.PublicKeyOfOther);
                    }
                    else
                    {
                        handshakeState = HandshakeState.GotEncryptionRequest;
                        Log.Warning($"Client {clientID} didn't provide their public key. sending servers...");
                        EncryptionPacket packet = new();
                        packet.PublicKey = encryption._publicKey;
                        packet.EncryptionType = 1;
                        Send(packet);
                        handshakeState = HandshakeState.SentEncryptionRequest;
                        UsingEncryption = true;
                        Log.Info($"Client {clientID} encrypted.");
                    }
                    break;
            }
        }



        private void HandleClient()
        {
            byte[] bytesFrom = new byte[Packet.MaxSizePreset];
            int requestCount = 0;

            while (!tokenSource.IsCancellationRequested)
            {
                try
                {
                    if (!IsConnected())
                    {
                        Log.Error("Client Disconnected: Socket Closed.");
                        DestroyClient();
                        break;
                    }
                    else
                    {
                        requestCount = requestCount + 1;
                        NetworkStream networkStream = GetStream();
                        int count = networkStream.Read(bytesFrom, 0, Packet.MaxSizePreset);
                        if (UsingEncryption)
                        {
                            bytesFrom = Utility.GetStringAsBytes(encryption.Decrypt(bytesFrom));
                        }
                        uint ID = BitConverter.ToUInt32(bytesFrom, 0);
                        int IDint = Convert.ToInt32(ID);
                        Log.Info($"Read data! Length: {count}, ID: {ID}");
                        HandlePacket(ID, bytesFrom);
                    }
                }
                catch (Exception ex)
                {
                    Log.Info($"Disconnecting client({clientID}) due to error. Error: " + ex.Message);
                    Kick(DisconnectReason.GeneralError);
                }
            }
            return;
        }
    }
}