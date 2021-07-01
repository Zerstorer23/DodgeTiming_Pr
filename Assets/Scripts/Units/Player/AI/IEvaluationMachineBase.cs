using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static ConstantStrings;
using static GameFieldManager;

public partial class IEvaluationMachine
{
   protected Unit_Player player;
    protected SkillManager skillManager;
    protected Unit_Movement movement;
    protected Unit_AutoDrive autoDriver;

    //-----FIND OBJECTS-----//
    public float range_Search;
    public float range_Knn;
    public float range_Collision;
    protected float escapePadding = 1f;

    //-----ATTACK PLAYERS-----//
    public float attackRange = 10f;
    public bool isKamikazeSkill = false;

    public List<GameObject> dangerList = new List<GameObject>();
    public Dictionary<int, GameObject> foundObjects = new Dictionary<int, GameObject>();
    protected ICachedComponent cachedComponent = new ICachedComponent();
    Vector3[] walls = new Vector3[2];//x,y
    public int myInstanceID;

    public IEvaluationMachine(Unit_AutoDrive autoDriver) {
        this.autoDriver = autoDriver;
        this.player = autoDriver.player;
        myInstanceID = player.gameObject.GetInstanceID();
        movement = player.movement;
        skillManager = player.skillManager;
        SetRange(20f);
    }

    public void AddFoundObject(int tid, GameObject go) {
        if (foundObjects.ContainsKey(tid))
        {
            foundObjects[tid] = go;
        }
        else
        {
            foundObjects.Add(tid, go);
        }
    }
    public void RemoveFoundObject(int tid) {
        foundObjects.Remove(tid);
    }

    [SerializeField] BitArray blockedAngles = new BitArray(360, false);

    protected Vector3 AbsoluteEvasion(Vector3 finalDir)
    {
        int searchRange = 15;
        blockedAngles.SetAll(false);
        // Collider2D[] collisions = Physics2D.OverlapCircleAll(transform.position, collideRadius, LayerMask.GetMask(TAG_PROJECTILE));
        foreach (GameObject go in foundObjects.Values)
        {
            if (go == null || !go.activeInHierarchy) continue;
            if (go.tag != TAG_PROJECTILE) continue;
            if (Vector2.Distance(go.transform.position, player.transform.position) > range_Collision) continue;
            if (!IsProjectileDangerous(go.GetInstanceID(), go)) continue;
            int angleToObj = (int)GetAngleBetween(movement.netTransform.networkPos, go.transform.position);
            int startAngle = (angleToObj - searchRange);
            if (startAngle < 0) startAngle += 360;
            for (float i = 0; i < searchRange * 2; i++)
            {
                int angleIndex = (int)((startAngle + i) % 360f);
                if (angleIndex >= 360) angleIndex %= 360;
                blockedAngles[angleIndex] = true;
            }
        }
        int initAngle = (int)(GetAngleBetween(Vector3.zero, finalDir));
        int searchIndex = initAngle - searchRange;
        if (searchIndex < 0) searchIndex += 360;
        int continuousCount = 0;
        int numSearch = 0;
        int endCount = searchRange * 2;
        while (continuousCount < endCount && numSearch < 360)
        {
            if (!blockedAngles[searchIndex])
            {
                continuousCount++;
            }
            else
            {
                continuousCount = 0;
            }
            searchIndex++;
            if (searchIndex >= 360) searchIndex %= 360;
            numSearch++;
        }
        float finalAngle = searchIndex - searchRange;
        if (finalAngle < 0) finalAngle += 360f;
        //   if (finalAngle != initAngle) Debug.LogWarning("Modify " + initAngle + " => " + finalAngle+" search count "+numSearch);
        return GetAngledVector(finalAngle, finalDir.magnitude);
    }
    protected Vector3 EvaluateToPoint(Vector3 point, bool positive, float flavour = 1f)
    {
        Vector3 dirToPoint = point - movement.netTransform.networkPos;
        dirToPoint.Normalize();
        float dist = Vector2.Distance(point, movement.netTransform.networkPos);
        Vector3 direction = dirToPoint * GetMultiplier(dist) * flavour;
        if (!positive) direction *= -1f;
        return direction;
    }
   protected Vector3 Drive_KNN()
    {
        float angleStep = 10f;
        float moveDist = Mathf.Max(movement.GetMovementSpeed() * Time.deltaTime, 1f);

        float maxDist = 0f;
        float maxAngle = 0f;
        int minDanger = -1;
        for (float currAngle = 0; currAngle < 360f; currAngle += angleStep)
        {
            Vector3 newPos = GetAngledVector(currAngle, moveDist) + player.transform.position;
            float totalDist = 0f;
            int dangerCount = 0;
            foreach (GameObject go in dangerList)
            {
                Projectile_Movement pMove = cachedComponent.Get<HealthPoint>(go.GetInstanceID(), go).movement as Projectile_Movement;
                float currentDist = Vector2.Distance(newPos, pMove.transform.position);
                Vector3 expectedTargetPos = pMove.GetNextPosition();
                float nextDist = Vector2.Distance(newPos, expectedTargetPos);
                if (currentDist > range_Collision && currentDist < nextDist) {
                    continue;
                }
                if (nextDist < range_Knn)
                {
                    totalDist += Mathf.Pow(nextDist, 2);
                    dangerCount++;
                }
            }
            for (int i = 0; i < walls.Length; i++) {
                float wallDist = Vector2.Distance(newPos, walls[i]);
                if (wallDist < range_Knn * 2)
                {
                    totalDist += Mathf.Pow(wallDist, 2);
                    dangerCount++;
                }
            }
         
            if (dangerCount < minDanger || minDanger < 0)
            {
                maxDist = totalDist;
                maxAngle = currAngle;
            }
            else if (dangerCount == minDanger)
            {
                if (totalDist > maxDist)
                {
                    maxDist = totalDist;
                    maxAngle = currAngle;
                }
            }
        }

        return GetAngledVector(maxAngle, moveDist);
    }


