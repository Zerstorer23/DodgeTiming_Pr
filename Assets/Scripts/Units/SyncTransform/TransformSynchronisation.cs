using Lex;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TransformSynchronisation : MonobehaviourLexSerialised
{

    public double networkExpectedTime;
    public Vector3 networkPos;//ONLY USE FOR PV MINE TODO
    public Quaternion networkQuaternion;
    Queue<TimeVector> positionQueue = new Queue<TimeVector>();
    private double lastSyncTime;

    public bool syncRotation = true;


    private void OnEnable()
    {
        networkPos = transform.localPosition;
        networkQuaternion = transform.rotation;

    }
    private void OnDisable()
    {
        positionQueue.Clear();
    }

    private void Update()
    {
        WriteSync();
        DequeuePositions();
    }
    public void EnqueueLocalPosition(Vector3 newPosition, Quaternion newQuaternion)
    {
        networkPos = newPosition;
        networkQuaternion = newQuaternion;
        networkExpectedTime = LexNetwork.NetTime + GameSession.STANDARD_PING;
    }
 
    void DequeuePositions()
    {

        TimeVector tv = null;
        while (positionQueue.Count > 0 && positionQueue.Peek().IsExpired())
        {
            tv = positionQueue.Dequeue();
        }
        if (tv != null)
        {
            transform.localPosition = tv.position;
            if (syncRotation)
            {
                transform.rotation = tv.quaternion;
            }

        }
/*        else {
            //nothing to dequeue, update my position
            EnqueueLocalPosition(transform.localPosition, transform.rotation);
        }*/

    }


   /* public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        bool needSync = CheckSyncCondition();
        //통신을 보내는 
        if (stream.IsWriting)
        {
            if (!needSync) return;
            if (networkExpectedTime != lastSyncTIme)
            {
                positionQueue.Enqueue(new TimeVector(networkExpectedTime, networkPos, networkQuaternion));
            }
            stream.SendNext(networkPos);
            if (syncRotation) {
                stream.SendNext(networkQuaternion);
            }
            stream.SendNext(networkExpectedTime);
            lastSyncTIme = networkExpectedTime;
        }

        //클론이 통신을 받는 
        else
        {
            var position = (Vector3)stream.ReceiveNext();
            var quaternion = transform.rotation;
            if (syncRotation)
            {
                quaternion = (Quaternion)stream.ReceiveNext();
            }
            double netTime = (double)stream.ReceiveNext();
            TimeVector tv = new TimeVector(netTime, position, quaternion);
            if (syncType == SyncType.Timed) {
                InvalidateAfter(netTime);
            }
            positionQueue.Enqueue(tv);
        }
    }*/
    public override void OnSyncView(params object[] parameters)
    {
        //통신을 보내는 
        if (isWriting)
        {
            if (networkExpectedTime != lastSyncTime)
            {
                positionQueue.Enqueue(new TimeVector(networkExpectedTime, networkPos, networkQuaternion));
            }
            object[] obj;
            if (syncRotation)
            {
                obj = new object[3];
                obj[1] = (networkQuaternion);
                obj[2] = (networkExpectedTime);
            }
            else
            {
                obj = new object[2];
                obj[1] = (networkExpectedTime);
            }
            obj[0] = (networkPos);
            lastSyncTime = networkExpectedTime;
        }

        //클론이 통신을 받는 
        else
        {
            Vector3 position = (Vector3)parameters[0];
            Quaternion rotation = transform.rotation;
            double netTime;
            if (syncRotation)
            {
                rotation = (Quaternion)parameters[1];
                netTime = (double)parameters[2];
            }
            else
            {
                netTime = (double)parameters[1];
            }
            TimeVector tv = new TimeVector(netTime, position, rotation);
            positionQueue.Enqueue(tv);
        }
    }
}
