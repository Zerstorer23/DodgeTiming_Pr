using Lex;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static ConstantStrings;

public class Unit_ProjectileFInder : MonoBehaviour
{
    [SerializeField] Unit_Player player;
    Dictionary<int, GameObject> foundObjects = new Dictionary<int, GameObject>();
    ICachedComponent cachedComponent = new ICachedComponent();

    float evasionRadius = 2.5f;
    private void OnEnable()
    {
        gameObject.SetActive(player.controller.IsLocal);
        foundObjects.Clear();
        cachedComponent.Clear();

    }
/*    private void OnDrawGizmos()
    {
        Gizmos.color = Color.white;
        Gizmos.DrawWireSphere(transform.position, evasionRadius);
    }*/
    private void FixedUpdate()
    {
        CheckEnter();
        CheckExit();
    }
    void CheckEnter() {
        Collider2D[] collisions = Physics2D.OverlapCircleAll(transform.position, evasionRadius, LayerMask.GetMask(TAG_PROJECTILE));

        for (int i = 0; i < collisions.Length; i++)
        {
            Collider2D c = collisions[i];
            int tid = c.gameObject.GetInstanceID();
            if (foundObjects.ContainsKey(tid)) continue;
            if (c.gameObject.CompareTag(TAG_PROJECTILE))
            {
                HealthPoint proj = cachedComponent.Get<HealthPoint>(tid, c.gameObject);// collision.gameObject.GetComponent<HealthPoint>();
                if (proj.IsMapProjectile())
                {
                    foundObjects.Add(tid, c.gameObject);
                }
            }
        }
    }
    void CheckExit() {
        List<int> keys = new List<int>(foundObjects.Keys);
        foreach (var key in keys)
        {
            GameObject go = foundObjects[key];
            if (go == null || !go.activeInHierarchy)
            {
                foundObjects.Remove(key);
            }
            else 
            {
                float dist = Vector2.Distance(transform.position, go.transform.position);
                if (dist >= evasionRadius + 0.5f)
                {
                    foundObjects.Remove(key);
                    player.IncrementEvasion();
                }
            }
        }
    }
/*    private void OnTriggerExit2D(Collider2D collision)
    {
        HealthPoint proj = collision.gameObject.GetComponent<HealthPoint>();
      //  Debug.Log(proj.gameObject.name);
        if (proj != null) {
         //   bool valid;
            if (proj.damageDealer == null) return;
            if (proj.damageDealer.isMapObject)
            {
                player.IncrementEvasion();
            }
            *//*else if (GameSession.gameMode == GameMode.TEAM)
            {
                valid = (proj.myTeam == player.myTeam);
            }
            else {
                valid = (!proj.pv.AmOwner);
            }
            if (valid) {
                player.IncrementEvasion();
            }*//*

        }
    }*/
}
