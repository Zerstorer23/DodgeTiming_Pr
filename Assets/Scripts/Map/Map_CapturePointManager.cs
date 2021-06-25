using Lex;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Map_CapturePointManager : MonoBehaviourLex
{
    [SerializeField] public Map_CapturePoint[] capturePoints;
    Dictionary<int, Map_CapturePoint> mapDictionary = new Dictionary<int, Map_CapturePoint>();
    GameField_CP gameFieldCP;
    public int maxIndex, homeNext, awayNext = 0;
    public bool serialCaptureRequired = true;
    float pointPerSec = 0.5f;
    public float endThreshold = 100f;
    public float currentPoint = 0;
    private void Awake()
    {
        capturePoints = GetComponentsInChildren<Map_CapturePoint>();
        gameFieldCP = GetComponentInParent<GameField_CP>();
        mapDictionary.Clear();
        foreach (var cp in capturePoints)
        {
            if (cp.captureIndex >= maxIndex)
            {
                maxIndex = cp.captureIndex;
            }
            mapDictionary.Add(cp.captureIndex, cp);
        }

    }
    private void OnEnable()
    {
        gameFieldCP.gameFieldFinished = false;
        currentPoint = 0;
        awayNext = maxIndex;
        homeNext = 0;
        UI_RemainingPlayerDisplay.SetCPManager(this);
        EventManager.StartListening(MyEvents.EVENT_CP_CAPTURED, OnPointCaptured);
        if (pointerRoutine != null) StopCoroutine(pointerRoutine);
        pointerRoutine = PointCounter();
      //  Debug.Log("pointer count " + pointerRoutine);
        StartCoroutine(pointerRoutine);
    }


    private void OnDisable()
    {
        if (pointerRoutine != null) StopCoroutine(pointerRoutine);
        EventManager.StopListening(MyEvents.EVENT_CP_CAPTURED, OnPointCaptured);
    }
    IEnumerator pointerRoutine;
    IEnumerator PointCounter()
    {
       // Debug.Log("field finish count " + gameFieldCP.gameFieldFinished);
        while (!gameFieldCP.gameFieldFinished) {
            if (LexNetwork.IsMasterClient) {
                float amount = GetCP_Sum() * pointPerSec;
              //  Debug.Log("Set point " + amount);
                lexView.RPC("SetPoint",   currentPoint + amount);
            }
            yield return new WaitForSeconds(1f);
        }
    }


    int GetCP_Sum() {
        int sum = 0;
        foreach (var cp in capturePoints) {
            if (cp.owner == Team.HOME)
            {
                sum++;
            }
            else if (cp.owner == Team.AWAY) {
                sum--;
            }
        }
        return sum;

    }
    [LexRPC]
    void SetPoint(float point) {
        currentPoint = point;
    }
    public Team GetTeamWithMaxPoint() {
        if (currentPoint > endThreshold)
        {
            return (Team.HOME);
        }
        else if (currentPoint < -endThreshold)
        {
            return (Team.AWAY);
        }
        else {
            return Team.NONE;
        }
    }
    public Team GetHighestTeam() {
        if (currentPoint < 0)
        {
            return (Team.AWAY);
        }
        else
        {
            return (Team.HOME);
        }
    }

    private void OnPointCaptured(EventObject arg0)
    {
        DefineOpenPoints();
        foreach (var cp in capturePoints) {
            cp.UpdateBanner();
        }
    }

    public void DefineOpenPoints()
    {
        int i = 0;
        while (i <= maxIndex) {
            if (mapDictionary[i].owner == Team.NONE
                || mapDictionary[i].owner == Team.AWAY)
            {
                homeNext = i;
                break;
            }
            else {
                i++;
            }
        }

        i = maxIndex;
        while (i >= 0)
        {
            if (mapDictionary[i].owner == Team.NONE
                || mapDictionary[i].owner == Team.HOME)
            {
                awayNext = i;
                break;
            }
            else
            {
                i--;
            }
        }
      
        if (homeNext > maxIndex)
        {
            homeNext = maxIndex;
        }
        if (awayNext < 0)
        {
            awayNext = 0;
        }
    }

    public bool IsValidCapturePoint(Team team, int index) {
        if (!serialCaptureRequired) return true;
        if (team == Team.HOME)
        {
            return homeNext == index;
        }
        else {
            return awayNext == index;
        }
    }

    public Map_CapturePoint GetNearestValidPoint(Team myTeam, Vector3 position) {
        if (serialCaptureRequired)
        {
            if (myTeam == Team.HOME)
            {
                return mapDictionary[homeNext];
            }
            else
            {
                return mapDictionary[awayNext];
            }
        }
        else {
            var list = (from Map_CapturePoint cp in mapDictionary.Values
                        where cp.owner != myTeam
                        select cp).ToArray();
            if (list.Length <= 0) return null;

            var nearest = list.Aggregate((x, y) => 
                Vector2.Distance(position, x.transform.position) < Vector2.Distance(position, y.transform.position) ? x : y);
            return nearest;
        }
    }

}
