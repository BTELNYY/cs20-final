using cs20_final_library;
using cs20_final_library.Packets;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace cs20_final_client_test
{
    public class ClientPlayer
    {
        public string Name { get; set; } = "";
        public UserFlags UserFlags { get; set; } = new();
        public uint ClientID { get; set; } = 0;

        public void UpdateOnClient(PlayerDataPacket packet)
        {
            Name = packet.PlayerName;
            UserFlags = packet.PermissionState;
            ClientID = packet.PlayerID;
        }
    }
}
