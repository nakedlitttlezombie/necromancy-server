using System;
using System.Collections.Generic;
using Arrowgene.Logging;
using Necromancy.Server.Data.Setting;
using Necromancy.Server.Logging;
using Necromancy.Server.Model;
using Necromancy.Server.Model.Stats;
using Necromancy.Server.Packet;
using Necromancy.Server.Packet.Receive.Area;

namespace Necromancy.Server.Systems.Item
{
    public class ItemService
    {
        public enum MoveType
        {
            Place,
            Swap,
            PlaceQuantity,
            AddQuantity,
            AllQuantity,
            None
        }

        private static readonly NecLogger _Logger = LogProvider.Logger<NecLogger>(typeof(ItemService));

        private readonly Character _character;
        private readonly IItemDao _itemDao;

        public ItemService(Character character)
        {
            _itemDao = new ItemDao();
            _character = character;
        }

        public ItemService(Character character, IItemDao itemDao)
        {
            _itemDao = itemDao;
            _character = character;
        }

        internal ItemInstance GetItem(ItemLocation location)
        {
            return _character.itemLocationVerifier.GetItem(location);
        }

        internal ItemInstance GetIdentifiedItem(ItemLocation location)
        {
            ItemInstance item = _character.itemLocationVerifier.GetItem(location);
            if (item.statuses.HasFlag(ItemStatuses.Unidentified)) item.statuses &= ~ItemStatuses.Unidentified;
            _itemDao.UpdateItemOwnerAndStatus(item.instanceId, _character.id, (int)item.statuses);
            return item;
        }

        public ItemInstance Equip(ItemLocation location, ItemEquipSlots equipSlot)
        {
            ItemInstance item = _character.itemLocationVerifier.GetItem(location);
            item.currentEquipSlot = equipSlot;
            if (_character.equippedItems.ContainsKey(equipSlot))
                _character.equippedItems[equipSlot] = item;
            else
                _character.equippedItems.Add(equipSlot, item);
            _itemDao.UpdateItemEquipMask(item.instanceId, equipSlot);
            return item;
        }

        public ItemInstance CheckAlreadyEquipped(ItemEquipSlots equipmentSlotType)
        {
            ItemInstance itemInstance = null;
            if ((equipmentSlotType == ItemEquipSlots.LeftHand) | (equipmentSlotType == ItemEquipSlots.RightHand))
            {
                if (_character.equippedItems.ContainsKey(ItemEquipSlots.LeftHand | ItemEquipSlots.RightHand))
                    _character.equippedItems.TryGetValue(ItemEquipSlots.LeftHand | ItemEquipSlots.RightHand, out itemInstance);
                else
                    _character.equippedItems.TryGetValue(equipmentSlotType, out itemInstance);
            }
            else
            {
                _character.equippedItems.TryGetValue(equipmentSlotType, out itemInstance);
            }

            return itemInstance;
        }

        /// <returns></returns>
        public ItemInstance Unequip(ItemEquipSlots equipSlot)
        {
            ItemInstance item = _character.equippedItems[equipSlot];
            _character.equippedItems.Remove(equipSlot);
            item.currentEquipSlot = ItemEquipSlots.None;
            _itemDao.UpdateItemEquipMask(item.instanceId, ItemEquipSlots.None);
            return item;
        }

        internal ItemInstance GetLootedItem(ItemLocation location)
        {
            ItemInstance item = _character.itemLocationVerifier.GetItem(location);
            if (item.currentEquipSlot != ItemEquipSlots.None) Unequip(item.currentEquipSlot);
            return item;
        }

        public ItemInstance PutLootedItem(ItemInstance itemInstance)
        {
            //ToDo,  make this find space in more than just your adventure bag.
            itemInstance.location = _character.itemLocationVerifier.PutItemInNextOpenSlot(ItemZoneType.AdventureBag, itemInstance);
            _itemDao.UpdateItemOwnerAndStatus(itemInstance.instanceId, _character.id, (int)itemInstance.statuses);
            _itemDao.UpdateItemLocation(itemInstance.instanceId, itemInstance.location);
            return itemInstance;
        }

        public ItemInstance SpawnItemInstance(ItemZoneType itemZoneType, int baseId, ItemSpawnParams spawnParam)
        {
            int[] itemIds = { baseId };
            ItemSpawnParams[] spawmParams = new ItemSpawnParams[1];
            spawmParams[0] = spawnParam;
            List<ItemInstance> items = SpawnItemInstances(itemZoneType, itemIds, spawmParams);
            return items[0];
        }

