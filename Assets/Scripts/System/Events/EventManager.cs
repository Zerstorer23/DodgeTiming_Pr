using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

[System.Serializable]
public class EventManager : MonoBehaviour
{
    private static EventManager prEvManager;

    public static EventManager eventManager
    {
        get
        {
            if (!prEvManager)
            {
                prEvManager = FindObjectOfType<EventManager>();
                if (!prEvManager)
                {
                }
                else
                {
                    prEvManager.Init();
                }
            }

            return prEvManager;
        }
    }

    private Dictionary<MyEvents, EventOneArg> eventDictionary;

    void Init() {

        if (eventDictionary == null)
        {
            eventDictionary = new Dictionary<MyEvents, EventOneArg>();
        }
    }

    public EventOneArg GetEvent(MyEvents eventName) {

        EventOneArg thisEvent;
        eventDictionary.TryGetValue(eventName, out thisEvent);
        return thisEvent;
//       bool found= eventDictionary.TryGetValue(eventName,out thisEvent);

    }
    public void AddEvent(MyEvents eventName, EventOneArg thisEvent) {
        eventDictionary.Add(eventName, thisEvent);
    }

    public static void StartListening(MyEvents eventName, UnityAction<EventObject> listener)
    {
        if (eventManager == null) return;
        EventOneArg thisEvent = eventManager.GetEvent(eventName);
        if (thisEvent != null)
        {
            thisEvent.AddListener(listener);
        }
        else
        {
            thisEvent = new EventOneArg();
            thisEvent.AddListener(listener);
            eventManager.AddEvent(eventName, thisEvent);
        }
    }

    public static void StopListening(MyEvents eventName, UnityAction<EventObject> listener)
    {
        if (eventManager == null) return;
        EventOneArg thisEvent = eventManager.GetEvent(eventName);
        if (thisEvent != null)
        {
            thisEvent.RemoveListener(listener);
        }
    }

    public static bool TriggerEvent(MyEvents eventName, EventObject variable)
    {
        if (eventManager == null) {
            Debug.LogWarning("On Destroy no EventManager.");
            return false;
        }
        EventOneArg thisEvent =  eventManager.GetEvent(eventName);
        if (thisEvent != null)
        {
            thisEvent.Invoke(variable);
            return true;
        }
        return false;
    }

}




