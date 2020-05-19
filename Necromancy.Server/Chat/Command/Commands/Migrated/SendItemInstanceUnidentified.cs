using System.Collections.Generic;
using Arrowgene.Buffers;
using Necromancy.Server.Common;
using Necromancy.Server.Model;
using Necromancy.Server.Packet.Id;

namespace Necromancy.Server.Chat.Command.Commands
{
    //adds an unidentified item to inventory
    public class SendItemInstanceUnidentified : ServerChatCommand
    {
        public SendItemInstanceUnidentified(NecServer server) : base(server)
        {
        }

        public override void Execute(string[] command, NecClient client, ChatMessage message,
            List<ChatResponse> responses)
        {
            //recv_item_instance_unidentified = 0xD57A,

            IBuffer res = BufferProvider.Provide();

            res.WriteInt64(10200101); //Item Object ID 

            res.WriteCString("10200701"); //Name

            res.WriteInt32(11); //Wep type

            res.WriteInt32(1); //Bit mask designation? (Only lets you apply this to certain slots dependant on what you send here) 1 for right hand, 2 for left hand

            res.WriteByte(1); //Number of items

            res.WriteInt32(0); //Item status 0 = identified  (same as item status inside senditeminstance)

            res.WriteInt32(10200701); //Item icon
            res.WriteByte(0);
            res.WriteByte(0);
            res.WriteByte(0);
            res.WriteInt32(10200701);
            res.WriteByte(0);
            res.WriteByte(0);
            res.WriteByte(0);

            res.WriteByte(0);
            res.WriteByte(0);
            res.WriteByte(0); // bool
            res.WriteByte(0);
            res.WriteByte(0);
            res.WriteByte(0);
            res.WriteByte(0);
            res.WriteByte(0);

            res.WriteByte(0); // 0 = adventure bag. 1 = character equipment
            res.WriteByte(0); // 0~2
            res.WriteInt16(3); // bag index

            res.WriteInt32(0); //bit mask. This indicates where to put items.   e.g. 01 head 010 arm 0100 feet etc (0 for not equipped)

            res.WriteInt64(10200701);

            res.WriteInt32(10200701);

            Router.Send(client, (ushort) AreaPacketId.recv_item_instance_unidentified, res, ServerType.Area);

            IBuffer res30 = BufferProvider.Provide();
            res30.WriteInt64(10200101);
            res30.WriteInt32(100); // MaxDura points
            Router.Send(client, (ushort) AreaPacketId.recv_item_update_maxdur, res30, ServerType.Area);

            //recv_item_update_durability = 0x1F5A, 
            IBuffer res31 = BufferProvider.Provide();
            res31.WriteInt64(10200101);
            res31.WriteInt32(10);
            Router.Send(client, (ushort) AreaPacketId.recv_item_update_durability, res31, ServerType.Area);
        }

        public override AccountStateType AccountState => AccountStateType.User;
        public override string Key => "itus";
    }
}
