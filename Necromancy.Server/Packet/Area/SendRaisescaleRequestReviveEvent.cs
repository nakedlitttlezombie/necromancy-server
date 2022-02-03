using System;
using System.Threading.Tasks;
using Necromancy.Server.Model;
using Necromancy.Server.Packet.Id;
using Necromancy.Server.Packet.Receive.Area;

namespace Necromancy.Server.Packet.Area
{
    public class SendRaisescaleRequestReviveEvent : ClientHandler
    {
        public SendRaisescaleRequestReviveEvent(NecServer server) : base(server)
        {
        }

        public override ushort id => (ushort)AreaPacketId.send_raisescale_request_revive_event;

        public override void Handle(NecClient client, NecPacket packet)
        {
            Task.Delay(TimeSpan.FromMilliseconds(8150)).ContinueWith
            (t1 =>
                {
                //if success
                RecvEventScriptPlay recvEventScriptPlay1 = new RecvEventScriptPlay("scale\revive_success", client.character.instanceId); //Plays the little re-annimation raise up and blue flash
                router.Send(recvEventScriptPlay1, client);
                //if fail
                RecvEventScriptPlay recvEventScriptPlay2 = new RecvEventScriptPlay("scale\revive_fail", client.character.instanceId); //moves the camera, but no animation or blue flash
                //router.Send(recvEventScriptPlay2, client);
                //if fail again. you're lost
                RecvEventScriptPlay recvEventScriptPlay3 = new RecvEventScriptPlay("scale\revive_lost", client.character.instanceId);
                //router.Send(recvEventScriptPlay3, client);
                }
            );

        }
    }
}
