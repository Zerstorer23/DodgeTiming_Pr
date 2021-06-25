using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;
using System;
using DG.Tweening;
using Lex;

public class MainCamera : MonoBehaviour
{
   public CinemachineStateDrivenCamera stateCam;
    [SerializeField] CinemachineVirtualCamera mainVcam;
    [SerializeField] CinemachineVirtualCamera fieldCam;
    [SerializeField] Transform fieldTransform;
    [SerializeField] Transform followTransform;
    CinemachineBrain cineBrain;
    private static MainCamera prMainCam;
    // Start is called before the first frame update
    public static MainCamera instance
    {
        get
        {
            if (!prMainCam)
            {
                prMainCam = FindObjectOfType<MainCamera>();
                if (!prMainCam)
                {
                    //  prEvManager = Instantiate(EventManager) as EventManager;
                    Debug.LogWarning("There needs to be one active MainCamera script on a GameObject in your scene.");
                }
            }

            return prMainCam;
        }
    }
    private void Awake()
    {
        cineBrain = GetComponent<CinemachineBrain>();
        EventManager.StartListening(MyEvents.EVENT_PLAYER_DIED, OnPlayerDied);
    }

    private void OnPlayerDied(EventObject obj)
    {
        GameObject player = GameFieldManager.GetNextActivePlayer();
        if (player == null)
        {
            fieldCam.Follow = fieldTransform;
        }
        else {
            fieldCam.Follow = player.transform;// instance.fieldTransform;
        }
    }
    bool flag = false;
    private void Update()
    {
       // ChangeCameraView();
        ChangeSpectator();
        CheckPositionZero();
    }
    void CheckPositionZero() {
        if (instance.gameObject.transform.localPosition.x != 0
            && instance.gameObject.transform.localPosition.y != 0
            ) {
            instance.gameObject.transform.localPosition = new Vector3(0, 0, -10);
            instance.gameObject.transform.localRotation = Quaternion.identity;
        }
    }

    private void ChangeCameraView()
    {
        if (Input.GetKeyDown(KeyCode.P) )
        {
            Debug.Log("Focus " + flag);
            flag = !flag;
            FocusOnField(flag);
        }
    }

    private void ChangeSpectator()
    {
        if (!instance.isFocusedField || UI_ChatBox.isSelected) return;
        if (!GameSession.gameStarted) return;
        if (Input.GetKeyDown(KeyCode.Space) 
            || Input.GetKeyDown(KeyCode.Joystick1Button5) 
            || Input.GetKeyDown(KeyCode.Joystick1Button7)
            || (Application.platform == RuntimePlatform.Android && UI_TouchPanel.isTouching)
            )
        {
            FocusOnAlivePlayer();
            UI_TouchPanel.isTouching = false;
        }
    }
   public void FocusOnAlivePlayer() {
        GameObject p = GameFieldManager.GetNextActivePlayer();
        if (p != null)
        {
            instance.fieldCam.Follow = p.transform;// instance.fieldTransform;
        }
    }



    private void OnDestroy()
    {
        EventManager.StopListening(MyEvents.EVENT_PLAYER_DIED, OnPlayerDied);
    }

    Transform playerTrans;
    public static void SetFollow(Transform trans) {       
        instance.mainVcam.Follow = trans;
        instance.playerTrans = trans;
    }
    /*
         public static void SetFollow(Transform trans) {
        //instance.mainVcam.Follow = trans;
        instance.mainVcam.transform.SetParent(trans,false);
        instance.mainVcam.transform.localPosition = new Vector3(0, 0, -10);
        instance.mainVcam.transform.localRotation = Quaternion.identity;

}
*/
    public bool isFocusedField = true;

    public static void FocusOnField(bool enable) {
        if (instance != null)
        {
            if (instance.shakeRoutine != null)
            {
                instance.StopCoroutine(instance.shakeRoutine);
                instance.Noise(0, 0);
            }
        }

        instance.GetComponent<Animator>().SetBool("ViewField", enable);
        instance.isFocusedField = enable;
        if (!enable)
        {
            instance.mainVcam.transform.localPosition = new Vector3(0, 0);
            instance.mainVcam.transform.localRotation = Quaternion.identity;
        }
        else {
            instance.FocusOnAlivePlayer();
        }
        instance.gameObject.transform.localPosition = new Vector3(0, 0,-10);
        instance.gameObject.transform.localRotation = Quaternion.identity;
    }
    /*
         public static void FocusOnField(bool enable) {
        if (instance != null)
        {
            if (instance.shakeRoutine != null)
            {
                instance.StopCoroutine(instance.shakeRoutine);
                instance.Noise(0, 0);
            }
        }

        instance.GetComponent<Animator>().SetBool("ViewField", enable);
        instance.isFocusedField = enable;
        if (enable) {
            instance.mainVcam.transform.SetParent(instance.stateCam.transform);
            instance.mainVcam.transform.localPosition = new Vector3(0, 0, -10);
            instance.mainVcam.transform.localRotation = Quaternion.identity;
        }
        instance.gameObject.transform.localPosition = new Vector3(0, 0, -10);
        instance.gameObject.transform.localRotation = Quaternion.identity;

}
*/

    IEnumerator shakeRoutine = null;

    public void DoShake(float intense = 7f, float time = 0.5f)
    {
        if (shakeRoutine != null) {
            StopCoroutine(shakeRoutine);
        }
        shakeRoutine = ProcessShake(intense, time);
        StartCoroutine(shakeRoutine);
    }

    private IEnumerator ProcessShake(float shakeIntensity, float shakeTiming)
    {
        Noise(1, shakeIntensity);
        yield return new WaitForSeconds(shakeTiming);
        Noise(0, 0);
    }

    private void Noise(float amplitudeGain, float frequencyGain)
    {
        var noise = mainVcam.GetCinemachineComponent<CinemachineBasicMultiChannelPerlin>();
        noise.m_AmplitudeGain = amplitudeGain;
        noise.m_FrequencyGain = frequencyGain;

    }
    public void DoRotation(float time = 0.5f)
    {
        if (rotateRoutine != null)
        {
            StopCoroutine(rotateRoutine);
            mainVcam.m_Lens.Dutch = 0f;
        }
        rotateRoutine = ProcessRotate(time);
        StartCoroutine(rotateRoutine);
    }
    IEnumerator rotateRoutine = null;
    private IEnumerator ProcessRotate(float rotateOver)
    {
        double start = LexNetwork.NetTime;
        double end = start + rotateOver;
        while(LexNetwork.NetTime < end){
            double elapsed = LexNetwork.NetTime - start;
            float angle = 360f * ((float)elapsed / rotateOver) - 180f;
            mainVcam.m_Lens.Dutch = angle;
            yield return new WaitForFixedUpdate();
        }
        mainVcam.m_Lens.Dutch = 0f;

    }

}
