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
            TcpClient clientSocket;
            uint counter = 0;
            serverSocket.Start();
            Log.Info("Accepting client connections.");
            ConsoleThread.Start();
            counter = 0;
            Log.Info("Server ready.");
            while (true)
            {
                counter += 1;
                clientSocket = serverSocket.AcceptTcpClient();
                Log.Debug("Client connected. ID: " + counter.ToString());
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
            //Log.Debug(chatPacket.Payload);
            foreach(var client in clients.Values)
            {
                client.Send(chatPacket);
            }
            Log.Info($"[SYSTEM] {message}");
        }

        public static void SendPrivateServerMessage(Client client, string message)
        {
            ChatPacket chatPacket = new("System", client.GetName(), message, ChatSource.System);
            //Log.Debug(chatPacket.Payload);
            client.Send(chatPacket);
            Log.Info($"[SYSTEM] [PRIVATE] Server -> {client.GetName()} {message}");
        }
    }
}
