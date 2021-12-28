using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using Necromancy.Server.Data.Setting;

namespace Necromancy.Server.Systems.Item
{
    internal class ItemDao : DatabaseAccessObject, IItemDao
    {
        private const string SQL_SELECT_ITEM_INSTANCE_BY_ID = @"
            SELECT
                *
            FROM
                nec_item_instance
            WHERE
                id = @id";

        private const string SQL_SELECT_ITEM_INSTANCE_BY_LOCATION = @"
            SELECT
                *
            FROM
                nec_item_instance
            WHERE
                character_id = @character_id
            AND
                zone = @zone
            AND
                bag = @bag
            AND
                slot = @slot";

        private const string SQL_SELECT_SPAWNED_ITEM_BY_IDS = @"
            SELECT
                *
            FROM
                nec_item_instance
            WHERE
                id IN ({0})";

        private const string SQL_SELECT_OWNED_INVENTORY_ITEMS = @"
            SELECT
                *
            FROM
                item_instance
            WHERE
                owner_id = @owner_id
            AND
                zone IN (0,1,2,8,12)"; //adventure bag, equipped bags,royal bag, bag slot, avatar inventory

        private const string SQL_SELECT_CLOAKROOM_ITEMS = @"
            SELECT
                item_instance.*, nec_character.id, nec_character.soul_id
            FROM
                item_instance
            INNER JOIN
                nec_character
            ON
                nec_character.id = item_instance.owner_id
            WHERE
               nec_character.soul_id = @soul_id
            AND
                zone = 3"; //Cloakroom

        private const string SQL_SELECT_LOOTABLE_INVENTORY_ITEMS = @"
            SELECT
                *
            FROM
                item_instance
            WHERE
                owner_id = @owner_id
            AND
                zone IN (0,1,2)"; //adventure bag, equipped bags,royal bag

        private const string SQL_UPDATE_ITEM_LOCATION = @"
            UPDATE
                nec_item_instance
            SET
                zone = @zone, container = @container, slot = @slot
            WHERE
                id = @id";

        private const string SQL_UPDATE_ITEM_QUANTITY = @"
            UPDATE
                nec_item_instance
            SET
                quantity = @quantity
            WHERE
                id = @id";

        private const string SQL_UPDATE_ITEM_EQUIP_MASK = @"
            UPDATE
                nec_item_instance
            SET
                current_equip_slot = @current_equip_slot
            WHERE
                id = @id";

        private const string SQL_UPDATE_ITEM_ENHANCEMENT_LEVEL = @"
            UPDATE
                nec_item_instance
            SET
                enhancement_level = @enhancement_level
            WHERE
                id = @id";

        private const string SQL_UPDATE_ITEM_CURRENT_DURABILITY = @"
            UPDATE
                nec_item_instance
            SET
                current_durability = @current_durability
            WHERE
                id = @id";

        private const string SQL_UPDATE_ITEM_OWNER_AND_STATUS = @"
            UPDATE
                nec_item_instance
            SET
                owner_id = @owner_id, statuses = @statuses
            WHERE
                id = @id";

        private const string SQL_DELETE_ITEM_INSTANCE = @"
            DELETE FROM
                nec_item_instance
            WHERE
                id = @id";

        private const string SQL_INSERT_ITEM_INSTANCES = @"
            INSERT INTO
	            nec_item_instance
		        (
			        owner_id,
			        zone,
			        container,
			        slot,
			        base_id,
                    statuses,
                    quantity,
                    gem_slot_1_type,
                    gem_slot_2_type,
                    gem_slot_3_type,
                    gem_id_slot_1,
                    gem_id_slot_2,
                    gem_id_slot_3,
                    plus_maximum_durability,
                    plus_physical,
                    plus_magical,
                    plus_gp,
                    plus_weight,
                    plus_ranged_eff,
                    plus_reservoir_eff
		        )
            VALUES
	            (
                    @owner_id,
                    @zone,
                    @container,
                    @slot,
                    @base_id,
                    @statuses,
                    @quantity,
                    @gem_slot_1_type,
                    @gem_slot_2_type,
                    @gem_slot_3_type,
                    @gem_id_slot_1,
                    @gem_id_slot_2,
                    @gem_id_slot_3,
                    @plus_maximum_durability,
                    @plus_physical,
                    @plus_magical,
                    @plus_gp,
                    @plus_weight,
                    @plus_ranged_eff,
                    @plus_reservoir_eff
                );
            SELECT last_insert_rowid()";

        //TODO move auction stuff to partial namespace for easier reading
        private const string SQL_SELECT_AUCTIONS = @"
            SELECT
                item_instance.*,
				nec_soul.id AS owner_soul_id
            FROM
                item_instance
			JOIN
				nec_soul
			ON
				item_instance.owner_id = nec_soul.id
            WHERE
                zone = 82
            AND
                owner_soul_id != @owner_soul_id";

