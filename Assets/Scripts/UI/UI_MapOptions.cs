using Lex;
using UnityEngine;
using UnityEngine.UI;
using static ConstantStrings;

public class UI_MapOptions : MonoBehaviourLex
{
    //*****GAME SETTING***//
    public static int default_lives_index = 1;
    public static MapDifficulty default_difficult = MapDifficulty.Easy;
   // public int playerLives;
   // public MapDifficulty mapDifficulty;
    bool gameStarted = false;
    LexView pv;


    [SerializeField] Text mapDiffText;
    [SerializeField] Text playerDiffText;
    [SerializeField] Dropdown mapDiffDropdown;
    [SerializeField] Dropdown livesDropdown;
    [SerializeField] Dropdown gamemodeDropdown;
    [SerializeField] GameObject[] masterOnlyObjects;
   public MapDifficulty mapDiff;
   public int livesIndex =0;

    public static int[] lives = new int[] { 1, 3, 5 };

    public void SetGameStarted(bool enable) {
        gameStarted = enable;
    }
    public bool GetGameStarted() => gameStarted;

    private void Awake()
    {
        pv = GetComponent<LexView>();
        foreach (var go in masterOnlyObjects)
        {
            go.SetActive(false);
        }
    }
    private void OnEnable()
    {
        UpdateSettingsUI();
    }
    public void OnDropdown_MapDifficulty()
    {
        if (LexNetwork.IsMasterClient)
        {
            int index = mapDiffDropdown.value;
            pv.RPC("SetMapDifficulty",   index);
            ChatManager.SendNotificationMessage(string.Format("난이도가 {0}로 변경되었습니다.", mapDiff));
        }
    }
    public void OnDropdown_PlayerDifficulty()
    {
        if (LexNetwork.IsMasterClient)
        {
            int index = livesDropdown.value;
            pv.RPC("SetPlayerLives",   index);
            ChatManager.SendNotificationMessage(string.Format("라이프가 {0}로 변경되었습니다.", lives[livesIndex]));
        }
    }
    public void OnDropdown_GameMode()
    {
        if (LexNetwork.IsMasterClient)
        {
            int index = gamemodeDropdown.value;
            pv.RPC("SetGameMode",   index);
            ChatManager.SendNotificationMessage(string.Format("게임모드가 {0}로 변경되었습니다.", (GameMode)index));
        }
    }
    public void UpdateSettingsUI()
    {
        bool isMaster = LexNetwork.LocalPlayer.IsMasterClient;
        foreach (var go in masterOnlyObjects) {
            go.SetActive(isMaster);
        }


        mapDiffDropdown.interactable = isMaster;
        livesDropdown.interactable = isMaster;
        gamemodeDropdown.interactable = isMaster;
        mapDiffDropdown.SetValueWithoutNotify((int)mapDiff);
        livesDropdown.SetValueWithoutNotify((int)livesIndex);
        int gmode = 0;
        if (GameSession.gameModeInfo != null) {
            gmode = (int)GameSession.gameModeInfo.gameMode;
        }
        gamemodeDropdown.SetValueWithoutNotify(gmode);
    }

    internal void LoadRoomSettings()
    {
        var hash = LexNetwork.CustomProperties;
        mapDiff = hash.Get(Property.MapDifficulty,MapDifficulty.Easy);
        livesIndex= hash.Get(Property.PlayerLives,1);
        string versionCode = hash.Get(Property.VersionCode, "0000");
        if (versionCode != GameSession.GetVersionCode())
        {
            Debug.Log("Received Wrong " + versionCode); 
            LexNetwork.NickName = string.Format(
            "<color=#ff0000>클라이언트 버전</color>이 맞지않습니다. 방장 버전 {0}",
             versionCode);
        }
        gameStarted = hash.Get<bool>(Property.GameStarted);
        GameSession.gameModeInfo = ConfigsManager.gameModeDictionary[hash.Get<GameMode>(Property.GameMode)];
        Debug.Log("난입유저 룸세팅 동기화 끝");
        UpdateSettingsUI();
    }

    public void OnClick_ChangeTeam()
    {
        if (UI_PlayerLobbyManager.localPlayerInfo == null) return;
        if (!GameSession.gameModeInfo.isTeamGame) return;
        UI_PlayerLobbyManager.localPlayerInfo.pv.RPC("ToggleTeam");
    }
    public void OnClick_AnonGame()
    {
        if (!LexNetwork.IsMasterClient) return;
        HUD_UserName[] users = FindObjectsOfType<HUD_UserName>();
        foreach (var user in users) {
            user.pv.RPC("ChangeName",   "ㅇㅇ");
        }
    }
    [LexRPC]
    public void SetMapDifficulty(int diff) {
        mapDiff = (MapDifficulty)diff;
        UpdateSettingsUI();
    }

    [LexRPC]
    public void SetPlayerLives(int index)
    {
        livesIndex =index;
        UpdateSettingsUI();
    }
    [LexRPC]
    public void SetGameMode(int index)
    {
        GameSession.gameModeInfo = ConfigsManager.gameModeDictionary[(GameMode)index];
        EventManager.TriggerEvent(MyEvents.EVENT_GAMEMODE_CHANGED, new EventObject() { objData = GameSession.gameModeInfo });
        UpdateSettingsUI();
    }
    public static LexHashTable GetInitOptions()
    {
        LexHashTable hash = new LexHashTable();
        hash.Add(Property.MapDifficulty, default_difficult);
        hash.Add(Property.PlayerLives, default_lives_index);
        hash.Add(Property.VersionCode, GameSession.GetVersionCode());
        hash.Add(Property.GameMode, GameMode.PVP);
        hash.Add(Property.GameStarted, false);
        hash.Add(Property.GameAuto, false);
        hash.Add(Property.RandomSeed, UnityEngine.Random.Range(0,133));
        return hash;
    }
    public void PushRoomSettings()
    {
        LexHashTable hash = new LexHashTable(); 
        hash.Add(Property.MapDifficulty, mapDiff);
        hash.Add(Property.PlayerLives, livesIndex);
        hash.Add(Property.VersionCode, GameSession.GetVersionCode());
        hash.Add(Property.GameStarted, gameStarted);
        hash.Add(Property.GameMode, GameSession.gameModeInfo.gameMode);
        LexNetwork.SetRoomCustomProperties(hash);
    }


}
