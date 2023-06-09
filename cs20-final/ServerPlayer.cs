using cs20_final_library;
using cs20_final_library.Packets;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace cs20_final
{
    public class ServerPlayer
    {
        public Client PlayerClient { get; private set; }
        public UserFlags UserPermissions { get; set; } = new UserFlags();
        public string Name { get; set; } = "";
        public uint PlayerID 
        {
            get
            {
                if(PlayerClient != null)
                {
                    return PlayerClient.clientID;
                }
                else
                {
                    Log.Warning("PlayerClient is null, returning 0 as PlayerID!");
                    return 0;
                }
            } 
        }

        public ServerPlayer(Client playerClient, string name)
        {
            PlayerClient = playerClient;
            Name = name;
        }

        public void UpdateOnServer(PlayerDataPacket packet)
        {
            Name = packet.PlayerName;
            if(UserPermissions != packet.UserState)
            {
                Log.Warning("Client attempted to change UserFlags, either desync or an exploit attempt.");
                PlayerClient.Kick("Attempted to modify UserFlags client side.");
            }
        }

        public void UpdateOnClient()
        {
            PlayerDataPacket playerDataPacket = new()
            {
                PlayerID = PlayerClient.clientID,
                PlayerName = Name,
                UserState = UserPermissions
            };
            PlayerClient.Send(playerDataPacket);
        }
    }
}
