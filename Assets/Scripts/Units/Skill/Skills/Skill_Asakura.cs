using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static ConstantStrings;

public class Skill_Asakura : ISkill
{
    public delegate void setFunc(ActionSet mySkill);
    public static setFunc skillSet;
    setFunc[] sets = new setFunc[] {
       KyoukoSet, AsakuraSet, FrontFire,
       ShotGunFire ,CurveFire
    };


    public override ActionSet GetSkillActionSet(SkillManager skm)
    {
        ActionSet mySkill = new ActionSet(skm);
        int rand = Random.Range(0, sets.Length);
        skillSet = sets[rand];
        skillSet(mySkill);
        Debug.Log("Play  " + skillSet.Method.Name);
        return mySkill;
    }
    public static void KyoukoSet(ActionSet mySkill)
    {
        mySkill.SetParam(SkillParams.MoveSpeed, 16f);
        mySkill.SetParam(SkillParams.Duration, 0.5f);
        mySkill.SetParam(SkillParams.PrefabName, PREFAB_BULLET_ASAKURA);
        mySkill.SetParam(SkillParams.ReactionType, ReactionType.None);
        mySkill.SetParam(SkillParams.RotateAngle, 90f);
        mySkill.SetParam(SkillParams.RotateSpeed, 210f);
        float angleOffset = mySkill.skillManager.unitMovement.GetAim();
        int stepSize = 4;
        for (int n = 0; n < 2; n++)
        {
            for (int i = 0; i < stepSize; i++)
            {
                float angle = angleOffset + (360f / stepSize) * i;
                mySkill.Enqueue(new Action_SetAngle() { paramValue = angle });
                mySkill.Enqueue(new Action_GetCurrentPlayerVector3_AngledOffset());
                mySkill.Enqueue(new Action_InstantiateBulletAt());
                mySkill.Enqueue(new Action_SetProjectileStraight());
            }
            //mySkill.Enqueue(new Action_Player_InvincibleBuff());//
            mySkill.Enqueue(new Action_WaitForSeconds());
            for (int i = 0; i < stepSize; i++)
            {
                float angle = angleOffset + ((360f / stepSize) / 2) + (360f / stepSize) * i;
                mySkill.Enqueue(new Action_SetAngle() { paramValue = angle });
                mySkill.Enqueue(new Action_GetCurrentPlayerVector3_AngledOffset());
                mySkill.Enqueue(new Action_InstantiateBulletAt());
                mySkill.Enqueue(new Action_SetProjectileCurves());
            }
            //  mySkill.Enqueue(new Action_Player_InvincibleBuff());//
            mySkill.Enqueue(new Action_WaitForSeconds());
        }
    }
    public static void AsakuraSet(ActionSet mySkill)
    {
        mySkill.SetParam(SkillParams.MoveSpeed, 22f);
        mySkill.SetParam(SkillParams.RotateAngle, 60f);
        mySkill.SetParam(SkillParams.RotateSpeed, 150f);
        mySkill.SetParam(SkillParams.Duration, 0.033f);
        mySkill.SetParam(SkillParams.PrefabName, PREFAB_BULLET_ASAKURA);
        mySkill.SetParam(SkillParams.ReactionType, ReactionType.None);
        float angleOffset = mySkill.skillManager.unitMovement.GetAim();
        int numStep = 15;
        for (int i = 0; i < numStep; i++)
        {
            float angle = angleOffset + (360 / numStep) * i;
            mySkill.Enqueue(new Action_SetAngle() { paramValue = angle });
            mySkill.Enqueue(new Action_GetCurrentPlayerVector3_AngledOffset());
            mySkill.Enqueue(new Action_InstantiateBulletAt());
            //  mySkill.Enqueue(new Action_SetProjectileStraight());
            mySkill.Enqueue(new Action_SetProjectileCurves());
            //  mySkill.Enqueue(new Action_Player_InvincibleBuff());//
            mySkill.Enqueue(new Action_WaitForSeconds());
        }

    }
    public static void FrontFire(ActionSet mySkill)
    {
        mySkill.SetParam(SkillParams.MoveSpeed, 18f);
        mySkill.SetParam(SkillParams.Duration, 0.1f);
        mySkill.SetParam(SkillParams.PrefabName, PREFAB_BULLET_ASAKURA);
        mySkill.SetParam(SkillParams.ReactionType, ReactionType.None);
        int numStep = 18;
        float stepAngle = 30f;
        int changeSignEvery = 6;
        float angleOffset = -(stepAngle * changeSignEvery*0.5f);
        Debug.Log("Start "+angleOffset);
        for (int i = 1; i < (numStep+1); i++)
        {
            angleOffset += stepAngle;
            Debug.Log(angleOffset);
            if (i % changeSignEvery == 0) {
                stepAngle *= -1f;
            }
            mySkill.Enqueue(new Action_Player_GetAim());
            mySkill.Enqueue(new Action_ModifyAngle() { paramValue = angleOffset });
            mySkill.Enqueue(new Action_GetCurrentPlayerVector3_AngledOffset());
            mySkill.Enqueue(new Action_InstantiateBulletAt());
            mySkill.Enqueue(new Action_SetProjectileStraight());
            mySkill.Enqueue(new Action_WaitForSeconds());
        }

    }
    public static void ShotGunFire(ActionSet mySkill)
    {
        mySkill.SetParam(SkillParams.MoveSpeed, 15f);
        mySkill.SetParam(SkillParams.Duration, 0.5f);
        mySkill.SetParam(SkillParams.PrefabName, PREFAB_BULLET_ASAKURA);
        mySkill.SetParam(SkillParams.ReactionType, ReactionType.None);
        for (int i = 0; i<3;i++)
        {
            float startAngle = -30f;
            for (int j = 0; j < 3; j++) {
                float modAngle = startAngle + (30f * j);
                mySkill.Enqueue(new Action_Player_GetAim());
                mySkill.Enqueue(new Action_ModifyAngle() { paramValue = modAngle });
                mySkill.Enqueue(new Action_GetCurrentPlayerVector3_AngledOffset());
                mySkill.Enqueue(new Action_InstantiateBulletAt());
                mySkill.Enqueue(new Action_SetProjectileStraight());
            }
            mySkill.Enqueue(new Action_WaitForSeconds());
        }
    }
    public static void CurveFire(ActionSet mySkill)
    {
        mySkill.SetParam(SkillParams.MoveSpeed, 15f);
        mySkill.SetParam(SkillParams.Duration, 0.5f);
        mySkill.SetParam(SkillParams.PrefabName, PREFAB_BULLET_ASAKURA);
        mySkill.SetParam(SkillParams.ReactionType, ReactionType.None);
        mySkill.SetParam(SkillParams.RotateAngle, 90f);
        mySkill.SetParam(SkillParams.RotateSpeed, 180f);
        for (int i = 0; i < 4; i++)
        {
            float startAngle = -30f;
            for (int j = 0; j < 3; j++)
            {
                float modAngle = startAngle + (30f * j);
                mySkill.Enqueue(new Action_Player_GetAim());
                mySkill.Enqueue(new Action_ModifyAngle() { paramValue = modAngle });
                mySkill.Enqueue(new Action_GetCurrentPlayerVector3_AngledOffset());
                mySkill.Enqueue(new Action_InstantiateBulletAt());
                mySkill.Enqueue(new Action_SetProjectileCurves());
            }
            mySkill.Enqueue(new Action_WaitForSeconds());
        }
    }
    public override void LoadInformation(SkillManager skm)
    {
        skm.cooltime = 3.5f;
        skm.maxStack = 1;
    }


}
