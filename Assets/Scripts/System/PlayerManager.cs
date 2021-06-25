/*using Lex;
using Photon.Realtime;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LexNetwork : MonoBehaviourLexCallbacks
{
    private static LexNetwork prConnMan;
    
    public int myId = 0;
    public bool init = false;
    private void Awake()
    {
        EventManager.StartListening(MyEvents.EVENT_GAME_CYCLE_RESTART, OnGameRestart);
    }
    private void OnDestroy()
    {

        EventManager.StopListening(MyEvents.EVENT_GAME_CYCLE_RESTART, OnGameRestart);
    }

    private void OnGameRestart(EventObject arg0)
    {
        RemoveAllBots();
        botIDnumber = 0;
    }

    public override void OnJoinedRoom()
    {
        Init();
    }
    public static LexNetwork instance
    {
        get
        {
            if (!prConnMan)
            {
                prConnMan = FindObjectOfType<LexNetwork>();
                if (!prConnMan)
                {
                }
            }

            return prConnMan;
        }
    }

    internal static void AddBotPlayer(LexPlayer botPlayer)
    {
      //  Debug.LogWarning("Add bot " + botPlayer);
        instance.playerDict.Add(botPlayer.uid, botPlayer);
        instance.currentPlayerNum++;
    }
    public static void RemoveBotPlayer(string uid) {
        if (instance.playerDict.ContainsKey(uid))
        {
           // Debug.LogWarning("Remove bot " + uid);
            instance.playerDict.Remove(uid);
            instance.currentPlayerNum--;
        }
    }

    public Dictionary<string, LexPlayer> playerDict = new Dictionary<string, LexPlayer>();

    public int currentPlayerNum = 0;
    public void Init() {
        if (init) return;
        init = true;
        playerDict.Clear();
        Player[] players = LexNetwork.PlayerList;
        foreach (Player p in players) {
            LexPlayer uPlayer = new LexPlayer(p);
            playerDict.Add(p.UserId,uPlayer);
        }
        currentPlayerNum = playerDict.Count;
        Debug.Log("<color=#00ff00>Conn man : current size</color> " + currentPlayerNum);
    }

   public static int botIDnumber = 0;
    internal static string PollBotID()
    {
        botIDnumber++;
        return "T-"+LexNetwork.LocalPlayer.UserId+"-"+botIDnumber;
    }

    public static Dictionary<string, LexPlayer> GetPlayerDictionary() {
        return instance.playerDict;
    }

    internal static int GetMyIndex(LexPlayer myPlayer, LexPlayer[] players, bool useRandom = false)
    {
        instance.Init();
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
    internal static SortedDictionary<string,int> GetIndexMap(LexPlayer[] players, bool useRandom = false)
    {
         instance.Init();
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
        var list = from LexPlayer p in instance.playerDict.Values
                   where p.IsHuman
                   select p;
        return list.ToArray();
    }
    internal static LexPlayer[] GetBotPlayers()
    {
        var list = from LexPlayer p in instance.playerDict.Values
                   where p.IsBot
                   select p;
        return list.ToArray();
    }
    internal static int GetBotCount() {
        return (
            from LexPlayer p in instance.playerDict.Values
            where p.IsBot
            select p).Count();
    }
    public static void RemoveAllBots()
    {

        var list = (from LexPlayer p in instance.playerDict.Values
                   where p.IsBot
                   select p.uid).ToArray();
        foreach (string s in list) {
          //  Debug.LogWarning("Remove bot " + s);
            instance.playerDict.Remove(s);
        }
    }
    internal static LexPlayer[] GetPlayers() {
        return instance.playerDict.Values.ToArray();
    }

    internal static Player GetRandomPlayerExceptMe()
    {
        Player[] players = LexNetwork.PlayerListOthers;
        if (players.Length > 0)
        {
            return players[UnityEngine.Random.Range(0, players.Length)];

        }
        else {
            return null;
        }
    }

    public override void OnPlayerEnteredRoom(Player newPlayer)
    {
        if (!playerDict.ContainsKey(newPlayer.UserId))
        {
            LexPlayer uPlayer = new LexPlayer(newPlayer);
            playerDict.Add(newPlayer.UserId, uPlayer);
            currentPlayerNum++;
            Debug.Log("<color=#00ff00> Addplayer </color> " + currentPlayerNum);
        }
    }
    public override void OnPlayerLeftRoom(Player newPlayer)
    {
        if (playerDict.ContainsKey(newPlayer.UserId))
        {
            playerDict.Remove(newPlayer.UserId);
            currentPlayerNum--;
            Debug.Log("<color=#00ff00> removePlayer </color> " + currentPlayerNum);
        }
        EventManager.TriggerEvent(MyEvents.EVENT_PLAYER_LEFT, new EventObject(newPlayer.UserId));
    }
    public static LexPlayer GetPlayerByID(string id) {
        instance.Init();
        if (id == null) return null;
        if (instance.playerDict.ContainsKey(id))
        {
            return instance.playerDict[id];
        }
        else
        {
            Debug.LogWarning("Couldnt find " + id + " size "+instance.playerDict.Count);
            return null;
        }
    }
    public static LexPlayer GetPlayerOfTeam(Team team)
    {
        instance.Init();
        return (from LexPlayer p in instance.playerDict.Values
                   where p.GetProperty(Property.Team, Team.NONE) == team
                   select p).First();
                   
    }

    internal static int GetNumberInTeam(Team myTeam)
    {
        return (from LexPlayer p in instance.playerDict.Values
                where p.GetProperty(Property.Team, Team.NONE) == myTeam
                select p).Count();

    }

    public static LexPlayer LocalPlayer { 
        get => instance.playerDict [LexNetwork.LocalPlayer.UserId];
    }
    public override void OnDisconnected(DisconnectCause cause)
    {
        Debug.LogWarning("Disconnected " + cause.ToString());
        ChatManager.SendLocalMessage("게임연결이 끊겼습니다. 클라이언트를 재가동해주세요.");
        instance.init = false;
        SceneManager.LoadScene(0);
       
    }
    public override void OnLeftRoom()
    {
        if (LexNetwork.IsConnected)
        StartCoroutine(WaitAndReset());
    }
    IEnumerator WaitAndReset() {
        Debug.LogWarning("Reload scene in 2 seconds");
        yield return new WaitForSeconds(2f);
        embarkCalled = false;
        instance.init = false;
        SceneManager.LoadScene(0);
        MenuManager.JoinRoom();
    }
  static  IEnumerator WaitAndQuit()
    {
        Debug.LogWarning("Reload scene in 1 seconds");
        yield return new WaitForSeconds(0.5f);
        GameSession.instance.LeaveRoom();
    }
    public static bool embarkCalled = false;
    public static void ReconnectEveryone() {
        if (!LexNetwork.IsMasterClient) return;
        GameSession.PushRoomASetting(ConstantStrings.HASH_GAME_STARTED, false);
        GameSession.instance.lexView.RPC("LeaveRoom", RpcTarget.Others);
        instance.StartCoroutine(WaitAndQuit());
    }
    public static void KickEveryoneElse()
    {
        if (!LexNetwork.IsMasterClient) return;
        GameSession.instance.lexView.RPC("QuitGame", RpcTarget.Others);
    }
    *//*   public void Disconnect()
       {
           LexNetwork.Disconnect();
       }

       public void Reconnect()
       {
           if (!LexNetwork.IsConnected && wasConnected)
           {
               LexNetwork.ReconnectAndRejoin();
           }
           else
           {
               LexNetwork.ConnectUsingSettings();
           }
       }*//*



}
*/