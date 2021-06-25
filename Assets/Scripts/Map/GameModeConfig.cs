using Lex;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "SO/My Game Mode")]
public class GameModeConfig : ScriptableObject
{
    public GameMode gameMode;
    public bool callSuddenDeath;
    public bool useDesolator;
    public bool scaleMapByPlayerNum = true;
    public bool isTeamGame = false;
    public bool isCoop = false;

    public bool IsFieldFinished(GameStatus stat)
    {
        bool fieldFinished = false;
        switch (GameSession.gameModeInfo.gameMode)
        {
            case GameMode.PVP:
                fieldFinished = (stat.alive <= 1 || stat.onlyBotsRemain);
                if (fieldFinished && stat.lastDied == null) fieldFinished = false;
                break;
            case GameMode.TEAM:
                fieldFinished = (stat.alive_otherTeam <= 0 || stat.alive_ourTeam <= 0 || stat.onlyBotsRemain);
                break;
            case GameMode.PVE:
                fieldFinished = (stat.alive == 0);
                break;
            case GameMode.Tournament:
                fieldFinished = (stat.alive <= 1);
                break;

            case GameMode.TeamCP:
                fieldFinished = false;
                break;
        }
        return fieldFinished;
    }
    public bool CheckBotGame()
    {
        if (!LexNetwork.IsMasterClient) return false;
        switch (GameSession.gameModeInfo.gameMode)
        {
            case GameMode.PVP:
                return (LexNetwork.PlayerCount <= 1);
            case GameMode.TEAM:
                return (LexNetwork.PlayerCount <= 2);
            case GameMode.Tournament:
                return false;
            case GameMode.PVE:
                return false;
            case GameMode.TeamCP:
                return true;
        }
        return false;

    }
}
