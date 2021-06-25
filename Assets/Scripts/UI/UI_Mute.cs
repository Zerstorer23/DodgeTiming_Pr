using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UI_Mute : MonoBehaviour
{
    Toggle muteTogle;

    private void Awake()
    {
        muteTogle = GetComponent<Toggle>();
    }
    public void Initialise() {
        if (muteTogle == null)
        {
            muteTogle = GetComponent<Toggle>();

        }
        muteTogle.isOn = PlayerPrefs.GetInt(ConstantStrings.PREFS_MUTED, 0) == 1;
        SetAudio();
    }

    public void OnToggleChanged() {
        int on = (muteTogle.isOn)? 1: 0;
        try { 
        PlayerPrefs.SetInt(ConstantStrings.PREFS_MUTED, on);
        PlayerPrefs.Save();
        }
        catch (Exception e)
        {
            Debug.LogWarning(e.Message);
        }
        SetAudio();
    }
    public void SetAudio() {
        AudioManager.SetMute(muteTogle.isOn);
    }
}
