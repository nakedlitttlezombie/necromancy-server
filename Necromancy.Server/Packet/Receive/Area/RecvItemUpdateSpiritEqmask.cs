using Arrowgene.Buffers;
using Necromancy.Server.Common;
using Necromancy.Server.Model;
using Necromancy.Server.Packet.Id;

namespace Necromancy.Server.Packet.Receive.Area
{
    public class RecvItemUpdateSpiritEqmask : PacketResponse
    {
        private ulong _instanceId;
        private int _spiritMask;
        public RecvItemUpdateSpiritEqmask(ulong instanceId, int spiritMask)
            : base((ushort)AreaPacketId.recv_item_update_spirit_eqmask, ServerType.Area)
        {
            _instanceId = instanceId;
            _spiritMask = spiritMask;
        }

        protected override IBuffer ToBuffer()
        {
            IBuffer res = BufferProvider.Provide();
            res.WriteUInt64(_instanceId);
            res.WriteInt32(_spiritMask);
            return res;
        }
    }
}
