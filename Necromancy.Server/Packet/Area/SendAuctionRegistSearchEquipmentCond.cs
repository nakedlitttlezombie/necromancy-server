using System;
using Arrowgene.Buffers;
using Necromancy.Server.Common;
using Necromancy.Server.Model;
using Necromancy.Server.Packet.Id;
using Necromancy.Server.Systems.Auction;
using Necromancy.Server.Systems.Item;

namespace Necromancy.Server.Packet.Area
{
    public class SendAuctionRegistSearchEquipmentCond : ClientHandler
    {
        public SendAuctionRegistSearchEquipmentCond(NecServer server) : base(server) { }
        public override ushort id => (ushort) AreaPacketId.send_auction_regist_search_equipment_cond;
        public override void Handle(NecClient client, NecPacket packet)
        {
            AuctionSearchConditions searchCond = new AuctionSearchConditions();
            searchCond.isItemSearch = false;
            
            byte index = packet.data.ReadByte();

            searchCond.searchText           = packet.data.ReadFixedString(AuctionSearchConditions.MAX_SEARCH_TEXT_LENGTH);

            searchCond.gradeMin             = packet.data.ReadByte();
            searchCond.gradeMax             = packet.data.ReadByte();

            searchCond.levelMin             = packet.data.ReadByte();
            searchCond.levelMax             = packet.data.ReadByte();

            searchCond.classIndex           = packet.data.ReadInt32();
            searchCond.raceIndex            = packet.data.ReadInt16();
            searchCond.qualities            = packet.data.ReadInt16();

            searchCond.goldCost             = packet.data.ReadUInt64(); 
            searchCond.isLessThanGoldCost   = packet.data.ReadByte();

            searchCond.hasGemSlot           = packet.data.ReadByte(); 
            searchCond.gemSlotType1         = packet.data.ReadByte(); 
            searchCond.gemSlotType2         = packet.data.ReadByte();
            searchCond.gemSlotType3         = packet.data.ReadByte();

            searchCond.typeSearchMask0      = packet.data.ReadInt64();
            searchCond.typeSearchMask1      = packet.data.ReadInt64();

            searchCond.description          = packet.data.ReadFixedString(AuctionSearchConditions.MAX_DESCRIPTION_LENGTH);

            //who knows
            searchCond.unknownByte0         = packet.data.ReadByte();
            searchCond.unknownByte1         = packet.data.ReadByte(); 
            
            //TODO missing some stuff here

            AuctionService auctionService = new AuctionService(client.character);
            int auctionError = 0;
            try
            {
                auctionService.RegistSearchCond(index, searchCond);
            }
            catch (AuctionException e)
            {
                auctionError = (int) e.type;
            }

            IBuffer res = BufferProvider.Provide();
            res.WriteInt32(auctionError);
            router.Send(client, (ushort) AreaPacketId.recv_auction_regist_search_equipment_cond_r, res, ServerType.Area);
        }
    }
}
