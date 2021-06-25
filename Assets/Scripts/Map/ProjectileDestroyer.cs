using Lex;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static ConstantStrings;

public class ProjectileDestroyer : MonoBehaviourLex
{
    [SerializeField] GameField gameField;
    private void OnTriggerEnter2D(Collider2D collision)
    {
        string tag = collision.gameObject.tag;
        Debug.Log("Exception detected " + collision.gameObject.name);
        if (tag == TAG_PLAYER || tag == TAG_PROJECTILE)
        {
            HealthPoint hp = collision.gameObject.GetComponent<HealthPoint>();
            if (hp == null || hp.dontKillByException) return;
            if (hp.associatedField != gameField.fieldNo) return;
            hp.Kill_Immediate();
        }
    }
    private void OnCollisionEnter2D(Collision2D collision)
    {
        string tag = collision.gameObject.tag;
        Debug.Log("Exception detected " + collision.gameObject.name);
        if (tag == TAG_PLAYER || tag == TAG_PROJECTILE)
        {
            HealthPoint hp = collision.gameObject.GetComponent<HealthPoint>();
            if (hp == null || hp.dontKillByException) return;
            if (hp.associatedField != gameField.fieldNo) return;
            hp.Kill_Immediate();
        }
    }
}
