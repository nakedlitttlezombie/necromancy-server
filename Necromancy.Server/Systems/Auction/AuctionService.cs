using System;
using System.Collections.Generic;
using Arrowgene.Logging;
using Necromancy.Server.Logging;
using Necromancy.Server.Model;
using Necromancy.Server.Systems.Item;

namespace Necromancy.Server.Systems.Auction
{
    public sealed class AuctionService
    {
        private const int SECONDS_PER_FOUR_HOURS = 60 * 60 * 4;
        private const int MAX_LOTS = 15; //this is with dimento TODO update
        private const double LISTING_FEE_PERCENT = .05;

        private readonly IAuctionDao _auctionDao;
        private readonly List<NecClient> _clientsInAuction = new List<NecClient>();

#pragma warning disable IDE1006 // Naming Styles
        private static readonly NecLogger _logger = LogProvider.Logger<NecLogger>(typeof(AuctionService));
        public static AuctionService Instance { get; } = new AuctionService();
#pragma warning restore IDE1006 // Naming Styles
        static AuctionService() { }
        private AuctionService()
        {
            _auctionDao = new AuctionDao();
        }

        public void AddClientInAuction(NecClient client)
        {
            lock (this) {
                _clientsInAuction.Add(client);
            }            
        }

        public void RemoveClientInAuction(NecClient client)
        {
            lock (this)
            {
                _clientsInAuction.Remove(client);
            }
        }

        public List<AuctionSearchConditions> GetEquipSearchConditions(NecClient client)
        {
            lock (this) {
                ValidateClientInAuction(client);
                return _auctionDao.SelectSearchConditions(client.soul.id, false);
            }
        }

        public void RegistSearchCond(NecClient client, int index, AuctionSearchConditions searchCond)
        {
            lock (this)
            {
                ValidateClientInAuction(client);
                _auctionDao.InsertSearchConditions(client.soul.id, index, searchCond);
            }
        }

        public void DeregistSearchCond(NecClient client, byte index, bool isItemSearch)
        {
            lock (this)
            {
                ValidateClientInAuction(client);
                _auctionDao.DeleteSearchConditions(client.soul.id, index, isItemSearch);
            }
        }

        public List<AuctionSearchConditions> GetItemSearchConditions(NecClient client)
        {
            lock (this)
            {
                ValidateClientInAuction(client);
                return _auctionDao.SelectSearchConditions(client.soul.id, true);
            }
        }

        public void ValidateExhibit(NecClient client, ItemLocation itemLocation, ItemLocation exhibitLocation, byte quantity, int auctionTimeSelector, ulong minBid, ulong buyoutPrice)
        {
            //check possible errors. these should only occur if client is compromised or players are attempting to cheat

            ValidateClientInAuction(client);
            ValidateItemExists(client, itemLocation);
            ValidateQuantity(client, itemLocation, quantity);
            ValidateExhibitSlotRange(exhibitLocation);
            ValidateItemAuctionable(client, itemLocation);
            ValidateMinBidAndBuyout(minBid, buyoutPrice);
            ValidateItemOwnedByCharacter(client, itemLocation);
            ValidateEnoughGold(client, calcListingFee(buyoutPrice));
            ValidateTimeSelector(auctionTimeSelector);

            //verify there is no item already in that slot
            if (hasToItem)
                throw new AuctionException("There is already an item in that slot.", AuctionExceptionType.SlotUnavailable);

            //verify the item is not equipped
            if (_character.equippedItems.ContainsValue(fromItem))
                throw new AuctionException("Equipped items cannot be listed.", AuctionExceptionType.EquipListing);

            

            //TODO CHECK DIMENTO
        }

        public void Exhibit(ItemLocation itemLocation, ItemLocation exhibitLocation, byte quantity, int auctionTimeSelector, ulong minBid, ulong buyoutPrice, string comment)
        {
            //subtract gold and update
            _character.adventureBagGold -= calcListingFee(buyoutPrice);

            ItemInstance fromItem = _character.itemLocationVerifier.GetItem(itemLocation);
            fromItem.consignerSoulName = _character.name;
            fromItem.minimumBid = minBid;
            fromItem.buyoutPrice = buyoutPrice;
            fromItem.comment = comment;

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

            fromItem.secondsUntilExpiryTime = auctionTimeInSecondsFromNow;

            _auctionDao.UpdateExhibit(fromItem);
        }

