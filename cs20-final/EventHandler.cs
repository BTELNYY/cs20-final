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
    }
}
