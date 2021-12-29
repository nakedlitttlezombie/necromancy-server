using System;
using System.Collections.Generic;
using System.Text;

namespace Necromancy.Server.Systems.Item
{
    public class AuctionExhibit
    {
        public ulong itemInstanceId { get; set; }
        public string consignerSoulName { get; set; }
        public int secondsUntilExpiry { get; set; }
        public ulong minimumBid { get; set; }
        public ulong buyoutPrice { get; set; }
        public string comment { get; set; }

    }
}
