using Arrowgene.Buffers;
using Necromancy.Server.Common;
using Necromancy.Server.Model;
using Necromancy.Server.Packet.Id;

namespace Necromancy.Server.Packet.Receive.Msg
{
    public class RecvCharaSelectBackSoulSelect : PacketResponse
    {
        private int _result;
        public RecvCharaSelectBackSoulSelect(int result)
            : base((ushort)MsgPacketId.recv_chara_select_back_soul_select_r, ServerType.Msg)
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
