using Lex;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.Reflection;

namespace Lex
{
    public class LexView : MonoBehaviour
    {
      //  public ControllerType controllerType = ControllerType.Human;
        public string uid;
        public LexPlayer Owner
        {
            get { return prOwner; }
            private set { prOwner = value; }
        }
        

        internal Dictionary<string, RPC_Info> cachedRPCs = new Dictionary<string, RPC_Info>();

        [SerializeField] [ReadOnly] private int viewID = -1;
        [SerializeField] [ReadOnly] private ViewType prViewType = ViewType.SceneView;
        [SerializeField] [ReadOnly] private bool __IsMine = true;
        [SerializeField] [ReadOnly] private LexPlayer prOwner = null;
        
        public string objTag { get; private set; }

        public int ViewID
        {
            get { return viewID; }
            private set
            {
                viewID = value;
                //            Debug.LogWarning("Change " + gameObject.name + " =" + value);
            }
        }
        public int ownerActorNr;// InstantiateObject, room이면 마스터id
        public int creatorActorNr;
        public bool IsMine
        {
            get { return  __IsMine; }
            private set { __IsMine = value; }
        }//씬오브젝트, 개인 오브젝트, 마스터일경우 RoomObject도

        public ViewType viewType
        {
            get { return prViewType; }
            private set { prViewType = value; }
        }
        public bool IsRoomView { get { return prViewType == ViewType.RoomView ; } }// 룸오브젝트, 씬오브젝트/ 마스터만 컨트롤
        public bool IsSceneView { get { return prViewType == ViewType.SceneView ; } } // 룸오브젝트, 씬오브젝트/ 마스터만 컨트롤

        public MonoBehaviourLex[] RpcMonoBehaviours { get; private set; }


        public object[] InstantiationData
        {
            get; private set;
        }


        public static LexView Get(Component component)
        {
            return component.gameObject.GetComponent<LexView>();
        }

        public static LexView Get(GameObject gameObj)
        {
            return gameObj.GetComponent<LexView>();
        }
        public void RefreshRpcMonoBehaviourCache()
        {
            this.RpcMonoBehaviours = this.gameObject.GetComponents<MonoBehaviourLex>();
            foreach (MonoBehaviourLex mono in RpcMonoBehaviours)
            {
                Type type = mono.GetType();
                //a     Debug.Log("Searching " + type.Name);
                var functions = type.GetMethods(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
                foreach (var function in functions)
                {
                    if (function.GetCustomAttribute(typeof(LexRPC)) == null) continue;
                    //  Debug.Log("Found function " + function.Name + " in " + type.Name);
                    RPC_Info rpcInfo = new RPC_Info(mono, function);
                    cachedRPCs.Add(function.Name, rpcInfo);
                }
            }
        }
        //c# 전처리
        /*
         씬 :IsMine = true, 
             IsRoomView = false,
             IsSceneView = true,
             Owner = localPlayer

        룸: IsMine = MasterClient
             IsRoomView = true,
             IsSceneView = false,
             Owner  = MasterClient
        개인: IsMine = Creator
            IsRoomView = false
            IsSceneview = false
             Owner = LocalPlayer


        씬뷰 ViewID =>0~ 0???      //ViewIDManager생성 (FindView후 순서대로 대입)
        룸뷰 ViewID =>0??? ~ 09999 //ViewIDManager생성 마지막대입부터 카운터 시작
                                    //ViewID카운터는 모두가 동기화 필요
                                    //마스터클라이언트 ->자기 데이터기반 다음view전송
                                    ->RoomInstantiateReceive시 각자 자기 데이터 업데이트

        개인뷰 => OwnerID ~ n9999 //카운터 각자
         */
        //카운터는 HashSet형으로 관리후 삭제시 remove
        //1~9999 Queue에 등록
        //remove된iD queue에 추가
        //dycjdgks queue에서 삭제
        public MonobehaviourLexSerialised serializedView;
        private void Awake()
        {
            //1. 시점 잡고
            //2. 에디터에서 바꾸면 이제 프로그램의 권한을 넘어가버림
            //3. 수정을 아예 안한 오브젝트는 플레이시 유지되지
            // ->수정을 막는다.
            // 
            serializedView = GetComponent<MonobehaviourLexSerialised>();//TODO 여러개일수 있음
            RefreshRpcMonoBehaviourCache();
            if (serializedView) serializedView.UpdateOwnership(); //SceneView case
        }
        private void Start()
        {

        }
        public void ReceiveSerializedVariable(params object[] parameters)
        {
            if (serializedView == null)
            {
                Debug.LogError("No sync view!!!");
                return;
            }
            serializedView.OnSyncView(parameters);
        }


        public void SetInformation(NetworkInstantiateParameter param)
        {
            //todo sceneview는 이게 올수도 안올수도 있음
            this.InstantiationData = param.data;
            this.ViewID = param.viewID;
    
            this.objTag = param.prefabName;
            this.ownerActorNr = param.ownerID;
            this.creatorActorNr = param.creatorID;
            this.viewType = (param.isRoomView) ? ViewType.RoomView : ViewType.PrivateView;
            this.uid = param.ownerID.ToString();
            if (IsRoomView)
            {
                IsMine = LexNetwork.IsMasterClient;
                Owner = LexNetwork.MasterClient;
            }
            else
            {
                IsMine = LexNetwork.LocalPlayer.actorID == param.ownerID;
                if (IsMine)
                {
                    Owner = LexNetwork.LocalPlayer;
                }
                else
                {
                    Owner = LexNetwork.GetPlayerByID(uid);
                }
            }
            if (serializedView) serializedView.UpdateOwnership();
            LexViewManager.AddViewtoDictionary(this);
        }
        public void UpdateOwnership()
        {
            if (IsRoomView)
            {
                IsMine = LexNetwork.IsMasterClient;
                Owner = LexNetwork.MasterClient;
                if (serializedView) serializedView.UpdateOwnership();
            }
        }

#if UNITY_EDITOR
        public void SetViewID(int newID)
        {
            ViewID = newID;
            EditorUtility.SetDirty(this);
        }
#endif
        /*    private void OnValidate()
            {
                Debug.Log(gameObject.name+" OnValidate called ");
                if (EditorApplication.isPlayingOrWillChangePlaymode || Application.isPlaying) return;
                if (IsPrefab(gameObject)) return;
                    //if (Application.platform == RuntimePlatform.WindowsEditor) return;
                    if (initialised) return;
                initialised = true;
                ViewID =  LexViewManager.RequestSceneViewID();
                Debug.Log(gameObject.name + " OnValidate passed " + ViewID);
                IsMine = true;
                IsSceneView = true;
                //TODO SceneView add to dictionary on start
                //세거나 저장하거나..
                //
                //매번 세서 순서대로 번호를 붙이는게
            }*/

        private bool GetIsMine()
        {
            if (IsSceneView) return true;
            if (IsRoomView) return LexNetwork.IsMasterClient;
            return creatorActorNr == LexNetwork.LocalPlayer.actorID;
        }
        public void RPC(string methodName,  params object[] parameters)
        {
             LexNetwork.instance.RPC_Send(this, methodName, parameters);
        }

    }
    [Serializable]
    public enum ViewType
    {
        SceneView, RoomView, PrivateView
    }

}