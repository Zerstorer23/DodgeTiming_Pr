using Lex;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static ConstantStrings;

public class Projectile_Homing : MonoBehaviourLex
{
    int oldtargetID =-1;
    public GameObject homingTarget;
    public float homingDetectRange = -1f;
    public bool findProjectile = true;
    public bool onlyFollowHostile = false;
    public float homingRotationSpeed = -1f;//Immediate
    Projectile projectile;
    public ReactionType homingReaction = ReactionType.None;
    HealthPoint health;
    LayerMask findMask;
    [SerializeField] int numBounce = 0;
    int maxBounce = 3;
    private void OnDrawGizmos()
    {
        if (homingTarget != null)
        {
            Gizmos.color = Color.cyan;
            Gizmos.DrawWireSphere(homingTarget.transform.position, 0.8f);
        }
        Gizmos.DrawWireSphere(transform.position, homingDetectRange);
    }
    // Update is called once per frame
    private void Awake()
    {
        projectile = GetComponent<Projectile>();
        health = GetComponent<HealthPoint>();
        findMask = (findProjectile) ? LayerMask.GetMask(TAG_PLAYER, TAG_PROJECTILE)
                                  : LayerMask.GetMask(TAG_PLAYER);
    }
    [LexRPC]
    public void SetHomingTarget(int pvID) {
        //    numBounce++;
      LexView targetPv =   LexNetwork.GetLexView(pvID);
        if (targetPv != null)
        {
            homingTarget = targetPv.gameObject;
            projectile.SetColor(Color.red);
        }
        else {
            homingTarget = null;
            projectile.SetColor(Color.white);
        }
    }
    [LexRPC]
    public void SetHomingInformation(int reaction, float detectRange, float rotationSpeed)
    {
        homingReaction = (ReactionType)reaction;
        homingDetectRange = detectRange;
        homingRotationSpeed = rotationSpeed;

    }

    public bool homingEnabled = true;

    [LexRPC]
    public void EnableHoming(bool enable)
    {
        homingEnabled = enable;
        if (!homingEnabled) {
            homingTarget = null;
        }
    }

    private void OnEnable()
    {
        if (lexView.IsMine) {
            EventManager.StartListening(MyEvents.EVENT_MY_PROJECTILE_HIT, OnProjectileHit);
        }
    }
    bool changeImmediate = false;
    private void OnProjectileHit(EventObject eo)
    {
        if (eo.sourceDamageDealer.lexView.ViewID != lexView.ViewID) return;
        if (homingTarget == null || !homingTarget.activeInHierarchy) return;
        if (eo.hitHealthPoint == null || !eo.hitHealthPoint.gameObject.activeInHierarchy) return;
        if (numBounce >= maxBounce)
         {
            homingTarget = null;
            projectile.SetColor(Color.white);
            return;
        }
        numBounce++;
        if (eo.hitHealthPoint.lexView.ViewID != homingTarget.GetComponent<LexView>().ViewID) return;
        // oldtargetID = eo.hitHealthPoint.pv.ViewID;
        if (homingReaction == ReactionType.Die)
        {
            health.Kill_Immediate();
        }
        else if (homingReaction == ReactionType.Bounce)
        {
      //      Debug.Log("Hit and Change");
            oldtargetID = homingTarget.GetComponent<LexView>().ViewID;
            homingTarget = null;
            changeImmediate = true;
            FindNearByObjects();
        }
    }

    private void OnDisable()
    {
        if (lexView.IsMine)
        {

            EventManager.StopListening(MyEvents.EVENT_MY_PROJECTILE_HIT, OnProjectileHit);
        }
        homingTarget = null;
        oldtargetID = -1;
        numBounce = 0;
    }

