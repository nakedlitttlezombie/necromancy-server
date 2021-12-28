using Arrowgene.Buffers;
using Necromancy.Server.Common;
using Necromancy.Server.Model;
using Necromancy.Server.Packet.Id;
using Necromancy.Server.Systems.Auction;
using Necromancy.Server.Systems.Item;

namespace Necromancy.Server.Packet.Area
{
    public class SendAuctionDeregistSearchItemCond : ClientHandler
    {
        public SendAuctionDeregistSearchItemCond(NecServer server) : base(server)
        {
        }


        public override ushort id => (ushort)AreaPacketId.send_auction_deregist_search_item_cond;

        public override void Handle(NecClient client, NecPacket packet)
        {
            byte index = packet.data.ReadByte(); //index to delete

            AuctionService auctionService = new AuctionService();
            int auctionError = 0;
            try
            {
                auctionService.DeregistSearchCond(client, index, true);
            }
            catch (AuctionException e)
            {
                auctionError = (int)e.type;
            }

            IBuffer res = BufferProvider.Provide();
            res.WriteInt32(auctionError);
            router.Send(client, (ushort)AreaPacketId.recv_auction_deregist_search_item_cond_r, res, ServerType.Area);
        }
    }
}
