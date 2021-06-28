using UnityEngine;
using static ConstantStrings;

public class Bot_Easy : IEvaluationMachine {
    public Bot_Easy(Unit_AutoDrive autoDriver) : base(autoDriver)
    {
        this.autoDriver = autoDriver;
        this.player = autoDriver.player;
        myInstanceID = player.gameObject.GetInstanceID();
        movement = player.movement;
        skillManager = player.skillManager;
        SetRange(15f);
    }

    public override void DetermineAttackType()
    {
        attackRange = Random.Range(3f,8f);
    }
    public override Vector3 EvaluateMoves()
    {
        Vector3 move = Vector3.zero;
        dangerList.Clear();
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
            float multiplier = 0f;
            switch (go.tag)
            {
                case TAG_PLAYER:
                    move += EvaluatePlayer(go, tid, directionToTarget, distance);
                    break;
                case TAG_PROJECTILE:
                    move += EvaluateProjectile(tid, go, distance, directionToTarget);
                    break;
                case TAG_BOX_OBSTACLE:
                    multiplier = GetMultiplier(distance);
                    move -= directionToTarget * multiplier;
                    break;
            }
        }
        if (dangerList.Count > 0)
        {
            move += Drive_KNN();
        }
        move += GetAwayFromWalls();
        move += GetToCapturePoint();
        // Debug.Log("Wall move " + move + " mag " + move.magnitude + " / " + move.sqrMagnitude);
        if (move.magnitude > 1f)
        {
            move.Normalize();
        }
        else if (move.magnitude <= 0.25f)
        {
            move = Vector3.zero;
        }

        return move;
    }
    public override Vector3 EvaluatePlayer(GameObject go, int tid, Vector3 directionToTarget, float distance)
    {
        Unit_Player enemyPlayer = cachedComponent.Get<Unit_Player>(tid, go);
        if (!IsPlayerDangerous(enemyPlayer)) {
            return -directionToTarget * GetMultiplier(distance*3); 
        }

        float multiplier = GetMultiplier(distance);

        bool skillInUse = skillManager.SkillInUse();
        if (!skillInUse)
        {
            multiplier *= -1f; 
            if (enemyPlayer.controller.IsBot)
            {
                multiplier *=0.5f;
            }
        }

        return directionToTarget * multiplier;
    }
    public override Vector3 EvaluateProjectile(int tid, GameObject go, float distance, Vector3 directionToTarget)
    {
        dangerList.Add(go);
        float multiplier = GetMultiplier(distance);
        return -directionToTarget * multiplier;
    }
    public override Vector3 PreventExtremeMove(Vector3 v)
    {
        return v;
    }
    public override float DiffuseAim(float angle)
    {
        float rand = Random.Range(-60f, 60f);
        return angle + rand;
    }
}
