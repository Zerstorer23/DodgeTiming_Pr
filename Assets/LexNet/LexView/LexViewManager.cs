namespace Lex
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;

    using UnityEngine;
    using UnityEngine.UI;
#if UNITY_EDITOR
    using UnityEditor;
#endif


    public class LexViewManager : MonoBehaviour
    {
        private static LexViewManager prInstance;
        private static Dictionary<int, LexView> viewDictionary = new Dictionary<int, LexView>();
        [SerializeField] Text lvText;/*
    private static int privateViewID = 0;
    private static int roomViewID = 0;*/
        private static Queue<int> privateViewID_queue;
        private static Queue<int> roomViewID_queue;

        private static Mutex viewMutex = new Mutex();
        static bool init = false;

        [SerializeField] int nextPrivate = 0;
        [SerializeField] int nextRoom = 0;
        [SerializeField] [ReadOnly] int nextPublicViewID = 0;
        private static int exceededID = 0;

        public static LexViewManager instance
        {
            get
            {
                if (!prInstance)
                {
                    prInstance = FindObjectOfType<LexViewManager>();
                    Init();
                }
                return prInstance;
            }
        }
        private void Awake()
        {
            Init();
        }
        private static void Init()
        {
            if (init) return;
            init = true;
            privateViewID_queue = new Queue<int>();
            roomViewID_queue = new Queue<int>();
            LexView[] sceneViews = Resources.FindObjectsOfTypeAll<LexView>();// FindObjectsOfType<LexView>();
            instance.nextPublicViewID = 0;
            for (int i = 0; i < LexNetwork.MAX_VIEW_IDS; i++)
            {
                privateViewID_queue.Enqueue(i);

                if (i < sceneViews.Length)
                {
                    if (sceneViews[i].ViewID == -1)
                        continue;
                    AddViewtoDictionary(sceneViews[i]);
                    instance.nextPublicViewID++;
                }
                else
                {
                    roomViewID_queue.Enqueue(i);
                }
            }
            Debug.Log("Initialised");
        }

#if UNITY_EDITOR
        internal void EnumerateSceneViews()
        {
            Debug.LogWarning("Called enumeration");
            nextPublicViewID = 0;
            LexView[] sceneViews = Resources.FindObjectsOfTypeAll<LexView>();// FindObjectsOfType<LexView>();
            foreach (var lv in sceneViews)
            {
                if (IsPrefab(lv.gameObject)) continue;
                lv.SetViewID(nextPublicViewID++);
            }
        }
#endif
        public static int RequestPrivateViewID()
        {
            //MUTEX
            int userIDoffset = LexNetwork.LocalPlayer.actorID * 10000;
            if (privateViewID_queue.Count <= 0)
            {
                Debug.LogWarning("Max view id exceeded / " + LexNetwork.MAX_VIEW_IDS);
                return userIDoffset + 100000 + (exceededID++);
            }
            int id = userIDoffset + privateViewID_queue.Dequeue();
            instance.nextPrivate = userIDoffset + privateViewID_queue.Peek();

            //MUTEX
            return id;
        }

        internal static bool CheckKey(int viewID)
        {
            if (viewDictionary.ContainsKey(viewID))
            {
                return true;
            }
            else
            {
                Debug.LogWarning("No view id with " + viewID + " found");
                return false;
            }
        }

        internal static LexView[] GetViewList()
        {
            return viewDictionary.Values.ToArray();
        }

        public static void AddViewtoDictionary(LexView lv)
        {
            viewDictionary.Add(lv.ViewID, lv);
        }
        public static void ReleaseViewID(LexView lv)
        {
            if (lv == null) { return; }
            viewDictionary.Remove(lv.ViewID);
            if (lv.IsRoomView)
            {
                privateViewID_queue.Enqueue(lv.ViewID);
            }
            else
            {
                roomViewID_queue.Enqueue(lv.ViewID);
            }
            NetObjectPool.SaveObject(lv.objTag, lv);
        }
        public static int RequestRoomViewID()
        {
            if (roomViewID_queue.Count <= 0)
            {
                Debug.LogWarning("Max view id exceeded / " + LexNetwork.MAX_VIEW_IDS);
                return 100000 + UnityEngine.Random.Range(0, LexNetwork.MAX_VIEW_IDS);
            }
            int id = roomViewID_queue.Dequeue();
            instance.nextRoom = roomViewID_queue.Peek();

            return id;
        }

        public static LexView GetViewByID(int ID)
        {
            if (CheckKey(ID))
            {
                return viewDictionary[ID];
            }
            else
            {
                return null;
            }
        }
        internal static void WaitMutex()
        {

            viewMutex.WaitOne();
        }
        internal static void ReleaseMutex()
        {
            viewMutex.ReleaseMutex();

        }

#if UNITY_EDITOR
        public static bool IsPrefab(GameObject go)
        {
#if UNITY_2018_3_OR_NEWER
            return UnityEditor.Experimental.SceneManagement.PrefabStageUtility.GetPrefabStage(go) != null || EditorUtility.IsPersistent(go);
#else
                return EditorUtility.IsPersistent(go);
#endif
        }
#endif
    }
}
