using UnityEngine;
using Lex;
using UnityEngine.UI;
using static ConstantStrings;

public class MenuManager : MonoBehaviourLexCallbacks
{
    public static int MAX_PLAYER_PER_ROOM = 18;
    [SerializeField] GameObject loadingChuu;
    [SerializeField] GameObject[] disableInLoading;
    const string PRIMARY_ROOM = "PrimaryRoom";
    [SerializeField] UI_PlayerLobbyManager lobbyManager;
    [SerializeField] UI_MapOptions mapOptions;

    private void Start()
    {
        if (!LexNetwork.IsConnected) {
            DoLoading();
        }
    }

    private void DoLoading()
    {
        loadingChuu.SetActive(true);
        foreach (GameObject go in disableInLoading)
        {
            go.SetActive(false);
        }
        LexNetwork.ConnectUsingSettings();
    }


    public void OnClickLeaderBoard() {
        GooglePlayManager.ShowLeaderBoard();
    }
    public void InitRoomSettings() {
        if (LexNetwork.IsMasterClient && LexNetwork.PlayerCount == 1)
        {
            var hash = UI_MapOptions.GetInitOptions();
            LexNetwork.SetRoomCustomProperties(hash);
        }
    }

    public override void OnJoinedRoom()
    {
        InitRoomSettings();
        LexNetwork.NickName = UI_ChangeName.default_name;
        loadingChuu.SetActive(false);
        foreach (GameObject go in disableInLoading)
        {
            go.SetActive(true);
        }
        lobbyManager.ConnectedToRoom();
    }

    [SerializeField] Toggle autoToggle;
    public void OnAutoToggle()
    {
        GameSession.auto_drive_enabled = autoToggle.isOn;
    }
    public override void OnRoomSettingsChanged(LexHashTable hashChanged)
    {
        lobbyManager.OnRoomPropertiesChanged();
    }
    public override void OnPlayerLeftRoom(LexPlayer newPlayer)
    {
        lobbyManager.OnPlayerLeftRoom(newPlayer);
        mapOptions.UpdateSettingsUI(); //Player Leave room
    }
    public override void OnMasterClientSwitched(int masterID)
    {
        Debug.Log("Master changed");
        if (LexNetwork.IsMasterClient) {
            GameSession.PushRoomASetting(Property.RandomSeed, Random.Range(0, 133));
        }
        LexNetwork.LocalPlayer.SetCustomProperties(Property.RandomSeed, Random.Range(0, 133));
        mapOptions.UpdateSettingsUI(); //Player Leave room
    }


    public void OnClick_OpenSettings() {
        EventManager.TriggerEvent(MyEvents.EVENT_POP_UP_PANEL, new EventObject() { objData = ScreenType.Settings, boolObj = true });
    
    }
}


public enum MapDifficulty
{
    None = 0, BoxOnly,Easy, Standard, Hard
}
