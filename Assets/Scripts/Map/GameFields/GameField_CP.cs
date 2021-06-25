using Lex;
using System.Collections;
using UnityEngine;

public class GameField_CP : GameField
{
    [SerializeField] Transform[] spawnPositions;
    internal Map_CapturePointManager cpManager;
    public override void Awake()
    {
        base.Awake();
        cpManager = GetComponentInChildren<Map_CapturePointManager>();
    }
    public override Vector3 GetPlayerSpawnPosition(LexPlayer myPlayer)
    {
        Team team = myPlayer.GetProperty(Property.Team, Team.NONE);
        if (team == Team.HOME)
        {
            return spawnPositions[0].position;
        }
        else {
            return spawnPositions[1].position;
        }
    }

    IEnumerator timeoutRoutine;
    public override void OnEnable()
    {
        base.OnEnable();
        timeoutRoutine = GameSession.CheckCoroutine(timeoutRoutine, WaitAndFinishGame());
        StartCoroutine(timeoutRoutine);
    }
    public static float timeout = 180f;
    IEnumerator WaitAndFinishGame() {
        float elapsedTime = 0f;
        Team winner;
        while (elapsedTime < timeout) {
            winner = cpManager.GetTeamWithMaxPoint();
            if (winner != Team.NONE)
            {
                if (LexNetwork.IsMasterClient) {
                    lexView.RPC("FinishGame",   (int)winner);
                }
                yield break;
            }
            else {
                elapsedTime++;
                yield return new WaitForSeconds(1f);
            }
        }
        if (LexNetwork.IsMasterClient)
        {
            winner = cpManager.GetHighestTeam();
            lexView.RPC("FinishGame",   (int)winner);
        }

    }
    public override void OnDisable()
    {
        base.OnDisable();
        if (timeoutRoutine != null) {

            StopCoroutine(timeoutRoutine);
        }
    }
    [LexRPC]
    public void FinishGame(Team winTeam)
    {
        LexPlayer winner = LexNetwork.GetPlayerOfTeam(winTeam);
        if (gameFieldFinished) return;
        if (timeoutRoutine != null)
        {
            StopCoroutine(timeoutRoutine);
        }

        Debug.LogWarning("GAME FISNISHED /  winner " + winner);
        gameFieldFinished = true;
        fieldWinner = winner;
        winnerName = winner.NickName;
        //  Debug.Log("FIeld " + fieldNo + " finished with winner " + fieldWinner);
        EventManager.TriggerEvent(MyEvents.EVENT_FIELD_FINISHED, new EventObject() { intObj = fieldNo });
        gameObject.SetActive(false);
        GameFieldManager.instance.FinishTheGame(winner.uid);
    }
    public override void CheckFieldConditions(GameStatus stat)
    {
        //Intentionally left blank to do nothing

    }
}
