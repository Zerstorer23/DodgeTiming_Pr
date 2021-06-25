using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static ConstantStrings;

public class Skill_Kyonmouto : ISkill
{
    public override ActionSet GetSkillActionSet(SkillManager skm)
    {
        ActionSet mySkill = new ActionSet(skm);
        mySkill.SetParam(SkillParams.PrefabName, PREFAB_BULLET_KYONMOUTO);
        mySkill.SetParam(SkillParams.LexView, skm.pv);
        mySkill.Enqueue(new Action_SetParameter() { paramType = SkillParams.MoveSpeed, paramValue = 12f });
        mySkill.Enqueue(new Action_SetParameter() { paramType = SkillParams.Distance, paramValue = 5.5f });
        mySkill.Enqueue(new Action_SetParameter() { paramType = SkillParams.ReactionType, paramValue = ReactionType.None });

        mySkill.Enqueue(new Action_GetCurrentPlayerPosition_AngledOffset());
        mySkill.Enqueue(new Action_InstantiateBulletAt());
        mySkill.Enqueue(new Action_SetProjectileStraight());
        mySkill.Enqueue(new Action_SetProjectile_Homing_Target());
        //---Homing info
        mySkill.Enqueue(new Action_SetParameter() { paramType = SkillParams.RotateSpeed, paramValue = -1f });
        mySkill.Enqueue(new Action_SetParameter() { paramType = SkillParams.Distance, paramValue = 0f });
        mySkill.Enqueue(new Action_SetParameter() { paramType = SkillParams.ReactionType, paramValue = ReactionType.Die });
        mySkill.Enqueue(new Action_SetProjectile_Homing_Information());
        //----
        //--Scale
/*       mySkill.Enqueue(new Action_SetParameter() { paramType = SkillParams.Width, paramValue = 2f }); 
        mySkill.Enqueue(new Action_SetParameter() { paramType = SkillParams.Duration, paramValue = 2f });
        mySkill.Enqueue(new Action_DoScaleTween());*/
        return mySkill;
    }

    public override void LoadInformation(SkillManager skm)
    {
        skm.cooltime = 3.25f;
        skm.maxStack = 2;
    }

}
