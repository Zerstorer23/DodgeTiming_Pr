using Lex;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UI_StatDisplay : MonoBehaviour
{
    Text displayText;
    static Unit_Player myPlayer;
    int kills = 0;
    private void Awake()
    {
        displayText = GetComponent<Text>();
        EventManager.StartListening(MyEvents.EVENT_PLAYER_DIED, OnPlayerDied);
    }
    private void OnDestroy()
    {
        EventManager.StopListening(MyEvents.EVENT_PLAYER_DIED, OnPlayerDied);

    }

    private void OnPlayerDied(EventObject arg0)
    {
        kills = StatisticsManager.GetStat(StatTypes.KILL, LexNetwork.LocalPlayer.uid);
    }

    public static void SetPlayer(Unit_Player p) {
        myPlayer = p;
    }

    void FixedUpdate()
    {
        if (myPlayer == null) return;
        displayText.text = string.Format("{0}...{1}킬 {2}회피", LexNetwork.NickName, kills.ToString(),StatisticsManager.GetStat(StatTypes.EVADE,LexNetwork.LocalPlayer.uid));
    }
}
