using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace cs20_final_library.Packets
{
    public class ChatPacket : Packet
    {
        public override uint PacketID => 6;
        public uint NameLength { get; private set; } = 0;
        public uint MessageLength { get; private set; } = 0;
        public string Name { get; set; } = "";
        public string Message { get; set; } = "";

        public ChatPacket(string name, string message)
        {
            Name = name; 
            Message = message;
            MessageLength = (uint)Encoding.ASCII.GetBytes(Message).Length;
            NameLength = (uint)Encoding.ASCII.GetBytes(Name).Length;
        }

        public override byte[] GetAsBytes()
        {
            byte[] bytes = new byte[MaxSize];
            bytes = Utility.OverwriteArrayValue(0, bytes, BitConverter.GetBytes(PacketID));
            bytes = Utility.OverwriteArrayValue(4, bytes, BitConverter.GetBytes(NameLength));
            bytes = Utility.OverwriteArrayValue(8, bytes, BitConverter.GetBytes(MessageLength));
            bytes = Utility.OverwriteArrayValue(12, bytes, Encoding.ASCII.GetBytes(Name));
            bytes = Utility.OverwriteArrayValue(12 + (int)NameLength, bytes, Encoding.ASCII.GetBytes(Message));
            return bytes;
        }

        public static new ChatPacket GetFromBytes(byte[] bytes)
        {
            uint nameLength = BitConverter.ToUInt32(bytes, 4);
            uint messageLength = BitConverter.ToUInt32(bytes, 8);
            byte[] nameBytes = Utility.Extract(bytes, 12, (int)nameLength);
            string name = Encoding.ASCII.GetString(nameBytes);
            byte[] messageBytes = Utility.Extract(bytes, 12 + (int)nameLength, (int)nameLength);
            string message = Encoding.ASCII.GetString(messageBytes);
            ChatPacket packet = new(name, message);
            return packet;
        }
    }
}
