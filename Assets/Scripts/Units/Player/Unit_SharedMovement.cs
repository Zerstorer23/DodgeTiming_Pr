using Lex;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class Unit_SharedMovement : MonoBehaviourLex
{
    public float moveSpeed = 10f;
    LexView pv;
    int fieldNo = -1;
    MapSpec mapSpec;
    float fireSpeed = 1.2f;
    [SerializeField]  Transform gunPosition;
    [SerializeField] Image dirFill;
    [SerializeField] Text dirText;
    Dictionary<string, int> controllers = new Dictionary<string, int>();
    TransformSynchronisation transSync;
    Controller controller;
    float yOffset = -1.5f;



    private void Awake()
    {
        pv = GetComponent<LexView>();
        transSync = GetComponent<TransformSynchronisation>();
        controller = GetComponent<Controller>();
        controller.SetControllerInfo(lexView.Owner);
    }
    private void OnEnable()
    {
        fieldNo = (int)pv.InstantiationData[0];
        GameField myField = GameFieldManager.gameFields[fieldNo];
        myField.playerSpawner.desolator = this;
        mapSpec = myField.mapSpec;
        controllers.Clear();
        Debug.Log("Controller size " + controllers.Count);
        transform.position = new Vector3(0, transform.position.y);
        EventManager.StartListening(MyEvents.EVENT_FIELD_FINISHED, OnFieldFinish);

        Vector3 newPosition = new Vector3(0, mapSpec.yMin + yOffset, 1);
        transform.position = newPosition;
        StartCoroutine(WaitAndFire());
    }


    private void OnDisable()
    {
        controllers.Clear();
        EventManager.StopListening(MyEvents.EVENT_FIELD_FINISHED, OnFieldFinish);
    }
    private void OnFieldFinish(EventObject arg0)
    {
        if (fieldNo != arg0.intObj) return;
        if (pv.IsMine) {
            LexNetwork.Destroy(pv);
        }
    }


    int prevDir = 0;
    // Update is called once per frame
    private void Update()
    {
        if (controllers.ContainsKey(LexNetwork.LocalPlayer.uid)) {
            var deltaX = InputHelper.GetInputHorizontal();// Input.GetAxis("Horizontal");
            int dir = deltaX == 0f ? 0 : (deltaX < 0f)? -1 : 1;
            if (prevDir != dir) {
                prevDir = dir;
                pv.RPC("GiveDirection",  LexNetwork.LocalPlayer.uid, dir);
            }  
        }

        Move(Time.deltaTime);
        
    }

    IEnumerator WaitAndFire() {
        while (gameObject.activeInHierarchy)
        {
            if (pv.IsMine)
            {
                if (controllers.Count > 0)
                {
                    Fire();
                }
            }
            yield return new WaitForSeconds(fireSpeed);
        }
    }
    void Fire() {
        GameObject obj = LexNetwork.InstantiateRoomObject(
            ConstantStrings.PREFAB_BULLET_DESOLATION, gunPosition.position,
            Quaternion.Euler(0,0,90)
            , 0, 
            new object[] { fieldNo, "-1", false }
            );
        LexView pv = obj.GetComponent<LexView>();
        pv.RPC("SetBehaviour",   (int)MoveType.Straight, (int)ReactionType.None, 90f);
    }

    [LexRPC]
    public void GiveDirection(string id, int dir) {
        controllers[id] = dir;
    }
    public void AddController(string userid)
    {
        if (!gameObject.activeInHierarchy) return;
        if (controllers.ContainsKey(userid))
        {
            Debug.LogWarning("Duplicated player in desolator " + userid + " / " + controllers.Count);
            return;
        }
        Debug.Log("Controller size " + controllers.Count);
        controllers.Add(userid, 0);
    }


    private void Move(float delta)
    {
        float sum = controllers.Sum(x => x.Value);
        int countActive = controllers.Where(x => x.Value != 0f).Count();
        /*      foreach (var entry in controllers.Values) {
                  sum += ((float)entry )/ controllers.Count;
              }*/
        if (countActive > 0)
        {
            float average = sum / countActive;
            dirFill.fillAmount = (average + 1f) * 0.5f;
            dirText.text = average.ToString("0.0");

            if (pv.IsMine)
            {
                float deltaX = average * moveSpeed * delta;
                float newX = Mathf.Clamp(transSync.networkPos.x + deltaX, mapSpec.xMin, mapSpec.xMax);//TODO
                Vector3 newPosition = new Vector3(newX, mapSpec.yMin + yOffset, 1);
                transSync.EnqueueLocalPosition(newPosition, Quaternion.identity);
            }
        }
        else {

            if (pv.IsMine)
            {
                Vector3 newPosition = new Vector3(transSync.networkPos.x, mapSpec.yMin + yOffset, 1);
                transSync.EnqueueLocalPosition(newPosition, Quaternion.identity);
            }
        }
        
    }
}

