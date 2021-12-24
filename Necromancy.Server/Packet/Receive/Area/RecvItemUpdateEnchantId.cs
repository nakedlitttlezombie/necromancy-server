using Arrowgene.Buffers;
using Necromancy.Server.Common;
using Necromancy.Server.Model;
using Necromancy.Server.Packet.Id;
using Necromancy.Server.Systems.Item;

namespace Necromancy.Server.Packet.Receive.Area
{
    public class RecvItemUpdateEnchantId : PacketResponse
    {
        private readonly NecClient _client;
        private readonly ItemInstance _itemInstance;
        public RecvItemUpdateEnchantId(NecClient client, ItemInstance itemInstance)
            : base((ushort) AreaPacketId.recv_item_update_enchantid, ServerType.Area)
        {
            _itemInstance = itemInstance;
            _client = client;
            clients.Add(_client);
        }

    protected override IBuffer ToBuffer()
        {
            IBuffer res = BufferProvider.Provide();
            res.WriteInt64((long)_itemInstance.instanceId);
            res.WriteInt32(8);
            return res;
        }
    }
}
