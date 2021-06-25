using DG.Tweening;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[System.Serializable]
public class ScreenPanel : MonoBehaviour
{
  
   public bool visibility = false;
    public GameObject panelObject;
    RectTransform rectTransform;
    public ScreenType mType = ScreenType.PreGame;
    public bool useTransition = false;
    public Vector3 offPosition;
    public Vector3 onPosition;
    public float transitionDelay = 0.5f;
    private void Awake()
    {
        rectTransform = panelObject.GetComponent<RectTransform>();

        EventManager.StartListening(MyEvents.EVENT_SHOW_PANEL, OnShowPanel);
        EventManager.StartListening(MyEvents.EVENT_POP_UP_PANEL, OnPopUp);
    }

    private void OnPopUp(EventObject arg0)
    {
        if ((ScreenType)arg0.objData == mType)
        {
            bool enable = arg0.boolObj;
            Debug.Log("Show " + mType);
            SetCanvasVisibility(enable);
        }
    }

    private void OnShowPanel(EventObject arg0)
    {
        if ((ScreenType)arg0.objData == mType)
        {
            SetCanvasVisibility(true);
        }
        else {
            SetCanvasVisibility(false);
        }
    }

    public void SetCanvasVisibility(bool isVisible)
    {
       // 
        visibility = isVisible;
        if (useTransition)
        {
            if (isVisible)
            {
                Show();
            }
            else {
                Hide();
            }
        }
        else {
            panelObject.SetActive(isVisible);
        }
    }

    private void Show()
    {
        panelObject.SetActive(true);
        rectTransform.DOMove(onPosition, transitionDelay);
    }
    private void Hide()
    {
        offPosition = new Vector3(0, -Screen.currentResolution.height, 0);
        rectTransform.DOLocalMove(offPosition, transitionDelay).OnComplete(
            () =>
            {
                panelObject.SetActive(false);
            }
        );
    }
    public bool GetVisibility() => visibility;

}

[System.Serializable]
public enum ScreenType
{
    PreGame, InGame, GameOver, Settings, TournamentResult
}