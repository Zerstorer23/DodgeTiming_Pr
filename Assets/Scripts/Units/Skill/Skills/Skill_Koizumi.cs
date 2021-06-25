using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static ConstantStrings;

public class Skill_Koizumi : ISkill
{
    public override ActionSet GetSkillActionSet(SkillManager skm)
    {
        ActionSet mySkill = new ActionSet( skm);
        mySkill.SetParam(SkillParams.Duration, 1.5f); //1.5
                                                      // mySkill.SetParam(SkillParams.Modifier, 0.75f);
        mySkill.SetParam(SkillParams.Color, "#c80000");
        mySkill.SetParam(SkillParams.Enable, true);
        mySkill.SetParam(SkillParams.PrefabName, PREFAB_BULLET_KOIZUMI);
        mySkill.SetParam(SkillParams.ReactionType, ReactionType.None);
        mySkill.SetParam(SkillParams.Modifier, 0.5f);
        mySkill.Enqueue(new Action_InstantiateBullet_FollowPlayer());
        mySkill.Enqueue(new Action_SetProjectileStatic());
        mySkill.Enqueue(new Action_DoDeathAfter());//
        mySkill.Enqueue(new Action_Player_MovespeedBuff());//
        mySkill.Enqueue(new Action_Player_InvincibleBuff());//
        mySkill.Enqueue(new Action_WaitForSeconds());//SkillInUse발동 대기임
        return mySkill;
    }


    public override void LoadInformation(SkillManager skm)
    {
        skm.cooltime = 5f;
        skm.maxStack = 1;
    }

}
