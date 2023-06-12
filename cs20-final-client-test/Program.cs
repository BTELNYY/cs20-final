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
        string? host;
        string? portstring;
        int port = 8888;
        string? name;
        if(args.Length < 3)
        {
            Console.WriteLine("Enter IP Address to connect to [Enter for localhost]");
            Console.Write(": ");
            host = Console.ReadLine();
            if (host is null || string.IsNullOrWhiteSpace(host))
            {
                Console.WriteLine("Using 127.0.0.1 as host!");
                host = "127.0.0.1";
            }
            Console.WriteLine("Enter Port [Enter for 8888]");
            Console.Write(": ");
            portstring = Console.ReadLine();
            port = 8888;
            if (portstring is not null || !string.IsNullOrWhiteSpace(portstring))
            {
                if (!int.TryParse(portstring, out int parsedport))
                {
                    Console.WriteLine("Using 8888 as port!");
                }
            }
            Console.WriteLine("Enter name [Enter for Player]");
            Console.Write(": ");
            name = Console.ReadLine() ?? "Player";
        }
        else
        {
            host = args[0];
            portstring = args[1];
            name = args[2];
            if(!int.TryParse(portstring, out int result))
            {
                Log.Warning("Can't parse port. using 8888");
                port = 8888;
            }
            port = result;
        }
        Console.WriteLine($"Connecting to {host}:{port} with name {name}");
        StaticClient.Host = host;
        StaticClient.Port = port;
        StaticClient.PlayerName = name;
        StaticClient.Init();
    }
}