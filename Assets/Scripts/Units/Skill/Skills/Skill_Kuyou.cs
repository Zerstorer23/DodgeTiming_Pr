using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static ConstantStrings;

public class Skill_Kuyou : ISkill
{
    public override ActionSet GetSkillActionSet(SkillManager skm)
    {
        ActionSet mySkill = new ActionSet(skm);
        mySkill.SetParam(SkillParams.PrefabName, PREFAB_BULLET_KUYOU);
        mySkill.SetParam(SkillParams.AnimationTag, "DoBatswing");
        mySkill.SetParam(SkillParams.Quarternion, Quaternion.identity);
        mySkill.SetParam(SkillParams.Vector3, skm.transform.position);
        mySkill.SetParam(SkillParams.Duration, 0.25f);
        mySkill.SetParam(SkillParams.ReactionType, ReactionType.None);
        //mySkill.Enqueue(new Action_Player_InvincibleBuff());//
        mySkill.Enqueue(new Action_InstantiateBullet_FollowPlayer());
        mySkill.Enqueue(new Action_DoDeathAfter());
        mySkill.Enqueue(new Action_GetCurrentPlayerPosition());
        mySkill.Enqueue(new Action_GunObject_SetAngle());
        mySkill.Enqueue(new Action_Projectile_ResetAngle());
        mySkill.Enqueue(new Action_PlayerDoGunAnimation());
        mySkill.Enqueue(new Action_WaitForSeconds());
        return mySkill;
    }


    public override void LoadInformation(SkillManager skm)
    {
        skm.cooltime = 3.25f;
        skm.maxStack = 1;
    }
}
