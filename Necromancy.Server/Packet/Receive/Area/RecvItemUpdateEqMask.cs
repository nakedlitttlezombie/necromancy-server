using Arrowgene.Buffers;
using Necromancy.Server.Common;
using Necromancy.Server.Model;
using Necromancy.Server.Packet.Id;
using Necromancy.Server.Systems.Item;

namespace Necromancy.Server.Packet.Receive.Area
{
    public class RecvItemUpdateEqMask : PacketResponse
    {
        private readonly ItemInstance _itemInstance;

        public RecvItemUpdateEqMask(NecClient client, ItemInstance itemInstance)
            : base((ushort)AreaPacketId.recv_item_update_eqmask, ServerType.Area)
        {
            _itemInstance = itemInstance;
            clients.Add(client);
        }

        protected override IBuffer ToBuffer()
        {
            IBuffer res = BufferProvider.Provide();
            res.WriteUInt64(_itemInstance.instanceId);
            res.WriteInt32((int)_itemInstance.currentEquipSlot);

            res.WriteInt32(_itemInstance.baseId); //Item Base Model ID
            res.WriteByte(0); //Item Revision.  Calls .\data\item\105\model\EM_10500501_05.nif   (note the 05 at the end if set to 5)
            res.WriteByte(0);//
            res.WriteByte(0b11111110); //Re-render on equip.  0 yes, 1 no

            res.WriteInt32(int.MaxValue); //
            res.WriteByte(0); //
            res.WriteByte(0); //
            res.WriteByte(0); //

            res.WriteByte(45); // Hair style from  chara\00\041\000\model  45 = this file C:\WO\Chara\chara\00\041\000\model\CM_00_041_11_045.nif
            res.WriteByte(00); // Face Style calls C:\Program Files (x86)\Steam\steamapps\common\Wizardry Online\data\chara\00\041\000\model\CM_00_041_10_010.nif.  must be 00 10, 20, 30, or 40 to work.
            res.WriteByte(0); //
            res.WriteByte(0); //
            res.WriteByte(0); //
            res.WriteByte(0); // 
            res.WriteByte(0); //

            res.WriteByte(0); // Load alternate Texture 1 yes : 0 no
            res.WriteByte(0); // center digits of alt texture ..\WizardryOnlineJP\data\item\105\tex\ET_00693401C.nif  setting to 15 gets 34 in the path. 14 for 33, 13 for 32, etc
            //17=41, 18=42, 20 = 31, 19 = 32, 1 = 02, 2 = 03, 7 = 13, 3 = 04, 4 = 05, 5 = 11, 
            return res;
        }
    }
}
