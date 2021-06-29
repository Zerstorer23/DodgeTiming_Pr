namespace Lex
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;

    public class LexNetwork_CallbackHandler
    {
        public void ParseCallback(int sentActorNumber, LexNetworkMessage netMessage)
        {
            //actorID , MessageInfo , callbackType, params
            string cbt = netMessage.GetNext();
            int cbtNum = Int32.Parse(cbt);
            LexCallback callbackType = (LexCallback)cbtNum;
            switch (callbackType)
            {
                case LexCallback.PlayerJoined:
                    Handle_Receive_PlayerJoin(netMessage);
                    break;
                case LexCallback.PlayerDisconnected:
                    Handle_Receive_PlayerDisconnect(netMessage);
                    break;
                case LexCallback.RoomInformationReceived:
                    Handle_Receive_RoomInformation(sentActorNumber, netMessage);
                    break;
                case LexCallback.MasterClientChanged:
                    Handle_Receive_SetMasterClient(sentActorNumber, netMessage);
                    break;
                case LexCallback.OnLocalPlayerJoined:
                    Handle_Receive_LocalJoinFinish(sentActorNumber);
                    break;
                case LexCallback.ModifyServerTime:
                    Handle_Receive_ServerTimeModification(netMessage);
                    break;
                case LexCallback.Ping_Received:
                    Handle_Receive_Ping(netMessage);
                    break;
                case LexCallback.Disconnected:
                    Handle_ConnectionLost();
                    break;
            }

        }

        private void Handle_ConnectionLost()
        {
            if (Application.isPlaying)
            {
                LexNetwork.Disconnect();
                NetworkEventManager.TriggerEvent(LexCallback.Disconnected, null);
            }
        }

        private void Handle_Receive_Ping(LexNetworkMessage netMessage)
        {
            double remoteServerTime = double.Parse(netMessage.GetNext()) / 1000d;

            LexNetwork.instance.ReceivePing(remoteServerTime);
        }

        private void Handle_Receive_SetMasterClient(int sentActorNumber, LexNetworkMessage netMessage)
        {
            string nextMaster = (netMessage.GetNext());
            LexPlayer player = LexNetwork.GetPlayerByID(nextMaster);
            LexNetwork.SetMasterClient_Receive(sentActorNumber.ToString(), nextMaster);
            NetworkEventManager.TriggerEvent(LexCallback.MasterClientChanged, new NetEventObject() { objData = player });
        }

        private void Handle_Receive_PlayerDisconnect(LexNetworkMessage netMessage)
        {
            //remove player dict
            //local destroy all rpc and obj
            string disconnActor = (netMessage.GetNext());
            LexPlayer player = LexNetwork.GetPlayerByID(disconnActor);
            LexNetwork.RemovePlayerFromDictionary(disconnActor);
            LexNetwork.DestroyPlayerObjects(disconnActor, true);
            NetworkEventManager.TriggerEvent(LexCallback.PlayerDisconnected, new NetEventObject() { objData = player });
        }

        private void Handle_Receive_PlayerJoin(LexNetworkMessage netMessage)
        {
            LexPlayer player = new LexPlayer(false, netMessage);
            LexNetwork.AddPlayerToDictionary(player);
            NetworkEventManager.TriggerEvent(LexCallback.PlayerJoined, new NetEventObject() { objData = player });
        }



        private void Handle_Receive_LocalJoinFinish(int sentActorNumber)
        {
            if (sentActorNumber != LexNetwork.LocalPlayer.actorID)
            {
                Debug.LogWarning("Not supposed to happen");
                return;
            }

            Debug.Log("Received RPCs and finished join");
            Debug.Assert(LexNetwork.IsConnected == false, "Connected but received rpc?");
            LexNetwork.instance.SetConnected(true);

            NetworkEventManager.TriggerEvent(LexCallback.OnLocalPlayerJoined, null);
        }

        private void Handle_Receive_RoomInformation(int sentActorNumber, LexNetworkMessage netMessage)
        {
            /*
             0 Sent Actor Num = -1
             1 MessageInfo = Callback
             2 RoomInfo Begin==
                2 - NumRoomHash
                    1 - key (int)
                    2 - value (string)
             3. Player Begin===
                4. NumPlayer
                5. LocalPlayer 
                   6. id, ismaster, numParam , key..value...
             7.Sertvertime in long long
             */
            //params = [int]numPlayers(local Included) , LocalPlayerInfo , players[...
            //Player Info = actorID, isMaster, customprop[num prop]
            //Load Room
            int numHash = Int32.Parse(netMessage.GetNext());
            LexDebug.Log("Number of room hash : " + numHash);
            for (int count = 0; count < numHash; count++)
            {
                int key = Int32.Parse(netMessage.GetNext());
                string typeName = netMessage.GetNext();
                string value = netMessage.GetNext();
                object hontoValue = LexNetwork_MessageHandler.ParserAParameter(typeName, value);
                LexDebug.Log("room hash : " + (Property)key + " / " + hontoValue);
                LexNetwork.CustomProperties.Add(key, hontoValue);
            }

            //Load Players
            int numPlayers = Int32.Parse(netMessage.GetNext());
            LexDebug.Log("Number of Players: " + numPlayers);//첫번째는무조건 로컬
                                                          //LexPlayer localPlayer = new LexPlayer(true, netMessage);

            for (int count = 0; count < numPlayers; count++)
            {
                bool isLocal = (count == 0);
                LexPlayer player = new LexPlayer(isLocal, netMessage);
                if (isLocal)
                {
                    LexNetwork.instance.SetLocalPlayer(player);
                }
                LexNetwork.AddPlayerToDictionary(player);
            }
            long serverTime = long.Parse(netMessage.GetNext());
            LexNetwork.instance.InitServerTime(serverTime);
            //.1 소켓접속, 2. 룸정보 받기 , 3. 플레이어 받기, 4. 서버시간 받기
           // RequestServerTimeModification(true, false);
            //5.서버시간변경요청
        }

        void RequestServerTimeModification(bool requestModification, bool requestBufferedRPCs)
        {
            //LEX / int: sentPlayer / MsgIngo: SAerverRequest / ReqInfo : modify time / bool: requestRPC
            Debug.Log("Request servertime " + requestModification);
            LexNetworkMessage requestMessage = new LexNetworkMessage(
                   LexNetwork.LocalPlayer.actorID,
                   (int)MessageInfo.ServerRequest,
                   (int)LexRequest.Receive_modifiedTime
                    , (requestBufferedRPCs) ? "1" : "0"
                   );
            LexNetwork.networkConnector.EnqueueAMessage(requestMessage);
        }
        private void Handle_Receive_ServerTimeModification(LexNetworkMessage netMessage)
        {
            //LEX / 0 =SERVER / PING=MESSAGEINFO / targetPlayer / modifiedTime
            int targetPlayer = Int32.Parse(netMessage.GetNext());
            Debug.Assert(targetPlayer == LexNetwork.LocalPlayer.actorID, "Received wrong message");
            long modValue = long.Parse(netMessage.GetNext());
            LexDebug.Log("Received servertime / " + modValue);
            int remain = LexNetwork.instance.ModifyServerTime(modValue);
            if (remain > 0)
            {
                bool requestBufferedRPCs = remain == 1;
                RequestServerTimeModification(true, requestBufferedRPCs);
            }
        }
    }

}