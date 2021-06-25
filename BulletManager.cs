using Photon.Pun;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;
using static GameFieldManager;
using ExitGames.Client.Photon;
using Hashtable = ExitGames.Client.Photon.Hashtable;

public class BulletManager : MonoBehaviour
{
    public float startSpawnAfter = 2f;
    double startTime;

    public int activeMax = 30;
    public int currentSpawned = 0;
    [Header("Spawner setting")]
    public float minDelay, maxDelay;
    public float minDuration, maxDuration;

    [Header("Projectile settings")]
    public float maxProjSpeed = 15f;
    public float maxProjRotateSpeed = 120f;
    [SerializeField] float minProjSize = 0.5f, maxProjSize = 1.5f;
    [Header("Box settings")]
    public float maxWidth = 10f;
    public float spawnDelay = 3f;

    int[] mapDifficulties = { 0, 12, 12, 24, 48 };
    public int modPerPerson = 5;
    public float modPerStep = 0.5f;

    MapDifficulty currentDifficult;

    double lastIncrementTime;

    [SerializeField]  GameField gameField;
    private void OnSuddenDeath(EventObject obj)
    {
       // activeMax += 4;
    }
    private void OnEnable()
    {

        EventManager.StartListening(MyEvents.EVENT_SPAWNER_EXPIRE, OnSpawnerExpired);
     //   EventManager.StartListening(MyEvents.EVENT_SPAWNER_SPAWNED, OnSpawnerSpawned);
        EventManager.StartListening(MyEvents.EVENT_REQUEST_SUDDEN_DEATH, OnSuddenDeath);
    }
    private void OnDisable()
    {

        EventManager.StopListening(MyEvents.EVENT_SPAWNER_EXPIRE, OnSpawnerExpired);
    //    EventManager.StopListening(MyEvents.EVENT_SPAWNER_SPAWNED, OnSpawnerSpawned);
        EventManager.StopListening(MyEvents.EVENT_REQUEST_SUDDEN_DEATH, OnSuddenDeath);
    }


    public void StartEngine(MapDifficulty mapDifficulty)
    {
        currentDifficult = mapDifficulty;
        float baseNum = mapDifficulties[(int)currentDifficult];
        float modifier = 1+ gameField.playerSpawner.playersOnMap.Count / modPerPerson * modPerStep;
        activeMax =(int)( baseNum * modifier);
        CheckYasumiMutex();
        startTime = PhotonNetwork.Time;
    }
    // Update is called once per frame
    void Update()
    {
        CheckIncrementByDifficulty();
        if (!PhotonNetwork.IsMasterClient || !GameSession.gameStarted) return;
        if (PhotonNetwork.Time <= startTime + startSpawnAfter) return;
        CheckSpawnerSpawns();

    }

    private void CheckIncrementByDifficulty()
    {
        if (GameSession.gameModeInfo.gameMode != GameMode.PVE) return;
        if (PhotonNetwork.Time <= instance.incrementEverySeconds + lastIncrementTime) return;
        activeMax++;
        lastIncrementTime = PhotonNetwork.Time;
    }

    private void CheckSpawnerSpawns()
    {
        if (currentSpawned >= activeMax ) return;
        while (currentSpawned < activeMax)
        {
            SpawnDirection spawnDir = GetRandomSpawnDir();
            MoveType moveType = GetRandomMoveType(spawnDir);
            ReactionType reaction = GetRandomReactionType();
            InstantiateSpanwer(spawnDir, moveType, reaction);
            currentSpawned++;
        }
    }

    private ReactionType GetRandomReactionType()
    {
        return (ReactionType)Random.Range(0, (int)ReactionType.None);
    }

    private void OnSpawnerExpired(EventObject eo)
    {
        if (eo.intObj == gameField.fieldNo) {

            currentSpawned--;
        }
    }
    private void OnSpawnerSpawned(EventObject arg0)
    {
        currentSpawned++;
    }
    private void InstantiateSpanwer(SpawnDirection spawnDir, MoveType moveType, ReactionType reaction)
    {
        if (!GameSession.gameStarted) return;
        //Projectile
        switch ((MoveType)moveType)
        {
            case MoveType.Static:
                InstantiateBox();
                break;
            case MoveType.Curves:
            case MoveType.Straight:
                Vector3 randPos = GetRandomBoundaryPos();
                UnityEngine.GameObject spawner = PhotonNetwork.InstantiateRoomObject("Prefabs/Units/BulletSpawner", randPos, Quaternion.identity,0, new object[] { gameField.fieldNo});
                SetProjectileInformation(spawner, spawnDir, moveType, reaction);
                SetProjectileBehaviour(spawner, randPos);
                break;
        }



    }

    public void CheckYasumiMutex() {
        if (GameSession.gameModeInfo.isCoop) return;
        var players = GetPlayersInField(gameField.fieldNo);
        int yasumiCount = 0;
        foreach (var p in players) {
            CharacterType character = (CharacterType)ConnectedPlayerManager.GetPlayerProperty(p,"CHARACTER", CharacterType.NONE);
            if (character == CharacterType.YASUMI) {
                yasumiCount++;
            }
        }
        if (yasumiCount > players.Length / 2) {
            if (PhotonNetwork.IsMasterClient) {
                ChatManager.SendNotificationMessage("야스미가 너무 많습니다");
                activeMax = 96;
            }
        }
    }

