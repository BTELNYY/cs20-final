using System.Security.Cryptography;
using System;
using cs20_final_library.Packets;
using System.Text;

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

        public static byte[] GetStringAsBytes(string input)
        {
            return Encoding.ASCII.GetBytes(input);
        }

        public static string GetBytesAsString(byte[] bytes)
        {
            return Encoding.ASCII.GetString(bytes);
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

        public static ushort[] GetUshortsFromVersionString(string version)
        {
            ushort[] shortver = { 1, 0, 0 };
            string[] strings = version.Split('.');
            shortver[0] = ushort.Parse(strings[0]);
            shortver[1] = ushort.Parse(strings[1]);
            shortver[2] = ushort.Parse(strings[2]);
            return shortver;
        }
    }
    public enum DisconnectReason
    {
        Unknown = -1,
        Custom = 0,
        BadPacket = 1,
        AuthError = 2,
        HandshakeError = 3,
        VersionMismatch = 4,
        GeneralError = 5,
        UnexpectedPacket = 6,
        HandshakeFailed = 7,
    }

    public enum HandshakeState
    {
        Unknown = -1,
        Disonnected = 0,
        Connected = 1,
        SentVersion = 2,
        GotVersion = 3,
        GotEncryptionRequest = 4,
        SentEncryptionRequest = 5,
        GotPlayerData = 6,
        SentPlayerData = 7,
    }
}