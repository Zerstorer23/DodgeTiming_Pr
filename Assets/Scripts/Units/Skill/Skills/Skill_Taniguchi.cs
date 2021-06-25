using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Skill_Taniguchi : ISkill
{
    public override ActionSet GetSkillActionSet(SkillManager skm)
    {
        ActionSet mySkill = new ActionSet(skm);
        mySkill.Enqueue(new Action_Player_Suicide());
        return mySkill;
    }

    public override void LoadInformation(SkillManager skm)
    {
        skm.cooltime = 1f;
    }

  
}
