using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyIndicator : MonoBehaviour
{
  
    float indicatorLength = 4f;
    [SerializeField] Unit_Player player;
   public Transform target;

    public void SetTargetAsNearestEnemy() {
        target = GameFieldManager.gameFields[player.fieldNo].playerSpawner.GetNearestPlayerFrom(transform.position, player.controller.uid);
        gameObject.SetActive(target != null);
    }
    private void FixedUpdate()
    {
        if (target == null) return;
        UpdateDirection();
    }
    void UpdateDirection()
    {
        float angle = GameSession.GetAngle(player.transform.position, target.position);
        float rad = angle / 180 * Mathf.PI;
        float dX = Mathf.Cos(rad) * indicatorLength;
        float dY = Mathf.Sin(rad) * indicatorLength;
        transform.localPosition = new Vector3(dX, dY);
        transform.localRotation = Quaternion.Euler(0, 0, angle);
    }
}
