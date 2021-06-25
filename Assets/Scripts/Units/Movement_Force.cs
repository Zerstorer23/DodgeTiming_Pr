using Lex;
using UnityEngine;
using System.Linq;
using static ConstantStrings;

public class Movement_Force : MonoBehaviourLex
{
   static int numIndex = 2;
    [SerializeField] float[] weight;
    [SerializeField] Vector3[] direction;

   
    
    ICachedComponent cachedComponent = new ICachedComponent();
    private void Awake()
    {
        weight = new float[numIndex];
        direction = new Vector3[numIndex];//Vector3.zero; // movespeed에 비례
    }
    public Vector3 AdjustForce(Vector3 currentDir) {
        float sumWeight = weight.Sum() +1f;
        if (sumWeight <= 1f) return currentDir;
        Vector3 newDir = currentDir * (1 / sumWeight);
        for (int i = 0; i < numIndex; i++) {
            newDir += Time.deltaTime* direction[i]  * weight[i] / sumWeight;
        }
        return newDir;    
    }

    [LexRPC]
    public void AddMovementForce(Vector3 direction, float weight) {
        this.direction[(int)GravityIndex.Dash] = direction;
        this.weight[(int)GravityIndex.Dash] = weight;
    }

    private void OnDisable()
    {
        weight = new float[numIndex];
        direction = new Vector3[numIndex];
        cachedComponent.Clear();
    }
    private void FixedUpdate()
    {
        Collider2D[] collisions = Physics2D.OverlapCircleAll(
        transform.position, 0.25f, LayerMask.GetMask(TAG_CONVEYER_BELT));
        this.direction[(int)GravityIndex.Belt] = Vector3.zero;
        weight[(int)GravityIndex.Belt] = 0f;

        foreach (var c in collisions)
        {
            int tid = c.gameObject.GetInstanceID();
            if (c.gameObject.tag == TAG_CONVEYER_BELT) {
                ConveyerBelt belt = cachedComponent.Get<ConveyerBelt>(tid,c.gameObject);
                this.direction[(int)GravityIndex.Belt] += belt.GetDirection();
                weight[(int)GravityIndex.Belt] = 2f;
            }
        }
    }

  
}
public enum GravityIndex
{
    Dash, Belt
}