using Arrowgene.Buffers;
using Arrowgene.Logging;
using Necromancy.Server.Common;
using Necromancy.Server.Logging;
using Necromancy.Server.Model;
using Necromancy.Server.Packet.Id;
using Necromancy.Server.Packet.Receive.Area;

namespace Necromancy.Server.Packet.Area
{
    public class send_movement_info : ClientHandler
    {
        private static readonly NecLogger Logger = LogProvider.Logger<NecLogger>(typeof(send_movement_info));

        public send_movement_info(NecServer server) : base(server)
        {
        }

        public override ushort Id => (ushort) AreaPacketId.send_movement_info;


        public override void Handle(NecClient client, NecPacket packet)
        {
            // If changing maps don't update position
            if (client.Character.mapChange)
            {
                return;
            }

            client.Character.X = packet.Data.ReadFloat();
            client.Character.Y = packet.Data.ReadFloat();
            client.Character.Z = packet.Data.ReadFloat();

            float percentMovementIsX = packet.Data.ReadFloat();
            float percentMovementIsY = packet.Data.ReadFloat();
            float verticalMovementSpeedMultiplier =
                packet.Data.ReadFloat(); //  Confirm by climbing ladder at 1 up or -1 down. or Jumping

            float movementSpeed = packet.Data.ReadFloat();

            float horizontalMovementSpeedMultiplier =
                packet.Data
                    .ReadFloat(); //always 1 when moving.  Confirm by Coliding with an  object and watching it Dip.

            client.Character.movementPose =
                packet.Data.ReadByte(); //Character Movement Type: Type 8 Falling / Jumping. Type 3 normal:  Type 9  climbing

            client.Character.movementAnim = packet.Data.ReadByte(); //Action Modifier Byte
            //146 :ladder left Foot Up.      //147 Ladder right Foot Up. 
            //151 Left Foot Down,            //150 Right Root Down .. //155 falling off ladder
            //81  jumping up,                //84  jumping down       //85 landing


            //Battle Logic until we find out how to write battle byte requrements in 'send_data_get_Self_chara_data_request' so the client can send the right info
            if (client.Character.battleAnim != 0)
            {
                client.Character.movementPose = 8 /*client.Character.battlePose*/
                    ; //Setting the pose byte to the 2nd and 3rd digits of our equipped weapon ID. For battle!!
                client.Character.movementAnim =
                    client.Character
                        .battleAnim; //Setting the animation byte to an animation from C:\WO\Chara\chara\00\041\anim. 231, 232, 233, and 244 are attack animations
            }


            IBuffer res2 = BufferProvider.Provide();

            res2.WriteUInt32(client.Character.movementId); //Character ID
            res2.WriteFloat(client.Character.X);
            res2.WriteFloat(client.Character.Y);
            res2.WriteFloat(client.Character.Z);

            res2.WriteFloat(percentMovementIsX);
            res2.WriteFloat(percentMovementIsY);
            res2.WriteFloat(verticalMovementSpeedMultiplier);

            res2.WriteFloat(movementSpeed);

            res2.WriteFloat(horizontalMovementSpeedMultiplier);

            res2.WriteByte(client.Character.movementPose); //MOVEMENT ANIM
            res2.WriteByte(client.Character.movementAnim); //JUMP & FALLING ANIM

            Router.Send(client.Map, (ushort) AreaPacketId.recv_0x8D92, res2, ServerType.Area, client);

            client.Character.battleAnim =
                0; //re-setting the byte to 0 at the end of every iteration to allow for normal movements.
            if (client.Character.castingSkill)
            {
                RecvSkillCastCancel cancelCast = new RecvSkillCastCancel();
                Router.Send(client.Map, cancelCast.ToPacket());
                client.Character.activeSkillInstance = 0;
                client.Character.castingSkill = false;
            }


            //Uncomment for debugging movement. causes heavy console output. recommend commenting out "Packet" method in NecLogger.CS when debugging movement
            /*
            if (movementSpeed != 0)
            {
                Logger.Debug($"Character {client.Character.Name} is in map {client.Character.MapId} @ : X[{client.Character.X}]Y[{client.Character.Y}]Z[{client.Character.Z}]");
                Logger.Debug($"X Axis Aligned : {percentMovementIsX.ToString("P", CultureInfo.InvariantCulture)} | Y Axis Aligned  : {percentMovementIsY.ToString("P", CultureInfo.InvariantCulture)}");
                Logger.Debug($"vertical Speed multi : {verticalMovementSpeedMultiplier}| Move Speed {movementSpeed} | Horizontal Speed Multi {horizontalMovementSpeedMultiplier}");
                Logger.Debug($"Movement Type[{client.Character.movementPose}]  Type Anim [{client.Character.movementAnim}] View Offset:{client.Character.Heading}");
                Logger.Debug($"---------------------------------------------------------------");

                //Logger.Debug($"Var 1 {(byte)(percentMovementIsX*255)} |Var 2 {(byte)(percentMovementIsY*255)}  ");
                //Logger.Debug($" Y to Map Y[{d}][{e}][{e1}][{f} |  and {percentMovementIsY}");
            }
            else
            {
                Logger.Debug($"Movement Stop Reset");
                Logger.Debug($"---------------------------------------------------------------");
            }
            */


            ///////////
            /////////-----ToDO:  Find a home for the commands below this line as solutions develop.  Do not Delete!
            ///////////


            if (client.Character.takeover == true)
            {
                Logger.Debug($"Moving object ID {client.Character.eventSelectReadyCode}.");
                IBuffer res = BufferProvider.Provide();
                IBuffer res3 = BufferProvider.Provide();


                res.WriteUInt32(client.Character.eventSelectReadyCode);
                res.WriteFloat(client.Character.X);
                res.WriteFloat(client.Character.Y);
                res.WriteFloat(client.Character.Z);
                res.WriteByte(client.Character.Heading); //Heading
                res.WriteByte(client.Character.movementAnim); //state

                Router.Send(client.Map, (ushort) AreaPacketId.recv_object_point_move_notify, res, ServerType.Area);
                Router.Send(client, (ushort) AreaPacketId.recv_object_point_move_r, res3, ServerType.Area);
            }

            //CheckMapChange(client);
        }

