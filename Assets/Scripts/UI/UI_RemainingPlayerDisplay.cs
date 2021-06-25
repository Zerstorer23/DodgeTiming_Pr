using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using static ConstantStrings;

public class UI_RemainingPlayerDisplay : MonoBehaviour
{
   [SerializeField] Text displayText, scoreText;
   [SerializeField] Image fillImage;
   [SerializeField] GameObject scoreBoard;
    [SerializeField] UI_CP_Icon[] cpIcons;
    Map_CapturePointManager cpManager;
    bool showRemainingPlayer = true;
    private static UI_RemainingPlayerDisplay instance;
    private void Awake()
    {
        instance = this;
    }
    public static void SetCPManager(Map_CapturePointManager cpman) {
        instance.cpManager = cpman;
        for (int i = 0; i < instance.cpIcons.Length; i++) {
            instance.cpIcons[i].gameObject.SetActive(i <= cpman.maxIndex);
        }
    }

    private void OnEnable()
    {
        showRemainingPlayer = GameSession.gameModeInfo.gameMode != GameMode.TeamCP;
        displayText.gameObject.SetActive(showRemainingPlayer);
        scoreBoard.SetActive(!showRemainingPlayer);
        if (showRemainingPlayer)
        {
      
            EventManager.StartListening(MyEvents.EVENT_PLAYER_SPAWNED, OnPlayerUpdate);
            EventManager.StartListening(MyEvents.EVENT_PLAYER_DIED, OnPlayerUpdate);
            StartCoroutine(WaitAFrame());
        }
        else {
            StartCoroutine(UpdateScore());
        }
    }
    private void OnDisable()
    {
        if (showRemainingPlayer)
        {
            EventManager.StopListening(MyEvents.EVENT_PLAYER_SPAWNED, OnPlayerUpdate);
            EventManager.StopListening(MyEvents.EVENT_PLAYER_DIED, OnPlayerUpdate);
        }
    }

    // Update is called once per frame
    void OnPlayerUpdate(EventObject eo)
    {
        if(gameObject.activeInHierarchy)
            StartCoroutine(WaitAFrame());
    }
    IEnumerator WaitAFrame() {
        yield return new WaitForFixedUpdate();
        int remain = GameFieldManager.GetRemainingPlayerNumber();
        displayText.text = "남은 플레이어 : " + remain;
    }

    IEnumerator UpdateScore() {
        while (gameObject.activeInHierarchy) {
            if (cpManager == null)
            {
                yield return new WaitForSeconds(1f);
                continue;
            }
            float point = cpManager.currentPoint;
            bool homeMajor = point > 0;
            if (point < 0) point *= -1;
         //   scoreText.text = "점수 : " + point.ToString("0");
          //  scoreText.color = GetColorByHex(homeMajor ? team_color[1] : team_color[2]);
            fillImage.color = GetColorByHex(homeMajor ? team_color[1] : team_color[2]);
            fillImage.fillAmount = ((float)point / cpManager.endThreshold);            
           yield return new WaitForSeconds(1f);
        }
    }

    public static void GetIcon(Map_CapturePoint cp) {
        instance.cpIcons[cp.captureIndex].RegisterCP(cp);
    }

}
