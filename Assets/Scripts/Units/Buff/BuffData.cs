using Lex;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;



[System.Serializable]
public class BuffData

{
    public BuffType buffType;
    public float modifier;
    public double duration;
    double endTime;
    bool timerStarted = false;
    public BuffData(BuffType bType, float mod, double _duration)
    {
        buffType = bType;
        modifier = mod;
        duration = _duration;
    }

    public BuffData(object[] data)
    {
        buffType = (BuffType) data[0];
        modifier = (float)data[1];
        duration = (double)data[2];
    }
    public double StartTimer() {
        endTime = LexNetwork.Time + duration;
        timerStarted = true;
        return endTime;
    }

    public bool IsBuffFinished() {
        if (!timerStarted) return false;
        return LexNetwork.Time >= endTime;
    }
    public void PrintContent() {
        Debug.Log("Buff : " + buffType + " " + modifier + " for " + duration);
    }

    public object[] SerialiseToList() {
        object[] obj = new object[4];
        obj[0] = buffType;
        obj[1] = modifier;
        obj[2] = duration;
        return obj;
    }
}
[System.Serializable]
public enum BuffType
{ 
    None,MoveSpeed,Cooltime,HealthPoint,InvincibleFromBullets, MirrorDamage,NumDamageReceivedWhileBuff
        ,HideBuffs,CameraShake,Boom,BlockSkill
}