        private ulong calcListingFee(ulong buyoutPrice)
        {
            return (ulong) Math.Ceiling(buyoutPrice * LISTING_FEE_PERCENT);
        }

        public void ValidateCancelExhibit(NecClient client, ItemLocation exhibitLocation)
        {
            ItemInstance fromItem = _character.itemLocationVerifier.GetItem(exhibitLocation);
            ItemLocation nextOpenLocation = _character.itemLocationVerifier.NextOpenSlotInInventory();
            bool hasToItem = _character.itemLocationVerifier.HasItem(exhibitLocation);

            //check possible errors. these should only occur if client is compromised or players are attempting to cheat

            ValidateClientInAuction(client);
            ValidateItemExists(client, exhibitLocation);

            //verify there is space in player's inventory
            if (nextOpenLocation.Equals(ItemLocation.InvalidLocation))
                throw new AuctionException("No free space in inventory", AuctionExceptionType.Generic);

            //unsure, bidding is completed?
            if (fromItem.bidderSoulId > 0) throw new AuctionException("Bidding already complete.", AuctionExceptionType.BiddingCompleted);

            //TODO don't allow cancelling auctions with bids
        }

        public void CancelExhibit(ItemLocation exhibitLocation)
        {            
            ItemInstance exhibit = _character.itemLocationVerifier.GetItem(exhibitLocation);
            _auctionDao.UpdateCancelExhibit(exhibit);
        }

        public void ValidateBid(NecClient client, byte isBuyout, int slot, ulong bid)
        {
            //TODO update more
            ulong instanceId = _character.auctionSearchIds[slot];
            ulong buyoutPrice = _auctionDao.SelectBuyoutPrice(instanceId);
            bool isAlreadyBought = _auctionDao.SelectWinnerSoulId(instanceId) != 0;

            _logger.Debug(instanceId.ToString());

            ValidateClientInAuction(client);
            ValidateEnoughGold(client, bid);

            if (isAlreadyBought) throw new AuctionException(AuctionExceptionType.BiddingCompleted);
            if (isBuyout == 1 && bid != buyoutPrice) throw new AuctionException(AuctionExceptionType.Generic);
            if (isBuyout == 0 && bid == buyoutPrice) throw new AuctionException(AuctionExceptionType.Generic);
            if (bid > buyoutPrice) throw new AuctionException(AuctionExceptionType.Generic);
            //TODO add errors that are actually listed in auction error
        }

        //TODO ADD LOCKS ON ALL AUCTION WHEN THESE ARE NOT ALL RUN IN THE SAME THREAD maybe not needed since it dosn't matter? most people arent bidding, they are buying
        public void Bid(byte isBuyout, int slot, ulong bid)
        {
            //subtract gold and update
            _character.adventureBagGold -= bid;
            ulong instanceId = _character.auctionSearchIds[slot];
            _auctionDao.InsertBid(instanceId, _character.soulId, bid); // should be soulid?
            if (isBuyout == 1) _auctionDao.UpdateWinnerSoulId(instanceId, _character.soulId);
        }


        public void ValidateCancelBid(NecClient client, byte isBuyout, int slot, ulong bid)
        {
            ValidateClientInAuction(client);
            //TODO     
            
        }

        public void CancelBid(byte slot)
        {
            //_itemDao.DeleteAuctionBid(_character.SoulId, instanceId);
        }

        /// <summary>
        /// Verify the client has the auction window open.
        /// </summary>
        /// <param name="client"></param>
        private void ValidateClientInAuction(NecClient client)
        {
            if (!_clientsInAuction.Contains(client))
                throw new AuctionException("Auction window is not open.", AuctionExceptionType.Generic);
        }

