/*using Lex;
using Photon.Realtime;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class LexPlayer
{
    ControllerType controllerType = ControllerType.Human;
    Player player;
    public string uid;
    Dictionary<string, object> customProperties = new Dictionary<string, object>();


    public bool IsBot {
        get => controllerType == ControllerType.Bot;
    }
    public bool IsHuman
    {
        get => controllerType == ControllerType.Human;
    }
    public string NickName
    {
        get { return GetNickName(); }
        set { SetNickName(value); }
    }
    public bool IsLocal {
        get => IsHuman && player.IsLocal;
    }
    public bool AmController()
    {
        if (IsHuman)
        {
            return player.IsLocal;
        }
        else
        {
            return LexNetwork.IsMasterClient;
        }
    }
    private void SetNickName(string value)
    {
        if (IsHuman)
        {
            player.NickName = value;
        }
        else {
            SetBotProperty("NICKNAME", value);
        }
    }

    private string GetNickName()
    {
        if (IsHuman)
        {
            return player.NickName;
        }
        else
        {
            if (!customProperties.ContainsKey("NICKNAME")) {
                SetBotProperty("NICKNAME", "ㅇㅇ");
            }
            return (string)customProperties["NICKNAME"];
        }
    }

    public LexPlayer(Player player) {
        controllerType = ControllerType.Human;
        this.player = player;
        uid = player.UserId;
    }

    public static string[] botNames = {
        "Langley","Saratoga","Lexington","Hornet","Ranger",
        "Yorktown","Enterprise","Wasp","Essex","Intrepid",
    "Franklin","Independence","Princeton","Bunker Hill",
    "Bataan","Kearsarge","Shangri-La","Midway","Saipan"};

    public LexPlayer(string uid) {
        controllerType = ControllerType.Bot;
        this.uid = uid;
        NickName = botNames[UnityEngine.Random.Range(0, botNames.Length)];
    }

    public T GetProperty<T>(string key) {
        if (controllerType == ControllerType.Human)
        {
            return (T)player.CustomProperties[key];
        }
        else {
            return (T)customProperties[key];
        }
    }
    public T GetProperty<T>(string key, T defaultValue)
    {
        if (IsHuman)
        {
            if (player.CustomProperties.ContainsKey(key))
            {
                return (T)player.CustomProperties[key];
            }
        }
        else
        {
            if (customProperties.ContainsKey(key))
            {
                return (T)customProperties[key];
            }
        }
        return defaultValue;
    }
    public bool HasProperty(string key) {
        if (controllerType == ControllerType.Human)
        {
            return player.CustomProperties.ContainsKey(key);
        }
        else
        {
            return customProperties.ContainsKey(key);
        }
    }
    public void SetCustomProperties(string tag, object value, bool broadcast = true) {
        if (IsHuman)
        {
            var hash = new ExitGames.Client.Photon.Hashtable();
            hash.Add(tag, value);
            player.SetCustomProperties(hash);
        }
        else {
            SetBotProperty(tag, value, broadcast);
        }
    }

    private void SetBotProperty(string tag, object value, bool broadcast = true) {
        if (!IsBot) return;
        if (customProperties.ContainsKey(tag))
        {
            customProperties[tag] = value;
        }
        else {
            customProperties.Add(tag, value);
        }
        if (broadcast && LexNetwork.IsMasterClient) {
            StatisticsManager.SetBotProperty_Broadcast(uid, tag, value);
        }
    }


    public override string ToString()
    {
        if (IsHuman)
        {
            return player.ToString();
        }
        else {
            return uid;
        }
    }
}
*/