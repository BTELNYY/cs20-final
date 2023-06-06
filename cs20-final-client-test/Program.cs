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
    public static string Version { get; } = "1.0.0";

    public static void Main(string[] args)
    {
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
                Console.WriteLine("Using 8888 as port!");
            }
        }
        Console.WriteLine("Enter name [Enter for Player]");
        Console.Write(": ");
        string? name = Console.ReadLine() ?? "Player";
        Console.WriteLine($"Connecting to {host}:{port} with name {name}");
        StaticClient.Host = host;
        StaticClient.Port = port;
        StaticClient.PlayerName = name;
        StaticClient.Init();
    }
}