using Arrowgene.Buffers;
using Necromancy.Server.Common;
using Necromancy.Server.Model;
using Necromancy.Server.Packet.Id;

namespace Necromancy.Server.Packet.Receive.Custom
{
    public class InvalidStructure : PacketResponse
    {
        private int _result;
        public InvalidStructure(int result)
            : base((ushort)AreaPacketId.recv_escape_start, ServerType.Area)
        {
            _result = result;
        }

        protected override IBuffer ToBuffer()
        {
            IBuffer res = BufferProvider.Provide();
            //res.WriteInt32(_result);   commenting out this makes the structure invalid
            return res;
        }
    }
}
