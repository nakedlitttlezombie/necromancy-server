using Arrowgene.Buffers;
using Necromancy.Server.Common;
using Necromancy.Server.Model;
using Necromancy.Server.Packet.Id;

namespace Necromancy.Server.Packet.Receive.Area
{
    public class RecvAuctionNotifyUpdateExhibitState : PacketResponse
    {
        public RecvAuctionNotifyUpdateExhibitState()
            : base((ushort)AreaPacketId.recv_auction_notify_update_exhibit_state, ServerType.Area)
        {
        }

        protected override IBuffer ToBuffer()
        {
            // only works while auction is open
            IBuffer res = BufferProvider.Provide();
            res.WriteInt32(0); //slot number
            res.WriteInt32(0); //state : 0 unbid, 1 accepted, 2 no bids 
            return res;
        }
    }
}
