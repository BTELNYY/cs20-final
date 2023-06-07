using cs20_final_library.Packets;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using cs20_final_library;

namespace cs20_final_client_test
{
    public class ClientConsoleHandler
    {
        public static void HandleChat()
        {
            while (true)
            {
                string? input = Console.ReadLine();
                if(input is null)
                {
                    continue;
                }
                ChatPacket packet = new(StaticClient.PlayerName, input);
                StaticClient.Send(packet);
            }
        }

        public static void HandleServerChat(ChatPacket packet)
        {
            string prefix = "";
            if(packet.ChatSource == ChatSource.User)
            {
                prefix += "[CHAT] ";
            }
            if(packet.ChatSource == ChatSource.System)
            {
                prefix += "[SYSTEM] ";
            }
            if (packet.IsPrivate)
            {
                prefix += "[PRIVATE] ";
            }
            if(packet.ChatSource == ChatSource.System) 
            {
                Log.Info(prefix + ": " + packet.Message);
                return;
            }
            Log.Info(prefix + packet.Name + ": " + packet.Message);
        }
    }
}
