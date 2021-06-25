using Lex;
using System;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
public class StatisticsManager : MonoBehaviourLex
{
    // Start is called before the first frame update
    private static StatisticsManager prStatManager;
    LexView pv;
    private void Awake()
    {
        pv = GetComponent<LexView>();
        EventManager.StartListening(MyEvents.EVENT_POP_UP_PANEL, OnPopUpPanel);
        EventManager.StartListening(MyEvents.EVENT_GAME_STARTED, OnGameStart);
    }
    private void OnDestroy()
    {

        EventManager.StopListening(MyEvents.EVENT_POP_UP_PANEL, OnPopUpPanel);
        EventManager.StopListening(MyEvents.EVENT_GAME_STARTED, OnGameStart);
    }

    private void OnGameStart(EventObject obj)
    {
        Init();
    }

    private void OnPopUpPanel(EventObject obj)
    {
        if((ScreenType)obj.objData == ScreenType.GameOver)
        SaveMyStats();
    }

    public static StatisticsManager instance
    {
        get
        {
            if (!prStatManager)
            {
                prStatManager = FindObjectOfType<StatisticsManager>();
                if (!prStatManager)
                {
                    Debug.LogWarning("There needs to be one active EventManger script on a GameObject in your scene.");
                }

            }

            return prStatManager;
        }
    }
    private Dictionary<StatTypes, Dictionary<string, int>> statLibrary;
    private Dictionary<string, int> localStatLibrary;
    public  void Init()
    {
 
        statLibrary = new Dictionary<StatTypes, Dictionary<string, int>>();
        for (int i = 0; i < (int)StatTypes.END;  i++) {
            StatTypes head = (StatTypes)i;
            Dictionary<string, int> library = new Dictionary<string, int>();
            statLibrary.Add(head, library);
        }

        localStatLibrary = new Dictionary<string, int>();
        LoadMyStats();
    }
   
    public static void RPC_AddToStat(StatTypes stype ,string tag, int amount)
    {
        instance.pv.RPC("AddToStat",  (int)stype, tag, amount);
    }
    [LexRPC]
    public void AddToStat(int statType, string tag, int amount) {
        if (statLibrary == null) {
            Init();
        }
        StatTypes head = (StatTypes)statType;
        if (head == StatTypes.END) return;

        if (!statLibrary[head].ContainsKey(tag))
        {
            statLibrary[head].Add(tag, amount);
        }
        else {
            statLibrary[head][tag] += amount;
        }
    }

    public  void LoadMyStats() {
        try { 
        int kills = PlayerPrefs.GetInt(ConstantStrings.PREFS_KILLS, 0);
        int wins = PlayerPrefs.GetInt(ConstantStrings.PREFS_WINS, 0);
        int evades = PlayerPrefs.GetInt(ConstantStrings.PREFS_EVADES, 0);
        localStatLibrary.Add(ConstantStrings.PREFS_KILLS, kills);
        localStatLibrary.Add(ConstantStrings.PREFS_WINS, wins);
        localStatLibrary.Add(ConstantStrings.PREFS_EVADES, evades);
        }
        catch (Exception e)
        {
            Debug.LogWarning(e.Message);
        }
    }
    public void SaveMyStats()
    {
        try
        {
                 
        PlayerPrefs.SetInt(ConstantStrings.PREFS_KILLS, localStatLibrary[ConstantStrings.PREFS_KILLS]);
        PlayerPrefs.SetInt(ConstantStrings.PREFS_WINS, localStatLibrary[ConstantStrings.PREFS_WINS]);
        PlayerPrefs.SetInt(ConstantStrings.PREFS_EVADES, localStatLibrary[ConstantStrings.PREFS_EVADES]);
        PlayerPrefs.Save();
        }
        catch (Exception e)
        {
            Debug.LogWarning(e.Message);
        }
    }
    public void AddToLocalStat(string tag, int value) {
        if (statLibrary == null)
        {
            Init();
        }
        localStatLibrary[tag] += value;
    }
    public int GetLocalStat(string tag, int value) {
        if (!localStatLibrary.ContainsKey(tag)) return value;
        return localStatLibrary[tag];
    }

    internal static string GetHighestPlayer(StatTypes header)
    {
        Dictionary<string, int> statBoard = instance.statLibrary[header];
        if (statBoard.Count == 0) {
            return null;
        }else      if (statBoard.Count == 1) {
            return statBoard.First().Key;
        }
        var keyOfMaxValue = statBoard.Aggregate((x, y) => x.Value > y.Value ? x : y).Key;
        return keyOfMaxValue;
    }


    public static int GetStat(StatTypes stype,string playerID) {
        if (instance.statLibrary[stype].ContainsKey(playerID))
        {
           return instance.statLibrary[stype][playerID];
        }
       return 0;
    }


    internal static void SetStat(StatTypes stype, string playerID, int v)
    {
        if (instance.statLibrary[stype].ContainsKey(playerID))
        {
            instance.statLibrary[stype][playerID] = v;
        }
        else
        {
            instance.statLibrary[stype].Add(playerID, v);
        }
    }

}
public enum StatTypes { 
    GENERAL, KILL,EVADE,MINIGAME,SCORE
        ,END
}
