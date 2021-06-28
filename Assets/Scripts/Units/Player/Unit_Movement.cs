using Lex;
using System.Collections.Generic;
using UnityEngine;

public class Unit_Movement :MonoBehaviourLex
{
    public float moveSpeed = 8f;
    LexView pv;
    Vector3 lastVector = Vector3.up;
    public float aimAngle = 0f;
    public Vector3 oldPosition;

    BuffManager buffManager;
    Unit_Player unitPlayer;
    Controller controller;
    public Unit_AutoDrive autoDriver;
    [SerializeField] internal GameObject directionIndicator;
    Transform networkPosIndicator;
    public TransformSynchronisation netTransform;

    /*
      패킷은 모두 순서대로 온다
    상대가 계산한 위치 지점은 정확하다
    스무스는 정확하진않지만 비슷하다.
    standardping이 커질수록 받는오차가 적어짐
    해석오차는 update속도에 달려있음
     */
    public MapSpec mapSpec;
    Movement_Force moveForce;
    Rigidbody2D myRigidBody;

    private void Awake()
    {
        pv = GetComponent<LexView>();
        buffManager = GetComponent<BuffManager>();
        unitPlayer = GetComponent<Unit_Player>();
        moveForce = GetComponent<Movement_Force>();
        myRigidBody = GetComponent<Rigidbody2D>();
        controller = GetComponent<Controller>();
        netTransform = GetComponent<TransformSynchronisation>();

        networkPosIndicator = GameSession.GetInst().networkPos;
    }
    public void OnEnable()
    {
        InputHelper.SetAxisNames();
        autoDriver.StartBot(DetermineBotType());
    }
    BotType DetermineBotType() {
        if (GameSession.auto_drive_enabled && !controller.IsBot) return BotType.Hard;
        if (controller.IsBot)
        {
            if (GameSession.gameModeInfo.gameMode == GameMode.TeamCP)
            {
                return BotType.Easy;
            }
            else
            {
                return BotType.Normal;
            }
        }
        return BotType.None;
    }


    public void SetMapSpec(MapSpec spec) {
        mapSpec = spec;
    }


    private void Start()
    {
        if (controller.IsMine)
        {
            if (controller.IsLocal) {
                directionIndicator.SetActive(true);
            }
            netTransform.networkPos = transform.position;
        }
    }
    // Update is called once per frame
    public float lastChangeToggle = 0;
    private void Update()
    {
        myRigidBody.velocity = Vector3.zero;
        CheckAutoToggle();
        Move(Time.deltaTime);
        DequeuePositions();
        UpdateDirection();
    }
    public float GetMovementSpeed() => moveSpeed * buffManager.GetBuff(BuffType.MoveSpeed);
    void CheckAutoToggle() {
        if (controller.IsLocal)
        {
            if (Input.GetKeyDown(KeyCode.F) && GameSession.auto_drive_enabled)
            {
                if (Time.time > (lastChangeToggle + 0.25f))
                {
                    lastChangeToggle = Time.time;
                    GameSession.toggleAutoDriveByKeyInput();
                }
            }

        }
    }

    private void Move(float delta)
    {
        float moveSpeedFinal = GetMovementSpeed() * delta;
     
        if (controller.IsMine)
        {
            if (GameSession.IsAutoDriving() || controller.IsBot)
            {
                GiveEvaluatedInput(moveSpeedFinal);
            }
            else
            {
                MoveByInput(moveSpeedFinal);
            }
        }


    }

    private void GiveEvaluatedInput(float moveSpeedFinal)
    {

        Vector3 moveDirection = autoDriver.lastEvaluatedVector * moveSpeedFinal;
        moveDirection = moveForce.AdjustForce(moveDirection);
        Vector3 newPosition = ClampPosition(netTransform.networkPos, moveDirection);   
        EnqueuePosition(newPosition);
    }

    private void EnqueuePosition(Vector3 newPosition)
    {
        lastVector = newPosition - oldPosition;
        netTransform.EnqueueLocalPosition(newPosition, Quaternion.identity);
        if (!controller.IsBot)
        {
            networkPosIndicator.position = newPosition;
        }
    }

    private void MoveByInput(float moveSpeedFinal)
    {
        var deltaX = InputHelper.GetInputHorizontal() * moveSpeedFinal;
        var deltaY = InputHelper.GetInputVertical() * moveSpeedFinal;
        var moveDirection = new Vector3(deltaX, deltaY);
        moveDirection = moveForce.AdjustForce(moveDirection);

        Vector3 newPosition = ClampPosition(netTransform.networkPos, moveDirection);////new Vector3(networkPos.x +  deltaX, networkPos.y + deltaY, 0f));
        EnqueuePosition(newPosition);
       

    }


    Vector3 ClampPosition(Vector3 position, Vector3 direction) {
       // direction = CheckWallContact_Slide(position, direction);
        float newX = Mathf.Clamp(position.x + direction.x, mapSpec.xMin, mapSpec.xMax);
        float newY = Mathf.Clamp(position.y + direction.y, mapSpec.yMin, mapSpec.yMax);
        Vector3 newPos = new Vector3(newX, newY);
        return newPos;
    }

