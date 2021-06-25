using Lex;
using UnityEngine;
using UnityEngine.UI;
using static ConstantStrings;

public class HUD_UserName : MonoBehaviourLex
{
    public LexView pv;
    public bool isReady = false;

    [SerializeField] Image readySprite;
    [SerializeField] Text nameText;
    [SerializeField] Image charPortrait;
    Image teamColorImage;
    string playerName = "ㅇㅇ";
    CharacterType selectedCharacter = CharacterType.HARUHI;
    Team myTeam = Team.HOME;
    public Controller controller;
    private void Awake()
    {
        pv = GetComponent<LexView>();
        teamColorImage = GetComponent<Image>();
        controller = GetComponent<Controller>();
        teamColorImage.enabled = false;
    }
    private void OnEnable()
    {
        EventManager.StartListening(MyEvents.EVENT_GAMEMODE_CHANGED, OnGamemodeChanged);
        playerName = controller.Owner.NickName;

        bool isBot = (bool)pv.InstantiationData[0];
        string uid = (string)pv.InstantiationData[1];
        controller.SetControllerInfo(isBot, uid);
        isReady = controller.Owner.IsBot;
        UpdateUI();
        EventManager.TriggerEvent(MyEvents.EVENT_PLAYER_JOINED, new EventObject() { stringObj = uid, goData = gameObject });
    }
    private void OnDisable()
    {
        EventManager.StopListening(MyEvents.EVENT_GAMEMODE_CHANGED, OnGamemodeChanged);

    }

    private void OnGamemodeChanged(EventObject arg0)
    {
        ResetTeam();
    }
    public void ResetTeam() {
        if (!gameObject.activeInHierarchy) return;
        if (controller.IsMine)
        {
            if (GameSession.gameModeInfo.isTeamGame)
            {
                int index = LexNetwork.GetMyIndex(controller.Owner, LexNetwork.PlayerList);
                myTeam = (Team)(index % 2 + 1);
            }
            pv.RPC("SetTeam",   (int)myTeam);
        }
    }

    [LexRPC]
    public void SetTeam(int teamNumber)
    {
        myTeam = (Team)teamNumber;
        if (controller.IsMine)
        {
           controller.Owner.SetCustomProperties(Property.Team, myTeam);
        }
        UpdateUI();
    }
    [LexRPC]
    public void ToggleTeam()
    {
        myTeam = (myTeam == Team.HOME) ? Team.AWAY : Team.HOME;
        if (controller.IsMine)
        {
            controller.Owner.SetCustomProperties(Property.Team, myTeam);
        }
        UpdateUI();
    }
    [LexRPC]
    public void ChangeCharacter(int character)
    {
        selectedCharacter = (CharacterType)character;
        if (controller.IsMine)
        {
            controller.Owner.SetCustomProperties(Property.Character, selectedCharacter);
        }
        UpdateUI();
    }

    [LexRPC]
    public void ChangeName(string text)
    {
        playerName = text;
        if (controller.IsMine)
        {
            controller.Owner.NickName = playerName;
        }
        UpdateUI();
    }

    [LexRPC]
    public void ToggleReady()
    {
        isReady = !isReady;
        EventManager.TriggerEvent(MyEvents.EVENT_PLAYER_TOGGLE_READY, new EventObject());
        UpdateUI();
    }


    public bool GetReady() {
        return isReady;
    }
    public void UpdateUI()
    {
        nameText.text = playerName;
        readySprite.color = (isReady) ? Color.green : Color.black;
        charPortrait.sprite = ConfigsManager.unitDictionary[selectedCharacter].portraitImage;

        if (GameSession.gameModeInfo.isTeamGame)
        {
            teamColorImage.enabled = true;
            teamColorImage.color = GetColorByHex(team_color[(int)myTeam]);
        }
        else
        {
            teamColorImage.enabled = false;
        };
    }

}
