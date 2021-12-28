using System;
using System.Collections.Generic;
using Necromancy.Server.Model;
using Necromancy.Server.Systems.Item;

namespace Necromancy.Server.Systems.Auction
{
    public class AuctionService
    {
        public const int MAX_BIDS = 8;
        public const int MAX_BIDS_NO_DIMENTO = 5;
        public const int MAX_LOTS = 5;
        public const int MAX_LOTS_NO_DIMENTO = 3;
        public const int SECONDS_IN_AN_HOUR = 60 * 60;
        public const int MAX_SEARCH_RESULTS = 100;

        private const int ITEM_NOT_FOUND_ID = -1;

        private const double LISTING_FEE_PERCENT = .05;

        private readonly IAuctionDao _auctionDao;

        public AuctionService() {
            _auctionDao = new AuctionDao();
        }

        public AuctionService(IAuctionDao auctionDao)
        {
            _auctionDao = auctionDao;
        }
        public List<AuctionSearchConditions> GetEquipSearchConditions(NecClient client)
        {
            return _auctionDao.SelectAuctionSearchConditions(client.character.id, false);
        }

        public void RegistSearchCond(NecClient client,  int index, AuctionSearchConditions searchCond)
        {
            _auctionDao.InsertAuctionSearchConditions(client.character.id, index, searchCond);
        }

        public void DeregistSearchEquipmentCond(NecClient client, byte index)
        {
            _auctionDao.DeleteAuctionSearchConditions(client.character.id, index, false);
        }

        internal List<AuctionSearchConditions> GetItemSearchConditions(NecClient client)
        {
            return _auctionDao.SelectAuctionSearchConditions(client.character.id, true);
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
