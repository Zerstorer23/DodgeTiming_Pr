using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using static ConstantStrings;

public class UI_CP_Icon : MonoBehaviour
{
    [SerializeField] Image fillSprite;
    [SerializeField] Image backSprite;
    public Map_CapturePoint cp;

    private void OnEnable()
    {
        EventManager.StartListening(MyEvents.EVENT_CP_CAPTURED, OnCPCaptured);
    }
    public void RegisterCP(Map_CapturePoint cp) {
        this.cp = cp;
    }

    private void OnCPCaptured(EventObject arg0)
    {
        int index = arg0.intObj;
        if (index != cp.captureIndex) return;
        Team owner = arg0.Get<Team>();
        backSprite.color = GetColorByHex(team_color[(int)owner]);
        fillSprite.color = backSprite.color;
    }

    private void FixedUpdate()
    {
        if (cp == null) return;
        if (cp.dominantTeam != cp.owner && cp.owner != Team.NONE)
        {
            fillSprite.color = GetColorByHex(team_color[0]);
            fillSprite.fillAmount = (1-cp.fillSprite.fillAmount);
        }
        else {

            fillSprite.color = cp.fillSprite.color;
            fillSprite.fillAmount = cp.fillSprite.fillAmount;
        }
    }
    private void OnDisable()
    {
        cp = null;
        EventManager.StopListening(MyEvents.EVENT_CP_CAPTURED, OnCPCaptured);
    }

}
