using Arrowgene.Buffers;
using Necromancy.Server.Common;
using Necromancy.Server.Model;
using Necromancy.Server.Packet.Id;

namespace Necromancy.Server.Packet.Area
{
    public class SendAuctionClose : ClientHandler
    {
        public SendAuctionClose(NecServer server) : base(server)
        {
        }


        public override ushort id => (ushort)AreaPacketId.send_auction_close;

        public override void Handle(NecClient client, NecPacket packet)
        {
            IBuffer res = BufferProvider.Provide();
            router.Send(client.map, (ushort)AreaPacketId.recv_auction_notify_close, res, ServerType.Area, client);

            res.WriteInt32(0);
            router.Send(client, (ushort)AreaPacketId.recv_auction_close_r, res, ServerType.Area);

            res = BufferProvider.Provide();
            res.WriteByte(0);
            router.Send(client, (ushort)AreaPacketId.recv_event_end, res, ServerType.Area);


        }
    }
}
