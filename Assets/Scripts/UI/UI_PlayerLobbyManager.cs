using Lex;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using static ConstantStrings;

public class UI_PlayerLobbyManager : MonoBehaviourLex
{

    //***Players***//
    LexView pv;
    [SerializeField] GameObject localPlayerObject;
    [SerializeField] Text numOfPlayers;
    [SerializeField] Text numReadyText;

    [SerializeField] UI_MapOptions mapOptions;
    [SerializeField] UI_CharacterSelector charSelector;
    public static HUD_UserName localPlayerInfo;
    public List<string> foundPlayers = new List<string>();
    Dictionary<string, HUD_UserName> playerDictionary = new Dictionary<string, HUD_UserName>();
    private void Awake()
    {
        pv = GetComponent<LexView>();
        EventManager.StartListening(MyEvents.EVENT_PLAYER_JOINED, OnNewPlayerEnter);
        EventManager.StartListening(MyEvents.EVENT_PLAYER_TOGGLE_READY, UpdateReadyStatus);
    }
    private void OnDestroy()
    {
        EventManager.StopListening(MyEvents.EVENT_PLAYER_JOINED, OnNewPlayerEnter);
        EventManager.StopListening(MyEvents.EVENT_PLAYER_TOGGLE_READY, UpdateReadyStatus);
    }
    public void ConnectedToRoom()
    {
        mapOptions.LoadRoomSettings();
        var playerHash = new LexHashTable();
        //TODO
        playerHash.Add(Property.Team, Team.HOME);
        playerHash.Add(Property.RandomSeed, UnityEngine.Random.Range(0, 133));
        Debug.Log("Game started? " + mapOptions.GetGameStarted());
        if (!mapOptions.GetGameStarted())
        {
            playerHash.Add(Property.Character, CharacterType.NONE);
            LexNetwork.LocalPlayer.SetCustomProperties(playerHash);
            LexNetwork.SendChat("2014년 10월 14일, 배우 김사은과 12월 13일에 결혼한다고 발표했다. 김사은 측에서는 부인했지만 성민 본인이 직접 공식.");
                        LexNetwork.LocalPlayer.SetCustomProperties(playerHash);
            LexNetwork.SendChat("초기에는 아이돌이 결혼하다고 하자 팬들이 분노한다는 점이 부각되어서 연예인은 결혼할 자유도 없냐면서 논란이 시작되었다.");
                        LexNetwork.LocalPlayer.SetCustomProperties(playerHash);
            LexNetwork.SendChat("성민이 비판받는 이유를 요약하면 가장 큰 이유로 성민 본인이 슈퍼주니어 그룹 활동을 전혀 배려하지 않고 이기적으로 행동했다는 것과 두 번째로 이런 무책임");
                        LexNetwork.LocalPlayer.SetCustomProperties(playerHash);
            LexNetwork.SendChat("한 처사에 팬들이 항의하자 이제까지 팬덤 덕에 유명해지고 인기를 얻은 아이돌이 오랫동안 자신을 지지해 준 팬들을 한순간에 안티.");
                        LexNetwork.LocalPlayer.SetCustomProperties(playerHash);
            LexNetwork.SendChat("성민이 김사은과 연애를 하고 있던 것은 팬들이라면 대부분 알고 있었던 사실로, 팬들도 다들 알지만 그룹을 위해 쉬쉬하는 분위기였다.");

            Debug.Log("Instantiate after connection");
            InstantiateMyself();
            UpdateReadyStatus();
            mapOptions.UpdateSettingsUI();//I join room
        }
        else
        {
            LexNetwork.LocalPlayer.SetCustomProperties(playerHash);
            //난입유저 바로시작 TODO MEMO NO NEED SETGAMEMAP
            // GameFieldManager.SetGameMap(GameSession.gameModeInfo.gameMode);
            GameFieldManager.ChangeToSpectator();
            Debug.Log("난입세팅끝");
            StartGame();
        }
    }

    private void OnEnable()
    {
        playerDictionary.Clear();
        if (!LexNetwork.IsConnected) return;
        mapOptions.SetGameStarted(false);
        Debug.Log("Instantiate after regame");
        if (LexNetwork.IsMasterClient)
        {
            LexPlayer randomPlayer =  LexNetwork.GetRandomHumanExceptMe();
            if (randomPlayer != null)
                LexNetwork.SetMasterClient(randomPlayer.actorID);
        }

        InstantiateMyself();
        UpdateReadyStatus();//I enter room
        mapOptions.UpdateSettingsUI();
    }
    private void InstantiateMyself()
    {
        Debug.Assert(localPlayerObject == null, "PLayer obj not removed");
        localPlayerObject = LexNetwork.Instantiate(PREFAB_STARTSCENE_PLAYERNAME, Vector3.zero, Quaternion.identity, 0, new object[] { false, LexNetwork.LocalPlayer.uid});
        localPlayerInfo = localPlayerObject.GetComponent<HUD_UserName>();
        string name = LexNetwork.NickName;
        CharacterType character = LexNetwork.LocalPlayer.GetProperty(Property.Character,(GameSession.instance.devMode)?GameSession.instance.debugChara : CharacterType.NONE);
        Team myTeam = LexNetwork.LocalPlayer.GetProperty(Property.Team, Team.HOME);
        localPlayerInfo.pv.RPC("ChangeName",   name);
        localPlayerInfo.pv.RPC("ChangeCharacter",   (int)character);
        localPlayerInfo.pv.RPC("SetTeam",   (int)myTeam);
        //StartCoroutine(ddd());
        charSelector.SetInformation(character);
        UpdateReadyStatus();
    }
    IEnumerator ddd() {
        while (true) {
            localPlayerInfo.pv.RPC("SetTeam", 1);
            yield return new WaitForSeconds(5f);
        }

    }


