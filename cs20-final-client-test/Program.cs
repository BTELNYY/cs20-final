using System;
using System.Net.Sockets;
using cs20_final_library;
using cs20_final_library.Packets;

namespace cs20_final_client_test;

public class Program
{
    static System.Net.Sockets.TcpClient clientSocket = new System.Net.Sockets.TcpClient();
    static NetworkStream serverStream;
    public static void Main(string[] args)
    {
        clientSocket.Connect("127.0.0.1", 8888);
        while (true)
        {
            NetworkStream serverStream = clientSocket.GetStream();
            PingPacket p = new();
            p.CompileTime = Utility.GetUnixTimestamp();
            byte[] packet = p.GetAsBytes();
            serverStream.Write(packet, 0, packet.Length);
            serverStream.Flush();
        }
    }
}