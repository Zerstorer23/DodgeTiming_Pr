using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Unit_HUD : MonoBehaviour
{
    [SerializeField] Image HP_fillSprite;
   [SerializeField] Image MP_fillCooltime;
   [SerializeField] Image MP_fillStack;
   [SerializeField] Image teamIndicator;
   [SerializeField] Text hpText;
   [SerializeField] Text nameText;
   [SerializeField] Unit_Player player;
    int maxLife;
    SkillManager skill;
    private void OnEnable()
    {
       
        if (player.controller.IsLocal)
        {
            nameText.enabled = false;
        }
        else
        {
            nameText.enabled = true;
            nameText.text = player.controller.Owner.NickName;
        }
        if (GameSession.gameModeInfo.isTeamGame)
        {
            teamIndicator.enabled = true;
            teamIndicator.color = ConstantStrings.GetColorByHex(ConstantStrings.team_color[(int)player.myTeam]);
        }
        else
        {
            teamIndicator.enabled = false;
        }

        skill = player.skillManager;
        maxLife = player.health.GetMaxLife();
    }

    private void FixedUpdate()
    {
        SetMP();
        SetHP();
    }

    private void SetHP()
    {
        HP_fillSprite.fillAmount = (float)player.health.currentLife / maxLife;
        hpText.text = player.health.currentLife.ToString("0");
    }

    private void SetMP()
    {
        if (!player.controller.IsLocal && player.buffManager.GetTrigger(BuffType.HideBuffs)) {
          //  Debug.Log("HIdden skill");
            MP_fillStack.fillAmount = 1;
            return;
        }
        float stackFill = (float)skill.currStack / skill.maxStack;
        MP_fillStack.fillAmount = stackFill;
        float coolFill = 1 - (skill.remainingStackTime / skill.cooltime);
        MP_fillCooltime.fillAmount = stackFill + (1f / player.skillManager.maxStack) * coolFill;
    }
}