    public float AdjustDirection(Vector3 projPosition, float currAngle) {
        if (homingTarget == null) {
            return currAngle;
        }
        float angleToTarget = GetAngleBetween(projPosition, homingTarget.transform.position);
        if (changeImmediate) {
            changeImmediate = false;
            return angleToTarget;
        }
     //   Debug.Log("I at " + projPosition + " target " + homingTarget.transform.position+" angle "+angleToTarget);
        if (homingRotationSpeed < 0) return angleToTarget;
        float rotateAmount = homingRotationSpeed * Time.deltaTime;
        if (currAngle == angleToTarget)
        {
            return currAngle;
        }

        Vector3 anti = GetAngledVector(currAngle + rotateAmount, 1f);
        Vector3 clock = GetAngledVector(currAngle - rotateAmount, 1f);
        Vector3 target = GetAngledVector(angleToTarget, 1f);
        float antiDist = Vector2.Distance(target, anti);
        float clockDist = Vector2.Distance(target, clock);

        bool moveClock = (clockDist < antiDist); 
      //  Debug.Log(angleToTarget+" vs Curr angle  " + currAngle + " clock av " + moveClock + " rotateAmount " + rotateAmount+ " clockDist " + clockDist + " / antiDist" + antiDist);
        if (moveClock)
        {
            return currAngle - rotateAmount;
        }
        else {
            return currAngle + rotateAmount;
        } 
    }
    private void FixedUpdate()
    {
        if (!lexView.IsMine || !homingEnabled) return;
        if (homingDetectRange <= 0) return;
        if (numBounce >= maxBounce) return;
        bool targetLost = CheckTargetLost();
        if (targetLost) {
            FindNearByObjects();
        
        }
    }
    bool CheckTargetLost() {
        if (homingTarget == null) return true;
        if (!homingTarget.activeInHierarchy) {
            oldtargetID = homingTarget.GetComponent<LexView>().ViewID;
            homingTarget = null;
            return true;
        }
        if (Vector2.Distance(homingTarget.transform.position, transform.position) > homingDetectRange)
        {
            oldtargetID = homingTarget.GetComponent<LexView>().ViewID;
            homingTarget = null;
            return true;
        }
        return false;
    }
    void FindNearByObjects() {

        Collider2D[] collisions = Physics2D.OverlapCircleAll(transform.position, homingDetectRange, findMask);
        LexView nearestPV = null;
        float nearestDist = 0;
        LexView nearestProjPV = null;
        float nearesProjtDist = float.MaxValue;

        for (int i = 0; i < collisions.Length; i++)
        {
            Collider2D c = collisions[i];

            HealthPoint targetHP = c.gameObject.GetComponent<HealthPoint>();
            if (targetHP == null) continue;
            if (targetHP.pv.ViewID == oldtargetID || targetHP.pv.ViewID == lexView.ViewID) continue;
            float dist = Vector2.Distance(c.gameObject.transform.position, transform.position);
            switch (c.gameObject.tag)
            {
                case TAG_PLAYER:
                    if (targetHP.pv.IsMine && oldtargetID == -1) {
                        oldtargetID = targetHP.pv.ViewID;
                        continue; 
                    }
                    if (onlyFollowHostile)
                    {
                        if (GameSession.gameModeInfo.isTeamGame && targetHP.myTeam == health.myTeam) continue;
                        if (GameSession.gameModeInfo.isCoop) continue;
                    }
                   
                    if (nearestPV == null || dist < nearestDist)
                    {
                        nearestPV = targetHP.pv;
                        nearestDist = dist;
                    }
                    break;
                case TAG_PROJECTILE:
                    if (!findProjectile) continue;
                    if (!targetHP.IsMapProjectile() && targetHP.pv.IsMine) continue;
                    if (onlyFollowHostile)
                    {
                        if (GameSession.gameModeInfo.isTeamGame && targetHP.myTeam == health.myTeam) continue;
                        if (GameSession.gameModeInfo.isCoop && !targetHP.damageDealer.isMapObject) continue;
                    }
                    if (nearestPV == null && dist < nearesProjtDist)
                    {
                        nearestProjPV = targetHP.pv;
                        nearesProjtDist = dist;
                    }
                    break;
            }
          
        }
        if (nearestPV == null) {
            nearestPV = nearestProjPV;
        }
        if (nearestPV != null)
        {
            homingTarget = nearestPV.gameObject;
            lexView.RPC("SetHomingTarget",   nearestPV.ViewID);
        }
        else
        {
            projectile.SetColor(Color.white);
        }

    }

}
