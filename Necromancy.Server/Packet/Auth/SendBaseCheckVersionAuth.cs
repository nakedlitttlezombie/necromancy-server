using Arrowgene.Buffers;
using Arrowgene.Logging;
using Necromancy.Server.Common;
using Necromancy.Server.Logging;
using Necromancy.Server.Model;
using Necromancy.Server.Packet.Id;

namespace Necromancy.Server.Packet.Auth
{
    public class SendBaseCheckVersionAuth : ConnectionHandler
    {
        private static readonly NecLogger _Logger = LogProvider.Logger<NecLogger>(typeof(SendBaseCheckVersionAuth));

        public SendBaseCheckVersionAuth(NecServer server) : base(server)
        {
        }

        public override ushort id => (ushort)AuthPacketId.send_base_check_version;

        public override void Handle(NecConnection connection, NecPacket packet)
        {
            uint major = packet.data.ReadUInt32();
            uint minor = packet.data.ReadUInt32();
            _Logger.Info($"{major} - {minor}");

            IBuffer res = BufferProvider.Provide();
            res.WriteInt32(0);
            res.WriteUInt32(major);
            res.WriteUInt32(minor);

            router.Send(connection, (ushort)AuthPacketId.recv_base_check_version_r, res);
        }
    }
}
