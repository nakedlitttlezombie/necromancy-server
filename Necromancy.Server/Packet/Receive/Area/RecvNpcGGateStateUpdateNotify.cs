using Arrowgene.Buffers;
using Necromancy.Server.Common;
using Necromancy.Server.Model;
using Necromancy.Server.Packet.Id;

namespace Necromancy.Server.Packet.Receive.Area
{
    public class RecvNpcGGateStateUpdateNotify : PacketResponse
    {
        public RecvNpcGGateStateUpdateNotify()
            : base((ushort)AreaPacketId.recv_npc_ggate_state_update_notify, ServerType.Area)
        {
        }

        protected override IBuffer ToBuffer()
        {
            IBuffer res = BufferProvider.Provide();
            res.WriteInt32(0);
            res.WriteInt32(0);
            return res;
        }
    }
}
