using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using cs20_final_library;
using cs20_final_library.Packets;

namespace cs20_final_server;

public class Program
{
    //http://csharp.net-informations.com/communications/csharp-multi-threaded-server-socket.htm

    public static Dictionary<uint, Client> clients = new Dictionary<uint, Client>();

    public static void Main(string[] args)
    {
        Utility.DefinePackets();
        TcpListener serverSocket = new TcpListener(IPAddress.Parse("127.0.0.1"), 8888);
        TcpClient clientSocket = default;
        uint counter = 0;

        serverSocket.Start();
        Console.WriteLine("Server Started");

        counter = 0;
        while (true)
        {
            counter += 1;
            clientSocket = serverSocket.AcceptTcpClient();
            Console.WriteLine("Client connected. ID: " + counter.ToString());
            Client client = new Client();
            client.clientID = counter;
            client.StartClient(clientSocket, counter);
            clients.Add(counter, client);
        }

        clientSocket.Close();
        serverSocket.Stop();
        Console.WriteLine(" >> " + "exit");
        Console.ReadLine();
    }

    public class Client
    {
        TcpClient clientSocket = new();
        public uint clientID = 0;
        public void StartClient(TcpClient inClientSocket, uint clientNum)
        {
            this.clientSocket = inClientSocket;
            this.clientID = clientNum;
            Thread ctThread = new(HandleClient);
            ctThread.Start();
        }

        public NetworkStream GetStream()
        {
            return clientSocket.GetStream();
        }

        public void Send(Packet p)
        {
            byte[] packet = p.GetAsBytes();
            Console.WriteLine($"Sending packet. Length: {packet.Length}");
            NetworkStream networkStream = GetStream();
            networkStream.Write(packet, 0, Packet.MaxSizePreset);
            networkStream.Flush();
        }

        public bool IsConnected()
        {
            return clientSocket.Connected;
        }

        public void DestroyClient()
        {
            clients.Remove(clientID);
            clientSocket = null;
        }

        private void HandlePacket(uint ID, byte[] data)
        {
            switch (ID)
            {
                case 1:
                    //reply to client
                    Send(new PingPacket() { CompileTime = Utility.GetUnixTimestamp() });
                    break;
            }
        }

        private void HandleClient()
        {
            int requestCount = 0;
            byte[] bytesFrom = new byte[Packet.MaxSizePreset];
            string dataFromClient = "";
            byte[] sendBytes = { };
            string serverResponse = "";
            string rCount = "";
            requestCount = 0;

            while (true)
            {
                try
                {
                    if (!IsConnected())
                    {
                        Console.WriteLine("Client Disconnected: Socket Closed.", ConsoleColor.Red);
                        DestroyClient();
                        break;
                    }
                    else
                    {
                        requestCount = requestCount + 1;
                        NetworkStream networkStream = GetStream();
                        int count = networkStream.Read(bytesFrom, 0, Packet.MaxSizePreset);
                        uint ID = BitConverter.ToUInt32(bytesFrom, 0);
                        int IDint = Convert.ToInt32(ID);
                        Console.WriteLine($"Read data! Length: {count}, ID: {ID}");
                        HandlePacket(ID, bytesFrom);
                    }
                    /*
                    rCount = Convert.ToString(requestCount);
                    serverResponse = "Server to clinet(" + clNo + ") " + rCount;
                    sendBytes = Encoding.ASCII.GetBytes(serverResponse);
                    networkStream.Write(sendBytes, 0, sendBytes.Length);
                    networkStream.Flush();
                    Console.WriteLine(" >> " + serverResponse);
                    */
                }
                catch (Exception ex)
                {
                    Console.WriteLine(" >> " + ex.ToString());
                }
            }
            return;
        }
    }
}