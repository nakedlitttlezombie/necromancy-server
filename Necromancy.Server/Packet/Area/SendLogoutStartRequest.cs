using System;
using System.Threading;
using System.Threading.Tasks;
using Arrowgene.Buffers;
using Necromancy.Server.Common;
using Necromancy.Server.Model;
using Necromancy.Server.Packet.Id;
using Necromancy.Server.Packet.Receive.Area;
using Necromancy.Server.Packet.Receive.Msg;

namespace Necromancy.Server.Packet.Area
{
    public class SendLogoutStartRequest : ClientHandler
    {
        public SendLogoutStartRequest(NecServer server) : base(server)
        {
        }

        public override ushort id => (ushort)AreaPacketId.send_logout_start_request;

        public override void Handle(NecClient client, NecPacket packet)
        {
            byte returnToCharacterSelect = packet.data.ReadByte();
            byte returnToSoulSelecct = packet.data.ReadByte();

            client.logoutCancelationCheck = false;
            int logoutCountDownInSeconds = 5;

            DateTime logoutTime = DateTime.Now.AddSeconds(logoutCountDownInSeconds);
            //client.character.characterTask.Logout(logoutTime, logOutType); //this would handle logout through character task LogOutRequest

            IBuffer res = BufferProvider.Provide();
            res.WriteInt32(logoutCountDownInSeconds); //time in seconds to display 
            router.Send(client, (ushort)AreaPacketId.recv_logout_start, res, ServerType.Area);

            Task.Delay(TimeSpan.FromSeconds(logoutCountDownInSeconds)).ContinueWith
            (t1 =>
                {
                    if (client.logoutCancelationCheck == false) //if client did not cancel the login, continue to the choices.  
                    {
                        if (returnToCharacterSelect == 1) //return to character select.
                        {
                            RecvCharaSelectBack recvCharaSelectBack = new RecvCharaSelectBack(1);
                            router.Send(client, recvCharaSelectBack.ToPacket());   //crashes if int32 = 0.  _________maybe check assembly to see why_______________

                            //Thread.Sleep(3000); //needed to delay firing of Disappear.  change to Task.Delay after core logic is established
                            RecvObjectDisappearNotify recvObjectDisappearNotify = new RecvObjectDisappearNotify(client.character.instanceId);
                            //router.Send(client, recvObjectDisappearNotify.ToPacket());
                              
                            RecvBaseExit recvBaseExit = new RecvBaseExit();
                            router.Send(client, recvBaseExit.ToPacket());
                        }
                        else if (returnToSoulSelecct == 1) // Return to soul Select
                        {
                            RecvBaseExit recvBaseExit = new RecvBaseExit();
                            router.Send(client, recvBaseExit.ToPacket());

                            RecvCharaSelectBackSoulSelect recvCharaSelectBackSoulSelect = new RecvCharaSelectBackSoulSelect(0);
                            router.Send(client, recvCharaSelectBackSoulSelect.ToPacket());

                            Thread.Sleep(3000); //needed to delay firing of Disappear.  change to Task.Delay after core logic is established
                            RecvObjectDisappearNotify recvObjectDisappearNotify = new RecvObjectDisappearNotify(client.character.instanceId);
                            router.Send(client, recvObjectDisappearNotify.ToPacket());
                        }
                        else  // Return to Title   also   Exit Game
                        {
                            RecvEscapeStart recvEscapeStart = new RecvEscapeStart(15);
                            router.Send(client, recvEscapeStart.ToPacket());
                            RecvEscapeExec recvEscapeExec = new RecvEscapeExec(15);
                            //router.Send(client, recvEscapeExec.ToPacket());
                        }
                    }

                    //Send the results of the logout request in case of failure or interuption
                    RecvLogoutStartRequest recvLogoutStartRequest = new RecvLogoutStartRequest(0);
                    router.Send(client, recvLogoutStartRequest.ToPacket());
                }
            );


            /*stuff that does nothing, but might be part of the puzzle. 
             *                             
            
                        res = BufferProvider.Provide();
                        server.router.Send(client, (ushort)AreaPacketId.recv_base_exit_r, res, ServerType.Area); //does nothing

                        RecvObjectDisappearNotify recvObjectDisappearNotify = new RecvObjectDisappearNotify(npcSpawn.instanceId);
                        router.Send(client, recvObjectDisappearNotify.ToPacket());

                        res = BufferProvider.Provide();
                        res.WriteInt32(0);
                        server.router.Send(client, (ushort)AreaPacketId.recv_escape_start, res, ServerType.Area); //Escape is an in dungeun logout that can be interupted


             * */

        }
    }
}
