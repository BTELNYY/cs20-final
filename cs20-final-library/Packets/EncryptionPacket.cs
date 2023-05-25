using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace cs20_final_library.Packets
{
    public class EncryptionPacket : Packet
    {
        public override uint PacketID => 4;
        public byte EncryptionType = 0;

    }
}
