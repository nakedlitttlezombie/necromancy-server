using Arrowgene.Buffers;
using Necromancy.Server.Common;
using Necromancy.Server.Model;
using Necromancy.Server.Packet.Id;

namespace Necromancy.Server.Packet.Receive.Area
{
    public class RecvLogoutStartRequest : PacketResponse
    {
        private int _result;
        public RecvLogoutStartRequest(int result)
            : base((ushort)AreaPacketId.recv_logout_start_request_r, ServerType.Area)
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
