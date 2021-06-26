using System;
using UnityEngine;

namespace Lex
{
    [Serializable]
    public class LexPlayer
    {
        public static readonly string[] botNames = {
        "Langley","Saratoga","Lexington","Hornet","Ranger",
        "Yorktown","Enterprise","Wasp","Essex","Intrepid",
    "Franklin","Independence","Princeton","Bunker Hill",
    "Bataan","Kearsarge","Shangri-La","Midway","Saipan"};
        public static readonly string default_name = "ㅇㅇ";

        [SerializeField] [ReadOnly] string myNickName = default_name;
        object tagObject;
        public int actorID;
        string botID;


        public bool IsMasterClient { get; internal set; }



        public LexHashTable CustomProperties { get; private set; }
        //  public SerializeDictionary<string,string> dict = new SerializeDictionary<string, string>();

        //--Universal
        ControllerType controllerType = ControllerType.Human;
        public string uid {
            get {
                if (IsHuman)
                {
                     return actorID.ToString();

                }
                else {
                    return botID;
                }  
            }
        }


        public bool IsBot
        {
            get => controllerType == ControllerType.Bot;
        }
        public bool IsHuman
        {
            get => controllerType == ControllerType.Human;
        }
        public string NickName
        {
            get => GetNickName();
            set => SetNickName(value);
        }
        private bool isLocalPlayer;
        public bool IsLocal
        {
            get => GetIsLocal();
            private set => isLocalPlayer = value;
        }

        private bool GetIsLocal()
        {

                return IsHuman && isLocalPlayer;

        }

        public bool AmController
        {
            get
            {
                if (IsHuman)
                {

                        return isLocalPlayer;

                }
                else
                {
                    return LexNetwork.IsMasterClient;
                }
            }
        }
        private void SetNickName(string value)
        {
            if (IsHuman)
            {

                    myNickName = value;
                    LexHashTable lexHash = new LexHashTable();
                    lexHash.Add(Property.NickName, myNickName);
                    SetCustomProperties(lexHash);

            }
            else
            {
                ReceiveBotProperty((int)Property.NickName, value);
            }
        }

        private string GetNickName()
        {
           return CustomProperties.Get(Property.NickName, myNickName);
        }



        public T GetProperty<T>(Property key) => GetProperty<T>((int)key);
        public T GetProperty<T>(Property key, T value) => GetProperty((int)key,value);

        internal void SetCustomProperties(Property key, object value)
        {
            LexHashTable hash = new LexHashTable();
            hash.Add(key, value);
            SetCustomProperties(hash);
        }

        public T GetProperty<T>(int key)
        {
            return (T)CustomProperties[key];
        }
        public T GetProperty<T>(int key, T value)
        {
            if (HasProperty(key))
            {
                return (T)CustomProperties[key];
            }
            return value;
        }
        public bool HasProperty(int key)
        {
            return CustomProperties.ContainsKey(key);
        }
        public bool HasProperty(Property key)
        {
            return CustomProperties.ContainsKey((int)key);
        }
        public void SetCustomProperties(LexHashTable lexHash)
        {
            Debug.Log("Update hash " + lexHash.lexHash.Count);
            CustomProperties.UpdateProperties(lexHash);
            if (IsHuman)
            {

                    LexNetwork.instance.CustomProperty_Send(actorID, lexHash);

            }
            else
            {
                if (LexNetwork.IsMasterClient)
                {
                    foreach (var entry in lexHash.lexHash)
                    {
                        LexNetwork.instance.lexView.RPC("SetBotProperty", uid,entry.Key,entry.GetType().Name,entry.Value);
                    }
                }
            }
        }
        internal void ReceiveBotProperty(int tag, object value)
        {
            Debug.Assert(IsBot, "Not a bot ??");
            CustomProperties.UpdateProperties(tag, value);
        }
        public override string ToString()
        {
             return uid;
        }


        /*   public NetPlayer(bool isLocal, int actorID, string name) {
               this.isLocal = isLocal;
               this.actorID = actorID;
               this.NickName = name;
               CustomProperties = new Dictionary<PlayerProperty, string>();
           }
       */
        public LexPlayer(bool isLocal, LexNetworkMessage netMessage)
        {
            this.IsLocal = isLocal;
            CustomProperties = new LexHashTable(this);
            this.actorID = Int32.Parse(netMessage.GetNext());
            this.IsMasterClient = netMessage.GetNext() == "1";
            int numHash = Int32.Parse(netMessage.GetNext());
            controllerType = ControllerType.Human;
            Debug.Log(string.Format("Received Player {0}, isMaster{1}", actorID, IsMasterClient));
            for (int i = 0; i < numHash; i++)
            {
                int key = Int32.Parse(netMessage.GetNext());
                string typeName = netMessage.GetNext();
                string value = netMessage.GetNext();
                object hontoValue = LexNetwork_MessageHandler.ParserAParameter(typeName, value);
                Debug.Log("Key " + (Property)key + " / " + hontoValue);
                CustomProperties.Add(key, hontoValue);
            }
        }

        public LexPlayer(string uid)
        {
            controllerType = ControllerType.Bot;
            this.botID = uid;
            NickName = botNames[UnityEngine.Random.Range(0, botNames.Length)];
            CustomProperties = new LexHashTable(this);
        }
        public LexPlayer() {
            this.IsLocal = true;
            this.IsMasterClient = false;
            CustomProperties = new LexHashTable(this);
        }

        public LexPlayer Next() {
            var players = LexNetwork.PlayerList;
            int i = 0;
            while (i < players.Length && players[i].uid != uid) {
                i++;
            }
            int index = (i + 1) % players.Length;
            return players[index];
        }



    }

    public enum Property
    {
        MapDifficulty, PlayerLives, VersionCode, GameMode, GameStarted, GameAuto, RandomSeed
        , NickName, Team, Character, RealCharacter
    }
}