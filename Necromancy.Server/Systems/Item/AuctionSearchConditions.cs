namespace Necromancy.Server.Systems.Item
{
    public class AuctionSearchConditions
    {
        public const int MAX_SEARCH_TEXT_LENGTH = 73;
        public const int MAX_DESCRIPTION_LENGTH = 193;
        public bool isItemSearch        { get; set; } = false;
        public string searchText        { get; set; } = "";
        public byte levelMin            { get; set; } = 0;
        public byte levelMax            { get; set; } = 99;
        public byte gradeMin            { get; set; } = 0;
        public byte gradeMax            { get; set; } = 99;
        public short qualities          { get; set; } = 0;
        public int classIndex           { get; set; } = 0;
        public short raceIndex          { get; set; } = 0;
        public ulong goldCost           { get; set; } = 0;
        public byte isLessThanGoldCost  { get; set; } = 0;
        public byte hasGemSlot          { get; set; } = 0;
        public byte gemSlotType1        { get; set; } = 0;
        public byte gemSlotType2        { get; set; } = 0;
        public byte gemSlotType3        { get; set; } = 0;
        public long typeSearchMask0     { get; set; } = 0;
        public long typeSearchMask1     { get; set; } = 1;
        public string description       { get; set; } = "";
        public byte unknownByte0        { get; set; } = 0; //seems to be 0?
        public byte unknownByte1        { get; set; } = 99; //seems to be 99?
    }
}
