using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Skill_Yasumi : ISkill
{
    public override ActionSet GetSkillActionSet(SkillManager skm)
    {
        ActionSet mySkill = new ActionSet(skm);
        mySkill.SetParam(SkillParams.Width, 2f); ;
        float duration = 8f;
        mySkill.SetParam(SkillParams.Duration, Time.fixedDeltaTime * 2);
        BuffData buff = new BuffData(BuffType.MirrorDamage, 0f, duration);
        BuffData invincible = new BuffData(BuffType.InvincibleFromBullets, 0f, duration);
        mySkill.SetParam(SkillParams.BuffData, buff);
        mySkill.Enqueue(new Action_Player_SetColliderSize());
        mySkill.Enqueue(new Action_SetParameter() { paramType = SkillParams.Width, paramValue = 0.33f });
        mySkill.Enqueue(new Action_WaitForSeconds());
        mySkill.Enqueue(new Action_Player_SetColliderSize());
        mySkill.Enqueue(new Action_Player_AddBuff());
        mySkill.Enqueue(new Action_SetParameter() { paramType = SkillParams.Duration, paramValue = duration });
        mySkill.Enqueue(new Action_SetParameter() { paramType = SkillParams.BuffData, paramValue = invincible });
        mySkill.Enqueue(new Action_Player_AddBuff());
        mySkill.Enqueue(new Action_WaitForSeconds());
        return mySkill;
    }

    public override void LoadInformation(SkillManager skm)
    {
        skm.cooltime = 5f;
    }

  
}