    internal void OnPlayerLeftRoom(LexPlayer newPlayer)
    {
        RemovePlayer(newPlayer.uid);

    }
    public void RemovePlayer(string uid) {
        Debug.Assert(playerDictionary.ContainsKey(uid), "Removing p doesnt exist");
        playerDictionary.Remove(uid);
        UpdateReadyStatus();
        debugUI();
    }

    [SerializeField] Transform playerListTransform;
    private void OnNewPlayerEnter(EventObject eo)
    {
        string id = eo.stringObj;
        HUD_UserName info = eo.goData.GetComponent<HUD_UserName>();
        if (playerDictionary.ContainsKey(id))
        {
            Debug.LogWarning("Add duplicate panel name?");
            playerDictionary[id] = info;
        }
        else
        {
            playerDictionary.Add(id, info);
        }
        eo.goData.GetComponent<Transform>().SetParent(playerListTransform, false);
        RebalanceTeam();
        UpdateReadyStatus();
        debugUI(); 
    }

    public void RebalanceTeam() {
        if (!GameSession.gameModeInfo.isTeamGame) return;
        foreach (var hud in playerDictionary.Values) {
            if (hud == null || !hud.gameObject.activeInHierarchy) continue;
            if (hud.controller.IsMine) {
                hud.ResetTeam();
            }
        }
    }

    void debugUI()
    {
        foundPlayers.Clear();
        foreach (var entry in playerDictionary.Keys)
        {
            foundPlayers.Add(entry);
        }

    }

    #region start game
    [SerializeField] Text readyButtonText;
    public void OnClick_Ready()
    {
        localPlayerInfo.pv.RPC("ToggleReady");
        bool ready = localPlayerInfo.GetReady();
        readyButtonText.text = (ready) ? "다른사람을 기다리는 중" : "준비되었음!";
        UpdateReadyStatus();
        if (readyPlayers == totalPlayers)
        {
            Debug.Log("Same number. start");
            pv.RPC("OnClick_ForceStart");
        }
    }
    [LexRPC]
    public void OnClick_ForceStart()
    {
        if (LexNetwork.IsMasterClient)
        {
            if (!CheckHalfAgreement()) return;
            if (!CheckTeamValidity()) return;
            //정식유저 룸프로퍼티 대기
            Debug.Log("Mastercleint push setting requested");
            mapOptions.SetGameStarted(true);
            mapOptions.PushRoomSettings();
        }
    }
    public bool CheckTeamValidity() {
        if (!GameSession.gameModeInfo.isTeamGame || GameSession.instance.devMode) return true;
        int numHome = LexNetwork.GetNumberInTeam(Team.HOME);
        int numAway = LexNetwork.GetNumberInTeam(Team.AWAY);
        if (numHome == 0 || numAway == 0)
        {

            ChatManager.SendNotificationMessage("최소 한명은 팀이 달라야합니다 장애인들아");
            return false;
        }
        else {
            return true;
        }
/*        Team masterTeam = (Team)LexNetwork.LocalPlayer.CustomProperties[Property.Team];
        Player[] players = LexNetwork.PlayerList;
        foreach (Player p in players)
        {
            Team away = (Team)p.CustomProperties[Property.Team];
            if (masterTeam != away) {
                return true;
            }
        }
        ChatManager.SendNotificationMessage("최소 한명은 팀이 달라야합니다 장애인들아");
        return false;*/
    }
    public bool CheckHalfAgreement()
    {
        if (readyPlayers < (totalPlayers) / 2 && GameSession.instance.requireHalfAgreement)
        {
            ChatManager.SendNotificationMessage(string.Format("{0}님이 강제시작을 하려다 실패하였습니다. 요구인원 :{1}", LexNetwork.MasterClient.NickName, (totalPlayers / 2)));
            return false;
        }
        return true;
    }

    public void OnRoomPropertiesChanged()
    {
        var Hash = LexNetwork.CustomProperties;
        bool gameStarted = Hash.Get<bool>(Property.GameStarted,false);
        mapOptions.SetGameStarted(gameStarted);
        //  Debug.Log("Start requested "+ gameStarted);
        if (gameStarted)
        {
            GameFieldManager.pv.RPC("SetGameMap",(int)GameSession.gameModeInfo.gameMode);
            Debug.Log("RPC Start game");
            StartGame();
        }
    }

  
    public void StartGame()
    {
        if (localPlayerObject != null)
        {
            LexNetwork.Destroy(localPlayerObject);
            localPlayerObject = null;
        }
        GetComponent<UI_BotLobby>().DestoryBotsPanel();
        
        EventManager.TriggerEvent(MyEvents.EVENT_SHOW_PANEL, new EventObject() { objData = ScreenType.InGame });
        EventManager.TriggerEvent(MyEvents.EVENT_GAME_STARTED, null);
    }

    #endregion

    #region helper

    int totalPlayers;
    int readyPlayers;
    private void UpdateReadyStatus(EventObject eo = null)
    {
        totalPlayers = LexNetwork.GetPlayerDictionary().Count;
        readyPlayers = 0;
        foreach (var entry in playerDictionary.Values)
        {
            if (entry.isReady)
            {
                readyPlayers++;
            }
        }
        numOfPlayers.text = "현재접속: " + totalPlayers + " / " + MenuManager.MAX_PLAYER_PER_ROOM;
        numReadyText.text = "준비: " + readyPlayers + " / " + totalPlayers;
        if (localPlayerInfo != null)
        {
            readyButtonText.text = (localPlayerInfo.GetReady()) ? "다른사람을 기다리는 중" : "준비되었음!";
        }
    }


    #endregion
}
