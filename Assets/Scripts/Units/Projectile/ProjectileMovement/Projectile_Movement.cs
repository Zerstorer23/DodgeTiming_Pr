using DG.Tweening;
using Lex;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static BulletManager;
using static ConstantStrings;

public partial class Projectile_Movement : MonoBehaviourLex
{


    TransformSynchronisation netTransform;
    bool transSync;

    Projectile_Homing projectileHoming;
    bool isHoming = false;

    public float eulerAngle;
    public float moveSpeed;

    //Rotation//

    public float rotateScale;
    public float angleClockBound;
    public float angleAntiClockBound;
    public float angleStack;
    public int goClockwise = 1;
    delegate void voidFunc();
    delegate Vector3 vectorFunc();
    delegate Quaternion quartFunc();
    voidFunc DoMove;
    vectorFunc GetPosition;
    quartFunc GetQuarternion;
    public MoveType moveType;
    public ReactionType reactionType = ReactionType.Bounce;
    public CharacterType characterUser = CharacterType.NONE;



    internal void SetAssociatedField(int fieldNo)
    {
        mapSpec = GameFieldManager.gameFields[fieldNo].mapSpec;
    }

    //Delay Move//
    float delay_enableAfter = 0f;
    [SerializeField] SpriteRenderer mySprite;
    MapSpec mapSpec;
    HealthPoint hp;

    private void Awake()
    {
        hp = GetComponent<HealthPoint>();
        netTransform = GetComponent<TransformSynchronisation>();
        transSync = netTransform != null;
        projectileHoming = GetComponent<Projectile_Homing>();
        isHoming = projectileHoming != null;
        transSync = netTransform != null;
        if (transSync)
        {
            GetPosition = GetNetworkPosition;
            GetQuarternion = GetNetworkQuarternion;
        }
        else
        {
            GetPosition = GetMyPosition;
            GetQuarternion = GetMyQuarternion;

        }
    }
    Vector3 GetMyPosition() => transform.localPosition;
    Quaternion GetMyQuarternion() => transform.rotation;
    Vector3 GetNetworkPosition() => netTransform.networkPos;
    Quaternion GetNetworkQuarternion() => netTransform.networkQuaternion;

    private void OnEnable()
    {
        distanceMoved = 0;
    }

    [LexRPC]
    public void SetBehaviour(int _moveType, int _reaction, float _direction)
    {
        moveType = (MoveType)_moveType;
        reactionType = (ReactionType)_reaction;
        switch (moveType)
        {
            case MoveType.Static:
                DoMove = DoMove_Static;
                eulerAngle = GetQuarternion().eulerAngles.z;
                break;
            case MoveType.Curves:
                DoMove = DoMove_Curve;
                eulerAngle = _direction;
                break;
            case MoveType.Straight:
                DoMove = DoMove_Straight;
                eulerAngle = _direction;// transform.rotation.eulerAngles.z;
                break;
            case MoveType.OrbitAround:
                DoMove = DoMove_Orbit;
                eulerAngle = _direction;
                break;
        }
    }


    [LexRPC]
    public void SetMoveInformation(float _speed,float _rotate,float rotateBound) {
        moveSpeed = _speed;
        rotateScale = _rotate;
        angleClockBound = rotateBound;
        angleAntiClockBound =  -rotateBound;
    }

    internal void TeleportPosition(Vector3 position)
    {
        if (transSync) {
            netTransform.networkPos = position;
        }
        transform.position = position;
    }

    [LexRPC]
    public void SetScale(float w, float h) {
        gameObject.transform.localScale = new Vector3(w, h, 1);
    }
    public bool tweenEase = false;
    [LexRPC]
    public void DoTweenScale(float delay, float maxScale)
    {
        if (tweenEase)
        {

            gameObject.transform.DOScale(new Vector3(maxScale, maxScale, 1), delay).SetEase(Ease.InQuint);
        }
        else
        {
            gameObject.transform.DOScale(new Vector3(maxScale, maxScale, 1), delay);

        }
    }


    private void OnDisable()
    {
        //TODO MEMO - 안하면 나가토 책상 걸림
        delay_enableAfter = 0f;
        mySprite.DORewind();
        gameObject.transform.DORewind();
        moveType = MoveType.Static;
        reactionType = ReactionType.None;
        DoMove = DoMove_Static;
    }

    private void Update()
    {
        if (DoMove == null) return;
        if (delay_enableAfter > 0)
        {
            delay_enableAfter -= Time.deltaTime;
        }
        else {
            DoMove();
        }

    }
    private void FixedUpdate()
    {
        if (mapSpec.IsOutOfBound(transform.position, 6f))
        {
            hp.Kill_Immediate();
        };
    }

    void ChangeTransform(Vector3 newDirection)
    {
        if (eulerAngle < 0) eulerAngle += 360;
        if (eulerAngle > 360) eulerAngle %= 360;
        if (isHoming) {
            eulerAngle = projectileHoming.AdjustDirection(GetPosition(),eulerAngle);
            newDirection = GetAngledVector(eulerAngle, newDirection.magnitude);
        }
        if (transSync)
        {
            netTransform.EnqueueLocalPosition(GetPosition() + newDirection, Quaternion.Euler(0, 0, eulerAngle));
            
        }
        else {
            transform.localPosition = GetPosition() + newDirection;
            transform.rotation = Quaternion.Euler(0, 0, eulerAngle);
        }
    }
    public Vector3 GetNextPosition() {
        Vector3 dir = Vector3.zero;
        switch (moveType)
        {
            case MoveType.Curves:
                float amount = rotateScale * Time.deltaTime * goClockwise;
                dir = GetAngledVector(eulerAngle + amount, moveSpeed * Time.deltaTime);
                break;
            case MoveType.Straight:
                dir = GetAngledVector(eulerAngle, moveSpeed * Time.deltaTime); 
                break;
            case MoveType.OrbitAround:
                if (distanceMoved < orbitLength)
                {
                    dir = GetAngledVector(eulerAngle, moveSpeed * Time.deltaTime * 2);
                }
                else
                {
                    dir = GetAngledVector(eulerAngle + orbitSpeed * Time.deltaTime, orbitLength);
                }
                break;
        }
        return GetPosition() + dir;
    }

}

