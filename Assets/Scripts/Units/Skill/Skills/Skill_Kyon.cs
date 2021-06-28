using Lex;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static ConstantStrings;

public class Skill_Kyon : ISkill
{
    /*    delegate void voidFunc();
        voidFunc DoSkill;
    */

    ISkill obtainedSkill = null;
    public override ActionSet GetSkillActionSet(SkillManager skillManager)
    {
        if (obtainedSkill == null)
        {
            return GetKyonkoSet(skillManager);
        }
        else
        {
            return obtainedSkill.GetSkillActionSet(skillManager);
        }
    }

    public override void LoadInformation(SkillManager skillManager)
    {
        skillManager.cooltime = 3.2f;
        original = skillManager;
    }
    SkillManager original;

    /*    public override void OnMyProjectileHit(EventObject eo)
        {
            HealthPoint targetHP = eo.hitHealthPoint;
            if (targetHP.unitType != UnitType.Player) return;
            if (targetHP.unitPlayer.myCharacter == CharacterType.KYONKO|| targetHP.unitPlayer.myCharacter == CharacterType.KYONKO) return;
            Debug.Log("Changed skill ");
            obtainedSkill = targetHP.unitPlayer.skillManager.mySkill;
            original.maxStack = 1;
            obtainedSkill.LoadInformation(original);
        }*/
    public override void OnPlayerKilledPlayer(EventObject eo)
    {
        HealthPoint targetHP = eo.hitHealthPoint;
        if (targetHP.unitType != UnitType.Player) return;
        if (eo.sourceDamageDealer.myHealth.controller.uid != original.controller.uid) return;
        //CheckYasumi(targetHP);
        CharacterType targetChar = targetHP.unitPlayer.myCharacter;
        if (targetChar == CharacterType.KYONKO
            || targetHP.unitPlayer.myCharacter == CharacterType.KYONKO
            || targetHP.unitPlayer.myCharacter == CharacterType.YASUMI
            ) return;

        Debug.Log("Changed skill ");
        obtainedSkill = targetHP.unitPlayer.skillManager.mySkill;
        original.maxStack = 1;
        obtainedSkill.LoadInformation(original);
        UI_SkillBox.SetSkillInfo(original, targetHP.unitPlayer.myCharacter);
    }
    void CheckYasumi(HealthPoint targetHP)
    {
        if (targetHP.unitPlayer.myCharacter == CharacterType.YASUMI)
        {
            if (original.pv.IsMine)
            {
                original.pv.RPC("AddBuff",   (int)BuffType.HideBuffs, 1f, -1d);
            }
        }
        else
        {
            BuffData buff = new BuffData(BuffType.HideBuffs, 1f, -1d);
            original.buffManager.RemoveBuff(buff);
        }
    }
    public ActionSet GetKyonkoSet(SkillManager skm)
    {
        ActionSet mySkill = new ActionSet(skm);
        mySkill.SetParam(SkillParams.PrefabName, PREFAB_BULLET_KYONKO);
        mySkill.SetParam(SkillParams.MoveSpeed, 25f);
        mySkill.SetParam(SkillParams.ReactionType, ReactionType.None);
        mySkill.SetParam(SkillParams.Duration, 0.17f);
        mySkill.Enqueue(new Action_GetCurrentPlayerPosition());
        // mySkill.Enqueue(new Action_GetCurrentPlayerPosition_AngledOffset());
        mySkill.Enqueue(new Action_InstantiateBulletAt());
        mySkill.Enqueue(new Action_SetProjectileStraight());
        mySkill.Enqueue(new Action_DoDeathAfter());
        return mySkill;
    }

}
