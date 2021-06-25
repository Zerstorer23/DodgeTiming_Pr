using Lex;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Projectile_CustomDamageDealer : MonoBehaviourLex
{

    string[] playerList;
    //   HashSet<string> foundTargets = new HashSet<string>();
    SortedDictionary<string, Unit_Player> playerDict = new SortedDictionary<string, Unit_Player>();
    Projectile_DamageDealer damageDealer;
    Projectile proj;
    public float colliderRadius;
    private void Awake()
    {
        damageDealer = GetComponent<Projectile_DamageDealer>();
        proj = GetComponent<Projectile>();
    }
    private void OnEnable()
    {
        int fieldNo = (int)lexView.InstantiationData[0];
        playerDict = GameFieldManager.GetPlayersInArea(fieldNo);
        playerList = playerDict.Keys.ToArray();// LexNetwork.GetPlayers();
    }
    private void Update()
    {
        FindNearByPlayers();
    }
    private void FindNearByPlayers()
    {
        foreach (string key in playerList) {
            if (!playerDict.ContainsKey(key)) continue;
            Unit_Player player = playerDict[key];
            if (proj.controller.Equals(key) || player == null || !player.gameObject.activeInHierarchy) continue;
            var go = player.gameObject;
            int tid = go.GetInstanceID();
            float dist = Vector2.Distance(gameObject.transform.position, player.transform.position);

            if (!damageDealer.duplicateDamageChecker.FindAttackHistory(tid))
            {
                if (dist <= colliderRadius)
                {
                    damageDealer.DoPlayerCollision(go);
                }
            }
            else {
                if (dist >= padding+ colliderRadius)
                {
                    damageDealer.duplicateDamageChecker.RemoveAttackHistory(tid);
                }
            }
        }
    }
    float padding = 0.2f;
   
    /*
       private void FindNearByPlayers()
    {
        //     Debug.Log("Num players : " + unit_Players.Count + " / Captured: " + foundTargets.Count);
        for (int i = 0; i < playerList.Count; i++) {
            string key = playerList[i];
            Unit_Player player = playerDict[key];
            if (proj.controller.IsSame(key) || player == null) continue;
            float dist = Vector2.Distance(gameObject.transform.position, player.transform.position);
            // Debug.Log(entry.Value.pv.Owner.NickName+": "+ entry.Key+ " dist: " + dist +" vs "+colliderRadius);
            if (foundTargets.ContainsKey(key))
            {
                if (dist > colliderRadius)
                {
                    Debug.Log(key + " leaves region");
                    foundTargets.Remove(key);
                }
            }
            else
            {
                if (dist <= colliderRadius)
                {
                    Debug.Log(key + " enters region");
                    var go = player.gameObject;
                    foundTargets.Add(key, go);
                    damageDealer.DoPlayerCollision(go);
                }
            }

        }
    }
     */
}
