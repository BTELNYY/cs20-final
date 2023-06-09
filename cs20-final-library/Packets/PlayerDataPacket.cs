using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata;
using System.Text;
using System.Threading.Tasks;

namespace cs20_final_library.Packets
{
    public class PlayerDataPacket : Packet
    {
        public override uint PacketID => 5;
        public uint PlayerID { get; set; } = 0;
        public uint NameLength { get; private set; } = 0;
        public uint UserStateLength { get; private set; } = 0;
        public string PlayerName { get; set; } = "";
        public string UserStateString { get; private set; } = "";
        public UserFlags UserState { get; set; } = new();

        public PlayerDataPacket() { }

        public PlayerDataPacket(uint playerID, string playerName)
        {
            PlayerID = playerID;
            PlayerName = playerName;
            NameLength = (uint)Encoding.ASCII.GetBytes(PlayerName).Length;
            UserStateString = JsonConvert.SerializeObject(UserState);
            UserStateLength = (uint)Encoding.ASCII.GetBytes(UserStateString).Length;
        }

        public override byte[] GetAsBytes()
        {
            byte[] bytes = new byte[MaxSize];
            Utility.OverwriteArrayValue(0, bytes, BitConverter.GetBytes(PacketID));
            Utility.OverwriteArrayValue(4, bytes, BitConverter.GetBytes(PlayerID));
            byte[] strbytes = Encoding.ASCII.GetBytes(PlayerName);
            NameLength = (uint)strbytes.Length;
            string json = JsonConvert.SerializeObject(UserState);
            byte[] permissionstate = Encoding.ASCII.GetBytes(json);
            UserStateLength = (uint)permissionstate.Length;
            bytes = Utility.OverwriteArrayValue(8, bytes, BitConverter.GetBytes(NameLength));
            bytes = Utility.OverwriteArrayValue(12, bytes, BitConverter.GetBytes(UserStateLength));
            bytes = Utility.OverwriteArrayValue(16, bytes, strbytes);
            //Log.Debug("GetAsBytes: " + json);
            bytes = Utility.OverwriteArrayValue(((int)NameLength + 16), bytes, permissionstate);
            return bytes;
        }

        public static new PlayerDataPacket GetFromBytes(byte[] bytes)
        {
            PlayerDataPacket p = new PlayerDataPacket();
            p.PacketID = BitConverter.ToUInt32(bytes, 0);
            p.PlayerID = BitConverter.ToUInt32(bytes, 4);
            p.NameLength = BitConverter.ToUInt32(bytes, 8);
            p.UserStateLength = BitConverter.ToUInt32(bytes, 12);
            byte[] strbytes = Utility.Extract(bytes, 16, (int)p.NameLength);
            p.PlayerName = Encoding.ASCII.GetString(strbytes);
            byte[] permissionbytes = Utility.Extract(bytes, 16 + (int)p.NameLength, (int)p.UserStateLength);
            string json = Encoding.ASCII.GetString(permissionbytes);
            //Log.Debug("GetFromBytes: " + json);
            p.UserState = JsonConvert.DeserializeObject<UserFlags>(json);
            return p;
        }
    }
}