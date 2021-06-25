using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static ConstantStrings;

public class Skill_Sasaki : ISkill
{
    public override ActionSet GetSkillActionSet(SkillManager skm)
    {
        ActionSet mySkill = new ActionSet(skm);
        mySkill.SetParam(SkillParams.PrefabName, PREFAB_BULLET_SASAKI);
        mySkill.SetParam(SkillParams.ReactionType, ReactionType.None);

        int steps = 30;
        float dur = 2f;
        mySkill.Enqueue(new Action_SetParameter() { paramType = SkillParams.Duration, paramValue = dur });
        mySkill.Enqueue(new Action_SetParameter() { paramType = SkillParams.Modifier, paramValue = 0.3f });
        mySkill.Enqueue(new Action_Player_MovespeedBuff());
        mySkill.Enqueue(new Action_SetParameter() { paramType = SkillParams.Duration, paramValue = dur });
        mySkill.Enqueue(new Action_Player_InvincibleBuff());
        for (int i = 0; i < steps; i++)
        {
            mySkill.Enqueue(new Action_SetParameter() { paramType = SkillParams.Width, paramValue = 1f });
            mySkill.Enqueue(new Action_SetParameter() { paramType = SkillParams.Height, paramValue = 1f });
            mySkill.Enqueue(new Action_SetParameter() { paramType = SkillParams.Duration, paramValue = dur });
            mySkill.Enqueue(new Action_GetCurrentPlayerPosition());
            mySkill.Enqueue(new Action_InstantiateBulletAt());
            mySkill.Enqueue(new Action_SetProjectileScale());
            mySkill.Enqueue(new Action_SetProjectileStatic());
            //   mySkill.Enqueue(new Action_SetProjectileExcludePlayer());
            mySkill.Enqueue(new Action_SetParameter() { paramType = SkillParams.Width, paramValue = 3f }); //5.5
            mySkill.Enqueue(new Action_SetParameter() { paramType = SkillParams.Height, paramValue = 3f });
            mySkill.Enqueue(new Action_DoScaleTween());
            mySkill.Enqueue(new Action_DoDeathAfter());
            mySkill.Enqueue(new Action_SetParameter() { paramType = SkillParams.Duration, paramValue = dur / steps });
            mySkill.Enqueue(new Action_WaitForSeconds());
        }
        return mySkill;
    }


    public override void LoadInformation(SkillManager skm)
    {
        skm.cooltime = 4.5f;
        skm.maxStack = 1;
    }

}
