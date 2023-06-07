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
        public ChatSource ChatSource { get; set; } = ChatSource.User;
        public bool IsPrivate { get; set; } = false;
        public uint NameLength { get; private set; } = 0;
        public uint PrivateTargetNameLength { get; private set; } = 0;
        public uint MessageLength { get; private set; } = 0;
        public string Name { get; set; } = "";
        public string PrivateTargetName { get; set; } = "";
        public string Message { get; set; } = "";

        public ChatPacket(string name, string message)
        {
            ChatSource = ChatSource.User;
            Name = name; 
            Message = message;
            MessageLength = (uint)Encoding.ASCII.GetBytes(Message).Length;
            NameLength = (uint)Encoding.ASCII.GetBytes(Name).Length;
        }

        public ChatPacket(string name, string message, ChatSource source) 
        {
            ChatSource = source;
            Name = name;
            Message = message;
            MessageLength = (uint)Encoding.ASCII.GetBytes(Message).Length;
            NameLength = (uint)Encoding.ASCII.GetBytes(Name).Length;
        }

        public ChatPacket(string name, string privatetargetname, string message, ChatSource source)
        {
            Name = name;
            PrivateTargetName = privatetargetname;
            Message = message;
            ChatSource = source;
            MessageLength = (uint)Encoding.ASCII.GetBytes(Message).Length;
            PrivateTargetNameLength = (uint)Encoding.ASCII.GetBytes(PrivateTargetName).Length;
            NameLength = (uint)Encoding.ASCII.GetBytes(Name).Length;
            if(PrivateTargetNameLength != 0)
            {
                IsPrivate = true;
            }
        }

        public override byte[] GetAsBytes()
        {
            byte[] bytes = new byte[MaxSize];
            bytes = Utility.OverwriteArrayValue(0, bytes, BitConverter.GetBytes(PacketID));
            bytes = Utility.OverwriteArrayValue(4, bytes, BitConverter.GetBytes((ushort)ChatSource));
            bytes = Utility.OverwriteArrayValue(6, bytes, BitConverter.GetBytes(IsPrivate));
            bytes = Utility.OverwriteArrayValue(7, bytes, BitConverter.GetBytes(NameLength));
            bytes = Utility.OverwriteArrayValue(11, bytes, BitConverter.GetBytes(PrivateTargetNameLength));
            bytes = Utility.OverwriteArrayValue(15, bytes, BitConverter.GetBytes(MessageLength));
            bytes = Utility.OverwriteArrayValue(19, bytes, Encoding.ASCII.GetBytes(Name));
            bytes = Utility.OverwriteArrayValue(19 + (int)NameLength, bytes, Encoding.ASCII.GetBytes(PrivateTargetName));
            bytes = Utility.OverwriteArrayValue(19 + (int)NameLength + (int)PrivateTargetNameLength, bytes, Encoding.ASCII.GetBytes(Message));
            return bytes;
        }

        public static new ChatPacket GetFromBytes(byte[] bytes)
        {
            ChatSource chatSource = (ChatSource)BitConverter.ToUInt16(bytes, 4);
            bool isPrivate = BitConverter.ToBoolean(bytes, 6);
            uint nameLength = BitConverter.ToUInt32(bytes, 7);
            uint privateTargetNameLength = BitConverter.ToUInt32(bytes, 11);
            uint messageLength = BitConverter.ToUInt32(bytes, 15);
            byte[] nameBytes = Utility.Extract(bytes, 19, (int)nameLength);
            string name = Encoding.ASCII.GetString(nameBytes);
            byte[] privateTargetBytes = Utility.Extract(bytes, 19 + (int)nameLength, (int)privateTargetNameLength);
            string privateTargetName = Encoding.ASCII.GetString(privateTargetBytes);
            byte[] messageBytes = Utility.Extract(bytes, 19 + (int)nameLength + (int)privateTargetNameLength, (int)nameLength);
            string message = Encoding.ASCII.GetString(messageBytes);
            ChatPacket packet = new(name, privateTargetName, message, chatSource);
            return packet;
        }
    }
}
