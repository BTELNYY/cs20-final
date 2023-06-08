using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace cs20_final_library.Packets
{
    public class ChatPacket : Packet
    {
        public override uint PacketID => 6;
        public ChatSource ChatSource { get; set; } = ChatSource.User;
        public bool IsPrivate { get; set; } = false;
        public uint PayloadLength { get; private set; } = 0;
        public string Payload { get; set; } = "";
        public PayloadJSON JSONPayload { get; set; } = new();

        public ChatPacket(string name, string message)
        {
            ChatSource = ChatSource.User;
            PayloadJSON json = new()
            {
                Name = name,
                PrivateTargetName = "",
                Message = message,
            };
            JSONPayload = json;
            string payload = JsonConvert.SerializeObject(json);
            Payload = payload;
            //Log.Debug("Init payload:" + Payload);
            PayloadLength = (uint)Encoding.ASCII.GetBytes(Payload).Length;
        }

        public ChatPacket(string name, string message, ChatSource source) 
        {
            PayloadJSON json = new()
            {
                Name = name,
                PrivateTargetName = "",
                Message = message,
            };
            JSONPayload = json;
            string payload = JsonConvert.SerializeObject(json);
            Payload = payload;
            //Log.Debug("Init payload:" + Payload);
            ChatSource = source;
            PayloadLength = (uint)Encoding.ASCII.GetBytes(Payload).Length;
        }

        public ChatPacket(string name, string privatetargetname, string message, ChatSource source)
        {
            PayloadJSON json = new()
            {
                Name = name,
                PrivateTargetName = privatetargetname,
                Message = message,
            };
            JSONPayload = json;
            string payload = JsonConvert.SerializeObject(json);
            Payload = payload;
            //Log.Debug("Init payload:" + Payload);
            ChatSource = source;
            PayloadLength = (uint)Encoding.ASCII.GetBytes(Payload).Length;
        }

        public override byte[] GetAsBytes()
        {
            byte[] bytes = new byte[MaxSize];
            bytes = Utility.OverwriteArrayValue(0, bytes, BitConverter.GetBytes(PacketID));
            bytes = Utility.OverwriteArrayValue(4, bytes, BitConverter.GetBytes((ushort)ChatSource));
            bytes = Utility.OverwriteArrayValue(6, bytes, BitConverter.GetBytes(IsPrivate));
            bytes = Utility.OverwriteArrayValue(7, bytes, BitConverter.GetBytes(PayloadLength));
            //Log.Debug("get as bytes payload " + Payload);
            bytes = Utility.OverwriteArrayValue(11, bytes, Encoding.ASCII.GetBytes(Payload));
            return bytes;
        }

        public static new ChatPacket GetFromBytes(byte[] bytes)
        {
            ChatSource chatSource = (ChatSource)BitConverter.ToUInt16(bytes, 4);
            bool isPrivate = BitConverter.ToBoolean(bytes, 6);
            uint nameLength = BitConverter.ToUInt32(bytes, 7);
            byte[] payloadBytes = Utility.Extract(bytes, 11, (int)nameLength);
            string payload = Encoding.ASCII.GetString(payloadBytes);
            //Log.Debug("Get from bytes payload" + payload);
            PayloadJSON json = JsonConvert.DeserializeObject<PayloadJSON>(payload);
            ChatPacket packet = new(json.Name, json.PrivateTargetName, json.Message, chatSource);
            packet.IsPrivate = isPrivate;
            packet.JSONPayload = json;
            return packet;
        }
    }

    public struct PayloadJSON
    {
        public string Name { get; set; }
        public string PrivateTargetName { get; set; }
        public string Message { get; set; }
    }
}