        private const string SQL_UPDATE_EXHIBIT = @"
            UPDATE
                nec_item_instance
            SET
                consigner_soul_name = @consigner_soul_name, expiry_datetime = @expiry_datetime, min_bid = @min_bid, buyout_price = @buyout_price, comment = @comment
            WHERE
                id = @id";

        private const string SQL_UPDATE_CANCEL_EXHIBIT = @"
            UPDATE
                nec_item_instance
            SET
                consigner_soul_name = @consigner_soul_name, expiry_datetime = @expiry_datetime, min_bid = @min_bid, buyout_price = @buyout_price, comment = @comment
            WHERE
                id = @id";

        private const string SQL_SELECT_BIDS = @"
            SELECT
                item_instance.*,
                bidder_soul_id,
                current_bid,
                (SELECT MAX(current_bid) FROM nec_auction_bids WHERE item_instance_id = id) AS max_bid
            FROM
                item_instance
            JOIN
                nec_auction_bids
            ON
                item_instance.id = nec_auction_bids.item_instance_id
            WHERE
                bidder_soul_id = @bidder_soul_id";

        private const string SQL_SELECT_LOTS = @"
            SELECT
                item_instance.*,
				nec_soul.id AS owner_soul_id
            FROM
                item_instance
			JOIN
				nec_soul
			ON
				item_instance.owner_id = nec_soul.id
            WHERE
                owner_soul_id = @owner_soul_id
            AND
                zone = 82"; //Probably auction lot zone, may be 83

        private const string SQL_SELECT_BUYOUT_PRICE = @"
            SELECT
                buyout_price
            FROM
                item_instance
            WHERE
                id = @id";

        private const string SQL_INSERT_AUCTION_BID = @"
            INSERT INTO
                nec_auction_bids
                (
                    item_instance_id,
                    bidder_soul_id,
                    current_bid
                )
            VALUES
                (
                    @instance_id,
                    @bidder_soul_id,
                    @current_bid
                )";

        private const string SQL_UPDATE_AUCTION_WINNER_SOUL_ID = @"
            UPDATE
                nec_item_instance
            SET
                winner_soul_id = @winner_soul_id
            WHERE
                id = @id";

        private const string SQL_SELECT_AUCTION_WINNER_SOUL_ID = @"
            SELECT
                winner_soul_id
            FROM
                item_instance
            WHERE id = @id";

        public ItemInstance InsertItemInstance(int baseId)
        {
            throw new NotImplementedException();
        }

        public ItemInstance SelectItemInstance(long instanceId)
        {
            ItemInstance itemInstance = null;
            ExecuteReader(SQL_SELECT_ITEM_INSTANCE_BY_ID,
                command => { AddParameter(command, "@id", instanceId); }, reader => { itemInstance = MakeItemInstance(reader); });
            return itemInstance;
        }

        public void DeleteItemInstance(ulong instanceId)
        {
            ExecuteNonQuery(SQL_DELETE_ITEM_INSTANCE,
                command => { AddParameter(command, "@id", instanceId); });
        }

        public void UpdateItemEquipMask(ulong instanceId, ItemEquipSlots equipSlots)
        {
            ExecuteNonQuery(SQL_UPDATE_ITEM_EQUIP_MASK,
                command =>
                {
                    AddParameter(command, "@current_equip_slot", (int)equipSlots);
                    AddParameter(command, "@id", instanceId);
                });
        }

        public void UpdateItemEnhancementLevel(ulong instanceId, int level)
        {
            ExecuteNonQuery(SQL_UPDATE_ITEM_ENHANCEMENT_LEVEL,
                command =>
                {
                    AddParameter(command, "@enhancement_level", level);
                    AddParameter(command, "@id", instanceId);
                });
        }

        public void UpdateItemOwnerAndStatus(ulong instanceId, int ownerId, int statuses)
        {
            ExecuteNonQuery(SQL_UPDATE_ITEM_OWNER_AND_STATUS,
                command =>
                {
                    AddParameter(command, "@statuses", statuses);
                    AddParameter(command, "@owner_id", ownerId);
                    AddParameter(command, "@id", instanceId);
                });
        }

        public void UpdateItemCurrentDurability(ulong instanceId, int currentDurability)
        {
            ExecuteNonQuery(SQL_UPDATE_ITEM_CURRENT_DURABILITY,
                command =>
                {
                    AddParameter(command, "@current_durability", currentDurability);
                    AddParameter(command, "@id", instanceId);
                });
        }

        public ItemInstance SelectItemInstance(int characterId, ItemLocation itemLocation)
        {
            throw new NotImplementedException();
        }

