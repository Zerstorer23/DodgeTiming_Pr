using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static ConstantStrings;
using static GameFieldManager;

public partial class IEvaluationMachine
{
    public void SetRange(float range)
    {
        range_Search = range;
        range_Collision = 3f;
        range_Knn = gameFields[player.fieldNo].bulletSpawner.activeMax < 4 ? range_Search : 5f;
        DetermineAttackType();
    }
    public void RemoveObjects()
    {
        List<int> keys = new List<int>(foundObjects.Keys);

        foreach (var key in keys)
        {
            GameObject go = foundObjects[key];
            if (go == null || !go.activeInHierarchy)
            {
                RemoveFoundObject(key);
            }
            else if (go.tag != TAG_BOX_OBSTACLE)
            {
                float dist = Vector2.Distance(player.movement.networkPos, go.transform.position);
                if (dist > (range_Search + escapePadding))
                {
                    RemoveFoundObject(key);
                }
            }
        }
    }
    public void FindNearByObjects()
    {
        Collider2D[] collisions = Physics2D.OverlapCircleAll(player.movement.networkPos, range_Search, LayerMask.GetMask(TAG_PLAYER, TAG_PROJECTILE, TAG_BUFF_OBJECT));

        for (int i = 0; i < collisions.Length; i++)
        {
            Collider2D c = collisions[i];
            int tid = c.gameObject.GetInstanceID();
            if (foundObjects.ContainsKey(tid)) continue;

            switch (c.gameObject.tag)
            {
                case TAG_PLAYER:
                    if (tid != myInstanceID)
                    {
                        AddFoundObject(tid, c.gameObject);
                    }
                    break;
                case TAG_PROJECTILE:
                    if (IsProjectileDangerous(tid, c.gameObject))
                    {
                        //   Debug.LogWarning("Add " + c.gameObject.name);
                        AddFoundObject(tid, c.gameObject);
                    }
                    break;
                case TAG_BUFF_OBJECT:
                    AddFoundObject(tid, c.gameObject);
                    break;
            }
        }
    }

}
