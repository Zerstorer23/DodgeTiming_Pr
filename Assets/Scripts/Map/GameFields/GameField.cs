using Lex;
using System.Collections;
using UnityEngine;

public class GameField : MonoBehaviourLex
{
    public int fieldNo = 0;
    public MapSpec mapSpec = new MapSpec();
    public PlayerSpawner playerSpawner;
    public BulletManager bulletSpawner;
    public BuffObjectSpawner buffSpawner;
    public WallManager wallManager;
    Transform mapTransform;
    public Transform[] map_transforms;
    public Vector3 originalMapSize;
    public bool suddenDeathCalled = false;
    public bool gameFieldFinished = false;
    public int expectedNumPlayer = 0;
    public virtual void Awake()
    {
        playerSpawner = GetComponentInChildren<PlayerSpawner>();
        bulletSpawner = GetComponentInChildren<BulletManager>();
        buffSpawner = GetComponentInChildren<BuffObjectSpawner>();
        wallManager = GetComponentInChildren<WallManager>();
    }

    public virtual void OnEnable()
    {
        EventManager.StartListening(MyEvents.EVENT_REQUEST_SUDDEN_DEATH, OnSuddenDeathTriggered);
        EventManager.StartListening(MyEvents.EVENT_GAME_FINISHED, OnGameFinished);
        if (suddenDeathTimeoutRoutine != null) {
            StopCoroutine(suddenDeathTimeoutRoutine);
        }
        suddenDeathTimeoutRoutine = WaitAndSuddenDeath();
        StartCoroutine(suddenDeathTimeoutRoutine);
    }
    public virtual void OnDisable()
    {
        EventManager.StopListening(MyEvents.EVENT_REQUEST_SUDDEN_DEATH, OnSuddenDeathTriggered);
        EventManager.StopListening(MyEvents.EVENT_GAME_FINISHED, OnGameFinished);

    }

    IEnumerator suddenDeathTimeoutRoutine = null;
    IEnumerator WaitAndSuddenDeath (){
        yield return new WaitForSeconds(GameFieldManager.instance.suddenDeathTime);

        if (!suddenDeathCalled)
        {
            Debug.Log("Time out do suddendeath");
            CheckSuddenDeath(0, true);
        }
    }

    private void OnGameFinished(EventObject obj)
    {
        if (resizeNumerator != null)
        {
            StopCoroutine(resizeNumerator);
        }
        gameObject.SetActive(false);
    }
    public virtual void CheckFieldConditions(GameStatus stat)
    {
        if (gameFieldFinished) return;
        CheckSuddenDeath(stat.alive);
        Debug.LogWarning(stat.ToString());
        bool gameFinished = GameSession.gameModeInfo.IsFieldFinished(stat);
        if (!gameFinished) return;

        LexPlayer winner = stat.lastSurvivor;
        Debug.Log("GAME FISNISHED /  winner "+winner);
        GameFieldManager.pv.RPC("NotifyFieldWinner", fieldNo, winner.uid);
       // NotifyFieldWinner(winner);
    }

    public (bool,LexPlayer) QueryFieldFinish()
    {
        if (gameFieldFinished) return (true, playerSpawner.lastDiedPlayer);
        GameStatus stat = new GameStatus(playerSpawner.unitsOnMap, playerSpawner.lastDiedPlayer);
        Debug.LogWarning(stat.ToString());
        bool finished = GameSession.gameModeInfo.IsFieldFinished(stat);
        return (finished, stat.lastSurvivor);
    }

    public LexPlayer fieldWinner = null;
    public string winnerName = null;

    public void CheckSuddenDeath(int numAlive, bool timeout = false)
    {
        if (!GameSession.gameModeInfo.callSuddenDeath) return;
        if (GameSession.instance.devMode) return;
        if (numAlive <= 2 && !suddenDeathCalled)
        {
            suddenDeathCalled = true;
            if (!timeout) {
                StopCoroutine(suddenDeathTimeoutRoutine);
            }
            EventManager.TriggerEvent(MyEvents.EVENT_REQUEST_SUDDEN_DEATH, new EventObject() { intObj = fieldNo });
        }
    }

