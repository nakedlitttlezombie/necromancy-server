using Arrowgene.Buffers;
using Necromancy.Server.Common;
using Necromancy.Server.Model;
using Necromancy.Server.Packet.Id;

namespace Necromancy.Server.Packet.Receive.Area
{
    public class RecvLogoutStart : PacketResponse
    {
        private int _result;
        public RecvLogoutStart(int result)
            : base((ushort)AreaPacketId.recv_logout_start, ServerType.Area)
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
