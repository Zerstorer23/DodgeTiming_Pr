using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static ConstantStrings;

public class Skill_Kimidori : ISkill
{
    public override ActionSet GetSkillActionSet(SkillManager skm)
    {
        ActionSet mySkill = new ActionSet(skm);
        mySkill.SetParam(SkillParams.MoveSpeed, 6f);
        mySkill.SetParam(SkillParams.PrefabName, PREFAB_BULLET_KIMIDORI);
        mySkill.SetParam(SkillParams.ReactionType, ReactionType.Die);
        mySkill.SetParam(SkillParams.Enable, !GameSession.gameModeInfo.isCoop);

        mySkill.Enqueue(new Action_GetCurrentPlayerPosition());
        mySkill.Enqueue(new Action_InstantiateBullet_FollowPlayer());
        mySkill.Enqueue(new Action_SetProjectile_Orbit());
        mySkill.Enqueue(new Action_SetProjectile_InvincibleFromMapBullets());
        return mySkill;
    }


    public override void LoadInformation(SkillManager skm)
    {
        skm.cooltime = 1.8f;
        skm.maxStack = 1;
    }


}
