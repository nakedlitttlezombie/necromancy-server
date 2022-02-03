using Arrowgene.Buffers;
using Necromancy.Server.Common;
using Necromancy.Server.Model;
using Necromancy.Server.Packet.Id;

namespace Necromancy.Server.Packet.Receive.Msg
{
    public class RecvCharaSelectBack : PacketResponse
    {
        private int _result;
        public RecvCharaSelectBack(int result)
            : base((ushort)MsgPacketId.recv_chara_select_back_r, ServerType.Msg)
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
