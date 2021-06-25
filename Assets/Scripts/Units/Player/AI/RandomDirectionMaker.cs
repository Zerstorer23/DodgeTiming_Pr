using Lex;
using UnityEngine;

public class RandomDirectionMaker 
{
    Vector3 randomDirection;
    double nextRandomTime;
    double randomPeriod = 1d;
    public Vector3 PollRandom() {
        if (LexNetwork.NetTime > nextRandomTime)
        {
            float rx = Random.Range(-1f, 1f);
            float ry = Random.Range(-1f, 1f);
            randomDirection = new Vector3(rx, ry).normalized;
            nextRandomTime = LexNetwork.NetTime + randomPeriod;
        }
        return randomDirection;

    }

}
