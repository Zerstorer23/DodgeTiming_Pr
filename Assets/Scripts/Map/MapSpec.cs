using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class MapSpec
{
    public float xMin, xMax, yMin, yMax, xMid, yMid;
    public override string ToString()
    {
        return string.Format("X {0}~ {1} + Y {2} ~ {3}", xMin, xMax, yMin, yMax);
    }

    internal bool IsOutOfBound(Vector3 position, float offset = 3f)
    {
        return (position.x < (xMin - offset)
            || position.x > (xMax + offset)
            || position.y < (yMin - offset)
            || position.y > (yMax + offset)
            );

    }
}