using System.Security.Cryptography;
using System;
using cs20_final_library.Packets;

namespace cs20_final_library
{
    public class Packet
    {
        public static int MaxSizePreset = 4096;
        public virtual uint MaxSize { get; } = (uint)MaxSizePreset;
        public virtual uint PacketID { get; internal set; }

        public virtual byte[] GetAsBytes()
        {
            throw new InvalidDataException("Can't call this on Packet base class!");
        }

        public static Packet GetFromBytes(byte[] bytes)
        {
            Packet p = new Packet();
            p.PacketID = BitConverter.ToUInt32(bytes, 0);
            return p;
        }

        public Packet() { }
    }

    public static class Utility
    {
        public static Dictionary<uint, Type> PacketDefs = new();

        public static void DefinePackets()
        {
            PacketDefs.Add(0, typeof(Packet));
            PacketDefs.Add(1, typeof(PingPacket));
        }

        public static long GetUnixTimestamp()
        {
            long unixTimestamp = (long)DateTime.UtcNow.Subtract(new DateTime(1970, 1, 1)).TotalSeconds;
            return unixTimestamp;
        }

        public static byte[] OverwriteArrayValue(int start, byte[] array, byte[] insert)
        {
            Array.Copy(insert, 0, array, start, insert.Length);
            return array;
        }

        public static Packet GetPacketFromBytes(byte[] array)
        {
            uint ID = BitConverter.ToUInt32(array, 0);
            switch (ID)
            {
                case 0:
                    return Packet.GetFromBytes(array);
                case 1:
                    return PingPacket.GetFromBytes(array);
                case 2:
                    return DisconnectPacket.GetFromBytes(array);
                default: 
                    return Packet.GetFromBytes(array);
            }
        }

        public static DisconnectReason GetReason(uint ID)
        {
            try
            {
                return (DisconnectReason)ID;
            }
            catch
            {
                return DisconnectReason.Unknown;
            }
        }

        public static void WriteLineColor(string message, ConsoleColor color)
        {
            Console.ForegroundColor = color;
            Console.WriteLine(message);
            Console.ResetColor();
        }
    }
    public enum DisconnectReason
    {
        Unknown = -1,
        None = 0,
        BadPacket = 1,
        AuthError = 2,
        HandshakeError = 3,
        VersionMismatch = 4,
        GeneralError = 5,
    }
}