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

        public override ushort id => (ushort)AreaPacketId.send_auction_regist_search_equipment_cond;

        public override void Handle(NecClient client, NecPacket packet)
        {
            AuctionEquipmentSearchConditions equipCond = new AuctionEquipmentSearchConditions();
            
            byte index = packet.data.ReadByte();

            equipCond.text                  = packet.data.ReadFixedString(AuctionEquipmentSearchConditions.MAX_TEXT_LENGTH);
            equipCond.forgePriceMin         = packet.data.ReadByte();
            equipCond.forgePriceMax         = packet.data.ReadByte();
            equipCond.soulRankMin           = packet.data.ReadByte();
            equipCond.soulRankMax           = packet.data.ReadByte();
            equipCond.classIndex            = packet.data.ReadInt32();
            equipCond.raceIndex             = packet.data.ReadInt16();
            equipCond.qualities             = (ItemQualities) packet.data.ReadInt16(); 
            equipCond.goldCost              = packet.data.ReadUInt64(); 
            equipCond.isLessThanGoldCost    = Convert.ToBoolean(packet.data.ReadByte());

            equipCond.hasGemSlot            = Convert.ToBoolean(packet.data.ReadByte()); 
            equipCond.gemSlotType1          = (GemType) packet.data.ReadByte(); 
            equipCond.gemSlotType2          = (GemType) packet.data.ReadByte();
            equipCond.gemSlotType3          = (GemType) packet.data.ReadByte();

            equipCond.itemTypeSearchMask    = packet.data.ReadUInt64();
            equipCond.unused0               = packet.data.ReadUInt64();
            equipCond.description           = packet.data.ReadFixedString(AuctionEquipmentSearchConditions.MAX_DESCRIPTION_LENGTH);
            byte option9 = packet.data.ReadByte();
            byte option10 = packet.data.ReadByte();
            //TODO missing some stuff here

            AuctionService auctionService = new AuctionService();
            int auctionError = 0;
            try
            {
                auctionService.RegistSearchEquipmentCond(client, equipCond);
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
