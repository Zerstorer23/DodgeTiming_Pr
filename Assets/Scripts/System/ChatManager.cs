using Lex;
using System;
using System.Collections;
using UnityEngine;

public class ChatManager : MonoBehaviourLexCallbacks
{
	private static ChatManager instance;

	/*	ScrollRect mainScroll;
		InputField mainInput;*/
	[SerializeField] UI_ChatBox mainChatBox;


	public static Chat_DC_Client dcClient;
	//MinigameManager minigameMachine;
    private void Awake()
    {
	//	minigameMachine = GetComponent<MinigameManager>();
		dcClient = GetComponent<Chat_DC_Client>();
		if (Application.platform == RuntimePlatform.Android)
		{

			dcClient.enabled = false;
		}
		else {

			dcClient.StartClient();
		}
		instance = this;
		EventManager.StartListening(MyEvents.EVENT_SHOW_PANEL, OnShowPanel);
    }
    private void OnDestroy()
    {
		EventManager.StopListening(MyEvents.EVENT_SHOW_PANEL, OnShowPanel);
	}

    private void OnShowPanel(EventObject obj)
    {
		ScreenType currentPanel = (ScreenType)obj.objData;
        switch (currentPanel)
        {
            case ScreenType.PreGame:
				mainChatBox.SetInputFieldVisibility(true);
                break;

            case ScreenType.InGame:
				mainChatBox.SetInputFieldVisibility(false);
				break;
        }
    }
    public override void OnChatReceived(string message)
    {
		AddLine(message);
    }
    public void AddLine(string lineString)
	{
		mainChatBox.AddLine(lineString);
	}


	public static void SendChatMessage(string message)
	{
			if (string.IsNullOrEmpty(message)) return;
			string msg = string.Format("<color=#ff00ff>[{0}]</color> {1}", LexNetwork.LocalPlayer.NickName, message);
			LexNetwork.SendChat(msg);
			dcClient.EnqueueAMessage(message);
		//	instance.DetectMinigame(message);
		
	}

	public static void SendLocalMessage(string message)
	{
		if (string.IsNullOrEmpty(message)) return;
		string msg = string.Format("<color=#C8C800>[{0}]</color>", message);
		instance.mainChatBox.AddLine(msg);
	}




	public static void SendNotificationMessage(string msg, string color = "#C8C800")
	{
		string fmsg = string.Format("<color={0}>{1}</color>", color, msg);
		LexNetwork.SendChat(fmsg);
	}
	public static void FocusField(bool doFocus)
	{
		instance.mainChatBox.FocusOnField(doFocus);
	}
	public static void SetInputFieldVisibility(bool enable)
	{
		instance.mainChatBox.SetInputFieldVisibility(enable);
	}

}
