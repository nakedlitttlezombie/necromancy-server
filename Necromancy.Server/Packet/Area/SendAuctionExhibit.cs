using System.Collections.Generic;
using Arrowgene.Buffers;
using Arrowgene.Logging;
using Necromancy.Server.Common;
using Necromancy.Server.Logging;
using Necromancy.Server.Model;
using Necromancy.Server.Packet.Id;
using Necromancy.Server.Systems.Auction;
using Necromancy.Server.Systems.Item;
using static Necromancy.Server.Systems.Item.ItemService;

namespace Necromancy.Server.Packet.Area
{
    public class SendAuctionExhibit : ClientHandler
    {

        private static readonly NecLogger _Logger = LogProvider.Logger<NecLogger>(typeof(ItemService));
        public SendAuctionExhibit(NecServer server) : base(server) { }
        public override ushort id => (ushort) AreaPacketId.send_auction_exhibit;
        public override void Handle(NecClient client, NecPacket packet)
        {
            byte exhibitSlot    = packet.data.ReadByte();
            ItemZoneType zone   = (ItemZoneType) packet.data.ReadByte();
            byte bag            = packet.data.ReadByte();
            short slot          = packet.data.ReadInt16();
            byte quantity       = packet.data.ReadByte();
            int time            = packet.data.ReadInt32(); //0:4hours 1:8 hours 2:12 hours 3:24 hours
            ulong minBid        = packet.data.ReadUInt64();
            ulong buyoutPrice   = packet.data.ReadUInt64();
            string comment      = packet.data.ReadCString();

            ItemLocation fromLoc = new ItemLocation(zone, bag, slot);
            ItemLocation exhibitLocation = new ItemLocation(ItemZoneType.ProbablyAuctionLots, 0, exhibitSlot);
            AuctionService auctionService = new AuctionService(client.character);
            ItemService itemService = new ItemService(client.character);
            int auctionError = 0;
            try
            {
                auctionService.ValidateExhibit(fromLoc, exhibitLocation, quantity, time, minBid, buyoutPrice);
                auctionService.Exhibit(fromLoc, exhibitLocation, quantity, time, minBid, buyoutPrice, comment);
                MoveResult moveResult = itemService.Move(fromLoc, exhibitLocation, quantity);
                List<PacketResponse> responses = itemService.GetMoveResponses(client, moveResult);
                router.Send(client, responses);
            }
            catch (AuctionException e)
            {
                auctionError = (int) e.type;
                _Logger.Exception(client, e);
            }

            IBuffer res = BufferProvider.Provide();
            res.WriteInt32(auctionError); //error check.
            res.WriteInt32(0); //unknown
            res.WriteUInt64(0); //unknown
            router.Send(client.map, (ushort)AreaPacketId.recv_auction_exhibit_r, res, ServerType.Area);
        }
    }
}
