using Arrowgene.Buffers;
using Necromancy.Server.Common;
using Necromancy.Server.Model;
using Necromancy.Server.Packet.Id;

namespace Necromancy.Server.Packet.Area
{
    public class SendUnionStorageDepositMoney : ClientHandler
    {
        public SendUnionStorageDepositMoney(NecServer server) : base(server)
        {
        }


        public override ushort id => (ushort)AreaPacketId.send_union_storage_deposit_money;

        public override void Handle(NecClient client, NecPacket packet)
        {
            byte unknown = packet.data.ReadByte();
            ulong depositeGold = packet.data.ReadUInt64();

            IBuffer res = BufferProvider.Provide();
            res.WriteInt32(0); // 0 to work
            router.Send(client, (ushort)AreaPacketId.recv_union_storage_deposit_money_r, res, ServerType.Area);

            //To-Do,  make a variable to track union gold //server.database.UpdateUnionGold(+=depositGold);
            client.character.adventureBagGold -= depositeGold; //Updates your Character.AdventureBagGold
            client.soul.warehouseGold += depositeGold; //Updates your Soul.warehouseGold
            server.database.UpdateCharacter(client.character);
            server.database.UpdateSoulGold(client.soul);



            res = BufferProvider.Provide();
            res.WriteUInt64(client.character.adventureBagGold); // Sets your Adventure Bag Gold
            router.Send(client, (ushort)AreaPacketId.recv_self_money_notify, res, ServerType.Area);

            res = BufferProvider.Provide();
            res.WriteByte(unknown);
            res.WriteUInt64(client.soul.warehouseGold /*client.Union.GeneralSafeGold*/);
            router.Send(client.union.unionMembers, (ushort)AreaPacketId.recv_event_union_storage_update_money, res, ServerType.Area);
        }
    }
}
