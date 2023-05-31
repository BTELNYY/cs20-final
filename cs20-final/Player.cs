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

        public string Name = "";
        public uint AssignedClientId = 0;
    }
}
