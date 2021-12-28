using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using Necromancy.Server.Model;
using Necromancy.Server.Systems.Item;

namespace Necromancy.Server.Systems.Auction
{
    public class AuctionDao : DatabaseAccessObject, IAuctionDao
    {
        private const string SQL_CREATE_ITEMS_UP_FOR_AUCTION_VIEW = @"
            DROP VIEW IF EXISTS items_up_for_auction;
            CREATE VIEW IF NOT EXISTS items_up_for_auction
	            (
		            id,
                    consigner_id,
		            consigner_name,
		            instance_id,
		            quantity,
		            expiry_datetime,
		            min_bid,
		            buyout_price,
		            current_bid,
		            bidder_id,
		            comment,
                    is_cancellable
	            )
            AS
            SELECT
                nec_auction.id,
                consigner.id,
                consigner.name,
                nec_auction.instance_id,
                nec_auction.quantity,
                nec_auction.expiry_datetime,
                nec_auction.min_bid,
                nec_auction.buyout_price,
                nec_auction.current_bid,
                nec_auction.bidder_id,
                nec_auction.comment,
                nec_auction.is_cancellable
            FROM
                nec_auction
			INNER JOIN
				nec_item_instance item_instance
			ON
				nec_auction.instance_id = item_instance.id
            INNER JOIN
                nec_character consigner
            ON
                item_instance.owner_id = consigner.id";

        private const string SQL_INSERT_LOT = @"
            INSERT INTO
                nec_auction
                (
                    slot
                    instance_id,
                    quantity,
                    expiry_datetime,
                    min_bid,
                    buyout_price,
                    comment
                )
            VALUES
                (
                    @slot
                    @instance_id,
                    @quantity,
                    @expiry_datetime,
                    @min_bid,
                    @buyout_price,
                    @comment
                )";

        private const string SQL_UPDATE_BID = @"
            UPDATE
                nec_auction_item
            SET
                bidder_id = @bidder_id,
                current_bid = @current_bid,
                is_cancellable = (bidder_id IS NULL)
            WHERE
                id = @id";

        private const string SQL_SELECT_BIDS = @"
            SELECT
                *
            FROM
                items_up_for_auction
            WHERE
                bidder_id = @character_id";

        private const string SQL_SELECT_LOTS = @"
            SELECT
                *
            FROM
                items_up_for_auction
            WHERE
                consigner_id = @character_id";

        private const string SQL_SELECT_ITEM = @"
            SELECT
                *
            FROM
                items_up_for_auction
            WHERE
                id = @id";
        private const string SQL_SELECT_ES_CONDS = @"
            SELECT
                *
            FROM
                nec_auction_es_conds
            WHERE
                character_id = @character_id";

        private const string SQL_INSERT_ES_CONDS = @"
            INSERT INTO
                nec_auction_es_conds
                (
	                character_id,
	                es_index,
	                search_text,
	                soul_rank_min,
	                soul_rank_max,
	                forge_price_min,
	                forge_price_max,
	                qualities,
	                class_index,
	                race_index,
	                gold_cost,
	                is_less_than_gold_cost,
	                has_gem_slot,
	                gem_slot_1,
	                gem_slot_2,
	                gem_slot_3,
	                item_type_search_mask,
	                description,
                    unknown_long_0,
                    unknown_byte_0,
                    unknown_byte_1
                )
            VALUES
                (
                    @character_id,
	                @es_index,
	                @search_text,
	                @soul_rank_min,
	                @soul_rank_max,
	                @forge_price_min,
	                @forge_price_max,
	                @qualities,
	                @class_index,
	                @race_index,
	                @gold_cost,
	                @is_less_than_gold_cost,
	                @has_gem_slot,
	                @gem_slot_1,
	                @gem_slot_2,
	                @gem_slot_3,
	                @item_type_search_mask,
	                @description,
                    @unknown_long_0,
                    @unknown_byte_0,
                    @unknown_byte_1
                )";

        private const string SQL_DELETE_ES_CONDS = @"
                DELETE FROM
                    nec_auction_es_conds
                WHERE
                    character_id = @character_id
                AND
                    es_index = @es_index";

        private const string SQL_UPDATE_ES_CONDS_INDEX = @"
                UPDATE
                    nec_auction_es_conds
                SET
                    es_index = es_index - 1
                WHERE
                    character_id = @character_id
                AND
                    es_index > @es_index";


        public AuctionDao()
        {
            CreateView();
        }

        private void CreateView()
        {
            ExecuteNonQuery(SQL_CREATE_ITEMS_UP_FOR_AUCTION_VIEW, command => { });
        }

