using Lex;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using System;
using Random = UnityEngine.Random;

public class BoxObstacle : MonoBehaviourLex
{
    float width;
    float height;
    float warnDelay;
    bool isDead = false;
    BoxCollider2D myCollider;
    int fieldNumber = 0;
    LexView pv;
    [SerializeField] SpriteRenderer fillSprite;
    [SerializeField] SpriteRenderer boundarySprite;
    IEnumerator deleteRoutine;

    private void Awake()
    {
        myCollider = GetComponent<BoxCollider2D>();
        pv = GetComponent<LexView>();
    }

    private void OnGameEnd(EventObject arg0)
    {
        DoDeath();
    }
    private void OnEnable()
    {
        EventManager.StartListening(MyEvents.EVENT_FIELD_FINISHED, OnGameEnd);
        transform.SetParent(GameSession.GetBulletHome());
        fieldNumber = (int)pv.InstantiationData[0];
        height = (float)pv.InstantiationData[1];
        width = (float)pv.InstantiationData[2];
        this.warnDelay = (float)pv.InstantiationData[3];
        transform.localScale = new Vector3(width, height, 1);
        StartFadeIn();


        isDead = false;
        boundarySprite.enabled = true;
        EventManager.TriggerEvent(MyEvents.EVENT_BOX_SPAWNED, new EventObject() { goData = gameObject });

    }

/*    private void Update()
     {
          CheckContacts();
        DebugDrawBox(transform.position,transform.localScale,transform.eulerAngles.z,Color.red,1f);
     }*/
    void DebugDrawBox(Vector2 point, Vector2 size, float angle, Color color, float duration)
    {

        var orientation = Quaternion.Euler(0, 0, angle);

        // Basis vectors, half the size in each direction from the center.
        Vector2 right = orientation * Vector2.right * size.x / 2f;
        Vector2 up = orientation * Vector2.up * size.y / 2f;

        // Four box corners.
        var topLeft = point + up - right;
        var topRight = point + up + right;
        var bottomRight = point - up + right;
        var bottomLeft = point - up - right;

        // Now we've reduced the problem to drawing lines.
        Debug.DrawLine(topLeft, topRight, color, duration);
        Debug.DrawLine(topRight, bottomRight, color, duration);
        Debug.DrawLine(bottomRight, bottomLeft, color, duration);
        Debug.DrawLine(bottomLeft, topLeft, color, duration);
    }

    private void OnDisable()
    {
        fillSprite.DORewind();
        EventManager.StopListening(MyEvents.EVENT_FIELD_FINISHED, OnGameEnd);
    }
    private void StartFadeIn()
    {
        Color color = Color.blue;
        color.a = 0;
        fillSprite.color = color;
        fillSprite.DOFade(1f, warnDelay).OnComplete(()=> {
            //CheckContacts
            fillSprite.color = Color.green;
            boundarySprite.enabled = false;
            if (deleteRoutine != null)StopCoroutine(deleteRoutine);
            deleteRoutine = WaitAndDestroy();
            StartCoroutine(deleteRoutine);
        });
        StartCoroutine(WaitAndCollide());
    }

    private void CheckContacts()
    {
       Collider2D[] collisions = Physics2D.OverlapBoxAll(transform.position,  transform.localScale,transform.eulerAngles.z,LayerMask.GetMask("Player","Projectile"),minDepth:-3f,maxDepth:3f);
     
        for (int i = 0; i < collisions.Length; i++) {
            Collider2D c = collisions[i];
            HealthPoint healthPoint = c.gameObject.GetComponent<HealthPoint>();
           if (healthPoint == null) return;
               switch (c.gameObject.tag) {
                  case ConstantStrings.TAG_PLAYER:
                    if (healthPoint.controller.IsLocal) {
                        EventManager.TriggerEvent(MyEvents.EVENT_SEND_MESSAGE, new EventObject() { stringObj = "갇혀죽었습니다!" });
                    }
                    healthPoint.Kill_Immediate();
                      break;
                  case ConstantStrings.TAG_PROJECTILE:
                    healthPoint.Kill_Immediate();
                    break;
              }

        }
    }
    IEnumerator WaitAndCollide()
    {
        yield return new WaitForSeconds(warnDelay);
        CheckContacts();
        EventManager.TriggerEvent(MyEvents.EVENT_BOX_ENABLED, new EventObject() { goData = gameObject });
        yield return new WaitForFixedUpdate();
        myCollider.enabled = true;
    }

    IEnumerator WaitAndDestroy()
    {
        float randTime = Random.Range(warnDelay, warnDelay * 3f);
        yield return new WaitForSeconds(randTime);
        DoDeath();
    }
    void DoDeath()
    {
        if (isDead) return;
        isDead = true;
        if (pv.IsMine) {
            EventManager.TriggerEvent(MyEvents.EVENT_SPAWNER_EXPIRE, new EventObject() { intObj = fieldNumber});
            LexNetwork.Destroy(pv);
        }

    }
}