        private void CheckMapChange(NecClient client)
        {
            MapPosition mapPos = new MapPosition();
            switch (client.Character.MapId)
            {
                case 1001001:
                    if ((client.Character.X < 4842.5 && client.Character.X > 4282) && client.Character.Y > 4448)
                    {
                        Map map = Server.Maps.Get(1001004);
                        mapPos.X = 1;
                        mapPos.Y = 1;
                        mapPos.Z = 1;
                        mapPos.Heading = 0;
                        map.EnterForce(client, mapPos);
                    }
                    else if ((client.Character.X < 225 && client.Character.X > 50) && client.Character.Y > 10200)
                    {
                        Map map = Server.Maps.Get(1001007);
                        mapPos.X = -5622;
                        mapPos.Y = -5874;
                        mapPos.Z = 1;
                        mapPos.Heading = 93;
                        map.EnterForce(client, mapPos);
                    }
                    else if (client.Character.X > 6800 && (client.Character.Y > 945 && client.Character.Y < 1723))
                    {
                        Map map = Server.Maps.Get(1001902);
                        mapPos.X = 22697;
                        mapPos.Y = -180;
                        mapPos.Z = 5;
                        mapPos.Heading = 132;
                        map.EnterForce(client, mapPos);
                    }

                    break;
                case 1001002:
                case 1001902:
                    if (client.Character.X < 21797 && (client.Character.Y > -755 && client.Character.Y < 485))
                    {
                        Map map = Server.Maps.Get(1001001);
                        mapPos.X = 6700;
                        mapPos.Y = 1452;
                        mapPos.Z = -3;
                        mapPos.Heading = 51;
                        map.EnterForce(client, mapPos);
                    }
                    else if ((client.Character.X > 36246 && client.Character.X < 37254) && client.Character.Y > 5313)
                    {
                        Map map = Server.Maps.Get(1001003);
                        mapPos.X = 3701;
                        mapPos.Y = -7057;
                        mapPos.Z = 5;
                        mapPos.Heading = 0;
                        map.EnterForce(client, mapPos);
                    }

                    break;
                case 1001003:
                    if ((client.Character.X < 3926 && client.Character.X > 3518) && client.Character.Y < -7511)
                    {
                        Map map = Server.Maps.Get(1001902);
                        mapPos.X = 36638;
                        mapPos.Y = 5216;
                        mapPos.Z = -10;
                        mapPos.Heading = 87;
                        map.EnterForce(client, mapPos);
                    }

                    break;
                case 1001004:
                    if ((client.Character.X < 1046 && client.Character.X > -1062) && client.Character.Y > 5300)
                    {
                        Map map = Server.Maps.Get(1001009);
                        mapPos.X = -410;
                        mapPos.Y = -859;
                        mapPos.Z = 68;
                        mapPos.Heading = 0;
                        map.EnterForce(client, mapPos);
                    }
                    else if (client.Character.X < -413 && (client.Character.Y > -712 && client.Character.Y < -345))
                    {
                        Map map = Server.Maps.Get(1001001);
                        mapPos.X = 4243;
                        mapPos.Y = 4492;
                        mapPos.Z = 405;
                        mapPos.Heading = 67;
                        map.EnterForce(client, mapPos);
                    }

                    break;
                case 1001005:
                    break;
                case 1001006:
                    break;
                case 1001007:
                    if ((client.Character.X < -5400 && client.Character.X > -5845) && client.Character.Y < -6288)
                    {
                        Map map = Server.Maps.Get(1001001);
                        mapPos.X = 159;
                        mapPos.Y = 9952;
                        mapPos.Z = 601;
                        mapPos.Heading = 46;
                        map.EnterForce(client, mapPos);
                    }

                    break;
                case 1001008:
                    break;
                case 1001009:
                    break;
                default:
                    break;
            }
        }
    }
}
