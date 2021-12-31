using System;
using System.Collections.Generic;
using Necromancy.Server.Model;
using Necromancy.Server.Systems.Item;

namespace Necromancy.Server.Systems.Auction
{
    public class AuctionService
    {
        private const int SECONDS_PER_FOUR_HOURS = 60 * 60 * 4;
        private const int MAX_LOTS = 15; //this is with dimento TODO update
        private const double LISTING_FEE_PERCENT = .05;

        private readonly Character _character;
        private readonly IAuctionDao _auctionDao;

        public AuctionService(Character character) {
            _auctionDao = new AuctionDao();
            _character = character;
        }

        public AuctionService(Character character, IAuctionDao auctionDao)
        {
            _auctionDao = auctionDao;
            _character = character;
        }
        public List<AuctionSearchConditions> GetEquipSearchConditions()
        {
            return _auctionDao.SelectSearchConditions(_character.id, false);
        }

        public void RegistSearchCond(int index, AuctionSearchConditions searchCond)
        {
            _auctionDao.InsertSearchConditions(_character.id, index, searchCond);
        }

        public void DeregistSearchCond(byte index, bool isItemSearch)
        {
            _auctionDao.DeleteSearchConditions(_character.id, index, isItemSearch);
        }

        public List<AuctionSearchConditions> GetItemSearchConditions()
        {
            return _auctionDao.SelectSearchConditions(_character.id, true);
        }

        public void ValidateExhibit(ItemLocation itemLocation, ItemLocation exhibitLocation, byte quantity, int auctionTimeSelector, ulong minBid, ulong buyoutPrice)
        {
            ItemInstance fromItem = _character.itemLocationVerifier.GetItem(itemLocation);
            bool hasToItem = _character.itemLocationVerifier.HasItem(exhibitLocation);

            //check possible errors. these should only occur if client is compromised or players are attempting to cheat

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
                || fromItem.ownerId != _character.id)
                throw new AuctionException("Not an owned item.", AuctionExceptionType.InvalidListing);

            //verify the character has enough gold to list the item
            if (_character.adventureBagGold < calcListingFee(buyoutPrice))
                throw new AuctionException("Not enough gold to list.", AuctionExceptionType.Generic);

            //verify there is no item already in that slot
            if (hasToItem)
                throw new AuctionException("There is already an item in that slot.", AuctionExceptionType.SlotUnavailable);

            //verify that the exhibit slot is valid
            if (exhibitLocation.slot < 0 || exhibitLocation.slot >= MAX_LOTS)
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

            //verify minimum bid is less than buyout
            if (minBid > buyoutPrice)
                throw new AuctionException("Minimum bid is higher than buyout price.", AuctionExceptionType.Generic);

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

        public void ValidateCancelExhibit(ItemLocation exhibitLocation)
        {
            ItemInstance fromItem = _character.itemLocationVerifier.GetItem(exhibitLocation);
            ItemLocation nextOpenLocation = _character.itemLocationVerifier.NextOpenSlotInInventory();
            bool hasToItem = _character.itemLocationVerifier.HasItem(exhibitLocation);

            //check possible errors. these should only occur if client is compromised or players are attempting to cheat

            //verify the function is not called outside of the auction window open
            if (_character.isAuctionWindowOpen == false)
                throw new AuctionException("Auction window is not open.", AuctionExceptionType.Generic);

            //verify there is an item to auction
            if (fromItem is null || fromItem.quantity == 0)
                throw new AuctionException("No item to auction.", AuctionExceptionType.Generic);

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

        public void ValidateBid(byte isBuyout, int slot, ulong bid)
        {
            //TODO update more
            ulong instanceId = _character.auctionSearchIds[slot];
            ulong buyoutPrice = _auctionDao.SelectBuyoutPrice(instanceId);
            bool isAlreadyBought = _auctionDao.SelectWinnerSoulId(instanceId) != 0;

            //verify the function is not called outside of the auction window open
            if (_character.isAuctionWindowOpen == false)
                throw new AuctionException("Auction window is not open.", AuctionExceptionType.Generic);

            //verify the character has enough gold to list the item
            if (_character.adventureBagGold < bid)
                throw new AuctionException("Not enough gold to list.", AuctionExceptionType.Generic);

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


        public void ValidateCancelBid(byte isBuyout, int slot, ulong bid)
        {
                        

            //verify the function is not called outside of the auction window open
            if (_character.isAuctionWindowOpen == false)
                throw new AuctionException("Auction window is not open.", AuctionExceptionType.Generic);

            
            
        }

        public void CancelBid(byte slot)
        {
            //_itemDao.DeleteAuctionBid(_character.SoulId, instanceId);
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
