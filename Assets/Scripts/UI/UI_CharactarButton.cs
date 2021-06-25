using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UI_CharactarButton : MonoBehaviour
{
    public UnitConfig myConfig;
    [SerializeField] Image portraitImage;
    [SerializeField] Text nameText;
    private void Awake()
    {
        portraitImage.sprite = myConfig.portraitImage;
        nameText.text = myConfig.txt_name;

    }
    public void OnClickCharacter() {
        EventManager.TriggerEvent(MyEvents.EVENT_PLAYER_SELECTED_CHARACTER, new EventObject() { intObj = (int)myConfig.characterID , objData  = myConfig});
    }

}
