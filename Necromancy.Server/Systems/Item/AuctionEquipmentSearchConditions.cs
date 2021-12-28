namespace Necromancy.Server.Systems.Item
{
    public class AuctionEquipmentSearchConditions
    {
        public const int MAX_TEXT_LENGTH = 73;
        public const int MAX_DESCRIPTION_LENGTH = 193;
        private const int MIN_SOUL_RANK = 0;
        private const int MAX_SOUL_RANK = 99;
        private const int MIN_FORGE_PRICE = 0;
        private const int MAX_FORGE_PRICE = 99;

        public string searchText { get; set; } = "";
        public byte soulRankMin { get; set; }
        public byte soulRankMax { get; set; }
        public byte forgePriceMin { get; set; }
        public byte forgePriceMax { get; set; }
        public ItemQualities qualities { get; set; }
        public int classIndex { get; set; }
        public short raceIndex { get; set; }
        public ulong goldCost { get; set; }
        public bool isLessThanGoldCost { get; set; }
        public bool hasGemSlot { get; set; }
        public GemType gemSlotType1 { get; set; }
        public GemType gemSlotType2 { get; set; }
        public GemType gemSlotType3 { get; set; }
        public long itemTypeSearchMask { get; set; }
        public ulong unknownLong0 { get; set; } //seems to be 1024? I have no idea why its set at that.
        public string description { get; set; } = "";
        public byte unknownByte0 { get; set; } //seems to be 0?
        public byte unknownByte1 { get; set; } //seems to be 99?

        private bool HasValidText()
        {
            return searchText.Length <= MAX_TEXT_LENGTH;
        }
        private bool HasValidQuality()
        {
            return (qualities & ItemQualities.All) == qualities;
        }
        private bool HasValidSoulRankMin()
        {
            return soulRankMin >= MIN_SOUL_RANK && soulRankMin <= MAX_SOUL_RANK;
        }
        private bool HasValidSoulRankMax()
        {
            return soulRankMax >= MIN_SOUL_RANK && soulRankMax <= MAX_SOUL_RANK;
        }
        private bool HasValidForgePriceMin()
        {
            return forgePriceMin >= MIN_FORGE_PRICE && forgePriceMin <= MAX_FORGE_PRICE;
        }
        private bool HasValidForgePriceMax()
        {
            return forgePriceMax >= MIN_FORGE_PRICE && forgePriceMax <= MAX_FORGE_PRICE;
        }
        public bool IsValid()
        {
            return HasValidText() && HasValidQuality() && HasValidSoulRankMin() && HasValidSoulRankMax() && HasValidForgePriceMin() && HasValidForgePriceMax();
        }
    }
}
