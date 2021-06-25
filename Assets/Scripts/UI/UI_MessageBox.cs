using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UI_MessageBox : MonoBehaviour
{
  [SerializeField] UnityEngine.GameObject panel;
    [SerializeField] Text text;

    IEnumerator coroutine;
    private void Awake()
    {
        EventManager.StartListening(MyEvents.EVENT_SEND_MESSAGE, OnMessageReceived);
    }

    private void OnMessageReceived(EventObject arg0)
    {
        text.text = arg0.stringObj;
        if (coroutine != null) {
            StopCoroutine(coroutine);   
        }
        coroutine = ShowMessage();
        StartCoroutine(coroutine);

    }

    IEnumerator ShowMessage() {
        panel.SetActive(true);
        yield return new WaitForSeconds(1f);
    ;
        panel.SetActive(false);
    }
}
