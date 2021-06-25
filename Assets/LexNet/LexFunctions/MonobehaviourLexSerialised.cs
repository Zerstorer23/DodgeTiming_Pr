
namespace Lex
{
    using Lex;
    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;

    public abstract class MonobehaviourLexSerialised :
        MonoBehaviourLex
#if !USE_LEX
    , IPunObservable
#endif

    {
        protected bool isWriting = false;
        public void UpdateOwnership()
        {
            isWriting = lexView.IsMine;
            Debug.LogWarning("Update ownership " + isWriting);
        }
        //실행순서를 어떻게 약속시켜야하는지. start에 밀어도 safe인지
        //event로 관리할수도 있을거같음
        //todo lv init이 끝나야됨.
        public abstract void OnSyncView(params object[] parameters);
        protected void WriteSync()
        {
#if USE_LEX
            if (isWriting)
            {
                OnSyncView(null);
            }
#endif
        }

        public void PushSync(params object[] parameters)
        {
            LexNetwork.instance.SyncVar_Send(lexView, parameters);
        }

#if !USE_LEX
        public virtual void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
        {

        }
#endif
    }
}