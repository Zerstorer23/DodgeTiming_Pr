using System;
using System.Collections;
using UnityEngine;
using static Lex.LexNetwork_MessageHandler;


namespace Lex
{
    public partial class LexNetwork : MonoBehaviourLexCallbacks
    {
#if USE_LEX
        public static bool useLexNet = true;
#else
        public static bool useLexNet = false;
#endif
        public static bool debugLexNet = false;
        public static readonly int MAX_VIEW_IDS = 10000;

        public static string ServerAddress;
        public static bool connected;
        public LexLogLevel logLevel;
        public static LexNetworkConnection networkConnector = new LexNetworkConnection();
        private static LexNetwork prNetwork;
        [SerializeField] [ReadOnly] bool amMaster;
        [SerializeField] [ReadOnly] int myActorID;

        public static LexHashTable CustomProperties { get; private set; } = new LexHashTable();

        public static LexNetwork instance
        {
            get
            {
                if (!prNetwork)
                {
                    prNetwork = FindObjectOfType<LexNetwork>();
                    if (!prNetwork)
                    {
                        Debug.LogWarning("There needs to be one active LexNetwork script on a GameObject in your scene.");
                    }

                }
                return prNetwork;
            }
        }

        public static void DestroyPlayerObjects(string playerID, bool localOnly = false)
        {
            var viewList = LexViewManager.GetViewList();
            foreach (var view in viewList)
            {
                if (view.IsRoomView || view.IsSceneView) continue;
                if (view.Owner.uid == playerID)
                {
                    if (localOnly)
                    {
                        LexViewManager.ReleaseViewID(view);
                    }
                    else
                    {
                        Destroy(view);
                    }
                }
            }

        }

        internal static LexView GetLexView(int pvID)
        {
            return LexViewManager.GetViewByID(pvID);
        }

        public static void DestroyAll(bool localOnly = false)
        {
            var viewList = LexViewManager.GetViewList();
            foreach (var view in viewList)
            {
                if (view.IsSceneView) continue;
                if (localOnly)
                {
                    LexViewManager.ReleaseViewID(view);
                }
                else
                {
                    Destroy(view);
                }
            }
        }

        public static bool ConnectUsingSettings()
        {

            //1 소켓 연결
            if (IsConnected) return false;
            bool success = networkConnector.Connect();
            if (!success) return false;
            //2 연결 성공시 Request(플레이어 정보, 해시정보 로드
            //  instance.RequestConnectedPlayerInformation();
            //3.해시로드callback받기
            //4. Request Buffered RPC
            Debug.Log("Connection..." + success);
            return success;
        }


        public static bool Reconnect()
        {
            Disconnect();
            return ConnectUsingSettings();
        }

        public static void Disconnect()
        {
            DestroyAll();
            playerDictionary.Clear();
            instance.SetConnected(false);
            networkConnector.Disconnect();
        }


        #region instantiation
        public static GameObject Instantiate(string prefabName, Vector3 position, Quaternion quaternion, byte group = 0, object[] parameters = null)
        {
            NetworkInstantiateParameter param = new NetworkInstantiateParameter(LexViewManager.RequestPrivateViewID(), prefabName, LocalPlayer.actorID, LocalPlayer.actorID, false, parameters);
            LexView lv = NetObjectPool.PollObject(position, quaternion, param);
            instance.Instantiate_Send(position, quaternion, param);
            return lv.gameObject;
        }


        public static GameObject InstantiateRoomObject(string prefabName, Vector3 position, Quaternion quaternion, byte group = 0, object[] parameters = null)
        {
            NetworkInstantiateParameter param = new NetworkInstantiateParameter(LexViewManager.RequestRoomViewID(), prefabName, MasterClient.actorID, LocalPlayer.actorID, true, parameters);
            LexView lv = NetObjectPool.PollObject(position, quaternion, param);
            instance.Instantiate_Send(position, quaternion, param);
            return lv.gameObject;
        }
        #endregion
        public static int GetPing()
        {
            if (!IsConnected)
            {
                return 0;
            }
            double ping = (instance.lastCalculatedPing) * 1000;
            return (int)ping;
        }


        public static bool CloseConnection(LexPlayer player = null)
        {
            //TODO
            //클라이언트에게 접속 해제를 요청 합니다.(KICK). 마스터 클라이언트만 이것을 수행 할 수 있습니다
            return true;
        }

        public static bool SetMasterClient(int masterPlayer)
        {
            //actorID , MessageInfo , callbackType, params
            if (!IsMasterClient) return false;
            LexNetworkMessage netMessage = new LexNetworkMessage();
            netMessage.Add(LocalPlayer.actorID);
            netMessage.Add(MessageInfo.ServerRequest);
            netMessage.Add(LexRequest.ChangeMasterClient);
            netMessage.Add(masterPlayer);
            networkConnector.EnqueueAMessage(netMessage);
            return true;
        }

        public static void Destroy(GameObject gameObject)
        {
            var lv = gameObject.GetComponent<LexView>();
            if (lv == null) {
                Debug.LogWarning("No View in " + gameObject.name);
                return;
            }
            Destroy(lv);
        }
        public static void Destroy(int viewID){
           var lv = LexViewManager.GetViewByID(viewID);
            if (lv == null)
            {
                Debug.LogWarning("No such ViewID " + viewID);
                return;
            }
            Destroy(lv);
        }

