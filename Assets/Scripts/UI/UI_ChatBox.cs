using Lex;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class UI_ChatBox : MonoBehaviour
{
	public InputField inputField;
	public Text outputText;
	public ScrollRect scrollRect;
	public UnityEngine.GameObject inputBox;
	[SerializeField] RectTransform contentTrans;

	List<string> chatQueue = new List<string>();
    private void Awake()
    {
		EventManager.StartListening(MyEvents.EVENT_SHOW_PANEL, ScrollToBottom);
    }
    private void OnDestroy()
	{
		EventManager.StopListening(MyEvents.EVENT_SHOW_PANEL, ScrollToBottom);

	}
    public void SetInputFieldVisibility(bool enable)
	{
		inputBox.SetActive(enable);
	}

	public static bool isSelected = false;
	int emptyEnter = 1;
	void Update()
	{

		if (Input.GetKeyUp(KeyCode.Return))
		{
			emptyEnter++;
			if (emptyEnter >= 2 && !isSelected)
			{
				FocusOnField(true);
			}
		}
		else
		if (Input.GetKeyUp(KeyCode.Escape))
		{
			FocusOnField(false);
		}

	}
	public void Input_OnValueChange()
	{
		isSelected = true;
	}
	public void Input_OnEndEdit()
	{

		if (string.IsNullOrEmpty(inputField.text))
		{
			FocusOnField(false);
		}
		else
		{
			string text = inputField.text;
			if (text[0] == '/')
			{
				ParseUserCommand(text);
			}else if (text[0] == '!') {
				ParseCommand(text);
			}
			else {
				ChatManager.SendChatMessage(inputField.text);
			}
			if (Application.platform != RuntimePlatform.Android) {
				FocusOnField(true);
			}
		}
		inputField.text = "";

	}
	void ParseUserCommand(string text) {
		if (text.Contains("음소거"))
		{
			outputText.enabled = !outputText.enabled;
		}
	}
	private void ParseCommand(string text) {

		if (text.Contains("자동"))
		{
			GameSession.auto_drive_enabled = !GameSession.auto_drive_enabled;
		}
		else if (text.Contains("탈취"))
		{
			GameSession.instance.lexView.RPC("ResignMaster", LexNetwork.LocalPlayer.actorID);
		}
		else if (text.Contains("입항"))
		{
		//	LexNetwork.ReconnectEveryone();
		}
		else if (text.Contains("점검"))
		{
		//	LexNetwork.KickEveryoneElse();
		}
	}
	public void AddLine(string lineString)
	{
		string newMsg = lineString + "\r\n";
		try
		{

			CheckBufferSize();
			chatQueue.Add(newMsg);
		}
		catch (Exception e) {
			Debug.LogWarning(e.StackTrace);
		}
		outputText.text += newMsg;
		if (gameObject.activeInHierarchy) ScrollToBottom();
	}

	int buffer = 64;
	void CheckBufferSize() {
		if (chatQueue.Count >= buffer) {
			int limit = buffer / 2;
			//remove 32
			while (chatQueue.Count >= limit) {
				chatQueue.RemoveAt(0);
			}
			string newPanel = "";
			foreach (string s in chatQueue)
			{
				newPanel += s;
			}
			outputText.text = newPanel;
		}
	}

	public void FocusOnField(bool enable)
	{
	
		if (enable)
		{
			inputField.ActivateInputField();
			inputField.Select();
			inputField.placeholder.GetComponent<Text>().text = "채팅을 입력";
		}
		else
		{
			inputField.DeactivateInputField();
			if (!EventSystem.current.alreadySelecting) EventSystem.current.SetSelectedGameObject(null);
			inputField.placeholder.GetComponent<Text>().text = "Enter로 채팅시작";
			//EventSystem.current.SetSelectedGameObject(null);
		}
		isSelected = enable;
		emptyEnter = 0;
	}

	IEnumerator scrollRoutine;

    public void ScrollToBottom(EventObject eo = null)
	{
		if (scrollRoutine != null)
		{
			StopCoroutine(scrollRoutine);
		}
		scrollRoutine = WaitAndScroll();
		StartCoroutine(scrollRoutine);
	}
	IEnumerator WaitAndScroll()
	{
		yield return new WaitForFixedUpdate();
		yield return new WaitForFixedUpdate();
		scrollRect.normalizedPosition = new Vector2(0, 0);

	}
}
