using System;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace cs20_final_server;

public class Program
{
    //http://csharp.net-informations.com/communications/csharp-multi-threaded-server-socket.htm

    public static void Main(string[] args)
    {
        TcpListener serverSocket = new TcpListener(IPAddress.Parse("127.0.0.1") ,8888);
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

        }
    }
}