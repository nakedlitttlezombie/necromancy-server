using System;
using System.Collections.Generic;
using System.Text;
using Necromancy.Server.Systems.Item;

namespace Necromancy.Server.Systems.Auction
{
    public interface IAuctionDao {
        public void InsertAuctionSearchConditions(int characterId, int index, AuctionSearchConditions auctionSearchConditions);
        void DeleteAuctionSearchConditions(int characterId, byte index, bool isItemSearchCond);
        List<AuctionSearchConditions> SelectAuctionSearchConditions(int characterId, bool isItemSearchCond);
    }
}
