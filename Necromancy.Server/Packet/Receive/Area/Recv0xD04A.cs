using Arrowgene.Buffers;
using Necromancy.Server.Common;
using Necromancy.Server.Model;
using Necromancy.Server.Packet.Id;

namespace Necromancy.Server.Packet.Receive.Area
{
    public class Recv0xD04A : PacketResponse
    {
        public Recv0xD04A()
            : base((ushort) AreaPacketId.recv_0xD04A, ServerType.Area)
        {
        }

        protected override IBuffer ToBuffer()
        {
            IBuffer res = BufferProvider.Provide();
            
            return res;
        }
    }
}
