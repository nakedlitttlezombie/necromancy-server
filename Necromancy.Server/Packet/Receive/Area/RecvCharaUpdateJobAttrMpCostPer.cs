using Arrowgene.Buffers;
using Necromancy.Server.Common;
using Necromancy.Server.Model;
using Necromancy.Server.Packet.Id;

namespace Necromancy.Server.Packet.Receive.Area
{
    public class RecvCharaUpdateJobAttrMpCostPer : PacketResponse
    {
        public RecvCharaUpdateJobAttrMpCostPer()
            : base((ushort)AreaPacketId.recv_chara_update_job_attr_mp_cost_per, ServerType.Area)
        {
        }

        protected override IBuffer ToBuffer()
        {
            IBuffer res = BufferProvider.Provide();
            res.WriteInt32(0);
            res.WriteInt16(0); //Percentage value most likely
            return res;
        }
    }
}
