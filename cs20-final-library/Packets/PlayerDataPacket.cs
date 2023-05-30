using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace cs20_final_library.Packets
{
    public class PlayerDataPacket : Packet
    {
        public override uint PacketID => 5;
        public uint PlayerID { get; set; } = 0;
        public uint NameLength { get; private set; } = 0;
        public string PlayerName { get; set; } = "";

        public PlayerDataPacket() { }

        public PlayerDataPacket(uint playerID, string playerName)
        {
            PlayerID = playerID;
            PlayerName = playerName;
            NameLength = (uint)Encoding.ASCII.GetBytes(PlayerName).Length;
        }

        public override byte[] GetAsBytes()
        {
            byte[] bytes = new byte[MaxSize];
            Utility.OverwriteArrayValue(0, bytes, BitConverter.GetBytes(PacketID));
            Utility.OverwriteArrayValue(4, bytes, BitConverter.GetBytes(PlayerID));
            byte[] strbytes = Encoding.ASCII.GetBytes(PlayerName);
            bytes = Utility.OverwriteArrayValue(8, bytes, BitConverter.GetBytes(strbytes.Length));
            bytes = Utility.OverwriteArrayValue(12, bytes, strbytes);
            return bytes;
        }

        public static new PlayerDataPacket GetFromBytes(byte[] bytes)
        {
            PlayerDataPacket p = new PlayerDataPacket();
            p.PacketID = BitConverter.ToUInt32(bytes, 0);
            p.PlayerID = BitConverter.ToUInt32(bytes, 4);
            p.NameLength = BitConverter.ToUInt32(bytes, 8);
            List<byte> strbytes = new();
            for (int i = 12; i < p.NameLength; i++)
            {
                strbytes.Add(bytes[i]);
            }
            p.PlayerName = Encoding.ASCII.GetString(strbytes.ToArray());
            return p;
        }
    }
}
