using Lex;
using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class GameSession : MonoBehaviourLex
{
    [SerializeField] Text versionText;
    [SerializeField] public Transform networkPos;
    public UI_TournamentPanel tournamentPanel;
    public Transform Home_Bullets;
    public UI_SkillBox skillPanelUI;
    public UI_Leaderboard leaderboardUI;
    public GameOverManager gameOverManager;
    public static ConfigsManager configsManager;
    public string versionCode;
    public static double STANDARD_PING = 0.08d;//0.075d;//자연스럽게 보이는 한 가장 크게
    public bool requireHalfAgreement = true;

    private static GameSession prGameSession;

    public static GameModeConfig gameModeInfo;
    public static int LocalPlayer_FieldNumber = -1;
    LexView pv;

    public float gameSpeed = 1f;
    public bool devMode = false;
    public CharacterType debugChara = CharacterType.NONE;

    public static bool auto_drive_enabled = false;
    public static bool auto_drive_toggled = false;
    private void FixedUpdate()
    {
        Time.timeScale = gameSpeed;
    }
    public static bool IsAutoDriving() {
        return auto_drive_toggled && auto_drive_enabled;
    }
    public static void toggleAutoDriveByKeyInput() {
        auto_drive_toggled = !auto_drive_toggled;
        Debug.Log("Driving " + auto_drive_toggled);
    }

    public static GameSession instance
    {
        get
        {
            if (!prGameSession)
            {
                prGameSession = FindObjectOfType<GameSession>();
                if (!prGameSession)
                {
                }
            }

            return prGameSession;
        }
    }
    public static GameSession GetInst() {
        return instance;
    }
    public static Transform GetBulletHome() => instance.Home_Bullets;
    private void Awake()
    {
        auto_drive_enabled = devMode;
        auto_drive_toggled = devMode;
        requireHalfAgreement = !devMode;

        pv = GetComponent<LexView>();
        configsManager = GetComponent<ConfigsManager>();
        versionText.text = versionCode + " 버전";
        EventManager.StartListening(MyEvents.EVENT_GAME_STARTED, OnGameStarted);
        EventManager.StartListening(MyEvents.EVENT_GAME_FINISHED, OnGameFinished);
    }

    public static bool gameStarted = false;
    private void OnGameFinished(EventObject obj)
    {
        gameStarted = false;
    }

    private void OnGameStarted(EventObject obj)
    {
        gameStarted = true;
    }

    private void Start()
    {
        EventManager.TriggerEvent(MyEvents.EVENT_SHOW_PANEL, new EventObject() { objData = ScreenType.PreGame });
    }


    internal static string GetVersionCode()
    {
        return instance.versionCode;
    }
    public static float GetAngle(Vector2 vec1, Vector2 vec2)
    {
        Vector2 diference = vec2 - vec1;
        float sign = (vec2.y < vec1.y) ? -1.0f : 1.0f;
        return Vector2.Angle(Vector2.right, diference) * sign;
    }
    public static void PushRoomASetting(Property key, object value) {
        var hash = new LexHashTable();
        hash.Add(key, value);
        LexNetwork.SetRoomCustomProperties(hash);
    }

    internal static void SetLocalPlayerFieldNumber(int myRoom)
    {
        LocalPlayer_FieldNumber = myRoom;
    }

    public static void ShowMainMenu() {
        instance.pv.RPC("ShowPanel");
    }
    [LexRPC]
    void ShowPanel()
    {
        EventManager.TriggerEvent(MyEvents.EVENT_SHOW_PANEL, new EventObject() { objData = ScreenType.PreGame });
        EventManager.TriggerEvent(MyEvents.EVENT_GAME_CYCLE_RESTART, null);
    }
    [LexRPC]
    public void ResignMaster(int newMaster) {
        if (LexNetwork.IsMasterClient) {
            LexNetwork.SetMasterClient(newMaster);
        }
    }
    [LexRPC]
    public void LeaveRoom()
    {
        Debug.LogWarning("Leave room");
        //LexNetwork.RemoveRPCs(LexNetwork.LocalPlayer);
    }
    [LexRPC]
    public void QuitGame()
    {
        Application.Quit();
    }
    public static IEnumerator CheckCoroutine(IEnumerator routine, IEnumerator newRoutine) {
        if (routine != null) {
            instance.StopCoroutine(routine);
        }
        return newRoutine;
    }

    public static CharacterType GetPlayerCharacter(LexPlayer player)
    {
        if (!player.HasProperty(Property.Character)) return CharacterType.NONE;
        CharacterType character = player.GetProperty<CharacterType>(Property.Character);
        if (character == CharacterType.NONE)
        {
            if (!player.HasProperty(Property.RealCharacter)) return CharacterType.NONE;
            character = player.GetProperty<CharacterType>(Property.RealCharacter);
        }
        return character;
    }
    void OnApplicationPause(bool paused)
    {
        if (Application.platform == RuntimePlatform.Android) {
            if (!GooglePlayManager.loggedIn) return;
            if (paused)
            {

                Application.Quit();
            }
        }
    }
    private void OnApplicationQuit()
    {
        try
        {

            PlayerPrefs.Save();
        }
        catch (Exception e) {
            Debug.LogWarning(e.Message);
        }
    }

}
