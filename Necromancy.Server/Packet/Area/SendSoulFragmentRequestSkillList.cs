using Arrowgene.Buffers;
using Necromancy.Server.Common;
using Necromancy.Server.Model;
using Necromancy.Server.Packet.Id;
using Necromancy.Server.Packet.Receive.Area;

namespace Necromancy.Server.Packet.Area
{
    public class SendSoulFragmentRequestSkillList : ClientHandler
    {
        public SendSoulFragmentRequestSkillList(NecServer server) : base(server)
        {
        }


        public override ushort id => (ushort)AreaPacketId.send_soul_fragment_request_skill_list;

        public override void Handle(NecClient client, NecPacket packet)
        {
            //recv_soul_fragment_set_skill_r
            //recv_soul_fragment_request_skill_list_r

            RecvSoulFragmentRequestSkillListR recvSoulFragmentRequestSkillListR = new RecvSoulFragmentRequestSkillListR();
            router.Send(client, recvSoulFragmentRequestSkillListR.ToPacket());
        }
    }
}
