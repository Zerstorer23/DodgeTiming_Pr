using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UI_OptionsPanel : MonoBehaviour
{
    [SerializeField] UI_Mute audioOption;
    [SerializeField] UI_AimOption aimOption;

    private void Start()
    {
        audioOption.Initialise();
        aimOption.Initialise();
    }
    public void OnClick_Exit() {
        EventManager.TriggerEvent(MyEvents.EVENT_POP_UP_PANEL, new EventObject() { objData = ScreenType.Settings, boolObj = false });
    }

}
