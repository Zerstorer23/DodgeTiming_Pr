using Lex;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using static BulletManager;
using static ConstantStrings;
using Random = UnityEngine.Random;
public class Skill_Mori : ISkill
{
    public override void LoadInformation(SkillManager skm)
    {
        skm.cooltime = 4f;
    }
    public override ActionSet GetSkillActionSet(SkillManager skm)
    {
        ActionSet mySkill = new ActionSet( skm);
        mySkill.SetParam(SkillParams.PrefabName, PREFAB_BULLET_MORI);
        mySkill.SetParam(SkillParams.ReactionType, ReactionType.None);
        mySkill.SetParam(SkillParams.Duration, 0.33f);
        float dashSpeed = 40f;
        mySkill.Enqueue(new Action_SetParameter() { paramType = SkillParams.Modifier, paramValue = 3f });
        mySkill.Enqueue(new Action_Player_MovespeedBuff());
        mySkill.Enqueue(new Action_Player_GetAim());
        mySkill.Enqueue(new Action_InstantiateBullet_FollowPlayer());
        mySkill.Enqueue(new Action_SetProjectileStatic());
        mySkill.Enqueue(new Action_DoDeathAfter());
        mySkill.Enqueue(new Action_Player_GetAim());
        mySkill.Enqueue(new Action_ModifyParameter_Vector3Multiply() { paramType = SkillParams.Vector3, paramValue = dashSpeed});
        mySkill.Enqueue(new Action_SetParameter() { paramType = SkillParams.Modifier, paramValue = 9f });
        mySkill.Enqueue(new Action_Player_AddGravity());
        mySkill.Enqueue(new Action_Player_InvincibleBuff());
        mySkill.Enqueue(new Action_WaitForSeconds());
        mySkill.Enqueue(new Action_Player_ResetGravity());
        return mySkill;
    }


}
