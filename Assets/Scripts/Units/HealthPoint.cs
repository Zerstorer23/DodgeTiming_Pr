using Lex;
using System.Collections;
using UnityEngine;

public class HealthPoint : MonoBehaviourLex
{
    public UnitType unitType;
    public Controller controller;
    int maxLife = 1;
    public int currentLife;
    bool isDead = false;
    [SerializeField] bool invincibleFromBullets = false;
    public bool invincibleFromMapBullets = false;
    public bool dontKillByException = false;
    internal LexView pv;

    
    internal BuffManager buffManager;
    internal Unit_Player unitPlayer;
    internal Projectile_DamageDealer damageDealer;
    internal Component movement;
    public Team myTeam {
        get => controller.Owner.GetProperty(Property.Team, Team.HOME);
    }
    public int associatedField = 0;

    public string killerUID=null;

    private void Awake()
    {
        pv = GetComponent<LexView>();
        buffManager = GetComponent<BuffManager>();
        controller = GetComponent<Controller>();
        unitPlayer = GetComponent<Unit_Player>();
        damageDealer = GetComponent<Projectile_DamageDealer>();
        if (unitType == UnitType.Player)
        {
            movement = GetComponent<Unit_Movement>();
        }
        else {
            movement = GetComponent<Projectile_Movement>();
        }  
    }

    private void OnFieldFinish(EventObject obj)
    {
        if (!isDead && obj.intObj == associatedField)
        {
            Kill_Immediate();
        }
    }

    private void FixedUpdate()  
    {
        if (pv.IsMine && currentLife <= 0) {
            isDead = true;
            CheckUnderlings();
           // Debug.Log("Destroy called " + pv.ViewID + " / " + gameObject.name);
            LexNetwork.Destroy(pv);
        }
    }
    public void SetMaxLife(int life)
    {
        maxLife = life;
        currentLife = maxLife;
    }
    public int GetMaxLife()
    {
        return maxLife;
    }

    private void OnEnable()
    {
        EventManager.StartListening(MyEvents.EVENT_FIELD_FINISHED, OnFieldFinish);
        currentLife = maxLife;
        //    unitType = DetermineType();
        isDead = false;
        killerUID = null;
        if (unitType == UnitType.Projectile) currentLife = 1;
    }

    [LexRPC]
    public void SetInvincibleFromMapBullets(bool enable) {
        invincibleFromMapBullets = enable;
    }

    public void SetAssociatedField(int no) {
        associatedField = no;
    }

    private void OnDisable()
    {
        isDead = true;
        EventManager.StopListening(MyEvents.EVENT_FIELD_FINISHED, OnFieldFinish);
    }

    internal void HealHP(int amount)
    {
        if (amount > 0 && currentLife >= maxLife) return;

        if (pv.IsMine)
        {
            pv.RPC("ChangeHP",   amount);
        }
        
    }
    public bool IsInvincible() {
        return (invincibleFromBullets || buffManager.GetTrigger(BuffType.InvincibleFromBullets));    
    }
    int expectedlife;

   [LexRPC]
    internal void DoDamage(string attackerUserID, bool instaDeath)
    {
        if (isDead) return;
        CheckMirrorDamage(attackerUserID);
        if (IsInvincible()) return;
        if (pv.IsMine)
        {
            pv.RPC("ChangeHP",   -1);
        }
        if (unitType == UnitType.Player)
        {
            expectedlife = currentLife - 1;
            if (pv.IsMine)
            {
                LexNetwork.Instantiate(ConstantStrings.PREFAB_HEAL_1, transform.position, Quaternion.identity, 0);
                if (controller.IsLocal)
                {

                                MainCamera.instance.DoShake();
            #if UNITY_ANDROID && !UNITY_EDITOR
                                                Handheld.Vibrate();
            #endif
                                unitPlayer.PlayHitAudio();
                }
            }
            NotifySourceOfDamage(attackerUserID, instaDeath);
        }
    }

