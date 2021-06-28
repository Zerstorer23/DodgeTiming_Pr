using Lex;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static ConstantStrings;

public class Projectile_DamageDealer : MonoBehaviourLex
{
    Projectile projectile;
    Projectile_Movement movement;
   internal HealthPoint myHealth;
    [SerializeField] bool canKillBullet = false;
    public bool isMapObject = false;
    public bool givesDamage = true;
    LexView pv;
    public List<BuffData> customBuffs = new List<BuffData>();
    // [SerializeField] bool nonRecyclableDamage = false;

    public Collider2D myCollider;
    bool hasCustomCollider = false;

    public DamageManifoldType damageManifold = DamageManifoldType.Once;
    public IDamageManifold duplicateDamageChecker = null;

    private void Awake()
    {
        duplicateDamageChecker = DamageManifold.SetDamageManifold(damageManifold);
        FindCollider();
        projectile = GetComponent<Projectile>();
        movement = GetComponent<Projectile_Movement>();
        myHealth = GetComponent<HealthPoint>();
        pv = GetComponent<LexView>();
        Debug.Assert(projectile != null, "Where is projectile");
        hasCustomCollider = GetComponent<Projectile_CustomDamageDealer>() != null;
    }

    private void OnDisable()
    {
        if (pv.IsMine && !isMapObject && hitCount <=0) {
            EventManager.TriggerEvent(MyEvents.EVENT_MY_PROJECTILE_MISS, new EventObject() { stringObj = myHealth.controller.uid });
        }
        myCollider.enabled = false;
        duplicateDamageChecker.Reset();//TODO MEMO moved to Disable
        hitCount = 0;
    }
    private void OnEnable()
    {
        myCollider.enabled = true;
    }
    void FindCollider()
    {
        myCollider = GetComponent<PolygonCollider2D>();
        if (myCollider == null)
        {
            myCollider = GetComponent<CircleCollider2D>();
        }
        if (myCollider == null)
        {
            myCollider = GetComponent<BoxCollider2D>();
        }
        if (myCollider == null)
        {
            myCollider = GetComponent<CapsuleCollider2D>();
        }
    }

  
    [LexRPC]
    public void ToggleDamage(bool enable) {
        givesDamage = enable;
    }

    // Start is called before the first frame update
    private void OnTriggerEnter2D(Collider2D collision)
    {
        string tag = collision.gameObject.tag;
       // Debug.Log(gameObject.name + "Trigger with " + collision.gameObject.name+" / tag "+tag);
        switch (tag)
        {
            case TAG_PLAYER:
                if (!hasCustomCollider) {
                    DoPlayerCollision(collision.gameObject);
                }
                break;
            case TAG_PROJECTILE:
                    DoProjectileCollision(collision.gameObject);
                break;
        }

    }
    private void OnCollisionEnter2D(Collision2D collision)
    {
        string tag = collision.gameObject.tag;
        ContactPoint2D contact;        
       // Debug.Log(gameObject.name + "Collision with " + collision.gameObject.name + " / tag " + tag);
        switch (tag)
        {
            case TAG_PLAYER:
                //Kill him and me
                if (!hasCustomCollider)
                {
                    DoPlayerCollision(collision.gameObject);
                    if (isMapObject) myHealth.Kill_Immediate();
                }
                break;
            case TAG_PROJECTILE:
                bool damageGiven = DoProjectileCollision(collision.gameObject);
                if (movement.reactionType == ReactionType.Bounce)
                {
                    contact = collision.GetContact(0);
                    DoBounceCollision(contact, collision.gameObject.transform.position);
                    //boxcast
                }
                else if (movement.reactionType == ReactionType.Die && damageGiven)
                {
                    Projectile_DamageDealer targetdd = collision.gameObject.GetComponent<Projectile_DamageDealer>();
                    if (targetdd.isMapObject && myHealth.invincibleFromMapBullets) break;
                    myHealth.Kill_Immediate();
                }
                break;
            case TAG_BOX_OBSTACLE:
            case TAG_WALL:
                //Bounce
                contact = collision.GetContact(0);
                //   Debug.Log("Contact at" + contact.point);
                DoBounceCollision(contact, collision.gameObject.transform.position);
                break;

        }
    }

