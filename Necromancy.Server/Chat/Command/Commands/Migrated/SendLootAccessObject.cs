using System.Collections.Generic;
using Arrowgene.Buffers;
using Necromancy.Server.Common;
using Necromancy.Server.Model;
using Necromancy.Server.Packet.Id;

namespace Necromancy.Server.Chat.Command.Commands
{
    //recv loot access object?  doesnt do anything
    public class SendLootAccessObject : ServerChatCommand
    {
        public SendLootAccessObject(NecServer server) : base(server)
        {
        }

        public override void Execute(string[] command, NecClient client, ChatMessage message,
            List<ChatResponse> responses)
        {
            IBuffer res = BufferProvider.Provide();
            res.WriteInt64(10003);
            res.WriteByte(0);
            res.WriteByte(0);
            res.WriteInt16(2);            
            Router.Send(client, (ushort)AreaPacketId.recv_item_update_place, res, ServerType.Area);


            //IBuffer res = BufferProvider.Provide();
            //res.WriteInt32(-10);
            /*
                LOOT	-1	It is carrying nothing
                LOOT	-10	No one can be looted nearby
                LOOT	-207	No space available in inventory
                LOOT	-1500	No permission to loot
            */

            
        }

        public override AccountStateType AccountState => AccountStateType.User;
        public override string Key => "accs";
    }
}
