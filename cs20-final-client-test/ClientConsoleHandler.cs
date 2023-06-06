using cs20_final_library.Packets;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
    }
}
