using Arrowgene.Buffers;
using Necromancy.Server.Common;
using Necromancy.Server.Model;
using Necromancy.Server.Packet.Id;

namespace Necromancy.Server.Packet.Receive.Area
{
    public class RecvSoulFragmentRequestSkillListR : PacketResponse
    {
        public RecvSoulFragmentRequestSkillListR()
            : base((ushort)AreaPacketId.recv_soul_fragment_request_skill_list_r, ServerType.Area)
        {
        }

        protected override IBuffer ToBuffer()
        {
            IBuffer res = BufferProvider.Provide();
            int numEntries = 0x5;
            res.WriteInt32(numEntries); //less than 0x5
            for (int k = 0; k < numEntries; k++)
            {
                //sub_496E30
                res.WriteInt32(k); //slot ID 0 to 4
                res.WriteInt32(211021); // Skill ID ????
                res.WriteByte(1); // 1 = Skill in slot.  
            }

            return res;
        }
    }
}