    private void InstantiateBox()
    {
        float randW = Random.Range(1f, maxWidth);
        Vector3 randPos = gameField.GetRandomPosition();

        UnityEngine.GameObject box = PhotonNetwork.InstantiateRoomObject("Prefabs/Units/BoxObstacle", randPos, Quaternion.identity,0, new object[] { gameField.fieldNo });
        box.GetComponent<PhotonView>().RPC("SetInformation", RpcTarget.AllBuffered, randW,spawnDelay);
       
    }
    void SetProjectileInformation(GameObject spawner, SpawnDirection spawnDir, MoveType moveType, ReactionType reaction)
    {
        float moveSpeed = Random.Range(5f, maxProjSpeed);
        float rotateSpeed = Random.Range(5f, maxProjRotateSpeed);
        float blockSize = Random.Range(minProjSize, maxProjSize);
        spawner.GetComponent<PhotonView>().RPC("SetProjectile", RpcTarget.AllBuffered, (int)spawnDir, (int)moveType, (int)reaction,blockSize, moveSpeed, rotateSpeed);
    }
    void SetProjectileBehaviour(UnityEngine.GameObject spawner, Vector3 randPos)
    {
        //Behaviour
        int headingDirection = (int)GetHeadingAngle(randPos);
        float angleRange;//= Random.Range(minProjRotateScale, masProjRotateSpeed); 
        if (headingDirection <= 3)
        {
            angleRange = 90f;// Random.Range(45f, 120f);
        }
        else
        {
            angleRange = 45f;// Random.Range(0f, 45f);
        }

        
        float delay = Random.Range(minDelay, maxDelay);
        float duration = Random.Range(minDuration, maxDuration);
        spawner.GetComponent<PhotonView>().RPC("SetBehaviour", RpcTarget.AllBuffered, headingDirection, angleRange, delay, duration);
    }

    private Directions GetHeadingAngle(Vector3 randPos)
    {
        if (randPos.x == gameField.mapSpec.xMin)
        {
            if (randPos.y == gameField.mapSpec.yMin)
            {
                return Directions.NE;

            }
            else if (randPos.y == gameField.mapSpec.yMax)
            {
                return Directions.SE;
            }
            else
            {
                return Directions.E;
            }
        }
        else if (randPos.x == gameField.mapSpec.xMax)
        {
            if (randPos.y == gameField.mapSpec.yMin)
            {
                //shoot 45'
                return Directions.NW;

            }
            else if (randPos.y == gameField.mapSpec.yMax)
            {
                return Directions.SW;
            }
            else
            {
                return Directions.W;
            }

        }
        else
        {
            if (randPos.y == gameField.mapSpec.yMin)
            {
                return Directions.N;

            }
            else
            {
                return Directions.S;
            }
        }
    }

    private Vector3 GetRandomBoundaryPos()
    {
        Vector3 randPos = gameField.GetRandomPosition();
        float randX = randPos.x;
        float randY = randPos.y;
        bool xClamp = Random.Range(0f, 1f) < 0.5f;
        if (xClamp)
        {
            randX = (randX < gameField.mapSpec.xMid) ? gameField.mapSpec.xMin : gameField.mapSpec.xMax;
        }
        else
        {
            randY = (randY < gameField.mapSpec.yMid) ? gameField.mapSpec.yMin : gameField.mapSpec.yMax;
        }
        return new Vector3(randX, randY);// new Vector3(randX, randY);
    }

    private MoveType GetRandomMoveType(SpawnDirection spawnDir)
    {
        if (spawnDir == SpawnDirection.Preemptive)
            return MoveType.Static;
        return (MoveType)Random.Range(1, 3);
    }
    public float boxProbability = 0.25f;
    private SpawnDirection GetRandomSpawnDir()
    {
        if (currentDifficult == MapDifficulty.BoxOnly) return SpawnDirection.Preemptive;
        if(Random.Range(0,1f) <= boxProbability) return SpawnDirection.Preemptive;
        return (SpawnDirection)Random.Range(1, (int)SpawnDirection.None);
    }

  
    public static float DirectionsToEuler(Directions dir)
    {
        switch (dir)
        {
            case Directions.N:
                return 90f;
            case Directions.S:
                return 270f;
            case Directions.W:
                return 180f;// 180f / 2f;
            case Directions.E:
                return 0f;
            case Directions.SW:
                return 225f;
            case Directions.SE:
                return 315f;
            case Directions.NW:
                return 135f;
            case Directions.NE:
                return 45f;
            default:
                Debug.LogWarning("Wrong cardinal");
                return 0f;
        }

    }

    /*
     bullet spawner 생성 - photoninstan
     info 주입 -master rpc

    spawner가 bullet생성
     - 마스터 네트워크 생성
    이동처리 각자
    충돌 각자
    스킬 각자
    사망 각자처리후 전송
     */


}
public enum SpawnDirection
{
    Preemptive, Straight, Spiral, None
}

//이동방식
public enum MoveType
{
    Static, Curves, Straight,OrbitAround
}
public enum ReactionType { 
    Die,Bounce,None
}

public enum Directions
{
    W = 0, E = 1, N = 2, S = 3, NW, NE, SW, SE,
}