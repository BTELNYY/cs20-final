﻿using cs20_final_library.Packets;
using cs20_final_library;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace cs20_final
{
    public class Client
    {
        TcpClient clientSocket = new();
        public HandshakeState handshakeState = HandshakeState.Connected;
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
            if (clientSocket is null)
            {
                return false;
            }
            return clientSocket.Connected;
        }

        public void Kick(DisconnectReason reason)
        {
            Send(new DisconnectPacket(reason));
            Console.WriteLine($"Disconnecting client {clientID}. With Reason: {reason.ToString()}");
            DestroyClient();
        }

        public void Kick(string reason)
        {
            Send(new DisconnectPacket(reason));
            Console.WriteLine($"Disconnecting client {clientID}. With reason: {reason}");
        }

        public void DestroyClient()
        {
            if (IsConnected())
            {
                clientSocket.Close();
            }
            Program.clients.Remove(clientID);
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
                    PingPacket p = PingPacket.GetFromBytes(data);
                    if (p != null && p.Reply)
                    {
                        Send(new PingPacket() { CompileTime = Utility.GetUnixTimestamp(), Reply = false });
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
                case 3:
                    HandleHandshake(ID, data);
                    break;
                default:
                    Send(new DisconnectPacket(DisconnectReason.BadPacket));
                    Console.WriteLine($"Disconnecting client {clientID} for bad packets.");
                    DestroyClient();
                    break;
            }
        }

        private void HandleHandshake(uint ID, byte[] data)
        {
            switch (ID)
            {
                case 3:
                    VersionPacket p = VersionPacket.GetFromBytes(data);
                    if (handshakeState == HandshakeState.Connected)
                    {
                        handshakeState = HandshakeState.GotVersion;
                        ushort[] ver = Utility.GetUshortsFromVersionString(Program.Version);
                        if (ver[0] != p.VersionMajor || ver[1] != p.VersionMinor || ver[2] != p.VersionPatch)
                        {
                            Kick(DisconnectReason.VersionMismatch);
                        }
                        else
                        {
                            Send(new VersionPacket(Program.Version));
                            handshakeState = HandshakeState.SentVersion;
                        }
                    }
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