        public void UpdateItemLocation(ulong instanceId, ItemLocation loc)
        {
            ulong[] instanceIds = new ulong[1] { instanceId };
            ItemLocation[] locs = new ItemLocation[1] { loc };
            UpdateItemLocations(instanceIds, locs);
        }

        public void UpdateItemLocations(ulong[] instanceIds, ItemLocation[] locs)
        {
            int size = instanceIds.Length;
            try
            {
                using DbConnection conn = GetSqlConnection();
                conn.Open();
                using DbCommand command = conn.CreateCommand();
                command.CommandText = SQL_UPDATE_ITEM_LOCATION;
                for (int i = 0; i < size; i++)
                {
                    command.Parameters.Clear();
                    AddParameter(command, "@zone", (byte)locs[i].zoneType);
                    AddParameter(command, "@container", locs[i].container);
                    AddParameter(command, "@slot", locs[i].slot);
                    AddParameter(command, "@id", instanceIds[i]);

                    command.ExecuteNonQuery();
                }
            }
            catch (Exception ex)
            {
                Logger.Error($"Query: {SQL_UPDATE_ITEM_LOCATION}");
                Exception(ex);
            }
        }

        public void UpdateItemQuantities(ulong[] instanceIds, byte[] quantities)
        {
            int size = instanceIds.Length;
            try
            {
                using DbConnection conn = GetSqlConnection();
                conn.Open();
                using DbCommand command = conn.CreateCommand();
                command.CommandText = SQL_UPDATE_ITEM_QUANTITY;
                for (int i = 0; i < size; i++)
                {
                    command.Parameters.Clear();
                    AddParameter(command, "@quantity", quantities[i]);
                    AddParameter(command, "@id", instanceIds[i]);
                    command.ExecuteNonQuery();
                }
            }
            catch (Exception ex)
            {
                Logger.Error($"Query: {SQL_UPDATE_ITEM_QUANTITY}");
                Exception(ex);
            }
        }

        /// <summary>
        ///     This selects only the items in the player's inventory: Adventure bag, Equipped bags, Royal bag, Bag Slots, and
        ///     Avatar inventory.
        /// </summary>
        /// <param name="ownerId">Owner of items.</param>
        /// <returns></returns>
        public List<ItemInstance> SelectOwnedInventoryItems(int ownerId)
        {
            List<ItemInstance> ownedItemInstances = new List<ItemInstance>();
            ExecuteReader(SQL_SELECT_OWNED_INVENTORY_ITEMS,
                command => { AddParameter(command, "@owner_id", ownerId); }, reader =>
                {
                    while (reader.Read()) ownedItemInstances.Add(MakeItemInstance(reader));
                });
            return ownedItemInstances;
        }

        public List<ItemInstance> SelectCloakRoomItems(int soulId)
        {
            List<ItemInstance> ownedItemInstances = new List<ItemInstance>();
            ExecuteReader(SQL_SELECT_CLOAKROOM_ITEMS,
                command => { AddParameter(command, "@soul_id", soulId); }, reader =>
                {
                    while (reader.Read()) ownedItemInstances.Add(MakeItemInstance(reader));
                });
            return ownedItemInstances;
        }

