using Arrowgene.Buffers;
using Necromancy.Server.Common;
using Necromancy.Server.Model;
using Necromancy.Server.Packet.Id;

namespace Necromancy.Server.Packet.Area
{
    public class send_party_cancel_member_recruit : ClientHandler
    {
        public send_party_cancel_member_recruit(NecServer server) : base(server)
        {
        }

        public override ushort Id => (ushort) AreaPacketId.send_party_cancel_member_recruit;

        public override void Handle(NecClient client, NecPacket packet)
        {
            IBuffer res = BufferProvider.Provide();

            res.WriteInt32(0); //Set to Party ID when we have Party IDs

            Router.Send(client, (ushort) AreaPacketId.recv_party_cancel_member_recruit_r, res, ServerType.Area);
        }
    }
}
