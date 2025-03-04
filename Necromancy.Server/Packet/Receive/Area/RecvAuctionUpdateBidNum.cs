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
            IBuffer res = BufferProvider.Provide();
            res.WriteInt32(0); // auction id

            res.WriteInt32(0); // "bidnumber"?
            return res;
        }
    }
}
