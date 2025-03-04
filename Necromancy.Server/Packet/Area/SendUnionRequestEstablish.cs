using System;
using Arrowgene.Buffers;
using Arrowgene.Logging;
using Necromancy.Server.Common;
using Necromancy.Server.Logging;
using Necromancy.Server.Model;
using Necromancy.Server.Model.Union;
using Necromancy.Server.Packet.Id;

namespace Necromancy.Server.Packet.Area
{
    public class SendUnionRequestEstablish : ClientHandler
    {
        private static readonly NecLogger _Logger = LogProvider.Logger<NecLogger>(typeof(SendUnionRequestEstablish));

        public SendUnionRequestEstablish(NecServer server) : base(server)
        {
        }


        public override ushort id => (ushort)AreaPacketId.send_union_request_establish;

        public override void Handle(NecClient client, NecPacket packet)
        {
            string unionName = packet.data.ReadCString(); //It's the Name of your new Union
            int sysMsg = 0;

            if (unionName.Length >= 16)
                sysMsg = -1;
            else if (client.character.adventureBagGold < 30000)
                sysMsg = -2;
            else if (client.soul.level < 3)
                sysMsg = -3;
            else if (client.character.unionId != 0) sysMsg = -4;
            /*
            else if (client.Soul.Level < 3) { sys_msg = -1711; }
            else if (unionNameRegExpressionCheck(unionName)== fail) { sys_msg = -1701; }
            else if (client.Character.unionId != 0) { sys_msg = -1702; }
            else if (Server.Database.SelectUnionByName(unionName) != null) { sys_msg = -1704; }
            else if (Server.Characters.GetAll().Count <= 5) { sys_msg = -1705; }
            else if (client.Character.UnionLastLeave <= Date.Time.Now() - 24) { sys_msg = -1719; }
             */

            IBuffer res2 = BufferProvider.Provide();
            res2.WriteInt32(sysMsg);
            router.Send(client, (ushort)AreaPacketId.recv_union_request_establish_r, res2, ServerType.Area);

            /*
            UNION_CREATE	0	Union created
            UNION_CREATE	-1	A union name must be 2 to 8 characters in length
            UNION_CREATE	-2	Not enough gold
            UNION_CREATE	-3	Insufficient Soul Rank
            UNION_CREATE	-4	You already belong to a union
            UNION_CREATE	-1711	Insufficient Soul Rank
            UNION_CREATE	-1701	The union name you have entered includes unavailable characters
            UNION_CREATE	-1702	You already belong to a union
            UNION_CREATE	-1704	This union name is already in use
            UNION_CREATE	-1705	Not enough people to create a union
            UNION_CREATE	-1719	You may not create a Union for 24 hours after leaving your last Union.
             */

            if (sysMsg != 0) return;

            Union myFirstUnion = new Union();
            server.instances.AssignInstance(myFirstUnion);
            client.character.unionId = (int)myFirstUnion.instanceId;
            myFirstUnion.name = unionName;
            myFirstUnion.leaderId = client.character.id;
            client.union = myFirstUnion;

            if (!server.database.InsertUnion(myFirstUnion))
            {
                _Logger.Error($"{unionName} could not be saved to database");
                _Logger.Error($"{unionName} could not be saved to database");
                _Logger.Error($"{unionName} could not be saved to database");
                return;
            }

            _Logger.Debug($"{unionName} established with Id {myFirstUnion.id} and instanceId {myFirstUnion.instanceId}");

            UnionMember myFirstUnionMember = new UnionMember();
            server.instances.AssignInstance(myFirstUnionMember);
            myFirstUnionMember.unionId = myFirstUnion.id;
            myFirstUnionMember.characterDatabaseId = client.character.id;
            myFirstUnionMember.memberPriviledgeBitMask = 0b11111111;
            myFirstUnionMember.rank = 0;

            if (!server.database.InsertUnionMember(myFirstUnionMember))
            {
                _Logger.Error("union member could not be saved to database table nec_union_member");
                return;
            }

            _Logger.Debug($"union member ID{myFirstUnionMember.id} added to nec_union_member table");

            myFirstUnion.Join(client); //to-do,  add to unionMembers table.
            TimeSpan differenceCreated = DateTime.Now - DateTime.UnixEpoch;
            int unionCreatedCalculation = (int)Math.Floor(differenceCreated.TotalSeconds);


            IBuffer res3 = BufferProvider.Provide();
            res3.WriteInt32(client.character.unionId); //Union ID
            res3.WriteUInt32(client.character.instanceId);
            res3.WriteFixedString($"{client.soul.name}", 0x31); //size is 0x31
            res3.WriteFixedString($"{client.character.name}", 0x5B); //size is 0x5B
            res3.WriteUInt32(client.character.classId);
            res3.WriteByte(client.character.level);
            res3.WriteInt32(client.character.mapId); // Location of your Union Member
            res3.WriteInt32(3); //??
            res3.WriteFixedString($"Channel {client.character.channel}", 0x61); // Channel location
            res3.WriteInt32(
                0b11111111); //permissions bitmask  obxxxx1 = invite | obxxx1x = kick | obxx1xx = News | 0bxx1xxxxx = General Storage | 0bx1xxxxxx = Deluxe Storage
            res3.WriteInt32(0); //Rank  3 = beginner 2 = member, 1 = sub-leader 0 = leader
            res3.WriteInt32(0); //online status. 0 = online, 1 = away, 2 = offline
            res3.WriteInt32(unionCreatedCalculation); //
            res3.WriteInt32(0); //
            res3.WriteInt32(0); //
            router.Send(client, (ushort)MsgPacketId.recv_union_notify_detail_member, res3, ServerType.Msg);

            //Notify client if msg server found Union settings in database(memory) for client character Unique Persistant ID.
            IBuffer res = BufferProvider.Provide();
            res.WriteInt32(client.character.unionId); //Union Instance ID
            res.WriteFixedString(unionName, 0x31); //size is 0x31
            res.WriteInt32(unionCreatedCalculation);
            res.WriteUInt32(client.character.instanceId); //Leader
            res.WriteInt32(-1);
            res.WriteInt32(0); //subleader1
            res.WriteInt32(-1);
            res.WriteInt32(0); //subleader2
            res.WriteInt32(-1);
            res.WriteByte(0); //Union Level
            res.WriteUInt32(myFirstUnion.currentExp); //Union EXP Current
            res.WriteUInt32(myFirstUnion.nextLevelExp); //Union EXP next level Target
            res.WriteByte(myFirstUnion
                .memberLimitIncrease); //Increase Union Member Limit above default 50 (See Union Bonuses
            res.WriteByte(3);
            res.WriteUInt32(client.character.instanceId);
            res.WriteInt16(myFirstUnion.capeDesignId); //Mantle/Cape design
            res.WriteFixedString($"You are all members of {unionName} now.  Welcome!", 0x196); //size is 0x196
            for (int i = 0; i < 8; i++)
                res.WriteInt32(i);
            res.WriteByte(0);

            router.Send(client, (ushort)MsgPacketId.recv_union_notify_detail, res, ServerType.Msg);

            IBuffer res36 = BufferProvider.Provide();
            res36.WriteUInt32(client.character.instanceId);
            res36.WriteInt32(client.character.unionId);
            res36.WriteCString(unionName);
            router.Send(client.map, (ushort)AreaPacketId.recv_chara_notify_union_data, res36, ServerType.Area);
        }
    }
}
