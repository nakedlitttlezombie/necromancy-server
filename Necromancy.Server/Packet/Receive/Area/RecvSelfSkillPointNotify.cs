using Arrowgene.Buffers;
using Necromancy.Server.Common;
using Necromancy.Server.Model;
using Necromancy.Server.Packet.Id;

namespace Necromancy.Server.Packet.Receive.Area
{
    public class RecvSelfSkillPointNotify : PacketResponse
    {
        private uint _skillPoints;
        public RecvSelfSkillPointNotify(uint skillPoints)
            : base((ushort)AreaPacketId.recv_self_skill_point_notify, ServerType.Area)
        {
            _skillPoints = skillPoints;
        }

        protected override IBuffer ToBuffer()
        {
            IBuffer res = BufferProvider.Provide();
            res.WriteUInt32(_skillPoints);
            return res;
        }
    }
}