        public List<ItemInstance> InsertItemInstances(int ownerId, ItemLocation[] locs, int[] baseId, ItemSpawnParams[] spawnParams)
        {
            int size = locs.Length;
            List<ItemInstance> itemInstances = new List<ItemInstance>(size);
            try
            {
                using DbConnection conn = GetSqlConnection();
                conn.Open();
                using DbCommand command = conn.CreateCommand();
                command.CommandText = SQL_INSERT_ITEM_INSTANCES;
                long[] lastIds = new long[size];
                for (int i = 0; i < size; i++)
                {
                    command.Parameters.Clear();
                    AddParameter(command, "@owner_id", ownerId);
                    AddParameter(command, "@zone", (byte)locs[i].zoneType);
                    AddParameter(command, "@container", locs[i].container);
                    AddParameter(command, "@slot", locs[i].slot);
                    AddParameter(command, "@base_id", baseId[i]);
                    AddParameter(command, "@statuses", (int)spawnParams[i].itemStatuses);
                    AddParameter(command, "@quantity", spawnParams[i].quantity);
                    AddParameter(command, "@plus_maximum_durability", spawnParams[i].plusMaximumDurability);
                    AddParameter(command, "@plus_physical", spawnParams[i].plusPhysical);
                    AddParameter(command, "@plus_magical", spawnParams[i].plusMagical);
                    AddParameter(command, "@plus_gp", spawnParams[i].plusGp);
                    AddParameter(command, "@plus_weight", spawnParams[i].plusWeight);
                    AddParameter(command, "@plus_ranged_eff", spawnParams[i].plusRangedEff);
                    AddParameter(command, "@plus_reservoir_eff", spawnParams[i].plusReservoirEff);

                    if (spawnParams[i].gemSlots.Length > 0)
                        AddParameter(command, "@gem_slot_1_type", (int)spawnParams[i].gemSlots[0].type);
                    else
                        AddParameter(command, "@gem_slot_1_type", 0);

                    if (spawnParams[i].gemSlots.Length > 1)
                        AddParameter(command, "@gem_slot_2_type", (int)spawnParams[i].gemSlots[1].type);
                    else
                        AddParameter(command, "@gem_slot_2_type", 0);

                    if (spawnParams[i].gemSlots.Length > 2)
                        AddParameter(command, "@gem_slot_3_type", (int)spawnParams[i].gemSlots[2].type);
                    else
                        AddParameter(command, "@gem_slot_3_type", 0);

                    if (spawnParams[i].gemSlots.Length > 0)
                        AddParameter(command, "@gem_id_slot_1", (int)spawnParams[i].gemSlots[0].type);
                    else
                        AddParameterNull(command, "@gem_id_slot_1");

                    if (spawnParams[i].gemSlots.Length > 1)
                        AddParameter(command, "@gem_id_slot_2", (int)spawnParams[i].gemSlots[1].type);
                    else
                        AddParameterNull(command, "@gem_id_slot_2");

                    if (spawnParams[i].gemSlots.Length > 2)
                        AddParameter(command, "@gem_id_slot_3", (int)spawnParams[i].gemSlots[2].type);
                    else
                        AddParameterNull(command, "@gem_id_slot_3");

                    lastIds[i] = (long)command.ExecuteScalar();
                }

                string[] parameters = new string[size];
                for (int i = 0; i < size; i++)
                {
                    parameters[i] = string.Format("@id{0}", i);
                    AddParameter(command, parameters[i], lastIds[i]);
                }

                command.CommandText = string.Format("SELECT * FROM item_instance WHERE id IN({0})", string.Join(", ", parameters));
                using DbDataReader reader = command.ExecuteReader();
                while (reader.Read()) itemInstances.Add(MakeItemInstance(reader));
            }
            catch (Exception ex)
            {
                Logger.Error($"Query: {SQL_INSERT_ITEM_INSTANCES}");
                Exception(ex);
            }

            return itemInstances;
        }

        public List<ItemInstance> SelectLootableInventoryItems(uint ownerId)
        {
            List<ItemInstance> lootableItemInstances = new List<ItemInstance>();
            ExecuteReader(SQL_SELECT_OWNED_INVENTORY_ITEMS,
                command => { AddParameter(command, "@owner_id", ownerId); }, reader =>
                {
                    while (reader.Read()) lootableItemInstances.Add(MakeItemInstance(reader));
                });
            return lootableItemInstances;
        }

        public List<ItemInstance> SelectAuctions(uint ownerSoulId)
        {
            List<ItemInstance> auctions = new List<ItemInstance>();
            int i = 0;
            ExecuteReader(SQL_SELECT_AUCTIONS,
                command => { AddParameter(command, "@owner_soul_id", ownerSoulId); }, reader =>
                {
                    while (reader.Read())
                    {
                        ItemInstance itemInstance = MakeItemInstance(reader);
                        auctions.Add(itemInstance);
                    }
                });
            return auctions;
        }

        public void UpdateAuctionExhibit(ItemInstance itemInstance)
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

        public void UpdateAuctionCancelExhibit(ulong instanceId)
        {
            ExecuteNonQuery(SQL_UPDATE_CANCEL_EXHIBIT, command =>
            {
                AddParameter(command, "@id", instanceId);
                AddParameterNull(command, "@consigner_soul_name");
                AddParameterNull(command, "@expiry_datetime");
                AddParameterNull(command, "@min_bid");
                AddParameterNull(command, "@buyout_price");
                AddParameterNull(command, "@comment");
                AddParameterNull(command, "@bidder_soul_id");
            });
        }

        public List<ItemInstance> SelectBids(int bidderSoulId)
        {
            List<ItemInstance> bids = new List<ItemInstance>();
            int i = 0;
            ExecuteReader(SQL_SELECT_BIDS,
                command => { AddParameter(command, "@bidder_soul_id", bidderSoulId); }, reader =>
                {
                    while (reader.Read())
                    {
                        ItemInstance itemInstance = MakeItemInstance(reader);
                        itemInstance.currentBid = reader.IsDBNull("current_bid") ? 0 : reader.GetInt32("current_bid");
                        itemInstance.bidderSoulId = reader.IsDBNull("bidder_soul_id") ? 0 : reader.GetInt32("bidder_soul_id");
                        itemInstance.maxBid = reader.IsDBNull("max_bid") ? 0 : reader.GetInt32("max_bid");
                        itemInstance.isBidCancelled = reader.GetBoolean("is_cancelled");
                        bids.Add(itemInstance);
                    }
                });
            return bids;
        }

