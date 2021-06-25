using Lex;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static ConstantStrings;

public partial class Projectile_Movement : MonoBehaviourLex
{

    private void DoMove_Straight()
    {
        Vector3 moveDir = GetAngledVector(eulerAngle, moveSpeed * Time.deltaTime);
        ChangeTransform(moveDir);
    }
    private void DoMove_Curve()
    {
        if (angleStack >= angleClockBound)
        {
            goClockwise = -1;
        }
        else if (angleStack <= -angleClockBound)
        {
            goClockwise = 1;
        }
        float amount = rotateScale * Time.deltaTime * goClockwise;
        angleStack += amount;
        eulerAngle += amount;
        Vector3 moveDir = GetAngledVector(eulerAngle, moveSpeed * Time.deltaTime);
        ChangeTransform(moveDir);

    }
    float orbitLength = 4f;
    float distanceMoved = 0;
    float orbitSpeed = 120f;
    private void DoMove_Orbit()
    {
        if (distanceMoved < orbitLength)
        {
            if (!hp.damageDealer.myCollider.isTrigger) hp.damageDealer.myCollider.isTrigger = true;
            Vector3 moveDir = GetAngledVector(eulerAngle, moveSpeed * Time.deltaTime * 2);
            distanceMoved += Vector2.Distance(moveDir, Vector3.zero);
            ChangeTransform(moveDir);
        }
        else
        {
            if (hp.damageDealer.myCollider.isTrigger) hp.damageDealer.myCollider.isTrigger = false;
            eulerAngle += orbitSpeed * Time.deltaTime;
            if (transSync)
            {
                netTransform.networkPos = Vector3.zero;
            }
            else
            {
                transform.localPosition = Vector3.zero;
            }
            Vector3 moveDir = GetAngledVector(eulerAngle, orbitLength);
            ChangeTransform(moveDir);
        }
    }


    private void DoMove_Static()
    {
        // ChangeTransform(Vector3.zero);
    }

    public void Bounce(ContactPoint2D contact, Vector3 contactPoint)
    {
        if (moveType == MoveType.OrbitAround) return;
        Vector3 normal = contact.normal;
        float rad = eulerAngle * Mathf.Deg2Rad;
        float dX = Mathf.Cos(rad) * moveSpeed * Time.deltaTime;
        float dY = Mathf.Sin(rad) * moveSpeed * Time.deltaTime;
        Vector3 velocity = new Vector3(dX, dY);
        velocity = Vector3.Reflect(velocity, normal);
        eulerAngle = (Mathf.Atan2(velocity.y, velocity.x) * Mathf.Rad2Deg);// * boundFactor;

        ChangeTransform(Vector3.zero);
    }

}