        public List<ItemInstance> SpawnItemInstances(ItemZoneType itemZoneType, int[] baseIds, ItemSpawnParams[] spawnParams)
        {
            if (_character.itemLocationVerifier.GetTotalFreeSpace(itemZoneType) < baseIds.Length) throw new ItemException(ItemExceptionType.InventoryFull);
            ItemLocation[] nextOpenLocations = _character.itemLocationVerifier.NextOpenSlots(itemZoneType, baseIds.Length);
            List<ItemInstance> itemInstances = _itemDao.InsertItemInstances(_character.id, nextOpenLocations, baseIds, spawnParams);
            foreach (ItemInstance item in itemInstances) _character.itemLocationVerifier.PutItem(item.location, item);
            return itemInstances;
        }

        /// <summary>
        /// </summary>
        /// <returns>A list of items in your adventure bag, equipped bags, bag slot, premium bag, and avatar inventory.</returns>
        public List<ItemInstance> LoadOwneditemInstances(NecServer server)
        {
            //Clear Equipped Items from send_data_get_self_chara_data_request
            _character.equippedItems.Clear();
            List<ItemInstance> ownedItems = _itemDao.SelectOwnedInventoryItems(_character.id);
            //load bags first
            foreach (ItemInstance item in ownedItems)
                if (item.location.zoneType == ItemZoneType.BagSlot)
                {
                    ItemLocation location = item.location; //only needed on load inventory because item's location is already populated and it is not in the manager
                    item.location = ItemLocation.InvalidLocation; //only needed on load inventory because item's location is already populated and it is not in the manager
                    _character.itemLocationVerifier.PutItem(location, item);
                }

            foreach (ItemInstance itemInstance in ownedItems)
            {
                if (itemInstance.location.slot < 0) //remove invalid db rows on login.
                {
                    _itemDao.DeleteItemInstance(itemInstance.instanceId);
                    continue;
                }

                if (itemInstance.location.zoneType != ItemZoneType.BagSlot)
                {
                    ItemLocation location = itemInstance.location; //only needed on load inventory because item's location is already populated and it is not in the manager
                    itemInstance.location = ItemLocation.InvalidLocation; //only needed on load inventory because item's location is already populated and it is not in the manager

                    _character.itemLocationVerifier.PutItem(location, itemInstance);
                }

                if (itemInstance.currentEquipSlot != ItemEquipSlots.None) _character.equippedItems.Add(itemInstance.currentEquipSlot, itemInstance);
            }

            return ownedItems;
        }

        /// <summary>
        /// </summary>
        /// <returns>A list of items in your cloakroom</returns>
        public List<ItemInstance> LoadCloakRoomItemInstances(NecServer server)
        {
            List<ItemInstance> ownedItems = _itemDao.SelectCloakRoomItems(_character.soulId);
            //load bags first
            foreach (ItemInstance item in ownedItems)
                if (item.location.zoneType == ItemZoneType.BagSlot)
                {
                    ItemLocation location = item.location; //only needed on load inventory because item's location is already populated and it is not in the manager
                    item.location = ItemLocation.InvalidLocation; //only needed on load inventory because item's location is already populated and it is not in the manager
                    _character.itemLocationVerifier.PutItem(location, item);
                }

            foreach (ItemInstance itemInstance in ownedItems)
            {
                if (itemInstance.location.slot < 0) //remove invalid db rows on login.
                {
                    _itemDao.DeleteItemInstance(itemInstance.instanceId);
                    continue;
                }

                if (itemInstance.location.zoneType != ItemZoneType.BagSlot)
                {
                    ItemLocation location = itemInstance.location; //only needed on load inventory because item's location is already populated and it is not in the manager
                    itemInstance.location = ItemLocation.InvalidLocation; //only needed on load inventory because item's location is already populated and it is not in the manager

                    _character.itemLocationVerifier.PutItem(location, itemInstance);
                }

                //if (itemInstance.currentEquipSlot != ItemEquipSlots.None) _character.equippedItems.Add(itemInstance.currentEquipSlot, itemInstance);
            }

            return ownedItems;
        }

