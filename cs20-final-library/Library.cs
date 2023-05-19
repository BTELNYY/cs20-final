using System.Security.Cryptography;
using System;

namespace cs20_final_library
{
    public class Packet
    {
        public virtual uint PacketID { get; private set; }

        public Packet(uint packetID)
        {
            PacketID = packetID;
        }

        public Packet() { }
    }

    public static class Utility
    {
        public static Packet GetPacketFromBytes(byte[] bytes)
        {
            Packet packet = new Packet(BitConverter.ToUInt32(bytes, 0));
            uint id = packet.PacketID;
            switch (id)
            {
                case 0:
                    return new Packet(id);

                default:
                    return new Packet(uint.MaxValue);
            }
        }
    }
}