        //public bool InsertLot(ItemInstance auctionLot)
        //{
        //      int rowsAffected = ExecuteNonQuery(SqlInsertLot, command =>
        //        {
        //            AddParameter(command, "@slot", auctionLot.Slot);
        //            AddParameter(command, "@instance_id", auctionLot.ItemInstanceId);
        //            AddParameter(command, "@quantity", auctionLot.Quantity);
        //            AddParameter(command, "@expiry_datetime", CalcExpiryTime(auctionLot.SecondsUntilExpiryTime));
        //            AddParameter(command, "@min_bid", auctionLot.MinimumBid);
        //            AddParameter(command, "@buyout_price", auctionLot.BuyoutPrice);
        //            AddParameter(command, "@comment", auctionLot.Comment);
        //        });
        //    return rowsAffected > 0;
        //}

        //public AuctionLot SelectItem(int auctionItemId)
        //{
        //    AuctionLot auctionItem = new AuctionLot();
        //    ExecuteReader(SqlSelectItem,
        //        command =>
        //        {
        //            AddParameter(command, "@id", auctionItemId);
        //        }, reader =>
        //        {
        //            MakeAuctionLot(reader);
        //        });
        //    return auctionItem;
        //}

        //public bool UpdateBid(AuctionLot auctionItem)
        //{
        //    int rowsAffected = ExecuteNonQuery(SqlUpdateBid, command =>
        //    {
        //        AddParameter(command, "@bidder_id", auctionItem.BidderId);
        //        AddParameter(command, "@current_bid", auctionItem.CurrentBid);
        //    });
        //    return rowsAffected > 0;
        //}

        //public AuctionLot[] SelectBids(Character character)
        //{
        //    AuctionLot[] bids = new AuctionLot[AuctionService.MAX_BIDS];
        //    int i = 0;
        //    ExecuteReader(SqlSelectBids,
        //        command =>
        //        {
        //            AddParameter(command, "@character_id", character.Id);
        //        }, reader =>
        //        {
        //            while (reader.Read())
        //            {
        //                if (i >= AuctionService.MAX_BIDS) break;
        //                AuctionLot bid = MakeAuctionLot(reader);
        //                bids[i] = bid;
        //                i++;
        //            }
        //        });
        //    AuctionLot[] truncatedBids = new AuctionLot[i];
        //    Array.Copy(bids, truncatedBids, i);
        //    return truncatedBids;
        //}

        //public AuctionLot[] SelectLots(Character character)
        //{
        //    AuctionLot[] lots = new AuctionLot[AuctionService.MAX_LOTS];
        //    int i = 0;
        //    ExecuteReader(SqlSelectLots,
        //        command =>
        //        {
        //            AddParameter(command, "@character_id", character.Id);
        //        }, reader =>
        //        {
        //            while (reader.Read())
        //            {
        //                if (i >= AuctionService.MAX_LOTS) break;
        //                AuctionLot lot = MakeAuctionLot(reader);
        //                lots[i] = lot;
        //                i++;
        //            }
        //        });
        //    AuctionLot[] truncatedLots = new AuctionLot[i];
        //    Array.Copy(lots, truncatedLots, i);
        //    return truncatedLots;
        //}

        //private AuctionLot MakeAuctionLot(DbDataReader reader)
        //{
        //    AuctionLot auctionItem = new AuctionLot();
        //    auctionItem.Id = reader.GetInt32("id");
        //    auctionItem.ConsignerId = reader.GetInt32("consigner_id");
        //    auctionItem.ConsignerName = reader.GetString("consigner_name");
        //    auctionItem.ItemInstanceId = (ulong) reader.GetInt64("spawn_id");
        //    auctionItem.Quantity = reader.GetInt32("quantity");
        //    auctionItem.SecondsUntilExpiryTime = CalcSecondsToExpiry(reader.GetInt64("expiry_datetime"));
        //    auctionItem.MinimumBid = (ulong) reader.GetInt64("min_bid");
        //    auctionItem.BuyoutPrice = (ulong) reader.GetInt64("buyout_price");
        //    auctionItem.CurrentBid = reader.GetInt32("current_bid");
        //    auctionItem.BidderId = reader.GetInt32("bidder_id");
        //    auctionItem.Comment = reader.GetString("comment");
        //    return auctionItem;
        //}

        private int CalcSecondsToExpiry(long unixTimeSecondsExpiry)
        {
            DateTime dNow = DateTime.Now;
            DateTimeOffset dOffsetNow = new DateTimeOffset(dNow);
            return (int)(unixTimeSecondsExpiry - dOffsetNow.ToUnixTimeSeconds());
        }

        private long CalcExpiryTime(int secondsToExpiry)
        {
            DateTime dNow = DateTime.Now;
            DateTimeOffset dOffsetNow = new DateTimeOffset(dNow);
            return dOffsetNow.ToUnixTimeSeconds() + secondsToExpiry;
        }

        public int SelectGold(Character character)
        {
            throw new NotImplementedException();
        }

        public void AddGold(Character character, int amount)
        {
            throw new NotImplementedException();
        }