    private void CheckMirrorDamage(string attackerUserID)
    {
        if (!controller.IsMine) return;
        if (IsMirrorDamage())
        {
            Unit_Player unit= GameFieldManager.gameFields[associatedField].playerSpawner.GetUnitByControllerID(attackerUserID);
            if (unit == null) return;
            if (controller.IsLocal) {
                EventManager.TriggerEvent(MyEvents.EVENT_SEND_MESSAGE, new EventObject() { stringObj = unit.controller.Owner.NickName + "님에게 피해 반사" });
                unit.pv.RPC("TriggerMessage",   "피해가 반사되었습니다!");
            }
            buffManager.AddStat(BuffType.NumDamageReceivedWhileBuff,1);
            unit.pv.RPC("ChangeHP",   -1);
//            damageDealer.DoPlayerCollision(unit.gameObject);
        }
    }

    private bool IsMirrorDamage()
    {
        return buffManager.GetTrigger(BuffType.MirrorDamage);
    }

    public void Kill_Immediate() {
        //Kill not called by RPC
        if (!pv.IsMine || isDead) return;
        isDead = true;
        if (wkRoutine != null) StopCoroutine(wkRoutine);
        CheckUnderlings();
        LexNetwork.Destroy(pv);
    }
    void CheckUnderlings() {
        if (unitType == UnitType.Player) {
            unitPlayer.KillUnderlings();
        }
    }

    void NotifySourceOfDamage(string attackerUserID , bool instaDeath)
    {
        LexPlayer p = LexNetwork.GetPlayerByID(attackerUserID);
        string attackerNickname = (p == null) ? "???" : p.NickName;
        bool targetIsDead = (expectedlife <= 0 || instaDeath);
        if (controller.IsMine)
        {
            targetIsDead = (currentLife <= 0 || instaDeath);
            if (attackerUserID == null)
            { //AttackedByMapObject
                if (controller.IsLocal)
                {
                    EventManager.TriggerEvent(MyEvents.EVENT_SEND_MESSAGE, new EventObject() { stringObj = "회피실패" });
                }
                if (targetIsDead)
                {
                    if (killerUID == null)
                    {
                        killerUID = "mapobj";
                        ChatManager.SendNotificationMessage(LexNetwork.NickName + "님이 사망했습니다.", "#FF0000");
                    }
                }
            }
            else if (controller.IsLocal)
            {
                EventManager.TriggerEvent(MyEvents.EVENT_SEND_MESSAGE, new EventObject() { stringObj = string.Format("{0}에게 피격", attackerNickname) });
            }

        }
        else if (LexNetwork.LocalPlayer.uid == attackerUserID)
        {
            EventManager.TriggerEvent(MyEvents.EVENT_SEND_MESSAGE, new EventObject() { stringObj = string.Format("{0}를 타격..!", controller.Owner.NickName) });
            if (targetIsDead)
            {
                if (killerUID == null)
                {
                    killerUID = attackerUserID;
                    EventManager.TriggerEvent(MyEvents.EVENT_PLAYER_KILLED_A_PLAYER, new EventObject() { stringObj = attackerUserID, hitHealthPoint = this });
                    ChatManager.SendNotificationMessage(attackerNickname + " 님이 " + controller.Owner.NickName + "님을 살해했습니다.", "#FF0000");
                }
            }
        }

    }

   // IEnumerator deathCoroutine;
    IEnumerator wkRoutine;
    private IEnumerator WaitAndKill(float delay)
    {
        yield return new WaitForSeconds(delay);
        Kill_Immediate();
    }

    public void DoDeathAfter(float delay)
    {
        wkRoutine = WaitAndKill(delay);
        StartCoroutine(wkRoutine);
    }

    [LexRPC]
    public void ChangeHP(int a)
    {
        currentLife += a;
    }
    public bool IsMapProjectile() {
        if (unitType != UnitType.Projectile) return false;
        if (damageDealer == null) return false;
        return damageDealer.isMapObject;
    }

}
public enum UnitType
{
    NONE, Player, Projectile
}