        public void LoadEquipmentModels()
        {
            _character.equippedItems.Clear();
            List<ItemInstance> ownedItems = _itemDao.SelectOwnedInventoryItems(_character.id);
            foreach (ItemInstance item in ownedItems)
                if (item.currentEquipSlot != ItemEquipSlots.None)
                {
                    if (!_character.equippedItems.ContainsKey(item.currentEquipSlot))
                    {
                        _character.equippedItems.Add(item.currentEquipSlot, item);
                    }
                    else
                    {
                        //Clean up duplicate equipped items since we don't have a unique constraint on table
                        item.currentEquipSlot = ItemEquipSlots.None;
                        _itemDao.UpdateItemEquipMask(item.instanceId, ItemEquipSlots.None);
                    }
                }
        }

        public ItemInstance Remove(ItemLocation location, byte quantity)
        {
            ItemInstance item = _character.itemLocationVerifier.GetItem(location);
            ulong instanceId = item.instanceId;
            if (item is null) throw new ItemException(ItemExceptionType.Generic);
            if (item.quantity < quantity) throw new ItemException(ItemExceptionType.Amount);

            item.quantity -= quantity;
            if (item.quantity == 0)
            {
                _itemDao.DeleteItemInstance(instanceId);
                _character.itemLocationVerifier.RemoveItem(item);
            }
            else
            {
                ulong[] instanceIds = new ulong[1];
                byte[] quantities = new byte[1];
                instanceIds[0] = instanceId;
                quantities[0] = item.quantity;
                _itemDao.UpdateItemQuantities(instanceIds, quantities);
            }

            return item;
        }

        public ulong Sell(ItemLocation location, byte quantity)
        {
            throw new NotImplementedException();
        }

        public MoveResult Move(ItemLocation from, ItemLocation to, byte quantity)
        {
            ItemInstance fromItem = _character.itemLocationVerifier.GetItem(from);
            bool hasToItem = _character.itemLocationVerifier.HasItem(to);
            ItemInstance toItem = _character.itemLocationVerifier.GetItem(to);
            MoveResult moveResult = new MoveResult();

            //check possible errors. these should only occur if client is compromised
            if (fromItem is null || quantity == 0) throw new ItemException(ItemExceptionType.Generic);
            if (quantity > fromItem.quantity) throw new ItemException(ItemExceptionType.Amount);
            if (quantity > 1 && quantity < fromItem.quantity && hasToItem && toItem.baseId != fromItem.baseId) throw new ItemException(ItemExceptionType.BagLocation);
            if (fromItem.location.zoneType == ItemZoneType.BagSlot && !_character.itemLocationVerifier.IsEmptyContainer(ItemZoneType.EquippedBags, fromItem.location.slot)) throw new ItemException(ItemExceptionType.BagLocation);

            if (!hasToItem && quantity == fromItem.quantity)
                moveResult = MoveItemPlace(to, fromItem);
            else if (!hasToItem && quantity < fromItem.quantity)
                moveResult = MoveItemPlaceQuantity(to, fromItem, quantity);
            else if (hasToItem && quantity == fromItem.quantity && (fromItem.baseId != toItem.baseId || fromItem.quantity == fromItem.maxStackSize))
                moveResult = MoveItemSwap(from, to, fromItem, toItem);
            else if (hasToItem && quantity < fromItem.quantity && toItem.baseId == fromItem.baseId)
                moveResult = MoveItemAddQuantity(fromItem, toItem, quantity);
            else if (hasToItem && quantity == fromItem.quantity && toItem.baseId == fromItem.baseId && quantity <= toItem.maxStackSize - toItem.quantity) moveResult = MoveItemAllQuantity(fromItem, toItem, quantity);

            return moveResult;
        }

        /// <summary>
        ///     Used when the there is no item already in the end location and the quantity moved is equal to the total quantity of
        ///     items in the original location.
        /// </summary>
        /// <param name="to">Move to this location.</param>
        /// <param name="fromItem">Move this item.</param>
        /// <returns>Result with origin item null and the destination the moved item.</returns>
        private MoveResult MoveItemPlace(ItemLocation to, ItemInstance fromItem)
        {
            MoveResult moveResult = new MoveResult(MoveType.Place);
            _character.itemLocationVerifier.PutItem(to, fromItem);
            moveResult.destItem = fromItem;

            ulong[] instanceIds = new ulong[1];
            ItemLocation[] locs = new ItemLocation[1];
            instanceIds[0] = moveResult.destItem.instanceId;
            locs[0] = moveResult.destItem.location;
            _itemDao.UpdateItemLocations(instanceIds, locs);

            return moveResult;
        }

