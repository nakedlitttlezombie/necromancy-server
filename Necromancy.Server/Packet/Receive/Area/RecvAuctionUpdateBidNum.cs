using Arrowgene.Buffers;
using Necromancy.Server.Common;
using Necromancy.Server.Model;
using Necromancy.Server.Packet.Id;

namespace Necromancy.Server.Packet.Receive.Area
{
    public class RecvAuctionUpdateBidNum : PacketResponse
    {
        public RecvAuctionUpdateBidNum()
            : base((ushort)AreaPacketId.recv_auction_update_bid_num, ServerType.Area)
        {
        }

        protected override IBuffer ToBuffer()
        {
            // only works while auction is open
            IBuffer res = BufferProvider.Provide();
            res.WriteInt32(0); // auction lot slot
            res.WriteInt32(0); // highest bid
            return res;
        }
    }
}
