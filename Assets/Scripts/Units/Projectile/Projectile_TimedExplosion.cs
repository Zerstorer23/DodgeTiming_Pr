using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Projectile_TimedExplosion : MonoBehaviour
{
    Projectile_DamageDealer damageDealer;
    HealthPoint health;
    public float timeout;
    float radius = 1f;
    //Tsuruya = 0.65
    private void Awake()
    {
        damageDealer = GetComponent<Projectile_DamageDealer>();
        health = GetComponent<HealthPoint>();
    }

    private void OnEnable()
    {
        debug_exploding = false;
        //  damageDealer.myCollider.enabled = false;
        if (timeoutRoutine != null) {
            StopCoroutine(timeoutRoutine);
        }
        timeoutRoutine = WaitAndExplode();
        StartCoroutine(timeoutRoutine);
    }

    bool debug_exploding = false;
    IEnumerator timeoutRoutine = null;
    IEnumerator WaitAndExplode() {
        yield return new WaitForSeconds(timeout); debug_exploding = true;
        //damageDealer.myCollider.enabled = true;
        if (health.controller.IsMine) {
            CheckContacts();
        }
        yield return new WaitForSeconds(0.48f-timeout);
        health.Kill_Immediate();
    }
    private void CheckContacts()
    {
        health.damageDealer.givesDamage = true;
        Collider2D[] collisions = Physics2D.OverlapCircleAll(transform.position, radius);
        foreach (Collider2D c in collisions)
        {
            switch (c.gameObject.tag)
            {
                case ConstantStrings.TAG_PLAYER:
                    damageDealer.DoPlayerCollision(c.gameObject);
                    break;
                case ConstantStrings.TAG_PROJECTILE:
                    damageDealer.DoProjectileCollision(c.gameObject);
                    break;
            }

        }
        health.damageDealer.givesDamage = false;
    }
    private void OnDrawGizmos()
    {
        if(debug_exploding)
        Gizmos.DrawWireSphere(transform.position, radius);
    }
}
