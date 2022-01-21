using Arrowgene.Buffers;
using Necromancy.Server.Common;
using Necromancy.Server.Model;
using Necromancy.Server.Packet.Id;

namespace Necromancy.Server.Packet.Receive.Area
{
    public class RecvRaisescaleRemoveItem : PacketResponse
    {
        private int _bagSlot;
        public RecvRaisescaleRemoveItem(int bagSlot)
            : base((ushort)AreaPacketId.recv_raisescale_remove_item_r, ServerType.Area)
        {
            _bagSlot = bagSlot;
        }

        protected override IBuffer ToBuffer()
        {
            //toDo , add error handling.
            IBuffer res = BufferProvider.Provide();
            res.WriteInt32(0); //Result
            return res;
        }
    }
}
