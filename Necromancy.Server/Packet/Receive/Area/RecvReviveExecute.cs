using Arrowgene.Buffers;
using Necromancy.Server.Common;
using Necromancy.Server.Model;
using Necromancy.Server.Packet.Id;

namespace Necromancy.Server.Packet.Receive.Area
{
    public class RecvReviveExecute : PacketResponse
    {
        private int _result;
        public RecvReviveExecute(int result)
            : base((ushort)AreaPacketId.recv_revive_execute_r, ServerType.Area)
        {
            _result = result;
        }

        protected override IBuffer ToBuffer()
        {
            IBuffer res = BufferProvider.Provide();
            res.WriteInt32(_result);
            return res;
        }
    }
}
