using Arrowgene.Buffers;
using Necromancy.Server.Common;
using Necromancy.Server.Model;
using Necromancy.Server.Packet.Id;
using Necromancy.Server.Packet.Receive.Area;

namespace Necromancy.Server.Packet.Area
{
    public class SendQuestGetRogueMissionQuestWorks : ClientHandler
    {
        public SendQuestGetRogueMissionQuestWorks(NecServer server) : base(server)
        {
        }

        public override ushort id => (ushort)AreaPacketId.send_quest_get_rogue_mission_quest_works;

        public override void Handle(NecClient client, NecPacket packet)
        {
            RecvQuestGetRogueMissionQuestHistoryR recvQuestGetRogueMissionQuestHistoryR = new RecvQuestGetRogueMissionQuestHistoryR();
            router.Send(client, recvQuestGetRogueMissionQuestHistoryR.ToPacket());
        }
    }
}
