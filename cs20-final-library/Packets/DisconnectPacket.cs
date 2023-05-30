using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using cs20_final_library;

namespace cs20_final_library.Packets
{
    public class DisconnectPacket : Packet
    {
        public override uint PacketID => 2;
        public uint DisconnectReason { get; set; } = 0;
        public uint CustomDisconnectReasonLength { get; private set; } = 0;
        public string CustomDisconnectReaon { get; set; } = "";

        public DisconnectPacket(uint disconnectReason)
        {
            DisconnectReason = disconnectReason;
        }

        public DisconnectPacket(DisconnectReason reason)
        {
            DisconnectReason = (uint)reason;
        }

        public DisconnectPacket(string reason)
        {
            DisconnectReason = (uint)cs20_final_library.DisconnectReason.Custom;
            CustomDisconnectReaon = reason;
            CustomDisconnectReasonLength = (uint)Encoding.ASCII.GetBytes(CustomDisconnectReaon).Length;
        }

        public DisconnectPacket() { }

        public override byte[] GetAsBytes()
        {
            byte[] bytes = new byte[MaxSizePreset];
            bytes = Utility.OverwriteArrayValue(0, bytes, BitConverter.GetBytes(PacketID));
            bytes = Utility.OverwriteArrayValue(4, bytes, BitConverter.GetBytes(DisconnectReason));
            byte[] strbytes = Encoding.ASCII.GetBytes(CustomDisconnectReaon);
            bytes = Utility.OverwriteArrayValue(8, bytes, BitConverter.GetBytes(strbytes.Length));
            bytes = Utility.OverwriteArrayValue(12, bytes, strbytes);
            return bytes;
        }

        public static new DisconnectPacket GetFromBytes(byte[] bytes)
        {
            DisconnectPacket p = new();
            p.PacketID = BitConverter.ToUInt32(bytes, 0);
            p.DisconnectReason = BitConverter.ToUInt32(bytes, 4);
            p.CustomDisconnectReasonLength = BitConverter.ToUInt32(bytes, 8);
            List<byte> strbytes = new();
            for(int i = 12; i < p.CustomDisconnectReasonLength; i++)
            {
                strbytes.Add(bytes[i]);
            }
            p.CustomDisconnectReaon = Encoding.ASCII.GetString(strbytes.ToArray());
            return p;
        }
    }
}
