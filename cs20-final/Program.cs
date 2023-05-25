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

    public static Dictionary<uint, Client> clients = new Dictionary<uint, Client>();

    static Thread ConsoleThread = new(ConsoleHandler.HandleCommands);

    public static void Main(string[] args)
    {
        Utility.DefinePackets();
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

    public class Client
    {
        TcpClient clientSocket = new();
        public uint clientID = 0;
        Thread clientThread;
        CancellationToken threadToken;
        CancellationTokenSource tokenSource;
        public void StartClient(TcpClient inClientSocket, uint clientNum, CancellationToken token, CancellationTokenSource source)
        {
            this.clientSocket = inClientSocket;
            this.clientID = clientNum;
            threadToken = token;
            tokenSource = source;
            clientThread = new(HandleClient);
            clientThread.Start();
        }

        public NetworkStream GetStream()
        {
            return clientSocket.GetStream();
        }

        public void Send(Packet p)
        {
            if (!IsConnected())
            {
                Utility.WriteLineColor("Cannot send packet: Socket not connected!", ConsoleColor.Red);
                return;
            }
            byte[] packet = p.GetAsBytes();
            Console.WriteLine($"Sending packet. Length: {packet.Length}");
            NetworkStream networkStream = GetStream();
            networkStream.Write(packet, 0, Packet.MaxSizePreset);
            networkStream.Flush();
        }

        public bool IsConnected()
        {
            if(clientSocket is null)
            {
                return false;
            }
            return clientSocket.Connected;
        }

        public void Kick(DisconnectReason reason)
        {
            Send(new DisconnectPacket(reason));
            Console.WriteLine($"Disconnecting client {clientID}.");
            DestroyClient();
        }

        public void DestroyClient()
        {
            if(IsConnected())
            {
                clientSocket.Close();
            }
            clients.Remove(clientID);
            clientSocket = null;
            tokenSource.Cancel();
            clientThread.Join();
            tokenSource.Cancel();
        }

        private void HandlePacket(uint ID, byte[] data)
        {
            switch (ID)
            {
                case 1:
                    //reply to client
                    PingPacket? p = Utility.GetPacketFromBytes(data) as PingPacket;
                    if (p != null && p.Reply)
                    {
                        Send(new PingPacket() { CompileTime = Utility.GetUnixTimestamp(), Reply = false});
                    }
                    break;
                case 2:
                    if (IsConnected())
                    {
                        Console.WriteLine($"Client with ID {clientID} has requested disconnect.");
                        clientSocket.Close();
                        DestroyClient();
                    }
                    else
                    {
                        Console.WriteLine($"Client with ID {clientID} requested disconnected but socket is already closed.");
                        DestroyClient();
                    }
                    break;
                default:
                    Send(new DisconnectPacket(DisconnectReason.BadPacket));
                    Console.WriteLine($"Disconnecting client {clientID} for bad packets.");
                    DestroyClient();
                    break;
            }
        }

        private void HandleClient()
        {
            byte[] bytesFrom = new byte[Packet.MaxSizePreset];
            int requestCount = 0;

            while (!tokenSource.IsCancellationRequested)
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
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Disconnecting client({clientID}) due to error. Error: " + ex.Message);
                    Kick(DisconnectReason.GeneralError);
                }
            }
            return;
        }
    }
}