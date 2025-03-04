using System;
using Arrowgene.Buffers;
using Arrowgene.Logging;
using Necromancy.Server.Common;
using Necromancy.Server.Logging;
using Necromancy.Server.Model;
using Necromancy.Server.Packet.Id;
using Necromancy.Server.Systems.Item;

/// <summary>
/// This receive loads your Character on the map.  dead or alive.
///
/// for deadbody stuff go to the CharaBodyData recv.
/// </summary>
namespace Necromancy.Server.Packet.Receive.Area
{
    public class RecvDataNotifyCharaData : PacketResponse
    {
        private static readonly NecLogger _Logger = LogProvider.Logger<NecLogger>(typeof(RecvDataNotifyCharaData));
        private readonly Character _character;
        private readonly ItemInstance[] _equippedItems;
        private readonly string _soulName;

        public RecvDataNotifyCharaData(Character character, string soulName)
            : base((ushort)AreaPacketId.recv_data_notify_chara_data, ServerType.Area)
        {
            _character = character;
            _soulName = soulName;
            _equippedItems = new ItemInstance[_character.equippedItems.Count];
            _character.equippedItems.Values.CopyTo(_equippedItems, 0);
        }

        protected override IBuffer ToBuffer()
        {
            TimeSpan differenceJoined = DateTime.Today.ToUniversalTime() - DateTime.UnixEpoch;
            int numEntries = _equippedItems.Length; //Max of 25 Equipment Slots for Character Player. must be 0x19 or less
            int numStatusEffects = _character.statusEffects.Length; /*_character.Statuses.Length*/ //0x80; //Statuses effects. Max 128
            int i = 0;
            if (_character.hasDied) numEntries = 0; //Dead mean wear no gear

            IBuffer res = BufferProvider.Provide();
            res.WriteUInt32(_character.instanceId);
            res.WriteCString(_soulName);
            res.WriteCString(_character.name);
            res.WriteFloat(_character.x);
            res.WriteFloat(_character.y);
            res.WriteFloat(_character.z);
            res.WriteByte(_character.heading);
            res.WriteInt32(_character.activeModel); // Character.ActiveModel  0 = default
            res.WriteInt16(_character.modelScale); //Character.Scale   100 = normal size.
            res.WriteUInt64((ulong)_character.stateFlags); //Character State
            res.WriteInt16(1); // current level ??

            //sub_483420
            res.WriteInt32(numEntries); // Number of equipment Slots
            //sub_483660
            for (i = 0; i < numEntries; i++) res.WriteInt32((int)_equippedItems[i].type);

            //sub_483420
            res.WriteInt32(numEntries); // Number of equipment Slots
            //sub_4948C0
            for (i = 0; i < numEntries; i++)
            {
                res.WriteInt32(_equippedItems[i].baseId); //Item Base Model ID
                res.WriteByte(00); //? TYPE data/chara/##/ 00 is character model, 01 is npc, 02 is monster
                res.WriteByte(0); //Race and gender tens place is race 1= human, 2= elf 3=dwarf 4=gnome 5=porkul, ones is gender 1 = male 2 = female
                res.WriteByte(0); //??item version

                res.WriteInt32(_equippedItems[i].baseId); //testing (Theory, texture file related)
                res.WriteByte(0); //hair
                res.WriteByte(1); //color
                res.WriteByte(0); //face

                res.WriteByte(45); // Hair style from  chara\00\041\000\model  45 = this file C:\WO\Chara\chara\00\041\000\model\CM_00_041_11_045.nif
                res.WriteByte((byte)(_character.faceId * 10)); //Face Style calls C:\Program Files (x86)\Steam\steamapps\common\Wizardry Online\data\chara\00\041\000\model\CM_00_041_10_010.nif.  must be 00 10, 20, 30, or 40 to work.
                res.WriteByte(00); // testing (Theory Torso Tex)
                res.WriteByte(0); // testing (Theory Pants Tex)
                res.WriteByte(0); // testing (Theory Hands Tex)
                res.WriteByte(0); // testing (Theory Feet Tex)
                res.WriteByte(0); //Alternate texture for item model  0 normal : 1 Pink

                res.WriteByte(0); // separate in assembly
                res.WriteByte(0); // separate in assembly
            }

            //sub_483420
            res.WriteInt32(numEntries); // Number of equipment Slots to display
            for (i = 0; i < numEntries; i++) res.WriteInt32((int)_equippedItems[i].currentEquipSlot); //bitmask per equipment slot

            //sub_4835C0
            res.WriteInt32(_character.charaPose); //1 here means crouching?
            //sub_484660
            res.WriteUInt32(_character.raceId); //race
            res.WriteUInt32(_character.sexId);
            res.WriteByte(_character.hairId); //hair
            res.WriteByte(_character.hairColorId); //color
            res.WriteByte(_character.faceId); //face
            res.WriteByte(_character.faceArrangeId); //face arrange
            res.WriteByte(_character.voiceId); //voice
            //weird 64 loop
            for (i = 0; i < 100; i++) res.WriteInt64(0);

            //sub_483420
            res.WriteUInt32(_character.partyId); // party id?
            //sub_4837C0
            res.WriteUInt32(0); // party id? // i don't think sooo'
            //sub_read_byte
            res.WriteByte(_character.criminalState); //Criminal name icon
            //sub_494890
            res.WriteByte((byte)_character.beginnerProtection); //Bool Beginner Protection
            //sub_4835E0
            res.WriteInt32(_character.movementPose); //pose, 1 = sitting, 0 = standing
            //sub_483920
            res.WriteInt32(0); //???
            //sub_491A00
            res.WriteByte(0); //newjp
            //sub_483440
            res.WriteInt16(_character.level); //Player level (stat gui)
            //sub_read_byte
            res.WriteByte(0); //no change?   MemberShip Status?
            //sub_read_byte
            res.WriteByte(0); //no change?
            //sub_read_int_32
            res.WriteInt32(90400101); //Title from Honor.csv   _character.Title
            //sub_483580
            res.WriteUInt32(_character.classId); //Signifies character class
            //sub_483420
            res.WriteInt32(numStatusEffects); //Number of Status Effects to display 128 Max

            //sub_485A70
            for (i = 0; i < numStatusEffects; i++)
            {
                res.WriteInt32(i); //instanceID or unique ID
                res.WriteUInt32(_character.statusEffects[i]); //Buff.SerialId from buff.csv
                res.WriteInt32(Util.GetRandomNumber(100, 6000)); //Time Remaining in seconds
                res.WriteInt32(1); //new
            }

            //sub_481AA0
            res.WriteCString("" /*_character.comment*/); //Comment string
            res.WriteInt32(0);
            return res;
        }
    }
}