        /// <summary>
        ///     Used when the there is an item already in the end location,the quantity moved is equal to the total quantity<br />
        ///     of items in the original location, and the item at the end location is a different base item or stacked full.
        /// </summary>
        /// <param name="from">Swap back to this location.</param>
        /// <param name="to">Move to this location.</param>
        /// <param name="fromItem">Move this item.</param>
        /// <param name="toItem">Swap this item.</param>
        /// <returns>Result with the origin item and destination item being the swapped items.</returns>
        private MoveResult MoveItemSwap(ItemLocation from, ItemLocation to, ItemInstance fromItem, ItemInstance toItem)
        {
            MoveResult moveResult = new MoveResult(MoveType.Swap);
            _character.itemLocationVerifier.PutItem(to, fromItem);
            _character.itemLocationVerifier.PutItem(from, toItem);
            moveResult.destItem = fromItem;
            moveResult.originItem = toItem;

            ulong[] instanceIds = new ulong[2];
            ItemLocation[] locs = new ItemLocation[2];
            instanceIds[0] = moveResult.originItem.instanceId;
            locs[0] = moveResult.originItem.location;
            instanceIds[1] = moveResult.destItem.instanceId;
            locs[1] = moveResult.destItem.location;

            _itemDao.UpdateItemLocations(instanceIds, locs);

            return moveResult;
        }

        /// <summary>
        ///     Used when the there is no item already in the end location and the quantity moved is less than total quantity of
        ///     items in the original location.
        /// </summary>
        /// <param name="to">Move to this location.</param>
        /// <param name="fromItem">Move a quantity from this item.</param>
        /// <returns>
        ///     Result containing the original item with less quantity and a new instance with the remaining amount at the
        ///     destination.
        /// </returns>
        private MoveResult MoveItemPlaceQuantity(ItemLocation to, ItemInstance fromItem, byte quantity)
        {
            MoveResult moveResult = new MoveResult(MoveType.PlaceQuantity);
            moveResult.originItem = fromItem;
            moveResult.originItem.quantity -= quantity;

            ItemSpawnParams spawnParam = new ItemSpawnParams();
            spawnParam.quantity = quantity;
            spawnParam.itemStatuses = moveResult.originItem.statuses;

            const int SIZE = 1;
            ItemLocation[] locs = new ItemLocation[SIZE];
            int[] baseIds = new int[SIZE];
            ItemSpawnParams[] spawnParams = new ItemSpawnParams[SIZE];

            locs[0] = to;
            baseIds[0] = moveResult.originItem.baseId;
            spawnParams[0] = spawnParam;

            List<ItemInstance> insertedItem = _itemDao.InsertItemInstances(fromItem.ownerId, locs, baseIds, spawnParams);
            _character.itemLocationVerifier.PutItem(to, insertedItem[0]);
            moveResult.destItem = insertedItem[0];

            return moveResult;
        }

        /// <summary>
        ///     Used if there is the same item at the end location that is not at max stack size and the quantity moved is less
        ///     than total quantity of items in the original location.<br />
        ///     If the stack would be filled with less than the passed quantity, fill the stack and return leftovers.
        /// </summary>
        /// <param name="fromItem">Item to subract quantity from.</param>
        /// <param name="toItem">Location of item to add quantity to.</param>
        /// <param name="quantity">Maximum amount to transfer.</param>
        /// <returns>Result containing the original items with updated quantities.</returns>
        private MoveResult MoveItemAddQuantity(ItemInstance fromItem, ItemInstance toItem, byte quantity)
        {
            MoveResult moveResult = new MoveResult(MoveType.AddQuantity);
            moveResult.originItem = fromItem;
            moveResult.destItem = toItem;

            int freeSpace = moveResult.destItem.maxStackSize - moveResult.destItem.quantity;
            if (freeSpace < quantity)
                quantity = (byte)freeSpace;
            moveResult.originItem.quantity -= quantity;
            moveResult.destItem.quantity += quantity;

            ulong[] instanceIds = new ulong[2];
            byte[] quantities = new byte[2];
            instanceIds[0] = moveResult.originItem.instanceId;
            quantities[0] = moveResult.originItem.quantity;
            instanceIds[1] = moveResult.destItem.instanceId;
            quantities[1] = moveResult.destItem.quantity;
            _itemDao.UpdateItemQuantities(instanceIds, quantities);

            return moveResult;
        }

