using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ConveyerBelt : MonoBehaviour
{
    Vector3 direction = Vector3.zero;
     float speed = 20f;

    private void Awake()
    {
        direction = ConstantStrings.GetAngledVector(transform.rotation.eulerAngles.z, 1f);
    }
    public Vector3 GetDirection() {
        return direction * speed;
    }
}
