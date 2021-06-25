using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UI_SkillBox : MonoBehaviour
{
    SkillManager skill;
    [SerializeField] Image portrait;
    [SerializeField] Image fillSprite;
    [SerializeField] Text desc;
    [SerializeField] Text stackText;
    [SerializeField] Text colltimeText;
    private static UI_SkillBox instance;
    private void Awake()
    {
        instance = this;
    }
    public static void SetSkillInfo(SkillManager skm) {
        instance.skill = skm;
        instance.portrait.sprite = ConfigsManager.unitDictionary[skm.myCharacter].portraitImage;
        instance.desc.text = ConfigsManager.unitDictionary[skm.myCharacter].txt_skill_desc;
    }
    public static void SetSkillInfo(SkillManager skm, CharacterType character)
    {
        if (!skm.controller.IsLocal) return;
            instance.skill = skm;
        instance.portrait.sprite = ConfigsManager.unitDictionary[character].portraitImage;
        instance.desc.text = ConfigsManager.unitDictionary[character].txt_skill_desc;
    }
    private void FixedUpdate()
    {
        if (skill == null) return;
        UpdateCooltime();
    }

    public void UpdateCooltime() {
        double remain = skill.remainingStackTime;
        double perc = remain / skill.cooltime;
        fillSprite.fillAmount =(float) perc;
        colltimeText.text = (skill.currStack == skill.maxStack) ? " " :remain.ToString("0.0");
        stackText.text = skill.currStack + "/" + skill.maxStack;    
    }
}
