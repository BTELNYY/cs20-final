using System;
using System.Net.Sockets;
using System.Net;
using System.Text;
using cs20_final_library;
using cs20_final_library.Packets;
using System.Security.Cryptography.X509Certificates;

namespace cs20_final_client_test;

public class Program
{
    static System.Net.Sockets.TcpClient clientSocket = new System.Net.Sockets.TcpClient();
    static Encryption encryption;
    static bool UsingEncryption = false;

    public static HandshakeState handshakeState = HandshakeState.Disonnected;
    public static string Version { get; } = "1.0.0";
    static byte[] bytesFrom = new byte[Packet.MaxSizePreset];
    public static void Main(string[] args)
    {
        Thread pingThread = new(SendPingPacket);
        Console.WriteLine("Enter IP Address to connect to [Enter for localhost]");
        Console.Write(": ");
        string? host = Console.ReadLine();
        if(host is null || string.IsNullOrWhiteSpace(host))
        {
            Console.WriteLine("Using 127.0.0.1 as host!");
            host = "127.0.0.1";
        }
        Console.WriteLine("Enter Port [Enter for 8888]");
        Console.Write(": ");
        string? portstring = Console.ReadLine();
        int port = 8888;
        if(portstring is not null || !string.IsNullOrWhiteSpace(portstring))
        { 
            if(!int.TryParse(portstring, out int parsedport))
            {
                Log.Info("Using 8888 as port!");
            }
        }
        Log.Info($"Connecting to {host}:{port}");
        try
        {
            clientSocket.Connect(host, port);
            Log.Success("Connected.");
            handshakeState = HandshakeState.Connected;
            Log.Info("Checking Version...");
            Send(new VersionPacket(Version));
        }catch(Exception e)
        {
            Log.Error("Failed to connect to server: " + e.Message);
            return;
        }
        try
        {
            //pingThread.Start();
            while (true)
            {
                NetworkStream networkStream = clientSocket.GetStream();
                int count = networkStream.Read(bytesFrom, 0, Packet.MaxSizePreset);
                if (UsingEncryption)
                {
                    bytesFrom = Utility.GetStringAsBytes(encryption.Decrypt(bytesFrom));
                }
                uint ID = BitConverter.ToUInt32(bytesFrom, 0);
                int IDint = Convert.ToInt32(ID);
                HandlePacket(ID, bytesFrom);
            }
        }catch (Exception ex)
        {
            pingThread.Interrupt();
            Log.Error(ex.ToString());
        }
    }
    
    public static void Send(Packet p)
    {
        if (!IsConnected())
        {
            Log.Error("Can't send packet, not connected!");
        }
        else
        {
            NetworkStream serverStream = clientSocket.GetStream();
            byte[] packet = p.GetAsBytes();
            if (UsingEncryption)
            {
                packet = Utility.GetStringAsBytes(encryption.EncryptToOther(packet));
            }
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
        switch(ID)
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
                if(Utility.GetReason(disconnectPacket.DisconnectReason) == DisconnectReason.Custom)
                {
                    Log.Error("Disconnected from server: Custom - " +  disconnectPacket.CustomDisconnectReaon);
                    Disconnect();
                }
                else
                {
                    Log.Error("Disconnected from Server: " + Utility.GetReason(disconnectPacket.DisconnectReason));
                    Disconnect();
                }
                break;
            case 3:
                HandleHandshake(ID, data); break;
            case 4:
                HandleHandshake(ID, data); break;
            default:
                Send(new DisconnectPacket(DisconnectReason.BadPacket));
                Log.Error("Bad packet! Disconnecting.");
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
                    ushort[] ver = Utility.GetUshortsFromVersionString(Version);
                    if (ver[0] != p.VersionMajor || ver[1] != p.VersionMinor || ver[2] != p.VersionPatch)
                    {
                        Disconnect();
                    }
                    else
                    {
                        //Send(new VersionPacket(Version));
                        Log.Success("Version check passed.");
                        handshakeState = HandshakeState.SentVersion;
                        EncryptionPacket packet = new EncryptionPacket();
                        packet.EncryptionType = 1;
                        Encryption enc = new Encryption();
                        encryption = enc;
                        packet.PublicKey = enc._publicKey;
                        Send(packet);
                        handshakeState = HandshakeState.SentEncryptionRequest;
                    }
                }
                break;
            case 4:
                EncryptionPacket encryptionPacket = EncryptionPacket.GetFromBytes(data);
                if(handshakeState == HandshakeState.SentEncryptionRequest)
                {
                    if(encryptionPacket.KeyLength > 0)
                    {
                        encryption.PublicKeyOfOther = encryptionPacket.PublicKey;
                        Log.Debug(encryption.PublicKeyOfOther);
                        UsingEncryption = true;
                    }
                    else
                    {
                        Log.Warning("Server sent empty public key!");
                    }
                }
                break;
        }
    }


    public static void Disconnect()
    {
        if (IsConnected()) 
        {
            Log.Info("Disconnecting!");
            clientSocket.Close();
        }
        else
        {
            Log.Warning("Called void::Disconect() on closed socket.");
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