    private void OnSuddenDeathTriggered(EventObject obj)
    {
        if (obj.intObj != fieldNo) return;
        if (resizeNumerator != null)
        {
            StopCoroutine(resizeNumerator);
        }
        startTime = LexNetwork.NetTime;
        EventManager.TriggerEvent(MyEvents.EVENT_SEND_MESSAGE, new EventObject() { stringObj = "맵 크기가 줄어듭니다!!" });
        resizeNumerator = ResizeMapByTime();
        StartCoroutine(resizeNumerator);
    }


    float resize_StartSize;
    public double startTime;
    IEnumerator resizeNumerator;

    public IEnumerator ResizeMapByTime()
    {

        bool doRoutine = true;
        double elapsedTime = 0;
        while (doRoutine)
        {
            float newLength = GameFieldManager.instance.resize_EndSize +
                (resize_StartSize - GameFieldManager.instance.resize_EndSize) * 
                (float)(1 - (elapsedTime / GameFieldManager.instance.resizeOver));
            mapTransform.localScale = new Vector3(newLength, newLength);


            yield return new WaitForFixedUpdate();
            ///  yield return new WaitForSeconds(0.05f);
            mapSpec.xMin = map_transforms[0].position.x;
            mapSpec.xMax = map_transforms[1].position.x;
            mapSpec.yMin = map_transforms[0].position.y;
            mapSpec.yMax = map_transforms[1].position.y;
            mapSpec.xMid = (mapSpec.xMin + mapSpec.xMax) / 2;
            mapSpec.yMid = (mapSpec.yMin + mapSpec.yMax) / 2;
            elapsedTime = LexNetwork.NetTime - startTime;
            doRoutine = elapsedTime < GameFieldManager.instance.resizeOver;
        }

    }



    public GameMode fieldType;

    public void InitialiseMap(int id = 0)
    {
        fieldNo = id;
        InitialiseMapSize();
    }



    internal void StartEngine(MapDifficulty mapDIfficulty)
    {
        ResetFieldProperties();
        playerSpawner.StartEngine();
        bulletSpawner.StartEngine(mapDIfficulty);
        buffSpawner.StartEngine();
        wallManager.SetWalls();
        //EventManager.TriggerEvent(MyEvents.EVENT_FIELD_STARTED, new EventObject(){ });
    }
    void ResetFieldProperties() {
        suddenDeathCalled = false;
        fieldWinner = null;
        winnerName = "";
        gameFieldFinished = false;
      //  Debug.Log("Reset field");
    }

    private void InitialiseMapSize()
    {
        mapTransform = GetComponent<Transform>();
        if (originalMapSize == Vector3.zero) {
            originalMapSize = transform.localScale;
        }

        int numPlayer = LexNetwork.PlayerList.Length;
        float modifiedLength;
        if (!GameSession.gameModeInfo.scaleMapByPlayerNum)
        {
            modifiedLength = 0f;
        }
        else {
             modifiedLength = GameFieldManager.instance.mapStepsize * (numPlayer / GameFieldManager.instance.mapStepPerPlayer);
        }
        Vector3 newSize = originalMapSize + new Vector3(modifiedLength, modifiedLength);
        mapTransform.localScale = newSize;
        resize_StartSize = newSize.x;
        mapSpec.xMin = map_transforms[0].position.x;
        mapSpec.xMax = map_transforms[1].position.x;
        mapSpec.yMin = map_transforms[0].position.y;
        mapSpec.yMax = map_transforms[1].position.y;
        mapSpec.xMid = (mapSpec.xMin + mapSpec.xMax) / 2;
        mapSpec.yMid = (mapSpec.yMin + mapSpec.yMax) / 2;
 
        DivideSpawningArea(numPlayer);
    }
    int w;
    int h;
    void DivideSpawningArea(int numPlayer)
    {
        w = 1;
        h = 1;
        bool multWidth = false;
        while (w * h < numPlayer)
        {
            if (multWidth)
            {
                w++;
            }
            else
            {
                h++;
            }
            multWidth = !multWidth;
        }
    }


