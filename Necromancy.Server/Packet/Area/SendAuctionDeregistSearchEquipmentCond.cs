using Arrowgene.Buffers;
using Necromancy.Server.Common;
using Necromancy.Server.Model;
using Necromancy.Server.Packet.Id;
using Necromancy.Server.Systems.Auction;
using Necromancy.Server.Systems.Item;

namespace Necromancy.Server.Packet.Area
{
    public class SendAuctionDeregistSearchEquipmentCond : ClientHandler
    {
        public SendAuctionDeregistSearchEquipmentCond(NecServer server) : base(server) { }
        public override ushort id => (ushort)AreaPacketId.send_auction_deregist_search_equipment_cond;
        public override void Handle(NecClient client, NecPacket packet)
        {
            byte index = packet.data.ReadByte(); //index to delete

            AuctionService auctionService = new AuctionService();
            int auctionError = 0;
            try
            {
                auctionService.DeregistSearchEquipmentCond(client, index);
            }
            catch (AuctionException e)
            {
                auctionError = (int)e.type;
            }

            IBuffer res = BufferProvider.Provide();
            res.WriteInt32(auctionError);
            router.Send(client, (ushort)AreaPacketId.recv_auction_regist_search_equipment_cond_r, res, ServerType.Area);
        }
    }
}
