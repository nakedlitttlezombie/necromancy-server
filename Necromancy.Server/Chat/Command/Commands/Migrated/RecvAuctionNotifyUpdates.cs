using System;
using System.Collections.Generic;
using System.Text;
using Arrowgene.Buffers;
using Arrowgene.Logging;
using Necromancy.Server.Common;
using Necromancy.Server.Logging;
using Necromancy.Server.Model;
using Necromancy.Server.Packet.Id;

namespace Necromancy.Server.Chat.Command.Commands.Migrated
{
    public class RecvAuctionNotifyUpdates : ServerChatCommand
    {

        private static readonly NecLogger _Logger = LogProvider.Logger<NecLogger>(typeof(RecvAuctionNotifyUpdates));
        public override AccountStateType accountState => AccountStateType.Admin;

        public override string key => "auctnotify";

        public RecvAuctionNotifyUpdates(NecServer server) : base(server)
        {
        }

        public override void Execute(string[] command, NecClient client, ChatMessage message, List<ChatResponse> responses)
        {
            IBuffer res = BufferProvider.Provide();
            res.WriteInt32(0);
            res.WriteInt32(60);
            router.Send(client.map, (ushort) AreaPacketId.recv_auction_update_bid_num, res, ServerType.Area);
        }
    }
}
