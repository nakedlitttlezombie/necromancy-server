using System;
using System.Collections.Generic;
using System.Text;
using Necromancy.Server.Systems.Item;

namespace Necromancy.Server.Systems.Auction
{
    public interface IAuctionDao    {

        public void InsertAuctionEquipSearchConditions(int characterId, int index, AuctionEquipmentSearchConditions auctionEquipmentSearchConditions);
    }
}
