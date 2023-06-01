using cs20_final_library;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace cs20_final
{
    public class Player
    {
        public Client PlayerClient { get; private set; }
        public UserPermissionState UserPermissions { get; set; } = new UserPermissionState();
        public string Name { get; set; } = "";

        public Player(Client playerClient, string name)
        {
            PlayerClient = playerClient;
            Name = name;
        }

        public void UpdateOnClient()
        {

        }
    }
}
