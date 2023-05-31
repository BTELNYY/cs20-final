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
        public static T[] Extract<T>(T[] source, int fromIndex, int length)
        {
            if (null == source)
            {
                throw new ArgumentNullException(nameof(source));
            }
            else if (fromIndex < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(fromIndex), "From Index must be non-negative");
            }
            else if (length < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(length), "Length must be non-negative");
            }


            if (fromIndex >= source.Length || length == 0)
                return new T[0];

            T[] result = new T[Math.Min(length, source.Length - fromIndex)];

            Array.Copy(source, fromIndex, result, 0, result.Length);

            return result;
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
    }

    public enum HandshakeState
    {
        Unknown = -1,
        Disonnected = 0,
        Connected = 1,
        SentVersion = 2,
        GotVersion = 3,
        GotPlayerData = 4,
        SentPlayerData = 5,
    }
}