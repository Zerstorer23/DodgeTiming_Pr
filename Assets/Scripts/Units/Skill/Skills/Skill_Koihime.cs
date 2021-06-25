using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static ConstantStrings;

public class Skill_Koihime : ISkill
{
    public override ActionSet GetSkillActionSet(SkillManager skm)
    {
        ActionSet mySkill = new ActionSet( skm);
        mySkill.isMutualExclusive = true;
        float duration = 0.22f;
        //6.6거리 0.33초간 -> 
        mySkill.SetParam(SkillParams.Duration, duration); //1.5
                                                      // mySkill.SetParam(SkillParams.Modifier, 0.75f);
        mySkill.SetParam(SkillParams.Color, "#c80000");
        mySkill.SetParam(SkillParams.Enable, true);
        mySkill.SetParam(SkillParams.PrefabName, PREFAB_BULLET_KOIZUMI);
        mySkill.SetParam(SkillParams.ReactionType, ReactionType.None);
        BuffData blockSkill = new BuffData(BuffType.BlockSkill, 1f,(double) duration);
        mySkill.Enqueue(new Action_SetParameter() { paramType = SkillParams.BuffData, paramValue = blockSkill});//
        mySkill.Enqueue(new Action_Player_AddBuff());//
                                                            // mySkill.SetParam(SkillParams.Modifier, 0.5f);
        float dashSpeed = 30f;
        mySkill.Enqueue(new Action_InstantiateBullet_FollowPlayer());
        mySkill.Enqueue(new Action_SetProjectileStatic());
        mySkill.Enqueue(new Action_DoDeathAfter());//
        mySkill.Enqueue(new Action_Player_InvincibleBuff());//
        mySkill.Enqueue(new Action_Player_GetAim());
        mySkill.Enqueue(new Action_ModifyParameter_Vector3Multiply() { paramType = SkillParams.Vector3, paramValue = dashSpeed });
        mySkill.Enqueue(new Action_SetParameter() { paramType = SkillParams.Modifier, paramValue = 9f });
        mySkill.Enqueue(new Action_Player_AddGravity());
        mySkill.Enqueue(new Action_WaitForSeconds());//SkillInUse발동 대기임
        mySkill.Enqueue(new Action_Player_ResetGravity());

        return mySkill;
    }


    public override void LoadInformation(SkillManager skm)
    {
        skm.cooltime = 3.5f;
        skm.maxStack = 3;
    }

}