        /// <summary>
        ///     Used if there is the same item at the end location that is not at max stack size and the quantity moved is less
        ///     equal to the quantity of items in the original location.
        /// </summary>
        /// <param name="fromItem">Removed item.</param>
        /// <param name="toItem">Updated item.</param>
        /// <param name="quantity">Quantity to add to end item</param>
        /// <returns>Result with the origin item null and the destination item with an updated quantity.</returns>
        private MoveResult MoveItemAllQuantity(ItemInstance fromItem, ItemInstance toItem, byte quantity)
        {
            MoveResult moveResult = new MoveResult(MoveType.AllQuantity);
            moveResult.destItem = toItem;
            moveResult.destItem.quantity += quantity;
            _character.itemLocationVerifier.RemoveItem(fromItem.location);

            ulong[] instanceIds = new ulong[1];
            byte[] quantities = new byte[1];
            instanceIds[0] = moveResult.destItem.instanceId;
            quantities[0] = moveResult.destItem.quantity;
            //TODO MAKE TRANSACTION
            _itemDao.DeleteItemInstance(fromItem.instanceId);
            _itemDao.UpdateItemQuantities(instanceIds, quantities);

            return moveResult;
        }

        public List<PacketResponse> GetMoveResponses(NecClient client, MoveResult moveResult)
        {
            List<PacketResponse> responses = new List<PacketResponse>();
            switch (moveResult.type)
            {
                case MoveType.Place:
                    RecvItemUpdatePlace recvItemUpdatePlace = new RecvItemUpdatePlace(client, moveResult.destItem);
                    responses.Add(recvItemUpdatePlace);
                    break;
                case MoveType.Swap:
                    RecvItemUpdatePlaceChange recvItemUpdatePlaceChange = new RecvItemUpdatePlaceChange(client, moveResult.originItem, moveResult.destItem);
                    responses.Add(recvItemUpdatePlaceChange);
                    break;
                case MoveType.PlaceQuantity:
                    RecvItemUpdateNum recvItemUpdateNum = new RecvItemUpdateNum(client, moveResult.originItem);
                    responses.Add(recvItemUpdateNum);
                    RecvItemInstance recvItemInstance = new RecvItemInstance(client, moveResult.destItem);
                    responses.Add(recvItemInstance);
                    break;
                case MoveType.AddQuantity:
                    RecvItemUpdateNum recvItemUpdateNum0 = new RecvItemUpdateNum(client, moveResult.originItem);
                    responses.Add(recvItemUpdateNum0);
                    RecvItemUpdateNum recvItemUpdateNum1 = new RecvItemUpdateNum(client, moveResult.destItem);
                    responses.Add(recvItemUpdateNum1);
                    break;
                case MoveType.AllQuantity:
                    RecvItemRemove recvItemRemove = new RecvItemRemove(client, moveResult.originItem);
                    responses.Add(recvItemRemove);
                    RecvItemUpdateNum recvItemUpdateNum2 = new RecvItemUpdateNum(client, moveResult.destItem);
                    responses.Add(recvItemUpdateNum2);
                    break;
                case MoveType.None:
                    break;
            }

            return responses;
        }

        //TODO no character stats should be calculated, should be updated on equip and unequip / buff application move
        public List<PacketResponse> CalculateBattleStats(NecClient client)
        {
            List<PacketResponse> responses = new List<PacketResponse>();
            BattleParam battleParam = new BattleParam();

            client.character.ConditionBonus();
            client.character.weight.SetCurrent(0);
            client.character.gp.SetMax(0);
            bool shieldCheck = false;

            foreach (ItemInstance itemInstance in client.character.equippedItems.Values)
            {
                if (itemInstance.currentEquipSlot.HasFlag(ItemEquipSlots.RightHand) | (itemInstance.currentEquipSlot == ItemEquipSlots.Quiver))
                {
                    battleParam.plusPhysicalAttack += (short)(itemInstance.physical + itemInstance.plusPhysical);
                    battleParam.plusMagicalAttack += (short)(itemInstance.magical + itemInstance.plusMagical);
                }
                else
                {
                    battleParam.plusPhysicalDefence += (short)(itemInstance.physical + itemInstance.plusPhysical);
                    battleParam.plusMagicalDefence += (short)(itemInstance.magical + itemInstance.plusMagical);
                }

                client.character.gp.SetMax(client.character.gp.max + itemInstance.gp + itemInstance.plusGp);
                client.character.weight.Modify(itemInstance.weight + itemInstance.plusWeight);
                if ((itemInstance.type == ItemType.SHIELD_LARGE) | (itemInstance.type == ItemType.SHIELD_MEDIUM) | (itemInstance.type == ItemType.SHIELD_SMALL)) shieldCheck = true;
            }

            //if you dont have a shield on,  set your GP to 0.  no blocking for you
            if (shieldCheck == false)
            {
                client.character.gp.SetMax(0);
                RecvCharaUpdateAc recvCharaUpdateAc = new RecvCharaUpdateAc(client.character.gp.max);
                responses.Add(recvCharaUpdateAc);
            }

            RecvCharaUpdateMaxWeight recvCharaUpdateMaxWeight = new RecvCharaUpdateMaxWeight(client.character.weight.max / 10, client.character.weight.current / 10 /*Weight.Diff*/);
            responses.Add(recvCharaUpdateMaxWeight);

            RecvCharaUpdateWeight recvCharaUpdateWeight = new RecvCharaUpdateWeight(client.character.weight.current / 10);
            responses.Add(recvCharaUpdateWeight);

            RecvCharaUpdateMaxAc recvCharaUpdateMaxAc = new RecvCharaUpdateMaxAc(client.character.gp.max);
            responses.Add(recvCharaUpdateMaxAc);

            RecvCharaUpdateBattleBaseParam recvCharaUpdateBattleBaseParam = new RecvCharaUpdateBattleBaseParam(client.character, battleParam);
            responses.Add(recvCharaUpdateBattleBaseParam);

            client.character.battleParam = battleParam;
            return responses;
        }

