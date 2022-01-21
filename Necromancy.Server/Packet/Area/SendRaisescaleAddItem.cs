using Arrowgene.Buffers;
using Necromancy.Server.Common;
using Necromancy.Server.Model;
using Necromancy.Server.Packet.Id;
using Necromancy.Server.Systems.Item;


namespace Necromancy.Server.Packet.Area
{
    public class SendRaisescaleAddItem : ClientHandler
    {
        public SendRaisescaleAddItem(NecServer server) : base(server)
        {
        }

        public override ushort id => (ushort)AreaPacketId.send_raisescale_add_item;

        public override void Handle(NecClient client, NecPacket packet)
        {
            ItemZoneType fromZone = (ItemZoneType)packet.data.ReadByte();
            byte fromContainer = packet.data.ReadByte();
            short fromSlot = packet.data.ReadInt16();
            byte raiseScaleSlot = packet.data.ReadByte();
            //ToDo -  Flag the item as on the raiseScale. Not sure why,  but these bytes can't be for nothing.


            IBuffer res = BufferProvider.Provide();
            res.WriteInt32(0); //Result
            router.Send(client, (ushort)AreaPacketId.recv_raisescale_add_item_r, res, ServerType.Area);
        }
    }
}
