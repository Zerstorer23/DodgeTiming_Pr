using Lex;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static BulletManager;

public class Action_InstantiateBullet : SkillAction
{
    
    public override float Activate() {

        int fieldNumber = parent.castingPlayer.fieldNo;
        GameObject obj = LexNetwork.Instantiate(
            GetParam<string>(SkillParams.PrefabName),
            parent.caster.transform.position,
            parent.caster.transform.rotation,0
            , new object[] {fieldNumber, parent.castingPlayer.controller.uid, false}
            );
        parent.spawnedObject = obj;
        parent.projectilePV = parent.spawnedObject.GetComponent<LexView>();
        return 0f;
    }

}
public class Action_InstantiateBullet_FollowPlayer : SkillAction
{

    public override float Activate()
    {
        int fieldNumber = parent.castingPlayer.fieldNo;
        float angle = parent.castingPlayerMovement.GetAim();
        Quaternion rotation = Quaternion.Euler(0, 0, angle);
        GameObject obj = LexNetwork.Instantiate(
            GetParam<string>(SkillParams.PrefabName),
            parent.caster.transform.position,
            rotation, 0
            , new object[] { fieldNumber, parent.castingPlayer.controller.uid, true
            }
            );
        parent.spawnedObject = obj;
        parent.projectilePV = parent.spawnedObject.GetComponent<LexView>();


        return 0f;
    }
}
public class Action_InstantiateBulletAt : SkillAction
{

    public override float Activate()
    {

        int fieldNumber = parent.castingPlayer.fieldNo;
        Vector3 pos = GetParam<Vector3>(SkillParams.Vector3);
        Quaternion angle = GetParam<Quaternion>(SkillParams.Quarternion);
    
        GameObject obj = LexNetwork.Instantiate(
          GetParam<string>(SkillParams.PrefabName), pos, angle, 0
            , new object[] { fieldNumber, parent.castingPlayer.controller.uid, false });
        parent.spawnedObject = obj;
        parent.projectilePV = parent.spawnedObject.GetComponent<LexView>();
        return 0;
    }
}


public class Action_Projectile_ResetAngle : SkillAction
{

    public override float Activate()
    {
        parent.projectilePV.RPC("ResetRotation");
        return 0;
    }

}
public class Action_GetCurrentPlayerPosition : SkillAction
{

    public override float Activate()
    {
        float angle = parent.castingPlayerMovement.GetAim();
        parent.SetParam(SkillParams.Quarternion, Quaternion.Euler(0, 0, angle));
        parent.SetParam(SkillParams.EulerAngle, angle);
        parent.SetParam(SkillParams.Vector3, parent.caster.transform.position);
        return 0;
    }
}
public class Action_Player_GetAim : SkillAction
{

    public override float Activate()
    {
        float angle = parent.castingPlayerMovement.GetAim();
        parent.SetParam(SkillParams.Quarternion, Quaternion.Euler(0, 0, angle));
        parent.SetParam(SkillParams.EulerAngle, angle);
        parent.SetParam(SkillParams.Vector3, ConstantStrings.GetAngledVector(angle,1f));
        return 0;
    }
}
public class Action_GetCurrentPlayerPosition_AngledOffset : SkillAction
{

    public override float Activate()
    {
        float angle = parent.castingPlayerMovement.GetAim();
        float distance = GetParam<float>(SkillParams.Distance, 1.4f);
        Vector3 angledVector = ConstantStrings.GetAngledVector(angle, distance);
        parent.SetParam(SkillParams.Quarternion, Quaternion.Euler(0, 0, angle));
        parent.SetParam(SkillParams.EulerAngle, angle);
        parent.SetParam(SkillParams.Vector3, parent.caster.transform.position + angledVector);
        return 0;
    }

}
public class Action_GetCurrentPlayerVector3_AngledOffset : SkillAction
{

    public override float Activate()
    {
        float angle = GetParam<float>(SkillParams.EulerAngle);
        float distance = GetParam<float>(SkillParams.Distance, 1.4f);
        float rad = angle / 180 * Mathf.PI;
        float dX = Mathf.Cos(rad) * distance;
        float dY = Mathf.Sin(rad) * distance;
        parent.SetParam(SkillParams.Vector3, parent.caster.transform.position + new Vector3(dX,dY));
        return 0;
    }
}
