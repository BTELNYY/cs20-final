using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using cs20_final_library.Packets;
using cs20_final_library;

namespace cs20_final
{
    public static class Server
    {
        public static Dictionary<uint, Client> clients = new Dictionary<uint, Client>();
        static Thread ConsoleThread = new(ConsoleHandler.HandleCommands);
        public static void Init()
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
        }

        public static void SendServerMessage(string message)
        {
            ChatPacket chatPacket = new("System", message, ChatSource.System);
            Log.Debug(chatPacket.Message);
            foreach(var client in clients.Values)
            {
                client.Send(chatPacket);
            }
            Log.Info($"[SYSTEM] {message}");
        }
    }
}
