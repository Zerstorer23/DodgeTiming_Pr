using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static ConstantStrings;

public class Skill_Mikuru : ISkill
{
    public override ActionSet GetSkillActionSet(SkillManager skm)
    {
        ActionSet mySkill = new ActionSet(skm);
        mySkill.SetParam(SkillParams.Distance, 1f);
        mySkill.SetParam(SkillParams.PrefabName, PREFAB_BULLET_MIKURU);
        mySkill.SetParam(SkillParams.ReactionType, ReactionType.None);

        mySkill.Enqueue(new Action_GetCurrentPlayerPosition_AngledOffset());
       // mySkill.Enqueue(new Action_InstantiateBulletAt_Mikuru());
        mySkill.Enqueue(new Action_InstantiateBulletAt());
      //  mySkill.Enqueue(new Action_SetProjectileStraight());
        mySkill.Enqueue(new Action_SetProjectileStatic());
     
        return mySkill;
    }


    public override void LoadInformation(SkillManager skm)
    {
        skm.cooltime = 3.2f;
        skm.maxStack = 1;
    }
    /*
       public override ActionSet GetSkillActionSet(SkillManager skm)
    {
        ActionSet mySkill = new ActionSet(skm);
        mySkill.SetParam(SkillParams.MoveSpeed, 165f);
        mySkill.SetParam(SkillParams.Distance, 5f);
        mySkill.SetParam(SkillParams.Duration, 0.35f);
        mySkill.SetParam(SkillParams.PrefabName, PREFAB_BULLET_MIKURU);
        mySkill.SetParam(SkillParams.ReactionType, ReactionType.None);
        int numBullets = 1;
        //      mySkill.Enqueue(new Action_Player_InvincibleBuff());//
        for (int n = 0; n < numBullets; n++)
        {
            mySkill.Enqueue(new Action_GetCurrentPlayerPosition_AngledOffset());
            mySkill.Enqueue(new Action_InstantiateBulletAt());
            mySkill.Enqueue(new Action_SetProjectileStraight());
        }
        return mySkill;
    }
     */

}
