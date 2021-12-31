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
        private const string SQL_SELECT_S_CONDS = @"
            SELECT
                *
            FROM
                nec_auction_s_conds
            WHERE
                character_id = @character_id
            AND
                is_item_search = @is_item_search"; //TODO change to soulid

        private const string SQL_INSERT_S_CONDS = @"
            INSERT INTO
                nec_auction_s_conds
                (
                    is_item_search,
	                character_id,
	                s_index,
	                search_text,
	                level_min,
	                level_max,
	                grade_min,
	                grade_max,
	                qualities,
	                class_index,
	                race_index,
	                gold_cost,
	                is_less_than_gold_cost,
	                has_gem_slot,
	                gem_slot_1,
	                gem_slot_2,
	                gem_slot_3,
	                type_search_mask_0,
                    type_search_mask_1,
	                description,                    
                    unknown_byte_0,
                    unknown_byte_1
                )
            VALUES
                (
                    @is_item_search,
                    @character_id,
	                @s_index,
	                @search_text,
	                @level_min,
	                @level_max,
	                @grade_min,
	                @grade_max,
	                @qualities,
	                @class_index,
	                @race_index,
	                @gold_cost,
	                @is_less_than_gold_cost,
	                @has_gem_slot,
	                @gem_slot_1,
	                @gem_slot_2,
	                @gem_slot_3,
	                @type_search_mask_0,
                    @type_search_mask_1,
	                @description,                    
                    @unknown_byte_0,
                    @unknown_byte_1
                )";

        private const string SQL_DELETE_S_CONDS = @"
                DELETE FROM
                    nec_auction_s_conds
                WHERE
                    character_id = @character_id
                AND
                    is_item_search = @is_item_search
                AND
                    s_index = @s_index";

        private const string SQL_UPDATE_S_CONDS_INDEX = @"
                UPDATE
                    nec_auctions_s_conds
                SET
                    s_index = s_index - 1
                WHERE
                    character_id = @character_id
                AND
                    is_item_search = @is_item_search
                AND
                    s_index > @s_index";

        private const string SQL_UPDATE_EXHIBIT = @"
            UPDATE
                nec_item_instance
            SET
                consigner_soul_name = @consigner_soul_name,
                expiry_datetime = @expiry_datetime,
                min_bid = @min_bid,
                buyout_price = @buyout_price,
                comment = @comment
            WHERE
                id = @id";

        private const string SQL_UPDATE_CANCEL_EXHIBIT = @"
            UPDATE
                nec_item_instance
            SET
                consigner_soul_name = @consigner_soul_name,
                expiry_datetime = @expiry_datetime,
                min_bid = @min_bid,
                buyout_price = @buyout_price,
                comment = @comment
            WHERE
                id = @id";

        private const string SQL_INSERT_BID = @"
            INSERT INTO
                nec_auction_bids
                (
                    item_instance_id,
	                bidder_soul_id,
	                bid
                )
            VALUES
                (
                    @item_instance_id,
	                @bidder_soul_id,
	                @bid
                )";


        private const string SQL_SELECT_BUYOUT_PRICE = @"
            SELECT
                buyout_price
            FROM
                item_instance
            WHERE
                id = @id";

        private const string SQL_UPDATE_WINNER_SOUL_ID = @"
            UPDATE
                nec_item_instance
            SET
                winner_soul_id = @winner_soul_id
            WHERE
                id = @id";

        private const string SQL_SELECT_WINNER_SOUL_ID = @"
            SELECT
                winner_soul_id
            FROM
                item_instance
            WHERE id = @id
            LIMIT 1";

        public AuctionDao() { }

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

        public List<AuctionSearchConditions> SelectSearchConditions(int characterId, bool isItemSearchCond)
        {
            List<AuctionSearchConditions> equipSearch = new List<AuctionSearchConditions>();
            ExecuteReader(SQL_SELECT_S_CONDS,
                command =>
                {
                    AddParameter(command, "@is_item_search", isItemSearchCond);
                    AddParameter(command, "@character_id", characterId);
                }, reader =>
                {
                    while (reader.Read())
                    {
                        AuctionSearchConditions equipConds = MakeAuctionSearchConditions(reader);
                        equipSearch.Add(equipConds);
                    }
                });
            return equipSearch;
        }

        public void InsertSearchConditions(int characterId, int index, AuctionSearchConditions searchCond)
        {
            int rowsAffected = ExecuteNonQuery(SQL_INSERT_S_CONDS, command =>
            {
                AddParameter(command, "@is_item_search", searchCond.isItemSearch);
                AddParameter(command, "@character_id", characterId);
                AddParameter(command, "@s_index", index);
                AddParameter(command, "@search_text", searchCond.searchText);
                AddParameter(command, "@level_min", searchCond.levelMin);
                AddParameter(command, "@level_max", searchCond.levelMax);
                AddParameter(command, "@grade_min", searchCond.gradeMin);
                AddParameter(command, "@grade_max", searchCond.gradeMax);
                AddParameter(command, "@qualities", (int) searchCond.qualities);
                AddParameter(command, "@class_index", searchCond.classIndex);
                AddParameter(command, "@race_index", searchCond.raceIndex);
                AddParameter(command, "@gold_cost", searchCond.goldCost);
                AddParameter(command, "@is_less_than_gold_cost", searchCond.isLessThanGoldCost);
                AddParameter(command, "@has_gem_slot", searchCond.hasGemSlot);
                AddParameter(command, "@gem_slot_1", (int) searchCond.gemSlotType1);
                AddParameter(command, "@gem_slot_2", (int) searchCond.gemSlotType2);
                AddParameter(command, "@gem_slot_3", (int) searchCond.gemSlotType3);
                AddParameter(command, "@type_search_mask_0", searchCond.typeSearchMask0);
                AddParameter(command, "@type_search_mask_1", searchCond.typeSearchMask1);
                AddParameter(command, "@description", searchCond.description);                
                AddParameter(command, "@unknown_byte_0", searchCond.unknownByte0);
                AddParameter(command, "@unknown_byte_1", searchCond.unknownByte1);
            });
        }

        private AuctionSearchConditions MakeAuctionSearchConditions(DbDataReader reader)
        {
            AuctionSearchConditions equipConds = new AuctionSearchConditions();
            equipConds.isItemSearch         = reader.GetBoolean("is_item_search");
            equipConds.searchText           = reader.GetString("search_text");
            equipConds.levelMin             = reader.GetByte("level_min");
            equipConds.levelMax             = reader.GetByte("level_max");
            equipConds.gradeMin             = reader.GetByte("grade_min");
            equipConds.gradeMax             = reader.GetByte("grade_max");
            equipConds.qualities            = reader.GetInt16("qualities");
            equipConds.classIndex           = reader.GetInt32("class_index");
            equipConds.raceIndex            = reader.GetInt16("race_index");
            equipConds.goldCost             = (ulong) reader.GetInt64("gold_cost");
            equipConds.isLessThanGoldCost   = reader.GetByte("is_less_than_gold_cost");
            equipConds.hasGemSlot           = reader.GetByte("has_gem_slot");
            equipConds.gemSlotType1         = reader.GetByte("gem_slot_1");
            equipConds.gemSlotType2         = reader.GetByte("gem_slot_2");
            equipConds.gemSlotType3         = reader.GetByte("gem_slot_3");
            equipConds.typeSearchMask0      = reader.GetInt64("type_search_mask_0");
            equipConds.typeSearchMask1      = reader.GetInt64("type_search_mask_1");
            equipConds.description          = reader.GetString("description");
            equipConds.unknownByte0         = reader.GetByte("unknown_byte_0");
            equipConds.unknownByte1         = reader.GetByte("unknown_byte_1");
            return equipConds;
        }

        public void DeleteSearchConditions(int characterId, byte index, bool isItemSearchCond)
        {
            ExecuteNonQuery(SQL_DELETE_S_CONDS, command =>
            {
                AddParameter(command, "@character_id", characterId);
                AddParameter(command, "@is_item_search", isItemSearchCond);
                AddParameter(command, "@s_index", index);
            });

            ExecuteNonQuery(SQL_UPDATE_S_CONDS_INDEX, command =>
            {
                AddParameter(command, "@character_id", characterId);
                AddParameter(command, "@is_item_search", isItemSearchCond);
                AddParameter(command, "@s_index", index);
            });
        }

        public void UpdateExhibit(ItemInstance itemInstance)
        {
            ExecuteNonQuery(SQL_UPDATE_EXHIBIT, command =>
            {
                AddParameter(command, "@id", itemInstance.instanceId);
                AddParameter(command, "@consigner_soul_name", itemInstance.consignerSoulName);
                AddParameter(command, "@expiry_datetime", CalcExpiryTime(itemInstance.secondsUntilExpiryTime));
                AddParameter(command, "@min_bid", itemInstance.minimumBid);
                AddParameter(command, "@buyout_price", itemInstance.buyoutPrice);
                AddParameter(command, "@comment", itemInstance.comment);
            });
        }

        public void UpdateCancelExhibit(ItemInstance itemInstance)
        {
            ExecuteNonQuery(SQL_UPDATE_CANCEL_EXHIBIT, command =>
            {
                AddParameter(command, "@id", itemInstance.instanceId);
                AddParameterNull(command, "@consigner_soul_name");
                AddParameterNull(command, "@expiry_datetime");
                AddParameterNull(command, "@min_bid");
                AddParameterNull(command, "@buyout_price");
                AddParameterNull(command, "@comment");
                AddParameterNull(command, "@bidder_soul_id");
            });
        }

        public void InsertBid(ulong instanceId, int bidderSoulId, ulong bid)
        {
            ExecuteNonQuery(SQL_INSERT_BID, command =>
            {
                AddParameter(command, "@item_instance_id", instanceId);
                AddParameter(command, "@bidder_soul_id", bidderSoulId);
                AddParameter(command, "@bid", bid);
            });
        }

        public ulong SelectBuyoutPrice(ulong instanceId)
        {
            ulong buyoutPrice = 0;
            ExecuteReader(SQL_SELECT_BUYOUT_PRICE,
                command => { AddParameter(command, "@id", instanceId); }, reader =>
                {
                    reader.Read();
                    buyoutPrice = reader.IsDBNull("buyout_price") ? 0 : (ulong)reader.GetInt64("buyout_price"); //TODO remove cast
                });
            return buyoutPrice;
        }

        public int SelectWinnerSoulId(ulong instanceId)
        {
            int soulId = 0;
            ExecuteReader(SQL_SELECT_WINNER_SOUL_ID,
                command =>
                {
                    AddParameter(command, "@id", instanceId);
                }, reader =>
                {
                    soulId = reader.GetInt32("winner_soul_id");
                });

            return soulId;
        }

        public void UpdateWinnerSoulId(ulong instanceId, int winnerSoulId)
        {
            ExecuteNonQuery(SQL_UPDATE_WINNER_SOUL_ID, command =>
            {
                AddParameter(command, "@winner_soul_id", winnerSoulId);
                AddParameter(command, "@id", instanceId);
            });
        }
    }
}
