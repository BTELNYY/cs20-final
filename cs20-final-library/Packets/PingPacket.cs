using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace cs20_final_library.Packets
{
    public class PingPacket : Packet
    {
        public override uint PacketID => 1;
        public long CompileTime;

        public override byte[] GetAsBytes()
        {
            byte[] bytes = new byte[MaxSize];
            bytes = Utility.OverwriteArrayValue(0, bytes, BitConverter.GetBytes(PacketID));
            bytes = Utility.OverwriteArrayValue(4, bytes, BitConverter.GetBytes(CompileTime));
            return bytes;
        }
        public static new PingPacket GetFromBytes(byte[] bytes)
        {
            PingPacket p = new PingPacket();
            p.PacketID = BitConverter.ToUInt32(bytes, 0);
            p.CompileTime = BitConverter.ToInt64(bytes, 4);
            return p;
        }

        public PingPacket(long compileTime) 
        {
            CompileTime = compileTime;
        }

        public PingPacket() { }
    }
}