    bool CheckValidDamageEvaluation(HealthPoint otherHP) {
        if (isMapObject)
        {
            return otherHP.controller.IsMine; // 맵오브젝트는 각자 알아서
        }
        else
        {
            return myHealth.controller.IsMine;
            //if (!pv.IsMine) return false; // 개인투사체는 주인이처리
        }
    }
    int hitCount = 0;
    public bool DoProjectileCollision(GameObject targetObj)
    {
        HealthPoint otherHP = targetObj.GetComponent<HealthPoint>();
        TriggerHit(otherHP);
        if (!CheckValidTeam(otherHP)) return false;
        GiveDamage(otherHP);
        return true;
    }

    public void DoPlayerCollision(GameObject targetObj)
    {
        HealthPoint otherHP = targetObj.GetComponent<HealthPoint>();
        if (!CheckValidDamageEvaluation(otherHP)) return ;
        if (!CheckManifoldDamage(otherHP)) return;
        TriggerHit(otherHP);
        if (!CheckValidTeam(otherHP)) return;
        ApplyBuff(otherHP);
        if (!givesDamage) return;
        GiveDamage(otherHP);
    }
    bool CheckValidTeam(HealthPoint otherHP) {
        if (GameSession.gameModeInfo.isTeamGame)
        {
            if (isMapObject || otherHP.IsMapProjectile())
            {
                return true;//맵 ->아무거나 무조건 딜
            }
            return (otherHP.myTeam != myHealth.myTeam); //그외 팀구분

        }
        else if (GameSession.gameModeInfo.isCoop)
        {

            if (isMapObject)
            {
                return true;//맵 ->아무거나 무조건 딜
            }
            else
            {
                return otherHP.IsMapProjectile();//아무거나 -> 맵 무조건 딜
            }


        }
        else if (!isMapObject)
        {
            if (otherHP.IsMapProjectile())
            {
                return true;
            }
            else
            {
                return myHealth.controller.uid != otherHP.controller.uid;
            }
        }
        else {
            return true;
        }
    }
    bool CheckManifoldDamage(HealthPoint otherHP) {
        if (!givesDamage) return true;
        int tid = otherHP.gameObject.GetInstanceID();
        return duplicateDamageChecker.CheckDuplicateDamage(tid);

    }

    void ApplyBuff(HealthPoint otherHP) {
        if (customBuffs.Count <= 0) return;
        BuffManager targetManager = otherHP.buffManager;
        if (targetManager == null || !targetManager.gameObject.activeInHierarchy) return;
        foreach (BuffData buff in customBuffs)
        {
            targetManager.pv.RPC("AddBuff",   (int)buff.buffType, buff.modifier, buff.duration);
        }
    }


    void DoBounceCollision(ContactPoint2D contact, Vector3 collisionPoint)
    {
        if (movement == null) return;
        movement.Bounce(contact, collisionPoint); 
    }

    public void GiveDamage(HealthPoint otherHP)
    {
        if (otherHP.unitType == UnitType.Player)
        {
            string sourceID = (isMapObject) ? null : myHealth.controller.uid;
      //      Debug.Log("Damage player");
            otherHP.pv.RPC("DoDamage",   sourceID, false);
            if (isMapObject) myHealth.Kill_Immediate();
        }
        else if(canKillBullet){

            if (otherHP.IsMapProjectile())
            {
                otherHP.Kill_Immediate();
            }
            if (movement.reactionType == ReactionType.Die)
            {
                if (otherHP.IsMapProjectile() && myHealth.invincibleFromMapBullets) return;
                myHealth.Kill_Immediate();
            }
        }
        return ;
    }

    public void TriggerHit(HealthPoint otherHP ){
        if (pv.IsMine && !isMapObject) {
            EventManager.TriggerEvent(MyEvents.EVENT_MY_PROJECTILE_HIT, new EventObject() { sourceDamageDealer = this, hitHealthPoint = otherHP });
            hitCount++;
        }

    }

}
