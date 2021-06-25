using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UI_AimOption : MonoBehaviour
{
    public static bool aimManual = false;
    Toggle toggle;
    public void Initialise() {
        toggle = GetComponent<Toggle>();

        aimManual = PlayerPrefs.GetInt(ConstantStrings.PREFS_MANUAL_AIM, 1) != 0;
        toggle.isOn = aimManual;
    }
    public void OnToggleChanged() {
        aimManual = toggle.isOn;
        try { 
        PlayerPrefs.SetInt(ConstantStrings.PREFS_MANUAL_AIM, (aimManual) ? 1 : 0);
        PlayerPrefs.Save();
    }  
        catch (Exception e)
            {
                Debug.LogWarning(e.Message);
            }
    }
}
