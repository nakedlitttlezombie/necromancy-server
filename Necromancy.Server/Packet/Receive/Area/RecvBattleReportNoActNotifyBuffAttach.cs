using Arrowgene.Buffers;
using Necromancy.Server.Common;
using Necromancy.Server.Model;
using Necromancy.Server.Packet.Id;

namespace Necromancy.Server.Packet.Receive.Area
{
    public class RecvBattleReportNoActNotifyBuffAttach : PacketResponse
    {
        public RecvBattleReportNoActNotifyBuffAttach()
            : base((ushort) AreaPacketId.recv_battle_report_noact_notify_buff_attach, ServerType.Area)
        {
        }

        protected override IBuffer ToBuffer()
        {
            IBuffer res = BufferProvider.Provide();
            res.WriteInt32(0);
            res.WriteInt32(0);
            res.WriteInt32(0);
            res.WriteInt32(0);
            return res;
        }
    }
}
