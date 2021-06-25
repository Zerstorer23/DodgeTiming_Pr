using Lex;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class InputHelper : MonoBehaviour
{
/*#if UNITY_ANDROID && !UNITY_EDITOR
    public static AndroidJavaClass AndroidPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
    public static AndroidJavaObject AndroidcurrentActivity = AndroidPlayer.GetStatic<AndroidJavaObject>("currentActivity");
    public static AndroidJavaObject AndroidVibrator = AndroidcurrentActivity.Call<AndroidJavaObject>("getSystemService", "vibrator");
#endif*/

    public delegate float FloatFunction();
    public static FloatFunction GetInputHorizontal;
    public static FloatFunction GetInputVertical;

    public delegate Vector3 Vector3Function();
    public static Vector3Function GetTargetVector;

    public delegate bool skillFireFunc();
    public static skillFireFunc skillKeyFired;


    public static string padXaxis = "RHorizontal";
    public static string padYaxis = "RVertical";

    static string MoveX = "Horizontal";
    static string MoveY = "Vertical";

    public bool test_ForceWASD = false;
    private void Awake()
    {
        SetInputFunctions();
        test_ForceWASD = PlayerPrefs.GetInt(ConstantStrings.PREFS_FORCE_WASD, 0) != 0;
        wasdToggle.isOn = test_ForceWASD;
    }
    void SetInputFunctions()
    {
        if (Application.platform == RuntimePlatform.Android)
        {
            GetInputHorizontal = Control_MobileStick.GetInputHorizontal;
            GetInputVertical = Control_MobileStick.GetInputVertical;
            GetTargetVector = GetTouchPosition;
            skillKeyFired = FireButtonDown_Mobile;
        }
        else
        {
            GetInputHorizontal = GetKeyInputHorizontal;
            GetInputVertical = GetKeyInputVertical;
            GetTargetVector = GetMousePosition;// GetMousePosition;
            skillKeyFired = FireButtonDown_PC;
        }
    }
    float GetKeyInputHorizontal()
    {
        if (test_ForceWASD) {
            return GetAD();
        }
        return Input.GetAxis(MoveX);
    }

    float GetKeyInputVertical()
    {
        if (test_ForceWASD)
        {
            return GetWS();
        }
        return Input.GetAxis(MoveY);
    }

    double fullTime = 0.2d;
    public double aDown = 0d;
    public double dDown = 0d;
    public double wDown = 0d;
    public double sDown = 0d;
    float GetAD() {
        if (Input.GetKeyDown(KeyCode.A))
        {
            aDown = LexNetwork.NetTime;
        }
        else if (Input.GetKeyDown(KeyCode.D))
        {
            dDown = LexNetwork.NetTime;
        }

        if (Input.GetKey(KeyCode.A)) {
            double ratio = ((LexNetwork.NetTime - aDown) / fullTime);
            return -(float)Math.Min(ratio, 1d);
        }
        else if(Input.GetKey(KeyCode.D))
        {
            double ratio = ((LexNetwork.NetTime - dDown) / fullTime);
            return (float)Math.Min(ratio, 1d);
        }else {
            return 0f;
        }
    }
    float GetWS() {
        if (Input.GetKeyDown(KeyCode.W))
        {
            wDown = LexNetwork.NetTime;
        }
        else if (Input.GetKeyDown(KeyCode.S))
        {
            sDown = LexNetwork.NetTime;
        }
        if (Input.GetKey(KeyCode.W))
        {
            double ratio = ((LexNetwork.NetTime - wDown) / fullTime);
            return (float)Math.Min(ratio, 1d);
        }
        else if (Input.GetKey(KeyCode.S))
        {
            double ratio = ((LexNetwork.NetTime - sDown) / fullTime);
            return -(float)Math.Min(ratio, 1d);
        }
        else
        {
            return 0f;
        }
    }

    Vector3 GetMousePosition()
    {
        return Camera.main.ScreenToWorldPoint(Input.mousePosition);
    }
    Vector3 GetTouchPosition()
    {
        return Camera.main.ScreenToWorldPoint(UI_TouchPanel.touchVector);
    }

    public static void SetAxisNames()
    {
        UI_GamePadOptions.QueryPadInfo();
        if (UI_GamePadOptions.useGamepad)
        {
            MoveX = "LHorizontal";
            MoveY = "LVertical";
        }
        else {
            MoveX = "Horizontal";
            MoveY = "Vertical";
        }
        switch (UI_GamePadOptions.padType)
        {
            case PadType.PS4:
                padXaxis = "RHorizontal";
                padYaxis = "RVertical";
                break;
            case PadType.XBOX:
                padXaxis = "RHorizontalXbox";
                padYaxis = "RVerticalXbox";
                break;
        }
    }
    private bool FireButtonDown_PC()
    {
        return //Input.GetAxis("Fire1") == 1f //TODO GetKeyDown
            Input.GetKeyDown(KeyCode.Space)
            || Input.GetKeyDown(KeyCode.Mouse0)
            || Input.GetKeyDown(KeyCode.Joystick1Button5)
            || Input.GetKeyDown(KeyCode.Joystick1Button7);
    }
    private bool FireButtonDown_Mobile()
    {
        return UI_TouchPanel.isTouching;
    }
    [SerializeField] Toggle wasdToggle;
    public void OnClick_ForceKeyboard(Toggle toggle ) {
        test_ForceWASD = toggle.isOn;
        Debug.Log("Force move " + test_ForceWASD);
        PlayerPrefs.SetInt(ConstantStrings.PREFS_FORCE_WASD, test_ForceWASD?1:0);
        PlayerPrefs.Save();

    }

    private void Update()
    {
        //Debug.Log("W: "+Input.Get)
    }
    /*    public static void Vibrate(long mills = 1000)
        {
            #if UNITY_ANDROID && !UNITY_EDITOR
                    if (Application.platform == RuntimePlatform.Android) {
                        AndroidVibrator.Call("vibrate", mills);
                }
            #endif
        }*/
}
