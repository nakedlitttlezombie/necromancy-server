using System;
using System.Collections.Generic;
using Necromancy.Server.Model;
using Necromancy.Server.Systems.Item;

namespace Necromancy.Server.Systems.Auction
{
    public class AuctionService
    {
        private const int SECONDS_PER_FOUR_HOURS = 60 * 60 * 4;
        private const int MAX_LOTS = 15;
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
            return _auctionDao.SelectAuctionSearchConditions(_character.id, false);
        }

        public void RegistSearchCond(int index, AuctionSearchConditions searchCond)
        {
            _auctionDao.InsertAuctionSearchConditions(_character.id, index, searchCond);
        }

        public void DeregistSearchCond(byte index, bool isItemSearch)
        {
            _auctionDao.DeleteAuctionSearchConditions(_character.id, index, isItemSearch);
        }

        public List<AuctionSearchConditions> GetItemSearchConditions()
        {
            return _auctionDao.SelectAuctionSearchConditions(_character.id, true);
        }

        public void VerifyListing(ItemLocation itemLocation, byte exhibitSlot, byte quantity, int auctionTimeSelector, ulong minBid, ulong buyoutPrice)
        {
            ItemInstance fromItem = _character.itemLocationVerifier.GetItem(itemLocation);
            ItemLocation exhibitLocation = new ItemLocation(ItemZoneType.ProbablyAuctionLots, 0, exhibitSlot);

            //TODO probably log some of these errors
            if(_character.isAuctionWindowOpen == false) throw new AuctionException(AuctionExceptionType.Generic);
            if (fromItem == null) throw new AuctionException(AuctionExceptionType.Generic);
            if (fromItem.location.zoneType != ItemZoneType.AdventureBag
                || fromItem.location.zoneType != ItemZoneType.EquippedBags
                || fromItem.location.zoneType != ItemZoneType.PremiumBag
                || fromItem.location.zoneType != ItemZoneType.AvatarInventory) throw new AuctionException(AuctionExceptionType.Generic);
            if (_character.adventureBagGold < Math.Ceiling(buyoutPrice * LISTING_FEE_PERCENT)) throw new AuctionException(AuctionExceptionType.Generic);
            if (_character.itemLocationVerifier.HasItem(exhibitLocation)) throw new AuctionException(AuctionExceptionType.SlotUnavailable);
            if (exhibitSlot < 0 || exhibitSlot >= MAX_LOTS) throw new AuctionException(AuctionExceptionType.SlotUnavailable);
            if (_character.equippedItems.ContainsValue(fromItem)) throw new AuctionException(AuctionExceptionType.EquipListing);
            if (!fromItem.isIdentified || !fromItem.isSellable || !fromItem.isTradeable) throw new AuctionException(AuctionExceptionType.InvalidListing);
            if (fromItem is null || quantity == 0) throw new AuctionException(AuctionExceptionType.Generic);
            if (quantity > fromItem.quantity) throw new AuctionException(AuctionExceptionType.IllegalItemAmount);
            if (auctionTimeSelector < 0 || auctionTimeSelector > 4) throw new AuctionException(AuctionExceptionType.Generic);

        }        

        //public void Bid(AuctionLot auctionItem, int bid)
        //{
        //    auctionItem.BidderId = _client.Character.Id;
        //    auctionItem.CurrentBid = bid;

        //    AuctionLot currentItem = _auctionDao.SelectItem(auctionItem.Id);

        //    if (currentItem.Id == ITEM_NOT_FOUND_ID) throw new AuctionException(AuctionExceptionType.Generic);

        //    if (auctionItem.CurrentBid < currentItem.CurrentBid) throw new AuctionException(AuctionExceptionType.NewBidLowerThanPrev);

        //    AuctionLot[] bids = _auctionDao.SelectBids(_client.Character);
        //    if(bids.Length >= MAX_BIDS) throw new AuctionException(AuctionExceptionType.BidSlotsFull);

        //    //TODO ADD CHECK FOR DIMENTO MEDAL / ROYAL after 5 bids
        //    if (false) throw new AuctionException(AuctionExceptionType.BidDimentoMedalExpired);

        //    int currentWealth = _auctionDao.SelectGold(_client.Character);
        //    if(currentWealth < bid) throw new AuctionException(AuctionExceptionType.Generic);

        //    _auctionDao.SubtractGold(_client.Character, bid);
        //    _auctionDao.UpdateBid(auctionItem);
        //}

        //public void CancelBid(AuctionLot auctionItem)
        //{
        //    AuctionLot currentItem = _auctionDao.SelectItem(auctionItem.Id);
        //    if (currentItem.SecondsUntilExpiryTime < SECONDS_IN_AN_HOUR) throw new AuctionException(AuctionExceptionType.NoCancelExpiry);
        //    if (!currentItem.IsCancellable) throw new AuctionException(AuctionExceptionType.AnotherCharacterAlreadyBid);

        //    _auctionDao.AddGold(_client.Character, currentItem.CurrentBid);

        //    auctionItem.BidderId = 0;
        //    auctionItem.CurrentBid = 0;
        //    _auctionDao.UpdateBid(auctionItem);
        //}

        //public void CancelExhibit(AuctionLot auctionItem)
        //{
        //    throw new NotImplementedException();
        //}

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
