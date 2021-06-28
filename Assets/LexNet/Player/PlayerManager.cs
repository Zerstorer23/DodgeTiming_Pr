using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Lex
{
    public partial class LexNetwork : MonoBehaviourLexCallbacks
    {
       static bool prIsConnected;
       static bool prIsMasterClient;
       static LexPlayer prMaster;
       static LexPlayer prLocal = new LexPlayer();
        public static double Time { get; private set; }

        public static string NickName { get { return LocalPlayer == null? "ㅇㅇ": LocalPlayer.NickName; } set { LocalPlayer.NickName = value; } }

        public static bool IsConnected { get { return prIsConnected; } private set { prIsConnected = value; } }
        public static bool IsMasterClient { get; private set; }
        public static LexPlayer[] PlayerList { get { return GetPlayerList(); } }
        public static LexPlayer[] PlayerListOthers { get { return GetPlayerListOthers(); } }
        public static int PlayerCount { get { return GetPlayerCount(); } }

        public static LexPlayer MasterClient { get; private set; }//{ get { return useLexNet ? prMaster : prMaster == null ? null : GetPlayerByID(prMaster.uid); } private set { prMaster = value; } }
        public static LexPlayer LocalPlayer { get { return prLocal; } private set { prLocal = value; } }//{ get { return useLexNet ? prLocal : prLocal == null ? null : GetPlayerByID(prLocal.uid); } private set { prLocal = value; } }

        internal static void AddBotPlayer(LexPlayer botPlayer)
        {
            AddPlayerToDictionary(botPlayer);
        }
        public static void RemoveBotPlayer(string uid)
        {
            if (playerDictionary.ContainsKey(uid))
            {
                RemovePlayerFromDictionary(uid);
            }
        }

        public static int botIDnumber = 0;
        internal static string PollBotID()
        {
            botIDnumber++;
            return "T-" + LexNetwork.LocalPlayer.uid + "-" + botIDnumber;
        }
        public static void ResetBotID() {
            botIDnumber = 0;
        }

        public static Dictionary<string, LexPlayer> GetPlayerDictionary()
        {
            return playerDictionary;
        }

        internal static int GetMyIndex(LexPlayer myPlayer, LexPlayer[] players, bool useRandom = false)
        {
            SortedSet<string> myList = new SortedSet<string>();
            foreach (LexPlayer p in players)
            {
                int seed = p.GetProperty(Property.RandomSeed, 0);
                string id = (useRandom) ? seed + p.uid : p.uid;
                myList.Add(id);
            }
            int i = 0;
            int mySeed = myPlayer.GetProperty(Property.RandomSeed, 0);
            string myID = (useRandom) ? mySeed + myPlayer.uid : myPlayer.uid;
            foreach (var val in myList)
            {
                if (val == myID) return i;
                i++;
            }
            return 0;
        }
        internal static SortedDictionary<string, int> GetIndexMap(LexPlayer[] players, bool useRandom = false)
        {
            SortedDictionary<string, string> decodeMap = new SortedDictionary<string, string>();
            foreach (LexPlayer p in players)
            {
                int seed = p.GetProperty(Property.RandomSeed, 0);
                string id = (useRandom) ? seed + p.uid : p.uid;
                decodeMap.Add(id, p.uid);
            }
            int i = 0;
            SortedDictionary<string, int> indexMap = new SortedDictionary<string, int>();
            foreach (var val in decodeMap)
            {
                indexMap.Add(val.Value, i++);
            }
            return indexMap;
        }

        internal static LexPlayer[] GetHumanPlayers()
        {
            var list = from LexPlayer p in playerDictionary.Values
                       where p.IsHuman
                       select p;
            return list.ToArray();
        }
        internal static LexPlayer[] GetBotPlayers()
        {
            var list = from LexPlayer p in playerDictionary.Values
                       where p.IsBot
                       select p;
            return list.ToArray();
        }
        public static void RemoveAllBots()
        {

            var list = (from LexPlayer p in playerDictionary.Values
                        where p.IsBot
                        select p.uid).ToArray();
            foreach (string s in list)
            {
                playerDictionary.Remove(s);
            }
        }
        internal static LexPlayer GetRandomPlayerExceptMe()
        {
            LexPlayer[] players = LexNetwork.PlayerListOthers;
            if (players.Length > 0)
            {
                return players[UnityEngine.Random.Range(0, players.Length)];

            }
            else
            {
                return null;
            }
        }
        public static LexPlayer GetPlayerOfTeam(Team team)
        {
            return (from LexPlayer p in playerDictionary.Values
                    where p.GetProperty(Property.Team, Team.NONE) == team
                    select p).First();

        }
     
        public static int GetBotCount()
        {
            return (
                from LexPlayer p in playerDictionary.Values
                where p.IsBot
                select p).Count();
        }

        public static int GetNumberInTeam(Team myTeam)
        {
            return (from LexPlayer p in playerDictionary.Values
                    where p.GetProperty(Property.Team, Team.NONE) == myTeam
                    select p).Count();

        }

        public static LexPlayer GetPlayerByID(string id)
        {
            if (id == null) return null;
            if (playerDictionary.ContainsKey(id))
            {
                return playerDictionary[id];
            }
            else
            {
                Debug.LogWarning("Couldnt find " + id + " size " + playerDictionary.Count);
                return null;
            }
        }
        public static bool ContainsPlayer(string id) {
            return playerDictionary.ContainsKey(id);        
        }
   

    }
}