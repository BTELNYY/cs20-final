using System;
using System.Net.Sockets;
using System.Net;
using System.Text;
using cs20_final_library;
using cs20_final_library.Packets;

namespace cs20_final_client_test;

public class Program
{
    static System.Net.Sockets.TcpClient clientSocket = new System.Net.Sockets.TcpClient();
    static NetworkStream serverStream;
    static byte[] bytesFrom = new byte[Packet.MaxSizePreset];
    public static void Main(string[] args)
    {
        Thread pingThread = new(SendPingPacket);
        try
        {
            clientSocket.Connect("127.0.0.1", 8888);
            pingThread.Start();
            while (true)
            {
                NetworkStream networkStream = clientSocket.GetStream();
                int count = networkStream.Read(bytesFrom, 0, Packet.MaxSizePreset);
                uint ID = BitConverter.ToUInt32(bytesFrom, 0);
                int IDint = Convert.ToInt32(ID);
                Console.WriteLine($"Read data! Length: {count}, ID: {ID}");
            }
        }catch (Exception ex)
        {
            pingThread.Interrupt();
            Console.WriteLine(ex.ToString());
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
            Console.WriteLine($"Sending packet. Length: {packet.Length}");
            serverStream.Write(packet, 0, Packet.MaxSizePreset);
            serverStream.Flush();
            Thread.Sleep(1000);
        }
    }
}