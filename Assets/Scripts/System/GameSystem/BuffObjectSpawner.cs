using Lex;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public class BuffObjectSpawner : MonoBehaviour
{


    double lastSpawnTime;
    double startTime;
   [SerializeField] GameField gameField;
    string prefabName = "Prefabs/BuffObjects/buffObject";

    internal void StartEngine()
    {
        startTime = LexNetwork.Time;
    }

    private void FixedUpdate()
    {
        if (!LexNetwork.IsMasterClient) return;
        if (!GameSession.gameStarted || LexNetwork.Time < startTime + GameFieldManager.instance.spawnAfter) return;
        float thisDelay = GameFieldManager.instance.spawnDelay;
        if (gameField.suddenDeathCalled) {
            thisDelay *= 0.75f;
        }
        if (LexNetwork.Time >= lastSpawnTime + thisDelay) {
            InatantiateBuffObject();
            lastSpawnTime = LexNetwork.Time;
        }
    }

    private void InatantiateBuffObject()
    {
        Vector3 randPos = GetRandomPosition();
        int randIndex = Random.Range(0, ConfigsManager.instance.buffConfigs.Length);
        LexNetwork.InstantiateRoomObject(prefabName, randPos, Quaternion.identity, 0,
            new object[] {gameField.fieldNo, randIndex }
            );
    }

    private Vector3 GetRandomPosition()
    {
        Unit_Player lowestPlayer = gameField.playerSpawner.GetLowestScoreActivePlayer();
        if (lowestPlayer != null)
        {
            return gameField.GetRandomPositionNear(lowestPlayer.gameObject.transform.position, 10f, 2f);
        }
        else {
            return gameField.GetRandomPosition(2f);
        }

    }


}
