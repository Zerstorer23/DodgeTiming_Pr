
namespace Lex
{
    using System;
    using System.Collections.Generic;
    using UnityEngine;
    using UnityEngine.Events;

    [System.Serializable]
    public class NetworkEventManager : MonoBehaviourLexCallbacks
    {
        private static NetworkEventManager prNetEvMan;

        public static NetworkEventManager instance
        {
            get
            {
                if (!prNetEvMan)
                {
                    prNetEvMan = FindObjectOfType<NetworkEventManager>();
                    if (!prNetEvMan)
                    {
                    }
                    else
                    {
                        prNetEvMan.Init();
                    }
                }

                return prNetEvMan;
            }
        }

        private Dictionary<LexCallback, NetEvent> eventDictionary;

        void Init()
        {

            if (eventDictionary == null)
            {
                eventDictionary = new Dictionary<LexCallback, NetEvent>();
            }
        }


        public NetEvent GetEvent(LexCallback eventID)
        {

            NetEvent thisEvent;
            eventDictionary.TryGetValue(eventID, out thisEvent);
            return thisEvent;
            //       bool found= eventDictionary.TryGetValue(eventName,out thisEvent);

        }

        public static void StartListening(LexCallback eventID, UnityAction<NetEventObject> listener)
        {
            if (instance == null) return;
            NetEvent thisEvent = instance.GetEvent(eventID);
            if (thisEvent != null)
            {
                thisEvent.AddListener(listener);
            }
            else
            {
                thisEvent = new NetEvent();
                thisEvent.AddListener(listener);
                instance.eventDictionary.Add(eventID, thisEvent);
            }
        }

        public static void StopListening(LexCallback eventID, UnityAction<NetEventObject> listener)
        {
            if (instance == null) return;
            NetEvent thisEvent = instance.GetEvent(eventID);
            if (thisEvent != null)
            {
                thisEvent.RemoveListener(listener);
            }
        }

        public static bool TriggerEvent(LexCallback eventID, NetEventObject variable)
        {
            if (instance == null)
            {
                Debug.LogWarning("On Destroy no EventManager.");
                return false;
            }
            NetEvent thisEvent = instance.GetEvent(eventID);
            if (thisEvent != null)
            {
                thisEvent.Invoke(variable);
                return true;
            }
            return false;
        }

    }




}