        public void SubtractGold(Character character, int amount)
        {
            throw new NotImplementedException();
        }

        public List<AuctionEquipmentSearchConditions> SelectAuctionEquipSearchConditions(int characterId)
        {
            List<AuctionEquipmentSearchConditions> equipSearch = new List<AuctionEquipmentSearchConditions>();
            int i = 0;
            ExecuteReader(SQL_SELECT_ES_CONDS,
                command => { AddParameter(command, "@character_id", characterId); }, reader =>
                {
                    while (reader.Read())
                    {
                        AuctionEquipmentSearchConditions equipConds = MakeAuctionEquipmentSearchConditions(reader);
                        equipSearch.Add(equipConds);
                    }
                });
            return equipSearch;
        }

        public void InsertAuctionEquipSearchConditions(int characterId, int index, AuctionEquipmentSearchConditions equipCond)
        {
            int rowsAffected = ExecuteNonQuery(SQL_INSERT_ES_CONDS, command =>
            {
                AddParameter(command, "@character_id", characterId);
                AddParameter(command, "@es_index", index);
                AddParameter(command, "@search_text", equipCond.searchText);
                AddParameter(command, "@soul_rank_min", equipCond.soulRankMin);
                AddParameter(command, "@soul_rank_max", equipCond.soulRankMax);
                AddParameter(command, "@forge_price_min", equipCond.forgePriceMin);
                AddParameter(command, "@forge_price_max", equipCond.forgePriceMax);
                AddParameter(command, "@qualities", (int) equipCond.qualities);
                AddParameter(command, "@class_index", equipCond.classIndex);
                AddParameter(command, "@race_index", equipCond.raceIndex);
                AddParameter(command, "@gold_cost", equipCond.goldCost);
                AddParameter(command, "@is_less_than_gold_cost", equipCond.isLessThanGoldCost);
                AddParameter(command, "@has_gem_slot", equipCond.hasGemSlot);
                AddParameter(command, "@gem_slot_1", (int) equipCond.gemSlotType1);
                AddParameter(command, "@gem_slot_2", (int) equipCond.gemSlotType2);
                AddParameter(command, "@gem_slot_3", (int) equipCond.gemSlotType3);
                AddParameter(command, "@item_type_search_mask", equipCond.itemTypeSearchMask);
                AddParameter(command, "@description", equipCond.description);
                AddParameter(command, "@unknown_long_0", equipCond.unknownLong0);
                AddParameter(command, "@unknown_byte_0", equipCond.unknownByte0);
                AddParameter(command, "@unknown_byte_1", equipCond.unknownByte1);
            });
        }

        public void DeleteAuctionEquipSearchConditions(int characterId, byte index)
        {

            ExecuteNonQuery(SQL_DELETE_ES_CONDS, command =>
            {
                AddParameter(command, "@character_id", characterId);
                AddParameter(command, "@es_index", index);
            });

            ExecuteNonQuery(SQL_UPDATE_ES_CONDS_INDEX, command =>
            {
                AddParameter(command, "@character_id", characterId);
                AddParameter(command, "@es_index", index);
            });
        }

        private AuctionEquipmentSearchConditions MakeAuctionEquipmentSearchConditions(DbDataReader reader)
        {
            AuctionEquipmentSearchConditions equipConds = new AuctionEquipmentSearchConditions();
            equipConds.searchText           = reader.GetString("search_text");
            equipConds.soulRankMin          = reader.GetByte("soul_rank_min");
            equipConds.soulRankMax          = reader.GetByte("soul_rank_max");
            equipConds.forgePriceMin        = reader.GetByte("forge_price_min");
            equipConds.forgePriceMax        = reader.GetByte("forge_price_max");
            equipConds.qualities            = (ItemQualities) reader.GetInt32("qualities");
            equipConds.classIndex           = reader.GetInt32("class_index");
            equipConds.raceIndex            = reader.GetInt16("race_index");
            equipConds.goldCost             = (ulong) reader.GetInt64("gold_cost");
            equipConds.isLessThanGoldCost   = reader.GetBoolean("is_less_than_gold_cost");
            equipConds.hasGemSlot           = reader.GetBoolean("has_gem_slot");
            equipConds.gemSlotType1         = (GemType)reader.GetInt32("gem_slot_1");
            equipConds.gemSlotType2         = (GemType)reader.GetInt32("gem_slot_2");
            equipConds.gemSlotType3         = (GemType)reader.GetInt32("gem_slot_3");
            equipConds.itemTypeSearchMask   = reader.GetInt64("item_type_search_mask");
            equipConds.description          = reader.GetString("description");
            equipConds.unknownLong0         = (ulong) reader.GetInt64("unknown_long_0");
            equipConds.unknownByte0         = reader.GetByte("unknown_byte_0");
            equipConds.unknownByte1         = reader.GetByte("unknown_byte_1");
            return equipConds;
        }
    }
}