        /// <summary>
        /// Verify there is an item to auction.
        /// </summary>
        /// <param name="client"></param>
        /// <param name="itemLocation"></param>
        private void ValidateItemExists(NecClient client, ItemLocation itemLocation)
        {
            ItemInstance itemInstance = client.character.itemLocationVerifier.GetItem(itemLocation);
            if (itemInstance is null || itemInstance.quantity == 0)
                throw new AuctionException("No item to auction.", AuctionExceptionType.Generic);
        }

        /// <summary>
        /// Verify there is enough of the item at the location specified.
        /// </summary>
        /// <param name="client"></param>
        /// <param name="itemLocation"></param>
        /// <param name="quantity"></param>
        private void ValidateQuantity(NecClient client, ItemLocation itemLocation, byte quantity)
        {
            ItemInstance itemInstance = client.character.itemLocationVerifier.GetItem(itemLocation);
            if (quantity > itemInstance.quantity)
                throw new AuctionException("Invalid Quantity.", AuctionExceptionType.IllegalItemAmount);
        }

        /// <summary>
        /// Verify that the exhibit slot is within the valid range.
        /// </summary>
        /// <param name="client"></param>
        /// <param name="itemLocation"></param>
        private void ValidateExhibitSlotRange(ItemLocation itemLocation)
        {
            if (itemLocation.slot < 0 || itemLocation.slot >= MAX_LOTS) //TODO check dimento
                throw new AuctionException("Outside the bounds of exhibit slots.", AuctionExceptionType.SlotUnavailable);
        }


        /// <summary>
        /// Verify the the item can be auctioned.
        /// </summary>
        /// <param name="client"></param>
        /// <param name="itemLocation"></param>
        private void ValidateItemAuctionable(NecClient client, ItemLocation itemLocation)
        {
            ItemInstance itemInstance = client.character.itemLocationVerifier.GetItem(itemLocation);
            if (!itemInstance.isIdentified || !itemInstance.isSellable || !itemInstance.isTradeable) //TODO maybe max durability?
                throw new AuctionException("Invalid listing.", AuctionExceptionType.InvalidListing);
        }

        /// <summary>
        /// Verify that the minimum bid is less than the buyout price.
        /// </summary>
        /// <param name="minBid"></param>
        /// <param name="buyoutPrice"></param>
        private void ValidateMinBidAndBuyout(ulong minBid, ulong buyoutPrice)
        {
            if (minBid > buyoutPrice)
                throw new AuctionException("Minimum bid is higher than buyout price.", AuctionExceptionType.Generic);
        }

        /// <summary>
        /// Verify character has enough gold.
        /// </summary>
        /// <param name="client"></param>
        /// <param name="gold"></param>
        private void ValidateEnoughGold(NecClient client, ulong gold)
        {
            if (client.character.adventureBagGold < gold)
                throw new AuctionException("Not enough gold.", AuctionExceptionType.Generic);
        }

        /// <summary>
        /// Verify the character owns and possesses the item.
        /// </summary>
        /// <param name="client"></param>
        /// <param name="itemLocation"></param>
        private void ValidateItemOwnedByCharacter(NecClient client, ItemLocation itemLocation)
        {
            ItemInstance itemInstance = client.character.itemLocationVerifier.GetItem(itemLocation);
            if (itemInstance.location.zoneType != ItemZoneType.AdventureBag
                && itemInstance.location.zoneType != ItemZoneType.EquippedBags
                && itemInstance.location.zoneType != ItemZoneType.PremiumBag
                || itemInstance.ownerId != client.character.id)
                throw new AuctionException("Not an owned item.", AuctionExceptionType.InvalidListing);
        }

        /// <summary>
        /// Verify that the time selector is within reason.
        /// </summary>
        /// <param name="auctionTimeSelector"></param>
        private void ValidateTimeSelector(int auctionTimeSelector)
        {            
            if (auctionTimeSelector < 0 || auctionTimeSelector > 3)
                throw new AuctionException("Invalid duration.", AuctionExceptionType.Generic);
        }

        


        //public void Close(AuctionLot auctionItem)
        //{
        //    throw new NotImplementedException();
        //}

        //public void ReExhibit(AuctionLot auctionItem)
        //{
        //    throw new NotImplementedException();
        //}
    }
}
