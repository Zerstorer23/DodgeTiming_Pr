using Lex;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ActionSet
{
    //Caster Object
    internal GameObject caster;
    internal SkillManager skillManager;
    internal Unit_Player castingPlayer;
    internal Unit_Movement castingPlayerMovement;

    //Cache
    internal LexView projectilePV,casterPV; 

    //GameObject
    internal GameObject spawnedObject;

    private Dictionary<SkillParams, object> parameters = new Dictionary<SkillParams, object>();
    public bool isMutualExclusive = false;
    internal T GetParam<T>(SkillParams key)
    {
        Debug.Assert(parameters.ContainsKey(key), " missing param : "+key);
        return (T)parameters[key];
    }
    internal T GetParam<T>(SkillParams key, object defaultVal)
    {
        if (!parameters.ContainsKey(key)) {
            parameters.Add(key, defaultVal);
        }
        return (T)parameters[key];
    }

    public ActionSet( SkillManager skm)
    {
        caster = skm.gameObject;
        castingPlayer = skm.gameObject.GetComponent<Unit_Player>();
        casterPV = caster.GetComponent<LexView>();
        castingPlayerMovement = caster.GetComponent<Unit_Movement>();
        skillManager = skm;
    }
    public void Init() {
        parameters = new Dictionary<SkillParams, object>();
        skillActions = new List<SkillAction>();
    }





    List<SkillAction> skillActions = new List<SkillAction>();

    public IEnumerator Activate()
    {
        foreach (SkillAction action in skillActions) {
            float delay= 0;
            try
            {

                delay = action.Activate();
             
            }
            catch (Exception e) {
                Debug.LogWarning(e.Message);
                Debug.LogWarning(e.StackTrace);
            }
            if (delay > Mathf.Epsilon)
            {
                yield return new WaitForSeconds(delay);
            }
        }
        skillManager.pv.RPC("SetSkillInUse",  false);
    }


    public void Enqueue(SkillAction action) {
        action.SetSkillSet(this);
        skillActions.Add(action);
    }

    internal void SetParam(SkillParams key, object v)
    {
       // Debug.Log("Set param " + key + " : " + v);
        if (parameters.ContainsKey(key))
        {
            parameters[key] = v;
        }
        else
        {
            parameters.Add(key, v);
        }
    }
}
public enum SkillParams { 
    Width,Height,PrefabName,Transform,Duration,GameObject,Quarternion,Vector3
        ,MoveSpeed,EulerAngle,UserID,Modifier,Color,Enable,AnimationTag,ReactionType
        ,BuffData,
    FieldNumber,
    RotateAngle,
    RotateSpeed,
    Distance,
    LexView
}

/*
 Skill
  -,time, Vector3 , GameObject 
  List<WaitAction,WaitAction>
  1. 3초를 대기
  2. D준비하고
  3. 1,1을 Vector3에 저장
  4. Vector3의 위치에 생성

Action.Activate();

/*
 float = 0
List.Add(WaitAction)
List.Add(PrepareAction)
List.Add(Action)
List.Add(Action)
 
 */

//3초후에 d를 1,1에 생성



