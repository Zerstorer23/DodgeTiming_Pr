using Lex;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using static ConstantStrings;
public class Projectile_Explosion : MonoBehaviourLex
{
    Projectile_DamageDealer damageDealer;
    public bool attackMyTeam = false;
    private void Awake()
    {
        damageDealer = GetComponent<Projectile_DamageDealer>();
    }
    private void OnTriggerEnter2D(Collider2D collision)
    {
        HandleCollision(collision.gameObject);
    }
    private void OnCollisionEnter2D(Collision2D collision)
    {
        HandleCollision(collision.gameObject);

    }
    void HandleCollision(GameObject go)
    {
        string tag = go.tag;
        switch (tag)
        {
            case TAG_PLAYER:
            case TAG_PROJECTILE:
                HealthPoint otherHP = go.GetComponent<HealthPoint>();
                if (otherHP.IsMapProjectile() || !otherHP.controller.IsLocal)
                {
                    DoExplosion();
                }
                break;
            case TAG_BOX_OBSTACLE:
            case TAG_WALL:
                DoExplosion();
                break;
        }
    }
    float radius = 2.25f;
    void DoExplosion() {
        Collider2D[] collisions = Physics2D.OverlapCircleAll(transform.position,radius, LayerMask.GetMask("Player", "Projectile"));
        for (int i = 0; i < collisions.Length; i++)
        {
            Collider2D c = collisions[i];
            HealthPoint healthPoint = c.gameObject.GetComponent<HealthPoint>();
            //if (healthPoint == null) return;
            switch (c.gameObject.tag)
            {
                case TAG_PLAYER:
                case TAG_PROJECTILE:
                    if (!attackMyTeam) {
                        if (GameSession.gameModeInfo.isTeamGame &&
                            healthPoint.myTeam == damageDealer.myHealth.myTeam &&
                            healthPoint.controller.uid != damageDealer.myHealth.controller.uid) {
                            continue;
                        }
                    }
                    damageDealer.GiveDamage(healthPoint);
                    if (lexView.IsMine) {
                        LexNetwork.Instantiate(PREFAB_EXPLOSION_1, transform.position, Quaternion.identity, 0);
                    }
                    break;
            }

        }
    }
/*    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, radius);
    }*/
}
