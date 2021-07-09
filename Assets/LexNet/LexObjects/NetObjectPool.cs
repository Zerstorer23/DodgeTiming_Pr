using Lex;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NetObjectPool : MonoBehaviour
{
    private static NetObjectPool prInstance;

    public static NetObjectPool instance
    {
        get
        {
            if (!prInstance)
            {
                prInstance = FindObjectOfType<NetObjectPool>();
                if (!prInstance)
                {
                    Debug.LogWarning("There needs to be one active NetObjectPool script on a GameObject in your scene.");
                }
                else
                {
                    prInstance.Init();
                }
            }

            return prInstance;
        }
    }
    private Dictionary<string, Queue<LexView>> objectLibrary;
    void Init()
    {
        if (objectLibrary == null)
        {
            objectLibrary = new Dictionary<string, Queue<LexView>>();
        }
    }

    public static void SaveObject(string tag, LexView go)
    {
        if (!instance.objectLibrary.ContainsKey(tag))
        {
            instance.objectLibrary.Add(tag, new Queue<LexView>());
        }
        go.gameObject.SetActive(false);
        go.transform.SetParent(prInstance.transform,true);
        go.gameObject.hideFlags = HideFlags.HideInHierarchy;
        instance.objectLibrary[tag].Enqueue(go);
    }
    /*
     TODO
    특이사항
    부모 tranform안바꾸면 나중에 하위오브젝트가 같이 활성화됨

     */
    public static LexView PollObject(Vector3 position, Quaternion quaternion, NetworkInstantiateParameter param)
    {
        LexView lv;
        GameObject go;
        string prefabName = param.prefabName;
        if (!instance.objectLibrary.ContainsKey(prefabName) ||
                instance.objectLibrary[prefabName].Count <= 0)
        {
            GameObject prefab = (GameObject)Resources.Load(prefabName);
            prefab.SetActive(false);
            go = Instantiate(prefab, position, quaternion);
            lv = go.GetComponent<LexView>();
            lv.SetInformation(param);
            prefab.SetActive(true);
        }
        else
        {
            lv = instance.objectLibrary[prefabName].Dequeue();
            lv.gameObject.transform.position = position;
            lv.gameObject.transform.rotation = quaternion;
            lv.SetInformation(param);
            go = lv.gameObject;
            //  Debug.LogWarning("Poll " + lv.gameObject.name + " / " + lv.gameObject.activeSelf + " / " + lv.ViewID + " / pos "+lv.gameObject.transform.position);
        }
        go.transform.SetParent(null, true);
        go.SetActive(true);
        go.hideFlags = HideFlags.None;
        return lv;

    }
    public static void ResetPool() {
        instance.objectLibrary.Clear();
    }

}
