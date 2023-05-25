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

        public DisconnectPacket(uint disconnectReason)
        {
            DisconnectReason = disconnectReason;
        }

        public DisconnectPacket(DisconnectReason reason)
        {
            DisconnectReason = (uint)reason;
        }

        public DisconnectPacket() { }

        public override byte[] GetAsBytes()
        {
            byte[] bytes = new byte[MaxSizePreset];
            bytes = Utility.OverwriteArrayValue(0, bytes, BitConverter.GetBytes(PacketID));
            bytes = Utility.OverwriteArrayValue(4, bytes, BitConverter.GetBytes(DisconnectReason));
            return bytes;
        }

        public static new DisconnectPacket GetFromBytes(byte[] bytes)
        {
            DisconnectPacket p = new();
            p.PacketID = BitConverter.ToUInt32(bytes, 0);
            p.DisconnectReason = BitConverter.ToUInt32(bytes, 4);
            return p;
        }
    }
}
