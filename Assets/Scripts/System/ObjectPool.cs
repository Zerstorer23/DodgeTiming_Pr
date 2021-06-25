using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObjectPool : MonoBehaviour
{
    private static ObjectPool prObjectPool;

    public static ObjectPool instance
    {
        get
        {
            if (!prObjectPool)
            {
                prObjectPool = FindObjectOfType<ObjectPool>();
                if (!prObjectPool)
                {
                    //  prEvManager = Instantiate(EventManager) as EventManager;

                    // prEvManager = FindObjectOfType<GameSession>().GetComponent<EventManager>();
                     Debug.LogWarning("There needs to be one active EventManger script on a GameObject in your scene.");
                }
                else
                {
                    prObjectPool.Init();
                }
            }

            return prObjectPool;
        }
    }
    private Dictionary<string, Queue<UnityEngine.GameObject>> objectLibrary;
    void Init()
    {

        if (objectLibrary == null)
        {
            objectLibrary = new Dictionary<string, Queue<UnityEngine.GameObject>>();
        }
    }

    public static void SaveObject(string tag , UnityEngine.GameObject obj) {
        if (!instance.objectLibrary.ContainsKey(tag)) {
            instance.objectLibrary.Add(tag, new Queue<UnityEngine.GameObject>());
        }
        if (!obj.activeSelf) {
            Debug.LogWarning("Saving inactive obj? " + tag);
        }
        obj.SetActive(false);
        instance.objectLibrary[tag].Enqueue(obj);
    }
    public static UnityEngine.GameObject PollObject(string tag, Vector3 position, Quaternion angle) {
        if (!instance.objectLibrary.ContainsKey(tag)||
                instance.objectLibrary[tag].Count <= 0) {
            return null;
        }
        UnityEngine.GameObject obj = instance.objectLibrary[tag].Dequeue();
        obj.SetActive(true);
        obj.transform.position = position;
        obj.transform.rotation = angle;
        return obj;
    }

}
