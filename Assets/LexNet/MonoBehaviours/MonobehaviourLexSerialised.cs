
namespace Lex
{
    using Lex;
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;

    public abstract class MonobehaviourLexSerialised :
        MonoBehaviourLex
    {
        protected bool IsWriting { get => lexView.IsSceneView? lexView.IsMine && LexNetwork.IsMasterClient : lexView.IsMine; }
        internal protected LexStream stream = new LexStream();

        //실행순서를 어떻게 약속시켜야하는지. start에 밀어도 safe인지
        //event로 관리할수도 있을거같음
        //todo lv init이 끝나야됨.

        public abstract void OnSyncView(LexStream stream);
        /*
         1. Update문에 WriteSync 있어야함
         2. isWriting안에 PushSync있어야함
         */


        protected void WriteSync()
        {
            if (IsWriting)
            {
                stream.Reset();
                OnSyncView(stream);
                if (stream.HasData()) {
                    PushSync(stream);
                }
            }

        }

        void PushSync(LexStream stream)
        {
            object[] parameters = stream.Serialise();
            LexNetwork.instance.SyncVar_Send(lexView, parameters);
        }


    }


    public class LexStream {
        List<object> stream = new List<object>();
        int receiveIndex = 0;
        public void SendNext(object o) {
            stream.Add(o);
        }
        public object ReceiveNext() {
            return stream[receiveIndex++];
        }   
        public T ReceiveNext<T>() {
            return (T) stream[receiveIndex++];
        }
        public void Reset() {
            stream.Clear();
            receiveIndex = 0;
        }
        public void Imbue(object[] data) {
            Reset();
            foreach (object d in data) stream.Add(d);
        }
        public bool HasData() {
            return stream.Count != 0;
        }

        internal object[] Serialise()
        {
            return stream.ToArray();
        }
    }

}