    Vector3 CheckContact_Slide(Vector3 position, Vector3 direction)
    {

        Collider2D[] collisions = Physics2D.OverlapCircleAll(
        position, 0.25f, LayerMask.GetMask(ConstantStrings.TAG_WALL));

        bool isContact = false;
        foreach (var c in collisions)
        {
            if (c.gameObject.tag == ConstantStrings.TAG_WALL) {
                isContact = true;
                break;
            }
        }
        if (isContact)
        {
         bool xMajor = Mathf.Abs(direction.x) > Mathf.Abs(direction.y);
            if (xMajor)
            {
                return new Vector3(direction.x * 0.5f, direction.y*0.25f);
            }
            else {
                return new Vector3(direction.x * 0.25f, direction.y * 0.5f);
            }
        }
        else {
            return direction;
        }
    }

    void DequeuePositions()
    {
        FlipBody(netTransform.xDelta);
        transform.position = ClampPosition(transform.position , Vector3.zero);
    }
    void FlipBody(float xDelta) {
        Vector3 localScale = unitPlayer.charBody.transform.localScale;
        float x = localScale.x;
        if (
            (xDelta < 0 && x > 0)
            ||
             (xDelta > 0 && x < 0)
            )
        {
            unitPlayer.charBody.transform.localScale = new Vector3(-localScale.x, localScale.y, localScale.z);
        }
    }

    float indicatorLength = 1f;
    public void UpdateDirection()
    {
        if (UI_AimOption.aimManual)
        {
            if (UI_GamePadOptions.useGamepad) { 
                var deltaX = Input.GetAxis(InputHelper.padXaxis) ;
                var deltaY = Input.GetAxis(InputHelper.padYaxis) ;
                if (Mathf.Abs(deltaX) <Mathf.Epsilon && Mathf.Abs(deltaY) < Mathf.Epsilon) return;
                Vector3 aimDir = new Vector3(deltaX, deltaY, 0f);
                aimAngle = GameSession.GetAngle(Vector3.zero, aimDir); //벡터 곱 비교
            }
            else {
                Vector3 target = InputHelper.GetTargetVector();
                aimAngle = GameSession.GetAngle(gameObject.transform.position, target); //벡터 곱 비교
            }
        }
        else {
            aimAngle = GameSession.GetAngle(Vector3.zero, lastVector); //벡터 곱 비교
        }
        float rad = aimAngle / 180 * Mathf.PI;
        float dX = Mathf.Cos(rad) * indicatorLength;
        float dY = Mathf.Sin(rad) * indicatorLength;
        directionIndicator.transform.localPosition = new Vector3(dX, dY);
        directionIndicator.transform.localRotation = Quaternion.Euler(0, 0, aimAngle);
    }
    public void TeleportPosition(Vector3 newPosition) {
        transform.position = newPosition;
        netTransform.networkPos = newPosition;
    }

    public float GetAim()
    {
        if (GameSession.IsAutoDriving() || controller.IsBot) {
            return autoDriver.EvaluateAim();
        }
        else if (Application.platform == RuntimePlatform.Android) {
            UpdateDirection();
            return aimAngle;
        }
        else {
            return aimAngle;
        }
    }
   /*
    * TODO MEMO
    * transform syunc 투사체와 통합함
    * public double networkExpectedTime;
    public Vector3 networkPos;
    public double lastSendTime;

    Vector3 oldPos;
    public override void OnSyncView(LexStream stream)
    {
        //통신을 보내는 
        if (IsWriting)
        {
            //if (networkExpectedTime != lastSendTime)
            if(oldPos != networkPos)
            {
                positionQueue.Enqueue(new TimeVector(networkExpectedTime, networkPos));
                stream.SendNext(networkPos);
                stream.SendNext(networkExpectedTime);
                lastSendTime = networkExpectedTime;
                oldPos = networkPos;
            }
        }

        //클론이 통신을 받는 
        else
        {
            //tcp
            //udp
            Vector3 position = stream.ReceiveNext<Vector3>();// parameters[0];
            double netTime = stream.ReceiveNext<double>(); //(double)parameters[1];
            TimeVector tv = new TimeVector(netTime, position);
            positionQueue.Enqueue(tv);
        }
    }*/

}


public class TimeVector
{
    public static bool useTimeSync = false;
    public double timestamp;
    public Vector3 position;
    public Quaternion quaternion;
    public TimeVector(double t, Vector3 v)
    {
        this.timestamp = t;
        this.position = v;
    }
    public TimeVector(double t, Vector3 v, Quaternion q)
    {
        this.timestamp = t;
        this.position = v;
        this.quaternion = q;
    }
    public bool IsExpired()
    {
        if (!useTimeSync) return true;
        return (timestamp <= LexNetwork.Time);
    }
    public override string ToString() {
        return timestamp + " : " + position;
    }

}