        public List<ItemInstance> GetLootableItems(uint characterInstanceId)
        {
            uint characterId = characterInstanceId - 200000000; //todo replace 200000000 with server.setting.NecSetting.poolCharacterIdLowerBound
            //TODO ADD PROTECTIONS TO SQL CALL SO ALL ITEMS CANT BE LOOTED
            List<ItemInstance> lootableItems = _itemDao.SelectLootableInventoryItems(characterId);
            foreach (ItemInstance itemInstance in lootableItems) itemInstance.statuses = ItemStatuses.Unidentified;
            return lootableItems;
        }

        //also exists in itemDAO. needs to match.
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

        public ForgeMultiplier ForgeMultiplier(int level)
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
                    factor = 1.10;
                    durability = 1.1;
                    hardness = 0;
                    break;
                case 3:
                    factor = 1.12;
                    durability = 1.1;
                    hardness = 0;
                    break;
                case 4:
                    factor = 1.12;
                    durability = 1.1;
                    hardness = 0;
                    break;
                case 5:
                    factor = 1.15;
                    durability = 1.1;
                    hardness = 1;
                    break;
                case 6:
                    factor = 1.15;
                    durability = 1.1;
                    hardness = 0;
                    break;
                case 7:
                    factor = 1.15;
                    durability = 1.1;
                    hardness = 0;
                    break;
                case 8:
                    factor = 1.15;
                    durability = 1.1;
                    hardness = 0;
                    break;
                case 9:
                    factor = 1.15;
                    durability = 1.1;
                    hardness = 0;
                    break;
                case 10:
                    factor = 1.15;
                    durability = 1.1;
                    hardness = 1;
                    break;
                default:
                    factor = 1.00;
                    durability = 1.0;
                    hardness = 0;
                    break;
            }