        public static void Destroy(LexView lv) {
            if (!lv.IsMine && !IsMasterClient)
            {
                Debug.LogWarning(lv.ViewID + " is not mine! ");
                return;
            }
            int viewID = lv.ViewID;
            RemoveBufferedRPCs(lv); //서버 버퍼에서 Instantiate와 모든 RPC제거
            LexViewManager.ReleaseViewID(lv);
            LexNetworkMessage netMessage = new LexNetworkMessage(LocalPlayer.actorID, (int)MessageInfo.Destroy, viewID);
            networkConnector.EnqueueAMessage(netMessage);
        }




        public static void RemoveRPCs(int actorID)
        {

            /*
             Remove all buffered RPCs from server that were sent by targetPlayer. Can only be called on local player (for "self") or Master Client (for anyone).

            This method requires either:

            This is the targetPlayer's client.
            This client is the Master Client (can remove any Player's RPCs).
            If the targetPlayer calls RPCs at the same time that this is called, network lag will determine if those get buffered or cleared like the rest.
             */
            //1. 내 송수신버퍼에 actorNumber관련 모든 RPC제거    
            //2. 서버 request 버퍼에 모든 플레이어로부터 rpc제거  <- 이거만 수행
            //3. 서버 callback 수신 rpc 제거
            LexNetworkMessage networkMessage = new LexNetworkMessage();
            networkMessage.Add(LocalPlayer.actorID);
            networkMessage.Add(MessageInfo.ServerRequest);
            networkMessage.Add(LexRequest.RemoveRPC);
            networkMessage.Add(actorID);
            networkMessage.Add("-1");
            networkConnector.EnqueueAMessage(networkMessage);

        }
        public static void RemoveBufferedRPCs(LexView lv)
        {

            LexNetworkMessage networkMessage = new LexNetworkMessage();
            networkMessage.Add(LocalPlayer.actorID);
            networkMessage.Add(MessageInfo.ServerRequest);
            networkMessage.Add(LexRequest.RemoveRPC);
            networkMessage.Add("-1");
            networkMessage.Add(lv.ViewID);
            networkConnector.EnqueueAMessage(networkMessage);
        }


        public static void SendChat(string chatMessage)
        {
            NetworkEventManager.TriggerEvent(LexCallback.ChatReceived, new NetEventObject() { stringObj = chatMessage });
            chatMessage = chatMessage.Replace(NET_DELIM, "^^");
            LexNetworkMessage netMessage = new LexNetworkMessage(LocalPlayer.actorID, (int)MessageInfo.Chat, chatMessage);
            networkConnector.EnqueueAMessage(netMessage);
        }


        public static void SetRoomCustomProperties(LexHashTable hash)
        {

            CustomProperties.UpdateProperties(hash);
            //Needs to be synced with server
            //server needs to keep all hash settings
            instance.CustomProperty_Send(0, hash);
            NetworkEventManager.TriggerEvent(LexCallback.HashChanged, new NetEventObject() {stringObj="0", objData = hash });
        }


        public static void SetPlayerCustomProperties(LexHashTable hash)
        {
            LocalPlayer.SetCustomProperties(hash);
            NetworkEventManager.TriggerEvent(LexCallback.HashChanged, new NetEventObject() {stringObj=LocalPlayer.uid, objData = hash });
        }


        [LexRPC]
        public void SetBotProperty(string uid, int key, string typename, object value)
        {
            var player = GetPlayerByID(uid);
            if (player == null) return;
            if (useLexNet)
            {
                object data = ParserAParameter(typename, (string)value);
                player.ReceiveBotProperty(key, value);//TODO
            }
        }

        private void Awake()
        {
            //  dict.Add("Hi", "A");
            // Debug.Log(dict["Hi"]);
            DontDestroyOnLoad(gameObject);
            LexDebug.LogLevel = logLevel;
        }
      
        private void OnEnable()
        {
            DoTimeStartUp();
        }
      

        private void Update()
        {
            Time += UnityEngine.Time.deltaTime;
            networkConnector.DequeueReceivedBuffer();
        }
        private void FixedUpdate()
        {
            amMaster = IsMasterClient;
            if (IsConnected && LocalPlayer != null)
            {

                myActorID = LocalPlayer.actorID;
                players = GetPlayerList();
            }
        }
        public LexPlayer[] players;
        private void OnApplicationQuit()
        {
            LexNetwork.Disconnect();
        }
    }
    public class NetworkInstantiateParameter{
       internal int viewID;
        internal string prefabName;
        internal int ownerID;
      internal  int creatorID;
      internal  bool isRoomView;
      internal  object[] data;
        public NetworkInstantiateParameter(int view,string name, int owner, int creator, bool isRoom, object[] param) {
            this.viewID = view;
            this.ownerID = owner;
            this.prefabName = name;
            this.creatorID = creator;
            this.isRoomView = isRoom;
            this.data = param;
        }
    }
}

