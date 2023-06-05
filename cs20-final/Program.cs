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
}