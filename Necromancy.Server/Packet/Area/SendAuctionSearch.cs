using Arrowgene.Logging;
using Necromancy.Server.Logging;
using Necromancy.Server.Model;
using Necromancy.Server.Packet.Id;

namespace Necromancy.Server.Packet.Area
{
    public class SendAuctionSearch : ClientHandler
    {
        public SendAuctionSearch(NecServer server) : base(server) {  }
        public override ushort id => (ushort)AreaPacketId.send_auction_search;
        public override void Handle(NecClient client, NecPacket packet)
        {
            //TODO apparently unused, probably delete.
        }
    }
}
