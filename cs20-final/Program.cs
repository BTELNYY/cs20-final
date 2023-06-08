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
    public static void Main(string[] args)
    {
        Client.PlayerConnected += EventHandler.OnJoin;
        Client.PlayerDisconnected += EventHandler.OnLeave;
        Log.Info("Starting server v" + Version + "...");
        Server.Init();
    }
}