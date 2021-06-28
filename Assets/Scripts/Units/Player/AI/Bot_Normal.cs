using UnityEngine;
using static ConstantStrings;

public class Bot_Normal : IEvaluationMachine {
    public Bot_Normal(Unit_AutoDrive autoDriver) : base(autoDriver)
    {
        this.autoDriver = autoDriver;
        this.player = autoDriver.player;
        myInstanceID = player.gameObject.GetInstanceID();
        movement = player.movement;
        skillManager = player.skillManager;
        SetRange(10f);
    }



    public override void DetermineAttackType()
    {
        attackRange = Random.Range(4f,12f);
    }
    public override Vector3 EvaluateMoves()
    {
        //1. Heuristic
        Vector3 move = Vector3.zero;
        dangerList.Clear();
        collideCount = 0;
        foreach (GameObject go in foundObjects.Values)
        {
            if (go == null || !go.activeInHierarchy)
            {
                continue;
            }
            int tid = go.GetInstanceID();
            Vector3 directionToTarget = go.transform.position - player.movement.netTransform.networkPos;
            directionToTarget.Normalize();
            float distance = Vector2.Distance(go.transform.position, player.movement.netTransform.networkPos) - GetRadius(go.transform.localScale);
            //   if (distance > range) continue;
            float multiplier = 0f;
            switch (go.tag)
            {
                case TAG_PLAYER:
                    move += EvaluatePlayer(go, tid, directionToTarget, distance);
                    break;
                case TAG_PROJECTILE:
                    if (distance < range_Knn)
                    {
                        dangerList.Add(go);
                    }
                    if (distance <= 2.5f)
                    {
                        collideCount++;
                    }
                    move += EvaluateProjectile(tid, go, distance, directionToTarget);

                    break;
                case TAG_BUFF_OBJECT:
                    move += EvaluateBuff(go, tid, directionToTarget, distance);
                    break;
                case TAG_BOX_OBSTACLE:
                    multiplier = GetMultiplier(distance);
                    collideCount++;
                    move -= directionToTarget * multiplier;
                    break;
            }
        }
        move += GetToCapturePoint();

        //2. KNNs
        // Debug.Log("Wall move " + move + " mag " + move.magnitude + " / " + move.sqrMagnitude);
        if (move.magnitude > 1f)
        {
            move.Normalize();
        }
        else if (move.magnitude <= 0.25f)
        {
            move = Vector3.zero;
        }
        //Debug.Log("Final move " + move +" mag "+move.magnitude + " / "+move.sqrMagnitude);
        return move;
    }
    public override Vector3 EvaluatePlayer(GameObject go, int tid, Vector3 directionToTarget, float distance)
    {
        Unit_Player enemyPlayer = cachedComponent.Get<Unit_Player>(tid, go);
        if (!IsPlayerDangerous(enemyPlayer)) return Vector3.zero;
        bool skillAvailable = skillManager.SkillIsReady();
        bool skillInUse = skillManager.SkillInUse();
        if ( skillInUse)
        {
            doApproach = true;
           /* if (player.myCharacter == CharacterType.SASAKI)
            {
                doApproach = true;
            }
            else
            {
                doApproach = !player.FindAttackHistory(tid);
            }*/
        }
        else
        {
            doApproach = skillAvailable;
        }

        if (doApproach)
        {
            return directionToTarget * GetMultiplier(distance);
        }
        else
        {
            return directionToTarget * -GetMultiplier(distance);
        }


    }
    public override Vector3 PreventExtremeMove(Vector3 v)
    {
        return v;
    }
    public override float DiffuseAim(float angle)
    {
        return angle + Random.Range(-60f, 60f); 
    }

}
