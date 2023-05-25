using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace cs20_final_library.Packets
{
    public class VersionPacket : Packet
    {
        public override uint PacketID => 3;
        public ushort VersionMajor = 1;
        public ushort VersionMinor = 0;
        public ushort VersionPatch = 0;

        public override byte[] GetAsBytes()
        {
            byte[] bytes = new byte[MaxSize];
            Utility.OverwriteArrayValue(0, bytes, BitConverter.GetBytes(PacketID));
            Utility.OverwriteArrayValue(4, bytes, BitConverter.GetBytes(VersionMajor));
            Utility.OverwriteArrayValue(6, bytes, BitConverter.GetBytes(VersionMinor));
            Utility.OverwriteArrayValue(8, bytes, BitConverter.GetBytes(VersionPatch));
            return bytes;
        }

        public static new VersionPacket GetFromBytes(byte[] data)
        {
            VersionPacket p = new VersionPacket();
            p.PacketID = BitConverter.ToUInt32(data, 0);
            p.VersionMajor = BitConverter.ToUInt16(data, 4);
            p.VersionMinor = BitConverter.ToUInt16(data, 6);
            p.VersionPatch = BitConverter.ToUInt16(data, 8);
            return p;
        }

        public VersionPacket() { } 

        public VersionPacket(string version)
        {
            string[] arr = version.Split('.');
            VersionMajor = ushort.Parse(arr[0]);
            VersionMinor = ushort.Parse(arr[1]);
            VersionPatch = ushort.Parse(arr[2]);
        }
    }
}
