
using UnityEngine;


public class EventObject
{
    public bool boolObj;
    public int intObj;
    public float floatObj;
    public string stringObj;
    public GameObject goData;
    public object objData;
    public Projectile_DamageDealer sourceDamageDealer;
    public HealthPoint hitHealthPoint;
    public Vector3 vectorObj;
    public EventObject() { 
    
    }
    public EventObject(object obj) {
        this.objData = obj;
    }
    public EventObject(string str)
    {
        this.objData = str;
        this.stringObj = str;
    }
    public T Get<T>() {
        return (T)objData; 
    }
}
