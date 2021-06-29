using Lex;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.EventSystems;

public class Unit_Player : MonoBehaviourLex
{
    [SerializeField] internal AudioClip hitAudio, shootAudio;
    public LexView pv;
    public CharacterType myCharacter;
    [SerializeField] SpriteRenderer myPortrait;
    internal HealthPoint health;
    internal SkillManager skillManager;
    internal Unit_Movement movement;
    internal BuffManager buffManager;
    public Transform gunTransform;
    [SerializeField]Animator gunAnimator;
    [SerializeField]EnemyIndicator enemyIndicator;
    public GameObject driverIndicator;
    public Controller controller;

    List<GameObject> myUnderlings = new List<GameObject>();
    Dictionary<int, HealthPoint> myProjectiles = new Dictionary<int, HealthPoint>();

    public Team myTeam = Team.HOME;
    public int fieldNo = 0;
    internal GameObject charBody;
    [SerializeField] CharacterBodyManager characterBodymanager;

    CircleCollider2D circleCollider;
    // Start is called before the first frame update
    private void Awake()
    {
        pv = GetComponent<LexView>();
        health = GetComponent<HealthPoint>();
        movement = GetComponent<Unit_Movement>();
        buffManager = GetComponent<BuffManager>();
        circleCollider = GetComponent<CircleCollider2D>();
        skillManager = GetComponent<SkillManager>();
        controller = GetComponent<Controller>();
    }
    private void OnDisable()
    {
        if (controller.IsLocal)
        {
            EventManager.StopListening(MyEvents.EVENT_PLAYER_KILLED_A_PLAYER, IncrementKill);
            EventManager.StopListening(MyEvents.EVENT_REQUEST_SUDDEN_DEATH, OnSuddenDeath);
            GameFieldManager.ChangeToSpectator();
        }
        circleCollider.radius = 0.33f;
        myProjectiles.Clear();
        EventManager.TriggerEvent(MyEvents.EVENT_PLAYER_DIED, new EventObject() { stringObj = controller.uid, intObj = fieldNo });
    }
    private void OnEnable()
    {
        myPortrait.color = new Color(1, 1, 1);
        myUnderlings.Clear();
        ParseInstantiationData();
        if (controller.IsLocal)
        {
            EventManager.StartListening(MyEvents.EVENT_REQUEST_SUDDEN_DEATH, OnSuddenDeath);
            EventManager.StartListening(MyEvents.EVENT_PLAYER_KILLED_A_PLAYER, IncrementKill);
            MainCamera.SetFollow(GameSession.GetInst().networkPos);
            MainCamera.FocusOnField(false);
            ChatManager.FocusField(false);
            if (GameSession.gameModeInfo.gameMode != GameMode.TeamCP)
            {
                ChatManager.SetInputFieldVisibility(false);
            }
            else
            {

                ChatManager.SetInputFieldVisibility(true);
            }
            UI_StatDisplay.SetPlayer(this);
        }
        GameFieldManager.gameFields[fieldNo].playerSpawner.RegisterPlayer(controller.uid, this);
        EventManager.TriggerEvent(MyEvents.EVENT_PLAYER_SPAWNED, new EventObject() { stringObj = controller.uid, goData = gameObject, intObj = fieldNo });
     
    }
    void ParseInstantiationData() {
        myCharacter = (CharacterType)pv.InstantiationData[0];
        myPortrait.sprite = ConfigsManager.unitDictionary[myCharacter].portraitImage;
        CheckCustomCharacter();
        int maxLife = (int)pv.InstantiationData[1];
    
        fieldNo = (int)pv.InstantiationData[2];
        LexDebug.Log(gameObject.name+ "Received field " + fieldNo);
        bool isBot = (bool)pv.InstantiationData[3];
        string uid = (string)pv.InstantiationData[4];
        controller.SetControllerInfo(isBot, uid);
        if (GameSession.gameModeInfo.isTeamGame)
        {
            maxLife += GetTeamBalancedLife(controller.Owner.GetProperty(Property.Team, Team.HOME), maxLife);
        }
        health.SetMaxLife(maxLife);
        myTeam = controller.Owner.GetProperty(Property.Team, Team.HOME);


        if (fieldNo < GameFieldManager.gameFields.Count) {
            movement.SetMapSpec(GameFieldManager.gameFields[fieldNo].mapSpec);
        }
        health.SetAssociatedField(fieldNo);
    }


