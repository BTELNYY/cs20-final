using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace cs20_final_library.Packets
{
    public class EncryptionPacket : Packet
    {
        public override uint PacketID => 4;
        public ushort EncryptionType { get; set; } = 0;
        public uint KeyLength { get; private set; } = 0;
        public string PublicKey { get; set; } = "";

        public override byte[] GetAsBytes()
        {
            byte[] bytes = new byte[MaxSize];
            bytes = Utility.OverwriteArrayValue(0, bytes, BitConverter.GetBytes(PacketID));
            bytes = Utility.OverwriteArrayValue(4, bytes, BitConverter.GetBytes(EncryptionType));
            byte[] strbytes = Encoding.ASCII.GetBytes(PublicKey);
            bytes = Utility.OverwriteArrayValue(6, bytes, BitConverter.GetBytes(strbytes.Length));
            bytes = Utility.OverwriteArrayValue(10, bytes, strbytes);
            return bytes;
        }

        public static new EncryptionPacket GetFromBytes(byte[] bytes)
        {
            EncryptionPacket p = new EncryptionPacket();
            p.PacketID = BitConverter.ToUInt32(bytes, 0);
            p.EncryptionType = BitConverter.ToUInt16(bytes, 4);
            p.KeyLength = BitConverter.ToUInt32(bytes, 6);
            List<byte> strbytes = new();
            for (int i = 10; i < p.KeyLength; i++)
            {
                strbytes.Add(bytes[i]);
            }
            p.PublicKey = Encoding.ASCII.GetString(strbytes.ToArray());
            return p;
        }
    }
}
