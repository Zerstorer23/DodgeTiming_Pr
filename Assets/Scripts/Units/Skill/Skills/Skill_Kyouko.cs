using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static ConstantStrings;

public class Skill_Kyouko : ISkill
{
    public override ActionSet GetSkillActionSet(SkillManager skm)
    {
        ActionSet mySkill = new ActionSet(skm);
        mySkill.SetParam(SkillParams.MoveSpeed, 25f);
        mySkill.SetParam(SkillParams.PrefabName, PREFAB_BULLET_KYOUKO);
        mySkill.SetParam(SkillParams.ReactionType, ReactionType.Die);
        mySkill.SetParam(SkillParams.Duration, 0.25f);
        for (int i = 0; i < 2; i++)
        {
            mySkill.Enqueue(new Action_Player_GetAim());
            mySkill.Enqueue(new Action_GetCurrentPlayerVector3_AngledOffset());
            mySkill.Enqueue(new Action_InstantiateBulletAt());
            mySkill.Enqueue(new Action_SetProjectileStraight());
            mySkill.Enqueue(new Action_WaitForSeconds());

        }
        /*        float angleSize = 15f;
                float angleOffset = skm.unitMovement.GetAim() - angleSize;
                for (int i = 0; i < 3; i++)
                {
                    float angle = angleOffset +angleSize * i;
                    mySkill.Enqueue(new Action_SetAngle() { paramValue = angle });
                    mySkill.Enqueue(new Action_GetCurrentPlayerVector3_AngledOffset());
                    mySkill.Enqueue(new Action_InstantiateBulletAt());
                    mySkill.Enqueue(new Action_SetProjectileStraight());
                }
        */
        return mySkill;
    }


    public override void LoadInformation(SkillManager skm)
    {   
        skm.cooltime = 3.5f;
        skm.maxStack = 1;
    }
    /*
     
    public override ActionSet GetSkillActionSet(SkillManager skm)
    {
        ActionSet mySkill = new ActionSet(skm);
        mySkill.SetParam(SkillParams.MoveSpeed, 16f);
        mySkill.SetParam(SkillParams.Duration, 0.5f);
        mySkill.SetParam(SkillParams.PrefabName, PREFAB_BULLET_KYOUKO);
        mySkill.SetParam(SkillParams.ReactionType, ReactionType.None);
        mySkill.SetParam(SkillParams.RotateAngle, 90f);
        mySkill.SetParam(SkillParams.RotateSpeed, 270f);
        float angleOffset = skm.unitMovement.GetAim();
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

        return mySkill;
    }


    public override void LoadInformation(SkillManager skm)
    {   
        skm.cooltime = 3.6f;
        skm.maxStack = 1;
    }
     */

}