    public SpriteRenderer mainSprite;
    void CheckCustomCharacter()
    {
        if (!(myCharacter == CharacterType.YASUMI
            || myCharacter == CharacterType.TSURUYA)
            )
        {
            charBody = myPortrait.gameObject;
            characterBodymanager.gameObject.SetActive(false);
            charBody.SetActive(true);
            mainSprite = myPortrait;
            return;
        }
        charBody = characterBodymanager.gameObject;
        myPortrait.gameObject.SetActive(false);
        charBody.SetActive(true);
        characterBodymanager.SetCharacterSkin(myCharacter);
        mainSprite = characterBodymanager.mainSprite;
    }

    private void OnSuddenDeath(EventObject obj)
    {
        enemyIndicator.SetTargetAsNearestEnemy();
    }
    [LexRPC]
    public void SetBodySize(float radius)
    {
        circleCollider.radius = radius;
    }
    [LexRPC]
    public void TriggerMessage(string msg)
    {
        if (!controller.IsLocal) return;
        EventManager.TriggerEvent(MyEvents.EVENT_SEND_MESSAGE, new EventObject(msg));
    }
    void Start()
    {
        StatisticsManager.RPC_AddToStat(StatTypes.KILL, controller.uid, 0);
        StatisticsManager.RPC_AddToStat(StatTypes.SCORE, controller.uid, 0);
        StatisticsManager.RPC_AddToStat(StatTypes.EVADE, controller.uid, 0);
    }  // Update is called once per frame

    private int GetTeamBalancedLife(Team myTeam, int maxLife) {
        int numMyTeam = LexNetwork.GetNumberInTeam(myTeam);
        int otherTeam = LexNetwork.GetNumberInTeam((myTeam == Team.HOME) ? Team.AWAY : Team.HOME);
        int underdogged = (otherTeam - numMyTeam) * maxLife;
        if (underdogged <= 0) return 0; //같거나 우리팀이 더 많음
        return underdogged / numMyTeam; //차이 /우리팀수 
    }


    [LexRPC]
    public void TriggerGunAnimation(string tag) {
        gunAnimator.SetTrigger(tag);
    }
    [LexRPC]
    public void SetGunAngle(float eulerAngle)
    {
        gunAnimator.transform.rotation = Quaternion.Euler(0, 0, eulerAngle);
    }
    public void SetMyProjectile(GameObject obj) {
        myUnderlings.Add(obj);
        obj.transform.SetParent(gunTransform,false);
        obj.transform.localPosition = Vector3.zero;

    }
    public void PlayHitAudio()
    {
        if (!health.controller.IsLocal) return;
        AudioManager.PlayAudioOneShot(hitAudio);
    }
    public void PlayShootAudio()
    {
        if (!health.controller.IsLocal) return;
        AudioManager.PlayAudioOneShot(shootAudio);

    }
    public void AddProjectile(int id, HealthPoint proj) {

        if (myProjectiles.ContainsKey(id))
        {
            myProjectiles[id] = proj;
        }
        else {
            myProjectiles.Add(id, proj);
        }
    }
    public void RemoveProjectile(int id) {
        if (myProjectiles.ContainsKey(id)) {
            myProjectiles.Remove(id);
        }
    }
    public void IncrementKill(EventObject eo)
    {
        if (controller.Equals(eo.stringObj)) {
            StatisticsManager.RPC_AddToStat(StatTypes.KILL,  controller.uid, 1);
            StatisticsManager.RPC_AddToStat(StatTypes.SCORE, controller.uid, 16);
            StatisticsManager.instance.AddToLocalStat(ConstantStrings.PREFS_KILLS, 1);
        }
    }
    public void IncrementEvasion()
    {
        if (controller.IsMine)
        {
            if (controller.IsLocal) {
                StatisticsManager.RPC_AddToStat(StatTypes.EVADE, controller.uid, 1);
                StatisticsManager.RPC_AddToStat(StatTypes.SCORE, controller.uid, 1);
                StatisticsManager.instance.AddToLocalStat(ConstantStrings.PREFS_EVADES, 1);
            }
            pv.RPC("AddBuff",   (int)BuffType.Cooltime, 0.2f, 5d);       //(int bType, float mod, double _duration)
        }
    }

    public void KillUnderlings() {
        for (int i = 0; i < myUnderlings.Count; i++) {
            if (myUnderlings[i] == null) continue;
            if (!myUnderlings[i].activeInHierarchy) continue;
            myUnderlings[i].GetComponent<HealthPoint>().Kill_Immediate();        
        }
    
    }

    internal bool FindAttackHistory(int tid)
    {
        foreach (var proj in myProjectiles.Values) {
            if (proj.damageDealer.duplicateDamageChecker.FindAttackHistory(tid)) return true;
        }
        return false;
    }

   
}