            ForgeMultiplier forgeMultiplier = new ForgeMultiplier();
            forgeMultiplier.factor = factor;
            forgeMultiplier.durability = durability;
            forgeMultiplier.hardness = hardness;
            forgeMultiplier.weight = 100; //toDo
            return forgeMultiplier;
        }

        public void UpdateEnhancementLevel(ItemInstance itemInstance)
        {
            _itemDao.UpdateItemEnhancementLevel(itemInstance.instanceId, itemInstance.enhancementLevel);
        }

        //todo add checks
        public List<ItemInstance> Repair(List<ItemLocation> locations)
        {
            List<ItemInstance> itemInstances = new List<ItemInstance>();
            foreach (ItemLocation location in locations)
            {
                ItemInstance itemInstance = _character.itemLocationVerifier.GetItem(location);
                itemInstances.Add(itemInstance);
                _itemDao.UpdateItemCurrentDurability(itemInstance.instanceId, itemInstance.maximumDurability);
            }

            return itemInstances;
        }

        //TODO remove and move to utils
        public ulong SubtractGold(ulong amount)
        {
            _character.adventureBagGold -= amount;
            return _character.adventureBagGold;
        }

        public ulong AddGold(ulong amount)
        {
            _character.adventureBagGold += amount;
            return _character.adventureBagGold;
        }

        /// <summary>
        ///     This may seem insane but the client requires every auction house listing, this dumps it into the client.
        /// </summary>
        /// <returns>Every single auction house listing.</returns>
        public List<ItemInstance> GetItemsUpForAuction()
        {
            List<ItemInstance> auctions = _itemDao.SelectAuctions((uint)_character.soulId); //TODO make all ids unsigned
            _character.auctionSearchIds = new ulong[auctions.Count];
            short i = 0;
            foreach (ItemInstance itemInstance in auctions)
            {
                _character.auctionSearchIds[i] = itemInstance.instanceId;
                itemInstance.location = new ItemLocation(ItemZoneType.ProbablyAuctionSearch, 0, i);
                i++;
            }

            return auctions;
        }


        //TODO ADD LOCKS ON ALL AUCTION WHEN THESE ARE NOT ALL RUN IN THE SAME THREAD
        public void Bid(byte isBuyout, int slot, ulong bid)
        {
            ulong instanceId = _character.auctionSearchIds[slot];
            ulong buyoutPrice = _itemDao.SelectBuyoutPrice(instanceId);
            bool isAlreadyBought = _itemDao.SelectAuctionWinnerSoulId(instanceId) != 0;

            if (isAlreadyBought) throw new AuctionException(AuctionExceptionType.BiddingCompleted);
            if (isBuyout == 1 && bid != buyoutPrice) throw new AuctionException(AuctionExceptionType.Generic);
            if (isBuyout == 0 && bid == buyoutPrice) throw new AuctionException(AuctionExceptionType.Generic);
            if (bid > buyoutPrice) throw new AuctionException(AuctionExceptionType.Generic);


            _itemDao.InsertAuctionBid(instanceId, _character.soulId, bid);
            if (isBuyout == 1) _itemDao.UpdateAuctionWinner(instanceId, _character.soulId);
        }

        //auction functions
        public MoveResult Exhibit(ItemLocation itemLocation, byte exhibitSlot, byte quantity, int auctionTimeSelector, ulong minBid, ulong buyoutPrice, string comment)
        {
            const int MAX_LOTS = 15; //this is with dimento TODO update
            const double LISTING_FEE_PERCENT = .05;

            ItemInstance fromItem = _character.itemLocationVerifier.GetItem(itemLocation);
            ItemLocation exhibitLocation = new ItemLocation(ItemZoneType.ProbablyAuctionLots, 0, exhibitSlot);
            bool hasToItem = _character.itemLocationVerifier.HasItem(exhibitLocation);
            ulong listingFee = (ulong) Math.Ceiling(buyoutPrice * LISTING_FEE_PERCENT);

            //check possible errors. these should only occur if client is compromised or players are attempting to cheat
            //TODO probably log some of these errors 

            //verify the function is not called outside of the auction window open
            if (_character.isAuctionWindowOpen == false)
                throw new AuctionException("Auction window is not open.", AuctionExceptionType.Generic);

            //verify there is an item to auction
            if (fromItem is null || quantity == 0)
                throw new AuctionException("No item to auction.", AuctionExceptionType.Generic);

            //verify the items are in the players inventory and owned by the player
            if (fromItem.location.zoneType != ItemZoneType.AdventureBag
                && fromItem.location.zoneType != ItemZoneType.EquippedBags
                && fromItem.location.zoneType != ItemZoneType.PremiumBag
                && fromItem.location.zoneType != ItemZoneType.AvatarInventory
                || fromItem.ownerId != _character.id)
                throw new AuctionException("Not an owned item.", AuctionExceptionType.InvalidListing);

            //verify the character has enough gold to list the item
            if (_character.adventureBagGold < listingFee)
                throw new AuctionException("Not enough gold to list.", AuctionExceptionType.Generic);

            //verify there is no item already in that slot
            if (hasToItem)
                throw new AuctionException("There is already an item in that slot.", AuctionExceptionType.SlotUnavailable);

            //verify that the exhibit slot is valid
            if (exhibitSlot < 0 || exhibitSlot >= MAX_LOTS)
                throw new AuctionException("Outside the bounds of exhibit slots.", AuctionExceptionType.SlotUnavailable);

            //verify the item is not equipped
            if (_character.equippedItems.ContainsValue(fromItem))
                throw new AuctionException("Equipped items cannot be listed.", AuctionExceptionType.EquipListing);

            //verify the item is valid to list
            if (!fromItem.isIdentified || !fromItem.isSellable || !fromItem.isTradeable)
                throw new AuctionException("Invalid listing.", AuctionExceptionType.InvalidListing);

            //verify that the item has enough quantity
            if (quantity > fromItem.quantity)
                throw new AuctionException("Invalid Quantity ", AuctionExceptionType.IllegalItemAmount);

            //verify valid duration
            if (auctionTimeSelector < 0 || auctionTimeSelector > 3)
                throw new AuctionException("Invalid duration.", AuctionExceptionType.Generic);

            //subtract gold and update
            _character.adventureBagGold -= listingFee;

            MoveResult moveResult = new MoveResult();
            if (quantity == fromItem.quantity)
                moveResult = MoveItemPlace(exhibitLocation, fromItem);
            else if (quantity < fromItem.quantity) moveResult = MoveItemPlaceQuantity(exhibitLocation, fromItem, quantity);

            moveResult.destItem.consignerSoulName = _character.name;
            moveResult.destItem.minimumBid = minBid;
            moveResult.destItem.buyoutPrice = buyoutPrice;
            moveResult.destItem.comment = comment;

            int auctionTimeInSecondsFromNow = 0;
            const int SECONDS_PER_FOUR_HOURS = 60 * 60 * 4;
            switch (auctionTimeSelector) //TODO something not working
            {
                case 0: // 4 hours
                    auctionTimeInSecondsFromNow = SECONDS_PER_FOUR_HOURS;
                    break;
                case 1: // 8 hours
                    auctionTimeInSecondsFromNow = SECONDS_PER_FOUR_HOURS * 2;
                    break;
                case 2: // 12 hours
                    auctionTimeInSecondsFromNow = SECONDS_PER_FOUR_HOURS * 3;
                    break;
                case 3: // 24 hours
                    auctionTimeInSecondsFromNow = SECONDS_PER_FOUR_HOURS * 6;
                    break;
            }

            moveResult.destItem.secondsUntilExpiryTime = auctionTimeInSecondsFromNow;

            _itemDao.UpdateAuctionExhibit(moveResult.destItem);
            return moveResult;
        }

        public MoveResult CancelExhibit(byte slot)
        {
            //TODO don't allow cancelling auctions with bids
            ItemLocation itemLocation = new ItemLocation(ItemZoneType.ProbablyAuctionLots, 0, slot);
            ItemInstance fromItem = _character.itemLocationVerifier.GetItem(itemLocation);
            ItemLocation nextOpenSlot = _character.itemLocationVerifier.NextOpenSlotInInventory();

            //check possible errors. these should only occur if client is compromised
            if (fromItem is null || nextOpenSlot.Equals(ItemLocation.InvalidLocation)) throw new AuctionException(AuctionExceptionType.Generic);
            if (fromItem.bidderSoulId > 0) throw new AuctionException(AuctionExceptionType.BiddingCompleted);

            MoveResult moveResult = MoveItemPlace(nextOpenSlot, fromItem);

            _itemDao.UpdateAuctionCancelExhibit(fromItem.instanceId);

            return moveResult;
        }

        public void CancelBid(byte slot)
        {
            //_itemDao.DeleteAuctionBid(_character.SoulId, instanceId);
        }

        public List<ItemInstance> GetBids()
        {
            //TODO modify their location to be bids
            return _itemDao.SelectBids(_character.id);
        }

        public List<ItemInstance> GetLots()
        {
            List<ItemInstance> itemInstances = _itemDao.SelectLots(_character.id);
            foreach (ItemInstance item in itemInstances) _character.itemLocationVerifier.PutItem(item.location, item);
            return _itemDao.SelectLots(_character.id);
        } 

        public void PutEquipmentSearchConditions(AuctionSearchConditions auctionEquipmentSearchConditions)
        {
            throw new NotImplementedException();
        }

        public class MoveResult
        {
            public MoveResult()
            {
            }

            public MoveResult(MoveType moveType)
            {
                type = moveType;
            }

            /// <summary>
            ///     The type of move that is done. Determines which server responses to send back.
            /// </summary>
            public MoveType type { get; internal set; } = MoveType.None;

            /// <summary>
            ///     The item that is at the location moved from. Can be null if there is no item swapped.
            /// </summary>
            public ItemInstance originItem { get; internal set; }

            /// <summary>
            ///     The item that is at the destination. Will not be null unless an error occurs.
            /// </summary>
            public ItemInstance destItem { get; internal set; }
        }
    }
}
