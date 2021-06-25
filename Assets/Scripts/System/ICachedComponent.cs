using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ICachedComponent
{
    Dictionary<int, object> componentDictionary = new Dictionary<int, object>();
   public T Get<T>(int tag, GameObject go)
    {
        if (!componentDictionary.ContainsKey(tag))
        {
            T comp = go.GetComponent<T>();
            componentDictionary.Add(tag, comp);
        }
        else
        {
            if (componentDictionary[tag] == null)
            {
                T comp = go.GetComponent<T>();
                componentDictionary[tag] = comp;
            }
        }
        return (T)componentDictionary[tag];
    }
    public T Get<T>(GameObject go)
    {
        int tag = go.GetInstanceID();
        if (!componentDictionary.ContainsKey(tag))
        {
            T comp = go.GetComponent<T>();
            componentDictionary.Add(tag, comp);
        }
        else
        {
            if (componentDictionary[tag] == null)
            {
                T comp = go.GetComponent<T>();
                componentDictionary[tag] = comp;
            }
        }
        return (T)componentDictionary[tag];
    }
    public void Clear() {
        componentDictionary.Clear();
    }
}
