using cs20_final_library;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace cs20_final
{
    internal static class ConsoleHandler
    {
        internal static void HandleCommands()
        {
            try
            {
                Console.WriteLine("Accepting commands.");
                while (true)
                {
                    Console.Write("> ");
                    string? input = Console.ReadLine();
                    if (string.IsNullOrEmpty(input))
                    {
                        Utility.WriteLineColor("Bad command.", ConsoleColor.Red);
                        continue;
                    }
                    string[] cmd = input.Split(' ');
                    switch (cmd[0])
                    {
                        case "list":
                            Utility.WriteLineColor("Currently connected sockets", ConsoleColor.White);
                            foreach (var clientid in Server.clients)
                            {
                                Console.WriteLine($"ID: {clientid.Value.clientID} Name: {clientid.Value.GetName()}");
                            }
                            break;
                        case "kick":
                            if (cmd.Length > 2 && uint.TryParse(cmd[1], out uint result) && uint.TryParse(cmd[2], out uint kickreason))
                            {
                                if (Server.clients.ContainsKey(result))
                                {
                                    Server.clients[result].Kick(Utility.GetReason(kickreason));
                                }
                            }
                            break;
                        default:
                            Utility.WriteLineColor("Unknown command.", ConsoleColor.Red);
                            break;
                    }
                }
            }catch(Exception ex)
            {
                Log.Error("Failed to execute command. Error: " + ex.ToString());
            }
        }
    }
}
