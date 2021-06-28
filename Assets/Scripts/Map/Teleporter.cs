using Lex;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using static ConstantStrings;

public class Teleporter : MonoBehaviour
{
    [SerializeField] Teleporter otherSide;
    [SerializeField] Text coolText;
    [SerializeField] Image directionIndicator;
    [SerializeField] SpriteRenderer hosSprite;
    double teleportDelay = 3d;
    public double nextTeleportTime = 0d;
    ICachedComponent cachedComponent = new ICachedComponent();
    float angleToOtherside;
    
    private void Start()
    {
        if (otherSide != null)
        {
            angleToOtherside = GetAngleBetween(transform.position, otherSide.transform.position);
            directionIndicator.transform.rotation = Quaternion.Euler(0, 0, angleToOtherside);
        }

    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (otherSide == null || LexNetwork.Time < (nextTeleportTime)) return;
        string tag = collision.gameObject.tag;
        int tid = collision.gameObject.GetInstanceID();
        // Debug.Log(gameObject.name + "Trigger with " + collision.gameObject.name+" / tag "+tag);
        switch (tag)
        {
            case TAG_PLAYER:
                Unit_Movement unit_Movement = cachedComponent.Get<Unit_Movement>(tid, collision.gameObject);
                DoTeleport(unit_Movement);
                break;
            case TAG_PROJECTILE:
                break;
        }


    }
    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, 1f);
    }
    private void FixedUpdate()
    {
        UpdateCoolTime();
        if (otherSide == null || LexNetwork.Time < (nextTeleportTime)) return;
        Collider2D[] collisions = Physics2D.OverlapCircleAll(
        transform.position, 1f, LayerMask.GetMask(TAG_PLAYER, TAG_PROJECTILE));
        
        foreach (var c in collisions)
        {
            int tid = c.gameObject.GetInstanceID();
            if (c.gameObject.CompareTag(TAG_PLAYER))
            {
                Unit_Movement unit_Movement = cachedComponent.Get<Unit_Movement>(tid, c.gameObject);
                DoTeleport(unit:unit_Movement);
                return;
            } else if (c.gameObject.CompareTag(TAG_PROJECTILE)) {

                HealthPoint health = cachedComponent.Get<HealthPoint>(tid, c.gameObject);
                if (health.IsMapProjectile()) return;
                Projectile_Movement pMove = (Projectile_Movement)health.movement;
                if (pMove.moveType == MoveType.Curves || pMove.moveType == MoveType.Straight) {
                    DoTeleport(pMove:pMove);
                    return;
                }
            }
        }
    }
    IEnumerator NumerateCooltime() {
        while (LexNetwork.Time < nextTeleportTime)
        {
            double remain = (nextTeleportTime - LexNetwork.Time);
            coolText.text = remain.ToString("0.0");
            yield return new WaitForFixedUpdate();
        }
   
        coolText.text = ""; 
        hosSprite.color = Color.cyan;
        directionIndicator.enabled = true;
        
    }

    private void UpdateCoolTime()
    {
        double remain = (nextTeleportTime - LexNetwork.Time);
        if (remain > 0)
        {
            coolText.text = remain.ToString("0.0");
            hosSprite.color = Color.cyan;
        }
        else {
            coolText.text = "";
            directionIndicator.enabled = true;
        }

    }

    void DoTeleport(Unit_Movement unit = null, Projectile_Movement pMove = null)
    {

        SetUsed(LexNetwork.Time + teleportDelay);
        otherSide.SetUsed(nextTeleportTime);
        otherSide.nextTeleportTime = nextTeleportTime;
        if (unit != null)
        {
            unit.TeleportPosition(otherSide.transform.position);
        }
        else if (pMove != null) { 
            pMove.TeleportPosition(otherSide.transform.position);
        }
    }
    public void SetUsed(double nextTime) {
        nextTeleportTime = nextTime;
        hosSprite.color = Color.white; 
        directionIndicator.enabled = false;
        StartCoroutine(NumerateCooltime());
    }

    private void OnDisable()
    {
        cachedComponent.Clear();
    }
}
