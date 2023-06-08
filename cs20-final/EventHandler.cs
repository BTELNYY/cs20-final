using cs20_final_library;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace cs20_final
{
    public static class EventHandler
    {
        public static void OnJoin(object? sender, ServerPlayer player) 
        {
            Server.SendServerMessage($"{player.Name} joined.");
        }

        public static void OnLeave(object? sender, Client.DisconnectStruct disconnectReason)
        {
            if(disconnectReason.DisconnectReason == DisconnectReason.Custom)
            {
                Server.SendServerMessage($"{disconnectReason.Player.Name} was kicked with reason: {disconnectReason.CustomDisconnectReason}");
            }
            else
            {
                Server.SendServerMessage($"{disconnectReason.Player.Name} left.");
            }
        }
    }
}
