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
            client.startClient(clientSocket, counter);
        }

        clientSocket.Close();
        serverSocket.Stop();
        Console.WriteLine(" >> " + "exit");
        Console.ReadLine();
    }

    public class Client
    {
        TcpClient clientSocket = new();
        uint clNo = 0;
        public void startClient(TcpClient inClientSocket, uint clientNum)
        {
            this.clientSocket = inClientSocket;
            this.clNo = clientNum;
            Thread ctThread = new Thread(handleClient);
            ctThread.Start();
        }

        private void handleClient()
        {
            int requestCount = 0;
            byte[] bytesFrom = new byte[10025];
            string dataFromClient = "";
            byte[] sendBytes = { };
            string serverResponse = "";
            string rCount = "";
            requestCount = 0;

            while (true)
            {
                try
                {
                    requestCount = requestCount + 1;
                    NetworkStream networkStream = clientSocket.GetStream();
                    int count = networkStream.Read(bytesFrom, 0, (int)clientSocket.ReceiveBufferSize);

                    uint ID = BitConverter.ToUInt32(bytesFrom, 0);
                    int IDint = Convert.ToInt32(ID);
                    Console.WriteLine($"Read data! Length: {count}, ID: {ID}");


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
        }
    }
}