    public bool doApproach = false;

    public bool IsInAttackRange(GameObject target) {
        return Vector2.Distance(target.transform.position, player.transform.position) <= attackRange;
    }
    public void Reset()
    {
        isKamikazeSkill = false;
        foundObjects.Clear();
        cachedComponent.Clear();
    }
    protected Vector3 PerpendicularEscape(Projectile_Movement projMove, Vector3 dirToTarget)
    {
        float guessedAngle = GetAngleBetween(Vector3.zero, dirToTarget);
        float currDist = Vector2.Distance(projMove.transform.position, player.transform.position);
        //  Debug.Log("Original proj " + projMove.transform.position + " Angle " + projMove.transform.rotation.eulerAngles.z+" length "+ currDist+" dsir to target" +dirToTarget);
        Vector3 translation = GetAngledVector(projMove.transform.rotation.eulerAngles.z, currDist);
        Vector3 projPos = projMove.transform.position + translation;
        //  Debug.Log("Translation " + translation);
        float highDist = 0f;
        float highAngle = -1f;
        for (float angle = -90f; angle <= 90f; angle += 180f)
        {
            float iterAngle = (guessedAngle + angle) % 360;
            Vector3 myPos = player.transform.position + GetAngledVector(iterAngle, movement.GetMovementSpeed() * Time.fixedDeltaTime);
            float dist = Vector2.Distance(myPos, projPos);
            //  Debug.Log("Original proj " + projMove.transform.position + " my original " + transform.position);
            //   Debug.Log(iterAngle+": projectile at "+projPos+" Me at"+(myPos)+" relativeVector"+ (projPos - myPos)+":"+((angle<0)?"Clock ":"AntiClock") + " = > " + dist);
            if (highAngle == -1f || dist > highDist)
            {
                highDist = dist;
                highAngle = iterAngle % 360f;

            }
        }
        //    Debug.Log("Recommend angle " + highAngle+" dist "+highDist+" vector "+ GetAngledVector(highAngle, dirToTarget.magnitude));
        return GetAngledVector(highAngle, dirToTarget.magnitude);
    }


    protected bool IsProjectileDangerous(int tid, GameObject go)
    {
        HealthPoint hp = cachedComponent.Get<HealthPoint>(tid, go);
        if (hp.IsMapProjectile()) return true;
        if (hp.controller.Equals(player.controller)) return false;
        if (GameSession.gameModeInfo.isTeamGame && hp.myTeam == player.myTeam) return false;
        return true;
    }
    protected bool IsPlayerDangerous(Unit_Player enemyPlayer) {
        if (GameSession.gameModeInfo.isTeamGame && enemyPlayer.myTeam == player.myTeam) return false;
        if (GameSession.gameModeInfo.isCoop) return false;
        return true;
    }
}
