using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BuffIndicator : MonoBehaviour
{

    [SerializeField] GameObject slow, fast, invincible, cooltime, skilldefault;

    [SerializeField] BuffManager buffManager;
    [SerializeField] Unit_Player unit;



    public void UpdateUI(BuffType changedBuff, bool enable)
    {
        switch (changedBuff)
        {
            case BuffType.MoveSpeed:
                float moveSpeed = buffManager.HasBuff(BuffType.MoveSpeed);
                slow.SetActive(moveSpeed < 0f);
                fast.SetActive(moveSpeed > 0f);
                break;
            case BuffType.Cooltime:
                cooltime.SetActive(buffManager.HasBuff(BuffType.Cooltime) != 0f);
                break;
            case BuffType.InvincibleFromBullets:
                bool isInvincible = buffManager.GetTrigger(BuffType.InvincibleFromBullets);
                unit.mainSprite.color = (isInvincible) ? Color.red : Color.white;
                break;
            case BuffType.MirrorDamage:
                skilldefault.SetActive(enable);
                break;
        }

    }

    internal void ClearBuffs()
    {
        slow.SetActive(false);
        fast.SetActive(false);
        cooltime.SetActive(false);
        unit.mainSprite.color = Color.white;
        skilldefault.SetActive(false);
    }
}
