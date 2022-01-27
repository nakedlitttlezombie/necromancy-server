using Arrowgene.Buffers;
using Necromancy.Server.Common;
using Necromancy.Server.Model;
using Necromancy.Server.Packet.Id;

namespace Necromancy.Server.Packet.Receive.Custom
{
    public class InvalidStructure2 : PacketResponse
    {
        private int _result;
        public InvalidStructure2(int result)
            : base((ushort)AreaPacketId.recv_escape_start, ServerType.Area)
        {
            _result = result;
        }

        protected override IBuffer ToBuffer()
        {
            IBuffer res = BufferProvider.Provide();
            res.WriteInt32(_result); //this one sends tooo much  data for the structure.  that will crash/close the client
            res.WriteInt32(_result); 
            res.WriteInt32(_result); 
            return res;
        }
    }
}
