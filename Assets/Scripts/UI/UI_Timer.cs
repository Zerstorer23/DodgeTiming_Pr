using Lex;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UI_Timer : MonoBehaviour
{
    Text timeText;
    bool showRemainingTime = false;
    private void Awake()
    {
        timeText = GetComponent<Text>();
    }
    public static double startTime;
    public static double endTime;
    private void OnEnable()
    {
        showRemainingTime = GameSession.gameModeInfo.gameMode == GameMode.TeamCP;
        startTime = LexNetwork.NetTime;
        if (showRemainingTime) { 
            endTime = LexNetwork.NetTime + GameField_CP.timeout;
        }
    }


    private void FixedUpdate()
    {
        double curr = LexNetwork.NetTime;
        if (showRemainingTime)
        {
            timeText.text = (endTime - curr).ToString("0.0");
        } else {
            timeText.text = (curr - startTime).ToString("0.0");
        }
    }
}