    public virtual Vector3 GetPlayerSpawnPosition(LexPlayer myPlayer)
    {
        int myIndex = LexNetwork.GetMyIndex(myPlayer, GameFieldManager.GetPlayersInField(fieldNo));
//        Debug.LogWarning(myPlayer+" My Index " + myIndex);
        int x = myIndex % w;
        int y = myIndex / w;
        return GetPoissonPositionNear(x, y);

    }
    public Vector3 GetRandomPosition(float boundOffset = 0)
    {
        float randX = Random.Range(mapSpec.xMin + boundOffset, mapSpec.xMax - boundOffset);
        float randY = Random.Range(mapSpec.yMin + boundOffset, mapSpec.yMax - boundOffset);
        return new Vector3(randX, randY, 0);

    }
    public Vector3 GetRandomPositionNear(Vector3 center, float window, float boundOffset = 0)
    {

        float randX = center.x +Random.Range(-window, window);
        float randY = center.y + Random.Range(-window, window);
        if (randX <= (mapSpec.xMin + boundOffset)) randX = mapSpec.xMin + boundOffset;
        if (randX >= (mapSpec.xMax - boundOffset)) randX = mapSpec.xMax - boundOffset;
        if (randY <= (mapSpec.yMin + boundOffset)) randY = mapSpec.yMin + boundOffset;
        if (randY <= (mapSpec.yMin + boundOffset)) randY = mapSpec.yMin + boundOffset;
        return new Vector3(randX, randY, 0);
    }
    public Vector3 GetPoissonPositionNear(int x, int y)
    {
        float width = (mapSpec.xMax - mapSpec.xMin) / w;
        float height = (mapSpec.yMax - mapSpec.yMin) / h;

        float xOffset = width / 2;
        float yOffset = height / 2;

        float randX = Random.Range(-width / 4, width / 4);
        float randY = Random.Range(-height / 4, height / 4);
        Vector3 location = new Vector3(mapSpec.xMin + xOffset + width * x + randX, mapSpec.yMin + yOffset + height * y + randY);
      /* Debug.Log("Width units " + width + "," + height);
        Debug.Log("Offset units " + xOffset + "," + yOffset);
        Debug.Log("rand units " + randX + "," + randY);
        Debug.Log("start units " + mapSpec.xMin + "," + mapSpec.yMin);
        Debug.Log("Indicated location " + location);*/
        return location;
    }


    /*    void OnDrawGizmos()
{
   // Green
   DrawRect(new Vector3(mapSpec.xMax - mapSpec.xMin, mapSpec.yMax - mapSpec.yMin), 5f);
}

void OnDrawGizmosSelected()
{
   // Orange
   Gizmos.color = new Color(1.0f, 1.0f, 1.0f);
   DrawRect(new Vector3(mapSpec.xMax - mapSpec.xMin, mapSpec.yMax - mapSpec.yMin), 5f);
}
private void DrawRect(Vector2 size, float thikness)
{
   var matrix = Gizmos.matrix;
   Gizmos.matrix = transform.localToWorldMatrix;

   //top cube
   Gizmos.DrawCube(Vector3.up * size.y / 2, new Vector3(size.x, thikness, 0.01f));

   //bottom cube
   Gizmos.DrawCube(Vector3.down * size.y / 2, new Vector3(size.x, thikness, 0.01f));

   //left cube
   Gizmos.DrawCube(Vector3.left * size.x / 2, new Vector3(thikness, size.y, 0.01f));

   //right cube
   Gizmos.DrawCube(Vector3.right * size.x / 2, new Vector3(thikness, size.y, 0.01f));

   Gizmos.matrix = matrix;
}*/

}
