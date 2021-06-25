using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UI_SubOptionsManager : MonoBehaviour
{
    [SerializeField] GameObject anonGame, changeTeam, addBot, removeBot;
    private void OnEnable()
    {
        EventManager.StartListening(MyEvents.EVENT_GAMEMODE_CHANGED, OnGamemodeChanged);
        EventManager.StartListening(MyEvents.EVENT_PLAYER_JOINED, OnGamemodeChanged);
    }
    private void OnDisable()
    {
        EventManager.StopListening(MyEvents.EVENT_GAMEMODE_CHANGED, OnGamemodeChanged);
        EventManager.StopListening(MyEvents.EVENT_PLAYER_JOINED, OnGamemodeChanged);

    }

    private void OnGamemodeChanged(EventObject arg0)
    {
        GameModeConfig mode = GameSession.gameModeInfo;
        StartCoroutine(WaitAndActive(mode));

    }
    IEnumerator WaitAndActive(GameModeConfig mode) {
        yield return new WaitForFixedUpdate();
        changeTeam.SetActive(mode.isTeamGame);
        bool allowBots = mode.CheckBotGame();
        addBot.SetActive(allowBots);
        removeBot.SetActive(allowBots);
    }
}
