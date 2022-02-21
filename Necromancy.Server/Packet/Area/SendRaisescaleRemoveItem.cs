using Arrowgene.Buffers;
using Necromancy.Server.Common;
using Necromancy.Server.Model;
using Necromancy.Server.Packet.Id;

namespace Necromancy.Server.Packet.Area
{
    public class SendRaisescaleRemoveItem : ClientHandler
    {
        public SendRaisescaleRemoveItem(NecServer server) : base(server)
        {
        }

        public override ushort id => (ushort)AreaPacketId.send_raisescale_remove_item;

        public override void Handle(NecClient client, NecPacket packet)
        {
            byte raiseScaleSlot = packet.data.ReadByte(); // 0 to 4

            IBuffer res = BufferProvider.Provide();
            res.WriteInt32(0); //Result. 0 for success
            router.Send(client, (ushort)AreaPacketId.recv_raisescale_remove_item_r, res, ServerType.Area);
        }
    }
}
