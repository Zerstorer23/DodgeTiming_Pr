using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UI_GamePadOptions : MonoBehaviour
{
    [SerializeField] GameObject optionPanel;
    [SerializeField] Toggle psToggle, xboxToggle;
    [SerializeField] Text nowUsingText;
    public static bool useGamepad = false;
    public static PadType padType;

    static UI_GamePadOptions instance;
   private void Awake()
    {
        EventManager.StartListening(MyEvents.EVENT_POP_UP_PANEL, OnPanelOpen);
        instance = this;
    }

    private void OnPanelOpen(EventObject arg0)
    { 
        if ((ScreenType)arg0.objData == ScreenType.Settings) {
                QueryPadInfo();
        }
    }

    private void OnDestroy()
    {
        EventManager.StopListening(MyEvents.EVENT_POP_UP_PANEL, OnPanelOpen);
    }
    private void Start()
    {
        QueryPadInfo();
    }

    public static void QueryPadInfo() {
        var names = Input.GetJoystickNames();
        int count = 0;
        foreach (var n in names) {
            if (!String.IsNullOrEmpty(n)) count++;
        }
        useGamepad = (count > 0);
        instance.UpdateUI();
        if (useGamepad)
        {
            padType = (PadType)PlayerPrefs.GetInt(ConstantStrings.PREFS_MY_PAD, 0);
        }
    }
    public void UpdateUI() {
        if (useGamepad)
        {
            nowUsingText.text = "사용중:게임패드";
        }
        else
        {
            nowUsingText.text = "사용중:키보드";
        }
        optionPanel.SetActive(useGamepad);
    }

    public void OnClickToggle_Pad()
    {
        if (psToggle.isOn)
        {
            padType = PadType.PS4;
        }
        else
        {
            padType = PadType.XBOX;
        }
        try { 
        PlayerPrefs.SetInt(ConstantStrings.PREFS_MY_PAD, (int)padType);
        PlayerPrefs.Save();
        }
        catch (Exception e)
        {
            Debug.LogWarning(e.Message);
        }
    }

}

public enum PadType
{
    PS4, XBOX
}