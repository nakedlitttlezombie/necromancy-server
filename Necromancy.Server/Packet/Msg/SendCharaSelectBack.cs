using Arrowgene.Buffers;
using Necromancy.Server.Common;
using Necromancy.Server.Model;
using Necromancy.Server.Packet.Id;

namespace Necromancy.Server.Packet.Msg
{
    public class SendCharaSelectBack : ClientHandler
    {
        public SendCharaSelectBack(NecServer server) : base(server)
        {
        }

        public override ushort id => (ushort)MsgPacketId.send_chara_select_back;


        public override void Handle(NecClient client, NecPacket packet)
        {
            IBuffer res = BufferProvider.Provide();
            res.WriteInt32(0);
            router.Send(client, (ushort)MsgPacketId.recv_chara_select_back_r, res, ServerType.Msg);
        }
    }
}
