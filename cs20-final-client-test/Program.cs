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
                Console.WriteLine("Bad port, using 8888");
            }
        }
        Console.WriteLine($"Connecting to {host}:{port}");
        try
        {
            clientSocket.Connect(host, port);
            Utility.WriteLineColor("Connected.", ConsoleColor.Green);
            handshakeState = HandshakeState.Connected;
            Console.WriteLine("Checking version...");
            Send(new VersionPacket(Version));
        }catch(Exception e)
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
        }catch (Exception ex)
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
                if(disconnectPacket != null)
                {
                    Utility.WriteLineColor($"Disconnected from Server: {Utility.GetReason(disconnectPacket.DisconnectReason)}", ConsoleColor.Red);
                    Disconnect();
                }
                break;
            case 3:
                HandleHandshake(ID, data); break;
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
                    ushort[] ver = Utility.GetUshortsFromVersionString(Version);
                    if (ver[0] != p.VersionMajor || ver[1] != p.VersionMinor || ver[2] != p.VersionPatch)
                    {
                        Disconnect();
                    }
                    else
                    {
                        //Send(new VersionPacket(Version));
                        Utility.WriteLineColor("Version check passed.", ConsoleColor.Green);
                        handshakeState = HandshakeState.SentVersion;
                    }
                }
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