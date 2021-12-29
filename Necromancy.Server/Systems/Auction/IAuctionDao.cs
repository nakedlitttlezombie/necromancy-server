using System;
using System.Collections.Generic;
using System.Text;
using Necromancy.Server.Systems.Item;

namespace Necromancy.Server.Systems.Auction
{
    public interface IAuctionDao {
        public void InsertSearchConditions(int characterId, int index, AuctionSearchConditions auctionSearchConditions);
        public void DeleteSearchConditions(int characterId, byte index, bool isItemSearchCond);
        List<AuctionSearchConditions> SelectSearchConditions(int characterId, bool isItemSearchCond);
        public void UpdateExhibit(ItemInstance itemInstance);
        public void UpdateCancelExhibit(ItemInstance exhibit);
    }
}