        public List<ItemInstance> SelectLots(int ownerSoulId)
        {
            List<ItemInstance> lots = new List<ItemInstance>();
            int i = 0;
            ExecuteReader(SQL_SELECT_LOTS,
                command => { AddParameter(command, "@owner_soul_id", ownerSoulId); }, reader =>
                {
                    while (reader.Read())
                    {
                        ItemInstance itemInstance = MakeItemInstance(reader);
                        lots.Add(itemInstance);
                    }
                });
            return lots;
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

        public void InsertAuctionBid(ulong instanceId, int bidderSoulId, ulong bid)
        {
            ExecuteNonQuery(SQL_INSERT_AUCTION_BID, command =>
            {
                AddParameter(command, "@instance_id", instanceId);
                AddParameter(command, "@bidder_soul_id", bidderSoulId);
                AddParameter(command, "@current_bid", bid);
            });
        }

        public void UpdateAuctionWinner(ulong instanceId, int winnerSoulId)
        {
            ExecuteNonQuery(SQL_UPDATE_AUCTION_WINNER_SOUL_ID, command =>
            {
                AddParameter(command, "@winner_soul_id", winnerSoulId);
                AddParameter(command, "@id", instanceId);
            });
        }

        public int SelectAuctionWinnerSoulId(ulong instanceId)
        {
            int winnerSoulId = 0;
            ExecuteReader(SQL_SELECT_AUCTION_WINNER_SOUL_ID,
                command => { AddParameter(command, "@id", instanceId); }, reader =>
                {
                    reader.Read();
                    winnerSoulId = reader.IsDBNull("winner_soul_id") ? 0 : reader.GetInt32("winner_soul_id"); //TODO remove cast
                });
            return winnerSoulId;
        }

        public ItemInstance[] SelectItemInstances(long[] instanceIds)
        {
            throw new NotImplementedException();
        }

        private ItemInstance MakeItemInstance(DbDataReader reader)
        {
            ulong instanceId = (ulong)reader.GetInt64("id");
            ItemInstance itemInstance = new ItemInstance(instanceId);
            itemInstance.ownerId = reader.GetInt32("owner_id");

            ItemZoneType zone = (ItemZoneType)reader.GetByte("zone");
            byte bag = reader.GetByte("container");
            short slot = reader.GetInt16("slot");
            ItemLocation itemLocation = new ItemLocation(zone, bag, slot);
            itemInstance.location = itemLocation;

            itemInstance.baseId = reader.GetInt32("base_id");

            itemInstance.quantity = reader.GetByte("quantity");

            itemInstance.statuses = (ItemStatuses)reader.GetInt32("statuses");

            itemInstance.currentEquipSlot = (ItemEquipSlots)reader.GetInt32("current_equip_slot");

            itemInstance.currentDurability = reader.GetInt32("current_durability");
            itemInstance.maximumDurability = reader.GetInt32("plus_maximum_durability");
            if (itemInstance.maximumDurability == 0) itemInstance.maximumDurability = 10; //toDo  update item library.  items shouldnt have 0 for core stats

            itemInstance.enhancementLevel = reader.GetByte("enhancement_level");

            itemInstance.specialForgeLevel = reader.GetByte("special_forge_level");

            itemInstance.physical = reader.GetInt16("physical");
            itemInstance.magical = reader.GetInt16("magical");

            itemInstance.hardness = reader.GetByte("hardness");
            itemInstance.plusPhysical = reader.GetInt16("plus_physical");
            itemInstance.plusMagical = reader.GetInt16("plus_magical");
            itemInstance.plusGp = reader.GetInt16("plus_gp");
            itemInstance.plusWeight = (short)(reader.GetInt16("plus_weight") * 10); //TODO REMOVE THIS MULTIPLICATION AND FIX TRHOUGHOUT CODE
            itemInstance.plusRangedEff = reader.GetInt16("plus_ranged_eff");
            itemInstance.plusReservoirEff = reader.GetInt16("plus_reservoir_eff");

            int gemSlotNum = 0;
            int gemSlot1Type = reader.GetByte("gem_slot_1_type");
            if (gemSlot1Type != 0) gemSlotNum++;
            int gemSlot2Type = reader.GetByte("gem_slot_2_type");
            if (gemSlot2Type != 0) gemSlotNum++;
            int gemSlot3Type = reader.GetByte("gem_slot_3_type");
            if (gemSlot3Type != 0) gemSlotNum++;
            GemSlot[] gemSlot = new GemSlot[gemSlotNum];

            itemInstance.enchantId = reader.GetInt32("enchant_id");
            itemInstance.gp = reader.GetInt16("plus_gp");
            itemInstance.type = (ItemType)Enum.Parse(typeof(ItemType), reader.GetString("item_type"));
            itemInstance.quality = (ItemQualities)Enum.Parse(typeof(ItemQualities), reader.GetString("quality"), true);
            itemInstance.maxStackSize = reader.GetByte("max_stack_size");

            if (reader.GetBoolean("es_hand_r"))         itemInstance.equipAllowedSlots |= ItemEquipSlots.RightHand;
            if (reader.GetBoolean("es_hand_l"))         itemInstance.equipAllowedSlots |= ItemEquipSlots.LeftHand;
            if (reader.GetBoolean("es_quiver"))         itemInstance.equipAllowedSlots |= ItemEquipSlots.Quiver;
            if (reader.GetBoolean("es_head"))           itemInstance.equipAllowedSlots |= ItemEquipSlots.Head;
            if (reader.GetBoolean("es_body"))           itemInstance.equipAllowedSlots |= ItemEquipSlots.Torso;
            if (reader.GetBoolean("es_legs"))           itemInstance.equipAllowedSlots |= ItemEquipSlots.Legs;
            if (reader.GetBoolean("es_arms"))           itemInstance.equipAllowedSlots |= ItemEquipSlots.Arms;
            if (reader.GetBoolean("es_feet"))           itemInstance.equipAllowedSlots |= ItemEquipSlots.Feet;
            if (reader.GetBoolean("es_mantle"))         itemInstance.equipAllowedSlots |= ItemEquipSlots.Cloak;
            if (reader.GetBoolean("es_ring"))           itemInstance.equipAllowedSlots |= ItemEquipSlots.Ring;
            if (reader.GetBoolean("es_earring"))        itemInstance.equipAllowedSlots |= ItemEquipSlots.Earring;
            if (reader.GetBoolean("es_necklace"))       itemInstance.equipAllowedSlots |= ItemEquipSlots.Necklace;
            if (reader.GetBoolean("es_belt"))           itemInstance.equipAllowedSlots |= ItemEquipSlots.Belt;
            if (reader.GetBoolean("es_talkring"))       itemInstance.equipAllowedSlots |= ItemEquipSlots.Talkring;
            if (reader.GetBoolean("es_avatar_head"))    itemInstance.equipAllowedSlots |= ItemEquipSlots.AvatarHead;
            if (reader.GetBoolean("es_avatar_body"))    itemInstance.equipAllowedSlots |= ItemEquipSlots.AvatarTorso;
            if (reader.GetBoolean("es_avatar_legs"))    itemInstance.equipAllowedSlots |= ItemEquipSlots.AvatarLegs;
            if (reader.GetBoolean("es_avatar_arms"))    itemInstance.equipAllowedSlots |= ItemEquipSlots.AvatarArms;
            if (reader.GetBoolean("es_avatar_feet"))    itemInstance.equipAllowedSlots |= ItemEquipSlots.AvatarFeet;
            if (reader.GetBoolean("es_avatar_feet"))    itemInstance.equipAllowedSlots |= ItemEquipSlots.AvatarCloak;

            if (reader.GetBoolean("req_hum_m")) itemInstance.requiredRaces |= Races.HumanMale;
            if (reader.GetBoolean("req_hum_f")) itemInstance.requiredRaces |= Races.HumanFemale;
            if (reader.GetBoolean("req_elf_m")) itemInstance.requiredRaces |= Races.ElfMale;
            if (reader.GetBoolean("req_elf_f")) itemInstance.requiredRaces |= Races.ElfFemale;
            if (reader.GetBoolean("req_dwf_m")) itemInstance.requiredRaces |= Races.DwarfMale;
            if (reader.GetBoolean("req_por_m")) itemInstance.requiredRaces |= Races.PorkulMale;
            if (reader.GetBoolean("req_por_f")) itemInstance.requiredRaces |= Races.PorkulFemale;
            if (reader.GetBoolean("req_gnm_f")) itemInstance.requiredRaces |= Races.GnomeFemale;

            if (reader.GetBoolean("req_fighter"))   itemInstance.requiredClasses |= Classes.Fighter;
            if (reader.GetBoolean("req_thief"))     itemInstance.requiredClasses |= Classes.Thief;
            if (reader.GetBoolean("req_mage"))      itemInstance.requiredClasses |= Classes.Mage;
            if (reader.GetBoolean("req_priest"))    itemInstance.requiredClasses |= Classes.Priest;
            if (reader.GetBoolean("req_samurai"))   itemInstance.requiredClasses |= Classes.Samurai;
            if (reader.GetBoolean("req_bishop"))    itemInstance.requiredClasses |= Classes.Bishop;
            if (reader.GetBoolean("req_ninja"))     itemInstance.requiredClasses |= Classes.Ninja;
            if (reader.GetBoolean("req_lord"))      itemInstance.requiredClasses |= Classes.Lord;
            if (reader.GetBoolean("req_clown"))     itemInstance.requiredClasses |= Classes.Clown;
            if (reader.GetBoolean("req_alchemist")) itemInstance.requiredClasses |= Classes.Alchemist;

            if (reader.GetBoolean("req_lawful"))    itemInstance.requiredAlignments |= Alignments.Lawful;
            if (reader.GetBoolean("req_neutral"))   itemInstance.requiredAlignments |= Alignments.Neutral;
            if (reader.GetBoolean("req_chaotic"))   itemInstance.requiredAlignments |= Alignments.Chaotic;

            itemInstance.requiredStrength       = reader.GetByte("req_str");
            itemInstance.requiredVitality       = reader.GetByte("req_vit");
            itemInstance.requiredDexterity      = reader.GetByte("req_dex");
            itemInstance.requiredAgility        = reader.GetByte("req_agi");
            itemInstance.requiredIntelligence   = reader.GetByte("req_int");
            itemInstance.requiredPiety          = reader.GetByte("req_pie");
            itemInstance.requiredLuck           = reader.GetByte("req_luk");

            itemInstance.requiredSoulRank = reader.GetByte("req_soul_rank");
            //todo max soul rank
            itemInstance.requiredLevel = reader.GetByte("req_lvl");

            itemInstance.physicalSlash  = reader.GetByte("phys_slash");
            itemInstance.physicalStrike = reader.GetByte("phys_strike");
            itemInstance.physicalPierce = reader.GetByte("phys_pierce");

            itemInstance.physicalDefenseFire    = reader.GetByte("pdef_fire");
            itemInstance.physicalDefenseWater   = reader.GetByte("pdef_water");
            itemInstance.physicalDefenseWind    = reader.GetByte("pdef_wind");
            itemInstance.physicalDefenseEarth   = reader.GetByte("pdef_earth");
            itemInstance.physicalDefenseLight   = reader.GetByte("pdef_light");
            itemInstance.physicalDefenseDark    = reader.GetByte("pdef_dark");

            itemInstance.magicalAttackFire  = reader.GetByte("matk_fire");
            itemInstance.magicalAttackWater = reader.GetByte("matk_water");
            itemInstance.magicalAttackWind  = reader.GetByte("matk_wind");
            itemInstance.magicalAttackEarth = reader.GetByte("matk_earth");
            itemInstance.magicalAttackLight = reader.GetByte("matk_light");
            itemInstance.magicalAttackDark  = reader.GetByte("matk_dark");

            itemInstance.hp = reader.GetByte("seffect_hp");
            itemInstance.mp = reader.GetByte("seffect_mp");
            itemInstance.str = reader.GetByte("seffect_str");
            itemInstance.vit = reader.GetByte("seffect_vit");
            itemInstance.dex = reader.GetByte("seffect_dex");
            itemInstance.agi = reader.GetByte("seffect_agi");
            itemInstance.@int = reader.GetByte("seffect_int");
            itemInstance.pie = reader.GetByte("seffect_pie");
            itemInstance.luk = reader.GetByte("seffect_luk");

            itemInstance.resistPoison = reader.GetByte("res_poison");
            itemInstance.resistParalyze = reader.GetByte("res_paralyze");
            itemInstance.resistPetrified = reader.GetByte("res_petrified");
            itemInstance.resistFaint = reader.GetByte("res_faint");
            itemInstance.resistBlind = reader.GetByte("res_blind");
            itemInstance.resistSleep = reader.GetByte("res_sleep");
            itemInstance.resistSilence = reader.GetByte("res_silent");
            itemInstance.resistCharm = reader.GetByte("res_charm");
            itemInstance.resistConfusion = reader.GetByte("res_confusion");
            itemInstance.resistFear = reader.GetByte("res_fear");

            //itemInstance.StatusMalus = (ItemStatusEffect)Enum.Parse(typeof(ItemStatusEffect), reader.GetString("status_malus"));
            itemInstance.statusMalusPercent = reader.GetInt32("status_percent");

            itemInstance.objectType = reader.GetString("object_type"); //not sure what this is for
            itemInstance.equipSlot2 = reader.GetString("equip_slot"); //not sure what this is for

            itemInstance.isUseableInTown = !reader.GetBoolean("no_use_in_town"); //not sure what this is for
            itemInstance.isStorable = !reader.GetBoolean("no_storage");
            itemInstance.isDiscardable = !reader.GetBoolean("no_discard");
            itemInstance.isSellable = !reader.GetBoolean("no_sell");
            itemInstance.isTradeable = !reader.GetBoolean("no_trade");
            itemInstance.isTradableAfterUse = !reader.GetBoolean("no_trade_after_used");
            itemInstance.isStealable = !reader.GetBoolean("no_stolen");

            itemInstance.isGoldBorder = reader.GetBoolean("gold_border");

            //itemInstance.Lore = reader.GetString("lore");

            itemInstance.iconId = reader.GetInt32("icon");

            itemInstance.talkRingName = "";
            //TODO fix all the data types once mysql is implemented
            itemInstance.bagSize = reader.GetByte("num_of_bag_slots");

            //grade,
            //weight
            itemInstance.weight = (int)(reader.GetDouble("weight") * 10000); // TODO DOUBLE CHECK THIS IS CORRECT SCALE

            //auction
            itemInstance.consignerSoulName = reader.IsDBNull("consigner_soul_name") ? "" : reader.GetString("consigner_soul_name");
            itemInstance.secondsUntilExpiryTime = reader.IsDBNull("expiry_datetime") ? 0 : CalcSecondsToExpiry(reader.GetInt64("expiry_datetime"));
            itemInstance.minimumBid = reader.IsDBNull("min_bid") ? 0 : (ulong)reader.GetInt64("min_bid");
            itemInstance.buyoutPrice = reader.IsDBNull("buyout_price") ? 0 : (ulong)reader.GetInt64("buyout_price");
            itemInstance.comment = reader.IsDBNull("comment") ? "" : reader.GetString("comment");

            //enhancement
            itemInstance = CalcEnhancement(itemInstance);

            return itemInstance;
        }

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

        private ItemInstance CalcEnhancement(ItemInstance itemInstance)
        {
            //Temporary ItemLibrary.CSV lookup until Item_decrypted.csv and Table are fully mapped/ populated
            //server.settingRepository.itemLibrary.TryGetValue(itemInstance.baseId, out ItemLibrarySetting itemLibrarySetting);
            //if (itemLibrarySetting != null)
            {
                //itemInstance.maximumDurability = itemLibrarySetting.durability; //Temporary until we get durability in itemLibrary
                if (itemInstance.currentDurability > itemInstance.maximumDurability) itemInstance.currentDurability = itemInstance.maximumDurability;
                if (itemInstance.weight == 0) itemInstance.weight += 1234;
                if (itemInstance.type == ItemType.SHIELD_LARGE || itemInstance.type == ItemType.SHIELD_MEDIUM || itemInstance.type == ItemType.SHIELD_SMALL)
                {
                    if (itemInstance.gp == 0) itemInstance.gp += 50;
                    if (itemInstance.maximumDurability <= 0) itemInstance.maximumDurability = 55;
                }
            }

            //update items base stats per enchantment level.
            ForgeMultiplier forgeMultiplier = LoginLoadMultiplier(itemInstance.enhancementLevel);
            itemInstance.physical = (short)(itemInstance.physical * forgeMultiplier.factor);
            itemInstance.magical = (short)(itemInstance.magical * forgeMultiplier.factor);
            itemInstance.maximumDurability = (short)(itemInstance.maximumDurability * forgeMultiplier.durability);
            itemInstance.hardness = (byte)(itemInstance.hardness + forgeMultiplier.hardness);
            itemInstance.weight = (short)(itemInstance.weight - forgeMultiplier.weight);
            if (itemInstance.weight < 0) itemInstance.weight = 0;
            return itemInstance;
        }
        //also exists in itemservice. needs to match
        public ForgeMultiplier LoginLoadMultiplier(int level)
        {
            double factor = 1;
            double durability = 1;
            int hardness = 0;
            switch (level)
            {
                case 0:
                    factor = 1.00;
                    durability = 1.0;
                    hardness = 0;
                    break;
                case 1:
                    factor = 1.05;
                    durability = 1.1;
                    hardness = 0;
                    break;
                case 2:
                    factor = 1.16;
                    durability = 1.2;
                    hardness = 0;
                    break;
                case 3:
                    factor = 1.29;
                    durability = 1.3;
                    hardness = 0;
                    break;
                case 4:
                    factor = 1.45;
                    durability = 1.4;
                    hardness = 0;
                    break;
                case 5:
                    factor = 1.67;
                    durability = 1.5;
                    hardness = 1;
                    break;
                case 6:
                    factor = 1.92;
                    durability = 1.6;
                    hardness = 0;
                    break;
                case 7:
                    factor = 2.20;
                    durability = 1.7;
                    hardness = 0;
                    break;
                case 8:
                    factor = 2.54;
                    durability = 1.8;
                    hardness = 0;
                    break;
                case 9:
                    factor = 2.91;
                    durability = 1.9;
                    hardness = 0;
                    break;
                case 10:
                    factor = 3.35;
                    durability = 2.0;
                    hardness = 2;
                    break;
            }

            ForgeMultiplier forgeMultiplier = new ForgeMultiplier();
            forgeMultiplier.factor = factor;
            forgeMultiplier.durability = durability;
            forgeMultiplier.hardness = hardness;
            forgeMultiplier.weight = 100; //toDo
            return forgeMultiplier;
        }


    }
}
