using Arrowgene.Buffers;
using Necromancy.Server.Common;
using Necromancy.Server.Model;
using Necromancy.Server.Packet.Id;

namespace Necromancy.Server.Packet.Receive.Area
{
    public class RecvBattleReportNoactNotifyHealLife : PacketResponse
    {
        private uint _instanceId;
        private int _isPlayEffect;
        public RecvBattleReportNoactNotifyHealLife(uint instanceId, int isPlayEffect)
            : base((ushort)AreaPacketId.recv_battle_report_noact_notify_heal_life, ServerType.Area)
        {
            _instanceId = instanceId;
            _isPlayEffect = isPlayEffect;
        }

        protected override IBuffer ToBuffer()
        {
            IBuffer res = BufferProvider.Provide();
            res.WriteUInt32(_instanceId); //object id
            res.WriteInt32(_isPlayEffect); //is play effect
            return res;
        }
    }
}
