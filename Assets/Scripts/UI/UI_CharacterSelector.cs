using Lex;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UI_CharacterSelector : MonoBehaviour
{
    [SerializeField] Text myCharName;
    [SerializeField] Image myCharImage;
    private void Awake()
    {
        EventManager.StartListening(MyEvents.EVENT_PLAYER_SELECTED_CHARACTER, OnCharacterSelected);
    }
    private void OnDestroy()
    {

        EventManager.StopListening(MyEvents.EVENT_PLAYER_SELECTED_CHARACTER, OnCharacterSelected);
    }
    public void OnCharacterSelected(EventObject eo)
    {
        int charID = eo.intObj;
        UnitConfig u = (UnitConfig)eo.objData;
        myCharName.text = u.txt_name;
        myCharImage.sprite = u.portraitImage;
        UI_PlayerLobbyManager.localPlayerInfo.pv.RPC("ChangeCharacter",   charID);
    }

    internal void SetInformation(CharacterType character)
    {
        UnitConfig u = ConfigsManager.unitDictionary[character];
        myCharName.text = u.txt_name;
        myCharImage.sprite = u.portraitImage;
    }
}
