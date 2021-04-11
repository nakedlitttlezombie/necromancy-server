using Arrowgene.Buffers;
using Arrowgene.Logging;
using Necromancy.Server.Common;
using Necromancy.Server.Logging;
using Necromancy.Server.Model;
using Necromancy.Server.Packet.Id;

namespace Necromancy.Server.Packet.Area
{
    public class send_event_removetrap_skill : ClientHandler
    {
        private static readonly NecLogger Logger =
            LogProvider.Logger<NecLogger>(typeof(send_event_removetrap_skill));

        public send_event_removetrap_skill(NecServer server) : base(server)
        {
        }


        public override ushort Id => (ushort) AreaPacketId.send_event_removetrap_skill;

        public override void Handle(NecClient client, NecPacket packet)
        {
            int disarmingSkill = packet.Data.ReadInt32();
            Logger.Debug($"Packet Contents as a Int32 : {disarmingSkill}");

            IBuffer res = BufferProvider.Provide();
            res.WriteInt32(0); //Error check - 0 for success
            res.WriteFloat(
                22); //Re-cast time in seconds.  To-Do.  Database lookup Skill_Base.CSV   Select where Column A = disarmingSkill, return Column J (cooldown time).
            //Router.Send(client.Map, (ushort)AreaPacketId.recv_event_removetrap_skill_r2, res, ServerType.Area);
            Router.Send(client, (ushort) AreaPacketId.recv_event_removetrap_skill_r2, res, ServerType.Area);
        }
    }
}
