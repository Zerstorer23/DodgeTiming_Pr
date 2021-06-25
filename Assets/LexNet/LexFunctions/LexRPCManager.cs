
namespace Lex
{
    using Lex;
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Reflection;
    using UnityEngine;

    public partial class LexNetwork
    {
        [LexRPC]
        public void RPC_Send(LexView lv, string functionName, params object[] parameters)
        {
            LexNetworkMessage netMessage = new LexNetworkMessage(LocalPlayer.actorID, (int)MessageInfo.RPC, lv.ViewID, functionName);
            netMessage.EncodeParameters(parameters);
            Run_RPC(lv, functionName, parameters);
            networkConnector.EnqueueAMessage(netMessage);
        }

        public void RPC_Receive(int viewID, string functionName, params object[] parameters)
        {
            if (debugLexNet)
            {
                foreach (object obj in parameters) Debug.Log(obj);
            }

            LexView lv = LexViewManager.GetViewByID(viewID);
            if (!lv) return;
            Run_RPC(lv, functionName, parameters);
        }
        public void Run_RPC(LexView lv, string functionName, object[] parameters)
        {

            if (!lv.cachedRPCs.ContainsKey(functionName))
            {
                Debug.LogWarning(string.Format("No such function [{0}] found in view{1}", functionName, lv.ViewID));
                return;
            }
            lv.cachedRPCs[functionName].Invoke(parameters);
            return;
        }


    }
    public struct RPC_Info
    {
        public MonoBehaviour monob;
        public MethodInfo mInfo;
        public RPC_Info(MonoBehaviour mono, MethodInfo mInfo)
        {
            this.monob = mono;
            this.mInfo = mInfo;
        }
        public object Invoke(object[] parameters)
        {
            return mInfo.Invoke((object)monob, parameters);
        }

    }
}