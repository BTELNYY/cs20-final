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
        public uint PermissionStateLength { get; private set; } = 0;
        public UserPermissions PermissionState { get; set; } = new();

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
            NameLength = (uint)strbytes.Length;
            bytes = Utility.OverwriteArrayValue(8, bytes, BitConverter.GetBytes(NameLength));
            bytes = Utility.OverwriteArrayValue(12, bytes, strbytes);
            byte[] permissionstate = PermissionState.GetAsBytes();
            PermissionStateLength = (uint)permissionstate.Length;
            bytes = Utility.OverwriteArrayValue(((int)NameLength + 12), bytes, BitConverter.GetBytes(PermissionStateLength));
            bytes = Utility.OverwriteArrayValue(((int)NameLength + 16), bytes, permissionstate);
            return bytes;
        }

        public static new PlayerDataPacket GetFromBytes(byte[] bytes)
        {
            PlayerDataPacket p = new PlayerDataPacket();
            p.PacketID = BitConverter.ToUInt32(bytes, 0);
            p.PlayerID = BitConverter.ToUInt32(bytes, 4);
            p.NameLength = BitConverter.ToUInt32(bytes, 8);
            byte[] strbytes = Utility.Extract(bytes, 12, (int)p.NameLength);
            p.PlayerName = Encoding.ASCII.GetString(strbytes);
            p.PermissionStateLength = BitConverter.ToUInt32(bytes, (int)p.NameLength + 12);
            byte[] permissionbytes = Utility.Extract(bytes, 16 + (int)p.NameLength, (int)p.PermissionStateLength);
            return p;
        }
    }
}