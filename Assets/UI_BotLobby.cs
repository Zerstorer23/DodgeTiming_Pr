using Lex;
using System.Collections.Generic;
using UnityEngine;
using static ConstantStrings;
public class UI_BotLobby : MonoBehaviourLexCallbacks
{
    List<HUD_UserName> botPanels = new List<HUD_UserName>();
    UI_PlayerLobbyManager lobbyManager;
    private void Awake()
    {
        lobbyManager = GetComponent<UI_PlayerLobbyManager>();
    }



    private void OnDisable()
    {
        EventManager.StopListening(MyEvents.EVENT_GAME_STARTED, OnGameStarted);
        EventManager.StopListening(MyEvents.EVENT_GAMEMODE_CHANGED, OnGamemodeChanged);
        EventManager.StopListening(MyEvents.EVENT_PLAYER_JOINED, OnGamemodeChanged);
        EventManager.StopListening(MyEvents.EVENT_PLAYER_LEFT, OnGamemodeChanged);
    }
    private void OnEnable()
    {
        botPanels.Clear();
        EventManager.StartListening(MyEvents.EVENT_GAME_STARTED, OnGameStarted);
        EventManager.StartListening(MyEvents.EVENT_GAMEMODE_CHANGED, OnGamemodeChanged);
        EventManager.StartListening(MyEvents.EVENT_PLAYER_JOINED, OnGamemodeChanged);
        EventManager.StartListening(MyEvents.EVENT_PLAYER_LEFT, OnGamemodeChanged);
    }
    private void OnGameStarted(EventObject arg0)
    {
        if (LexNetwork.IsMasterClient)
        {
            foreach (var panel in botPanels)
            {
                LexNetwork.Destroy(panel.pv);
            }
        }
    }
    private void OnGamemodeChanged(EventObject arg0)
    {
        GameModeConfig info = GameSession.gameModeInfo;
        if (!info.CheckBotGame())
        {
            foreach (var panel in botPanels)
            {
                lobbyManager.RemovePlayer(panel.controller.uid); 
                if (LexNetwork.IsMasterClient)
                {
                    LexNetwork.Destroy(panel.pv);
                }
            }
            botPanels.Clear();
            if (LexNetwork.IsMasterClient) {
                lexView.RPC("RemoveAllBots");
            }
        }
    }
    public void DestoryBotsPanel() {
        if (LexNetwork.IsMasterClient) {

            foreach (var entry in botPanels)
            {
                LexNetwork.Destroy(entry.gameObject);
            }
        }
        botPanels.Clear();
    }

    [LexRPC]
    public void RemoveAllBots()
    {
        LexNetwork.RemoveAllBots();

    }


    int maxBots = 4;
    public void OnAddBot()
    {
        if (!LexNetwork.IsMasterClient || !GameSession.gameModeInfo.CheckBotGame())
        {
            return;
        }
        if (LexNetwork.GetBotCount()+1 > maxBots) return;
        string newID = LexNetwork.PollBotID();
        lexView.RPC("AddBotPlayer", newID);
    }
    public void OnRemoveBot()
    {
        if (!LexNetwork.IsMasterClient )
        {
            return;
        }
        LexPlayer[] bots = LexNetwork.GetBotPlayers();
        if (bots.Length > 0)
        {
            lexView.RPC("RemoveBotPlayer",   bots[0].uid);
        }
    }

    /// <summary>
    /// TODO MEMO
    /// Add bot ro list before update hash
    /// </summary>
    /// <param name="uid"></param>
    [LexRPC]
    public void AddBotPlayer(string uid)
    {
        Debug.Log("Add bot player " + uid);
        LexPlayer botPlayer = new LexPlayer(uid);
        LexNetwork.AddBotPlayer(botPlayer);
        if (LexNetwork.IsMasterClient)
        {
            LexHashTable hash = new LexHashTable();
            hash.Add(Property.RandomSeed, UnityEngine.Random.Range(0, 133));
            hash.Add(Property.Team, (int)Team.HOME);
            hash.Add(Property.Character, (int)CharacterType.NONE);
            botPlayer.SetCustomProperties(hash);
        }
        InstantiateBotPanel(botPlayer);
    }
    

    [LexRPC]
    public void RemoveBotPlayer(string uid)
    {
        Debug.LogWarning("Remove bot " + uid);
        LexNetwork.RemoveBotPlayer(uid);
        for (int i = 0; i < botPanels.Count; i++)
        {
            HUD_UserName panel = botPanels[i];
            if (panel.controller.Equals(uid))
            {
                if (LexNetwork.IsMasterClient)
                {
                    LexNetwork.Destroy(panel.gameObject);
                }
                lobbyManager.RemovePlayer(uid);
                botPanels.RemoveAt(i);
                lobbyManager.RebalanceTeam();
                return;
            }
        }


    }
    private void InstantiateBotPanel(LexPlayer botPlayer)
    {
        if (!LexNetwork.IsMasterClient) return;
        var go = LexNetwork.Instantiate(PREFAB_STARTSCENE_PLAYERNAME, Vector3.zero, Quaternion.identity, 0, new object[] { true, botPlayer.uid });
        var info = go.GetComponent<HUD_UserName>();
        botPanels.Add(info);
        string name = botPlayer.NickName;
        CharacterType character = botPlayer.GetProperty(Property.Character, (GameSession.instance.devMode) ? GameSession.instance.debugChara : CharacterType.NONE);
        Team myTeam = botPlayer.GetProperty(Property.Team, Team.HOME);
        info.pv.RPC("ChangeName",   name);
        info.pv.RPC("ChangeCharacter",   (int)character);
        info.pv.RPC("SetTeam",   (int)myTeam);
        /*        charSelector.SetInformation(character);
                UpdateReadyStatus();*/
    }
}
