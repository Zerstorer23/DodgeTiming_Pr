using Lex;
using System.Collections;
using UnityEngine;

public class BulletSpawner : MonoBehaviourLex
{
   [SerializeField] int fieldNumber = 0;
    IEnumerator deleteRoutine;
    LexView pv;
    public Directions shootDir;
    [SerializeField] Transform spawnPos;
    public float rotationSpeed;
    public float angleClockBound;
    public float angleAntiClockBound;
    public float angleStack;
    public int goClockwise = 1;
    public float delay = 0.25f;
    float delayStack;

    //Bulllet
    public SpawnDirection spawnDir;
    public MoveType moveType;
    public ReactionType reactionType;
    private float blockWidth = 1f;
    public float moveSpeed;
    public float rotateSpeed;
    private bool isDead = false;
    public bool debug = false;
    [SerializeField] float debugDuration = 50f; //TODO REFACTOR THESES
    [SerializeField] float debugAngleRange = 50f;
    [SerializeField] float debugMovespeed = 20f;
    [SerializeField] float debugRotateSpeed = 90f;
    private void Awake()
    {

        pv = GetComponent<LexView>();
        SetDebug();
    }

    void SetDebug()
    {
        if (debug)
        {
            SetBehaviour(0, debugAngleRange, 0.25f, debugDuration);
            SetProjectile(2, 1,(int) reactionType, 1f, debugMovespeed, debugRotateSpeed);
        }
    }

    private void OnEnable()
    {
        EventManager.StartListening(MyEvents.EVENT_GAME_FINISHED, OnGameEnd);
        EventManager.StartListening(MyEvents.EVENT_FIELD_FINISHED, OnGameEnd);
        isDead = false;
        Debug.LogWarning(gameObject.name + " v " + lexView.ViewID);
        fieldNumber = (int)pv.InstantiationData[0];
        transform.SetParent(GameSession.GetBulletHome());
    }
    private void OnDisable()
    {
        EventManager.StopListening(MyEvents.EVENT_GAME_FINISHED, OnGameEnd);
        EventManager.StopListening(MyEvents.EVENT_FIELD_FINISHED, OnGameEnd);
    }
    private void OnGameEnd(EventObject arg0)
    {
        DoDeath();
    }

 

    [LexRPC]
    public void SetBehaviour(int shootDirection, float angleRange , float _delay, float duration)
    {
        SetAngles((Directions)shootDirection, angleRange);
        rotationSpeed = 25f;
        delay = _delay;

        DoTimer(duration);
    }
   
    private void DoTimer(float duration)
    {
        if (deleteRoutine != null)
            StopCoroutine(deleteRoutine);
        deleteRoutine = WaitAndDestroy(duration);
        StartCoroutine(deleteRoutine);
    }
    IEnumerator WaitAndDestroy(float duration)
    {
        yield return new WaitForSeconds(duration);
        DoDeath();
    }
    void DoDeath() {
        if (!pv.IsMine || isDead) return;
        isDead = true;
        EventManager.TriggerEvent(MyEvents.EVENT_SPAWNER_EXPIRE, new EventObject() { intObj = fieldNumber });
        LexNetwork.Destroy(pv);
    }
    void SetAngles(Directions shootDirection, float angleRange)
    {
        shootDir = shootDirection;
        float angle = BulletManager.DirectionsToEuler(shootDir);

        transform.eulerAngles = new Vector3(0, 0, angle);
        angleClockBound = angleRange;
        angleAntiClockBound = -angleRange;
    }

    [LexRPC]
    public void SetProjectile(int _spawnDir, int _moveType, int _reaction,float width, float _pMovespeed,float _pRotateSpeed)
    {
        spawnDir = (SpawnDirection)_spawnDir;
        moveType = (MoveType)_moveType;
        reactionType = (ReactionType)_reaction;
        blockWidth = width;
        moveSpeed = _pMovespeed;
        rotateSpeed = _pRotateSpeed;


    }

    private void Update()
    {
        if (!LexNetwork.IsMasterClient) return;
        SetDebug();
        DoRotation();
        CheckFire();
    }

    private void CheckFire()
    {
        delayStack += Time.deltaTime;
        if (delayStack >= delay) {
            delayStack -= delay;
            //
            GameObject obj = LexNetwork.InstantiateRoomObject(ConstantStrings.PREFAB_BULLET_1, spawnPos.position, transform.rotation, 0,
                new object[] { fieldNumber, "-1", false }
                );
            LexView pv = obj.GetComponent<LexView>();
            pv.RPC("SetMoveInformation",   moveSpeed, rotateSpeed, angleClockBound);
            pv.RPC("SetScale",  blockWidth, blockWidth);
            pv.RPC("SetBehaviour",   (int)moveType,(int)reactionType, transform.eulerAngles.z);
        }
    }

    void DoRotation() {
        if (angleStack >= angleClockBound)
        {
            goClockwise = -1;
        }
        else if (angleStack <= -angleClockBound)
        {
            goClockwise = 1;
        }
        float amount = rotationSpeed * Time.deltaTime * goClockwise;
        angleStack += amount;
        float newAngle = transform.eulerAngles.z + amount;
      //  Debug.Log(transform.eulerAngles.z+" => newAngle" + newAngle);

        transform.eulerAngles = new Vector3(0, 0, newAngle);
    }

}

