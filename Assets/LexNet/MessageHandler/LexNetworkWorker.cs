
namespace Lex
{
    using Lex;
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using UnityEngine;

    public partial class LexNetwork
    {

        internal static Dictionary<string, LexPlayer> playerDictionary = new Dictionary<string, LexPlayer>();
        private static Mutex playerDictionaryMutex = new Mutex();
        public LexNetwork_TimeHandler timeHandler = new LexNetwork_TimeHandler();

        private static LexPlayer[] GetPlayerList()
        {
            return playerDictionary.Values.ToArray();
        }
        private static LexPlayer[] GetPlayerListOthers()
        {
            return playerDictionary.Values.OrderBy((x) => x.actorID).Where(x => !x.IsLocal).ToArray();
        }
        private static int GetPlayerCount()
        {

            return playerDictionary.Count ;
        }
        internal static void PrintStringToCode(string str)
        {
            char[] arr = str.ToCharArray();
            string code = "";
            foreach (char c in arr)
            {
                code += " " + (int)c;
            }
            Debug.Log(code);
            Debug.Log(str);
        }

        internal void SetLocalPlayer(LexPlayer player)
        {
            LocalPlayer = player;
            Debug.Log("Local player set : " + player.actorID);
            //    playerDictionary.Add(player.actorID, player);
        }

        internal static void AddPlayerToDictionary(LexPlayer player)
        {
            playerDictionaryMutex.WaitOne();
            playerDictionary.Add(player.uid, player);
            Debug.LogWarning("Add player " + player.uid);
            if (player.IsLocal) {
                LocalPlayer = player;
            }
            if (player.IsMasterClient)
            {
                SetMasterClient_Receive(player.uid, player.uid);
                // MasterClient = player;
            }
            playerDictionaryMutex.ReleaseMutex();
        }
        internal static void RemovePlayerFromDictionary(string actorID)
        {
            playerDictionaryMutex.WaitOne();

            if (playerDictionary.ContainsKey(actorID))
            {
                LexPlayer player = playerDictionary[actorID];
                if (player.IsMasterClient)
                {
                    MasterClient = null;
                }
                playerDictionary.Remove(actorID);
            }
            playerDictionaryMutex.ReleaseMutex();
        }


        internal void CustomProperty_Send(int actorID, LexHashTable hash)
        {
            LexNetworkMessage netMessage = new LexNetworkMessage(LocalPlayer.actorID, (int)MessageInfo.SetHash, actorID, hash.lexHash.Count);
            foreach (var entry in hash.lexHash)
            {
                netMessage.Add(entry.Key);
                netMessage.Add(entry.Value.GetType().Name);
                netMessage.Add(entry.Value);
            }
            networkConnector.EnqueueAMessage(netMessage);
        }
 
        internal void CustomProperty_Receive(string actorID, LexHashTable hash)
        {
            if (actorID == "0")
            {
                CustomProperties.UpdateProperties(hash);
            }
            else
            {
                GetPlayerByID(actorID).CustomProperties.UpdateProperties(hash);
            }
        }

        public void Instantiate_Send(Vector3 position, Quaternion quaternion, NetworkInstantiateParameter param)
        {
            LexNetworkMessage netMessage = new LexNetworkMessage(LocalPlayer.actorID, (int)MessageInfo.Instantiate,param.viewID,param.ownerID,param.prefabName,param.isRoomView, position, quaternion);
            netMessage.EncodeParameters(param.data);
            networkConnector.EnqueueAMessage(netMessage);
        }


        public void SyncVar_Send(LexView lv, params object[] parameters)
        {
            LexNetworkMessage netMessage = new LexNetworkMessage(LocalPlayer.actorID, (int)MessageInfo.SyncVar, lv.ViewID);
            netMessage.EncodeParameters(parameters);
            networkConnector.EnqueueAMessage(netMessage);
        }
        public void SyncVar_Receive(int viewID, params object[] parameters)
        {
            LexView lv = LexViewManager.GetViewByID(viewID);
            if (!lv) return;

            if (debugLexNet)
            {
                foreach (object obj in parameters) Debug.Log(obj);
            }
            lv.ReceiveSerializedVariable(parameters);
        }



        public void DestroyPlayerObjects(string actorID)
        {
            //플레이어나가면
            //1. [서버] 해당플레이어 모든 RPC제거
            //2. [서버] PlayerDisconnect 콜백
            //3. [클라이언트] 해당유저 Destroy 콜
            //4. [클라이언트] playerlist업데이트
            //Mutex
            var viewList = LexViewManager.GetViewList();
            foreach (var lv in viewList)
            {
                if (lv.Owner.uid == actorID)
                {
                    LexViewManager.ReleaseViewID(lv);
                }
            }
            //Mutex
        }
        internal static void SetMasterClient_Receive(string sentUID, string nextMaster)
        {
            //지금마스터 해제
            //새 마스터 등록
            //view아이디 owner정보 변경
            if (playerDictionary.ContainsKey(sentUID))
            {
                playerDictionary[sentUID].IsMasterClient = false;
            }
            playerDictionary[nextMaster].IsMasterClient = true;
            MasterClient = playerDictionary[nextMaster];
            Debug.LogWarning(LocalPlayer);
            Debug.LogWarning(MasterClient);
            if (LocalPlayer !=null && nextMaster == LocalPlayer.uid)
            {
                IsMasterClient = true;
            }
            else
            {
                IsMasterClient = false;
            }
            var viewList = LexViewManager.GetViewList();
            foreach (var entry in viewList)
            {
                entry.UpdateOwnership();
            }
        }
        internal void InitServerTime(long time)
        {
            NetTime = (double)time / 1000; //long is in mills
            Debug.Log("서버 시작시간 : " + NetTime);
        }
        internal int ModifyServerTime(long timeValue)
        {
            double timeInMills = (double)timeValue / 1000;
            int remainingRuns = timeHandler.Append(timeInMills);
            if (remainingRuns <= 0)
            {
                timeInMills = timeHandler.Finalise();
                NetTime += timeInMills;
                Debug.Log("Modified time : " + timeInMills);
            }
            Debug.Log("Remaining : " + remainingRuns);
            return remainingRuns;
        }

        double lastSentPing = 0;
        double lastReceivedPing = 0;
        double pingPeriodInSec = 5;
        public void SendPing()
        {
            lastSentPing = NetTime;
            LexNetworkMessage netMessage = new LexNetworkMessage();
            netMessage.Add(LocalPlayer.actorID);
            netMessage.Add(MessageInfo.ServerRequest);
            netMessage.Add(LexRequest.Ping);
            networkConnector.EnqueueAMessage(netMessage);
        }
        public void ReceivePing()
        {
            lastReceivedPing = NetTime;
        }
        internal void SetConnected(bool v)
        {
            Debug.Log("Connected : " + v);
            IsConnected = v;
        }
    }
}