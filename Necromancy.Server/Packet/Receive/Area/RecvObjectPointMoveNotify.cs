using Arrowgene.Buffers;
using Necromancy.Server.Common;
using Necromancy.Server.Model;
using Necromancy.Server.Packet.Id;

namespace Necromancy.Server.Packet.Receive.Area
{
    public class RecvObjectPointMoveNotify : PacketResponse
    {
        private uint _instanceId;
        private float _x;
        private float _y;
        private float _z;
        private byte _heading;
        private byte _pose;
        public RecvObjectPointMoveNotify(uint instanceId, float x, float y, float z, byte heading, byte pose)
            : base((ushort)AreaPacketId.recv_object_point_move_notify, ServerType.Area)
        {
            _instanceId = instanceId;
            _x = x;
            _y = y;
            _z = z;
            _heading = heading;
            _pose = pose;
        }

        protected override IBuffer ToBuffer()
        {
            IBuffer res = BufferProvider.Provide();
            res.WriteUInt32(_instanceId);
            res.WriteFloat(_x); //x
            res.WriteFloat(_y); //y
            res.WriteFloat(_z); //z
            res.WriteByte(_heading); //view offset
            res.WriteByte(_pose);
            return res;
        }
    }
}
