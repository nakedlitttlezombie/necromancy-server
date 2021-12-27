using Arrowgene.Buffers;
using Necromancy.Server.Common;
using Necromancy.Server.Model;
using Necromancy.Server.Packet.Id;

namespace Necromancy.Server.Packet.Receive.Area
{
    public class RecvItemUpdateEnchantId : PacketResponse
    {
        private ulong _instanceId;
        private int _enchantId;
        public RecvItemUpdateEnchantId(ulong instanceId, int enchantId)
            : base((ushort)AreaPacketId.recv_item_update_enchantid, ServerType.Area)
        {
            _instanceId = instanceId;
            _enchantId = enchantId;
        }

        protected override IBuffer ToBuffer()
        {
            IBuffer res = BufferProvider.Provide();
            res.WriteUInt64(_instanceId);
            res.WriteInt32(_enchantId);
            return res;
        }
    }
}
