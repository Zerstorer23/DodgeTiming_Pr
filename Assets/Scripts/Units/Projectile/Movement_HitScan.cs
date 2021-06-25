using Lex;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static ConstantStrings;

public class Movement_HitScan : 
//  MonoBehaviour
//MonoBehaviourLexCallbacks, IPunObservable
MonoBehaviourLex
{
    float moveSpeed = 600f;
    Rigidbody2D myRigidBody;
    CircleCollider2D myCollider;
    Projectile_Movement pMove;
    //다이나믹 -> 키네마틱 안줌
    //키네마틱-.다이나믹 줌
    Queue<VelocityVector> velocityQueue = new Queue<VelocityVector>();
    private void Awake()
    {
        myRigidBody = GetComponent<Rigidbody2D>();
        pMove = GetComponent<Projectile_Movement>();
        myCollider = GetComponent<CircleCollider2D>();

    }
    private void OnEnable()
    {
        velocityQueue.Clear();
     //  networkExpectedTime =(double) lexView.InstantiationData[3];
    }
    /*
     * TODO
     * Image에서 터치이벤트 받기
     * 계층기반 터치순서
     */
    //Image에 계층기반 ImageClick예제
    private void Update()
    {

            pMove.moveSpeed = moveSpeed;
            myRigidBody.velocity = GetAngledVector(pMove.eulerAngle, moveSpeed);
      
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        string tag = collision.gameObject.tag;
        // Debug.Log(gameObject.name + "Collision with " + collision.gameObject.name + " / tag " + tag);
        switch (tag)
        {
            case TAG_PLAYER:
            case TAG_PROJECTILE:
                myCollider.isTrigger = true;
                break;
        }
    }
    private void OnCollisionExit2D(Collision2D collision)
    {
        string tag = collision.gameObject.tag;
        // Debug.Log(gameObject.name + "Collision with " + collision.gameObject.name + " / tag " + tag);
        switch (tag)
        {
            case TAG_PLAYER:
            case TAG_PROJECTILE:
                myCollider.isTrigger = false;
                break;
        }
    }

}

public class VelocityVector
{
    public double timestamp;
    public Vector3 velocity;
    public VelocityVector(double t, Vector3 v)
    {
        this.timestamp = t;
        this.velocity = v;
    }

    public bool IsExpired()
    {
        return (timestamp <= LexNetwork.NetTime);
    }
    public override string ToString()
    {
        return timestamp + " : " + velocity;
    }

}