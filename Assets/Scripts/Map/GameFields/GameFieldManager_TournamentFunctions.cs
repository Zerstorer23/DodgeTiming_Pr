using Lex;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public partial class GameFieldManager : MonoBehaviourLex
{
    // Start is called before the first frame update
    [SerializeField] GameField[] tournamentFields;
    public static void SetUpTournament()
    {
        int i = 0;
        foreach (GameField field in instance.tournamentFields)
        {
            gameFields.Add(field);
            field.InitialiseMap(i++);
        }
    }
    private IEnumerator WaitAndContinueTournament(List<LexPlayer> survivors)
    {
        float delay = 3f;
        //  Debug.Log("Open tourny panel ...");
        GameSession.instance.tournamentPanel.SetPanel(survivors.ToArray(), delay);
        EventManager.TriggerEvent(MyEvents.EVENT_POP_UP_PANEL, new EventObject() { objData = ScreenType.TournamentResult, boolObj = true });
        AssignMyRoom(survivors.ToArray(), 2);
        GameSession.instance.tournamentPanel.SetNext(playersInFieldsMap);
        yield return new WaitForSeconds(delay);
        StartGame();
        if (GameSession.LocalPlayer_FieldNumber == -1)
        {
            ChangeToSpectator();
        }
    }

}
