using System;
using System.Collections.Generic;
using Arrowgene.Buffers;
using Arrowgene.Logging;
using Necromancy.Server.Common;
using Necromancy.Server.Logging;
using Necromancy.Server.Model;
using Necromancy.Server.Packet.Id;
using Necromancy.Server.Packet.Receive.Area;
using Necromancy.Server.Systems.Item;

namespace Necromancy.Server.Chat.Command.Commands
{
    //opens auction house
    public class SendAuctionNotifyOpen : ServerChatCommand
    {
        private static readonly NecLogger _Logger = LogProvider.Logger<NecLogger>(typeof(SendAuctionNotifyOpen));

        public SendAuctionNotifyOpen(NecServer server) : base(server)
        {
        }

        public override AccountStateType accountState => AccountStateType.Admin;
        public override string key => "auct";

        public override void Execute(string[] command, NecClient client, ChatMessage message,
            List<ChatResponse> responses)
        {
            ItemService itemService = new ItemService(client.character);
            List<ItemInstance> lots = itemService.GetLots();
            List<ItemInstance> bids = itemService.GetBids();
            List<AuctionEquipmentSearchConditions> equipSearch = itemService.GetEquipmentSearchConditions();
            List<AuctionItemSearchConditions> itemSearch = itemService.GetItemSearchConditions();
            const byte IS_IN_MAINTENANCE_MODE = 0x0;
            const int MAX_LOTS = 15;

            IBuffer res = BufferProvider.Provide();

            foreach (ItemInstance lotItem in lots)
            {
                RecvItemInstance recvItemInstance = new RecvItemInstance(client, lotItem);
                router.Send(recvItemInstance);
            }

            int j = 0;
            res.WriteInt32(lots.Count); //Less than or equal to 15
            foreach (ItemInstance lotItem in lots)
            {
                res.WriteByte((byte)j); // row number?
                res.WriteInt32(j); // row number ??
                res.WriteUInt64(lotItem.instanceId);
                res.WriteUInt64(lotItem.minimumBid);
                res.WriteUInt64(lotItem.buyoutPrice);
                res.WriteFixedString(lotItem.consignerSoulName, 49);
                res.WriteByte(0); // criminal status of seller?
                res.WriteFixedString(lotItem.comment, 385);
                res.WriteInt16((short)lotItem.currentBid); // Bid why convert to short?
                res.WriteInt32(lotItem.secondsUntilExpiryTime);

                res.WriteInt64(0); //unknown
                res.WriteInt32(0); //unknown
                res.WriteInt32(0); //unknown
                j++;
            }

            foreach (ItemInstance bidItem in bids)
            {
                RecvItemInstance recvItemInstance = new RecvItemInstance(client, bidItem);
                router.Send(recvItemInstance);
            }

            j = 0;
            res.WriteInt32(bids.Count); //Less than or equal to 0xE
            foreach (ItemInstance bidItem in bids)
            {
                res.WriteByte((byte)j); // row number?
                res.WriteInt32(j); // row number ??
                res.WriteUInt64(bidItem.instanceId);
                res.WriteUInt64(bidItem.minimumBid);
                res.WriteUInt64(bidItem.buyoutPrice);
                res.WriteFixedString(bidItem.consignerSoulName, 49);
                res.WriteByte(0); // criminal status of seller?
                res.WriteFixedString(bidItem.comment, 385);
                res.WriteInt16((short)bidItem.maxBid); // The current bid, why convert to short?
                res.WriteInt32(bidItem.secondsUntilExpiryTime);

                res.WriteInt64(bidItem.currentBid); //Your current bid
                res.WriteInt32(0); //0: you are the highest bidder, 1: you won the item, 2: you were outbid, 3: seller cancelled
                j++;
            }

            res.WriteInt32(equipSearch.Count); //Less than or equal to 0x8
            foreach (AuctionEquipmentSearchConditions equipCond in equipSearch)
            {
                res.WriteFixedString(equipCond.text, AuctionEquipmentSearchConditions.MAX_TEXT_LENGTH); //V| Search Text
                res.WriteByte(equipCond.forgePriceMin);         //V| Grade min
                res.WriteByte(equipCond.forgePriceMax);         //V| Grade max
                res.WriteByte(equipCond.soulRankMin);           //V| Level min
                res.WriteByte(equipCond.soulRankMax);           //V| Level max
                res.WriteInt32(equipCond.classIndex);           //V| Index for Class 
                res.WriteInt16(equipCond.raceIndex);            //V| Index for Race
                res.WriteInt16((short)equipCond.qualities);     //V| Qualities
                res.WriteUInt64(equipCond.goldCost);            //V| Gold
                res.WriteByte(Convert.ToByte(equipCond.isLessThanGoldCost));

                res.WriteByte(Convert.ToByte(equipCond.hasGemSlot));    //V| Effectiveness
                res.WriteByte((byte)equipCond.gemSlotType1);            //V| Gem slot 1
                res.WriteByte((byte)equipCond.gemSlotType2);            //V| Gem slot 2
                res.WriteByte((byte)equipCond.gemSlotType3);            //V| Gem slot 3

                res.WriteUInt64(equipCond.itemTypeSearchMask); //V| Item type mask
                res.WriteUInt64(equipCond.unused0);
                res.WriteFixedString(equipCond.description, AuctionEquipmentSearchConditions.MAX_DESCRIPTION_LENGTH); //v| Saved Search Description
                res.WriteByte(0); //TODO UNKNOWN
                res.WriteByte(0); //TODO UNKNOWN
            }


            //item search conditions
            int numEntries = 1;
            res.WriteInt32(numEntries); //Less than or equal to 0x8

            for (int i = 0; i < numEntries; i++)
            {
                res.WriteFixedString("fs0x49V2", 0x49);
                res.WriteByte(0);
                res.WriteByte(0);
                res.WriteByte(0);
                res.WriteByte(0);
                res.WriteInt64(0);
                res.WriteByte(0);
                res.WriteInt64(0);
                res.WriteInt64(0);
                res.WriteFixedString("fs0xC1V2", 0xC1); //Fixed string of 0xC1 or 0xC1 bytes.
                res.WriteByte(0);
                res.WriteByte(0);
            }

            res.WriteByte(0); //Bool
            res.WriteInt32(0);
            router.Send(client, (ushort)AreaPacketId.recv_auction_notify_open, res, ServerType.Area);

            RecvAuctionNotifyOpenItemStart recvAuctionNotifyOpenItemStart = new RecvAuctionNotifyOpenItemStart(client);
            RecvAuctionNotifyOpenItemEnd recvAuctionNotifyOpenItemEnd = new RecvAuctionNotifyOpenItemEnd(client);

            List<ItemInstance> auctionList = itemService.GetItemsUpForAuction();

            j = 0;
            client.character.auctionSearchIds = new ulong[auctionList.Count];
            foreach (ItemInstance auctionItem in auctionList)
            {
                client.character.auctionSearchIds[j] = auctionItem.instanceId;
                RecvItemInstance recvItemInstance = new RecvItemInstance(client, auctionItem);
                router.Send(recvItemInstance);
                j++;
            }

            router.Send(recvAuctionNotifyOpenItemStart);
            int divideBy100 = auctionList.Count / 100 + (auctionList.Count % 100 == 0 ? 0 : 1); // TOTAL NUMBER OF RECVS TO SEND
            for (int i = 0; i < divideBy100; i++)
            {
                RecvAuctionNotifyOpenItem recvAuctionNotifyOpenItem;
                if (i == divideBy100 - 1)
                    recvAuctionNotifyOpenItem = new RecvAuctionNotifyOpenItem(client, auctionList.GetRange(i, auctionList.Count % 100));
                else
                    recvAuctionNotifyOpenItem = new RecvAuctionNotifyOpenItem(client, auctionList.GetRange(i, 100));
                router.Send(recvAuctionNotifyOpenItem);
            }

            router.Send(recvAuctionNotifyOpenItemEnd);
        }
    }
}
