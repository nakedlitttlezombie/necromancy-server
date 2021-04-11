using Arrowgene.Buffers;
using Necromancy.Server.Common;
using Necromancy.Server.Model;
using Necromancy.Server.Packet.Id;

namespace Necromancy.Server.Packet.Area
{
    public class send_auction_receive_gold : ClientHandler
    {
        public send_auction_receive_gold(NecServer server) : base(server)
        {
        }


        public override ushort Id => (ushort) AreaPacketId.send_auction_receive_gold;

        public override void Handle(NecClient client, NecPacket packet)
        {
            IBuffer res = BufferProvider.Provide();
            res.WriteInt32(0); /*

            0. Work

            Error Codes :

            1 The item may be listed

            */
            Router.Send(client.Map, (ushort) AreaPacketId.recv_auction_receive_gold_r, res, ServerType.Area);
        }
    }
}
