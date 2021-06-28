namespace Lex
{
    using System;
    using UnityEngine;

    public class LexNetwork_MessageHandler
    {
        public readonly static string NET_DELIM = "#";
        public readonly static string NET_SIG = "LEX";
        LexNetwork_CallbackHandler callbackHandler = new LexNetwork_CallbackHandler();

        public void HandleMessage(string str)
        {
            LexNetworkMessage netMessage = new LexNetworkMessage();
            netMessage.Split(str);
            while (netMessage.HasNext())
            {

                string signature = netMessage.GetNext();
                bool isMyPacket = (signature == NET_SIG);
                if (!isMyPacket) continue;
   /*             try
                {*/
                 //   Debug.LogWarning("처리중: " + netMessage.Peek());
                    int lengthOfMessages = Int32.Parse(netMessage.GetNext());
                    int sentActorNumber = Int32.Parse(netMessage.GetNext());
                    MessageInfo messageInfo = (MessageInfo)Int32.Parse(netMessage.GetNext());
                    if (messageInfo != MessageInfo.SyncVar) {
                        Debug.LogWarning(netMessage.PrintOut(messageInfo));
                    }
                    switch (messageInfo)
                    {
                        case MessageInfo.RPC:
                            ParseRPC(sentActorNumber, netMessage);
                            break;
                        case MessageInfo.SyncVar:
                            ParseSyncVar(sentActorNumber, netMessage);
                            break;
                        case MessageInfo.Chat:
                            ParseChat(sentActorNumber, netMessage);
                            break;
                        case MessageInfo.Instantiate:
                            ParseInstantiate(sentActorNumber, netMessage);
                            break;
                        case MessageInfo.Destroy:
                            ParseDestroy(sentActorNumber, netMessage);
                            break;
                        case MessageInfo.SetHash:
                            ParseSetHash(sentActorNumber, netMessage);
                            break;
                        case MessageInfo.ServerRequest:
                            break;
                        case MessageInfo.ServerCallbacks:
                            callbackHandler.ParseCallback(sentActorNumber, netMessage);
                            break;
                    }
/*                }
                catch (Exception e)
                {
                    Debug.LogWarning("Handle message fatal error");
                    Debug.LogWarning(e.Message);
                    Debug.LogWarning(e.StackTrace);
                    Debug.LogWarning(netMessage.Peek());
                }*/


            }


        }



        private void ParseSetHash(int sentActorNumber, LexNetworkMessage netMessage)
        {
            if (sentActorNumber == LexNetwork.LocalPlayer.actorID) return;
            string targetHashID = (netMessage.GetNext()); //0 = Room,
            int numHash = Int32.Parse(netMessage.GetNext());
            LexHashTable hash = new LexHashTable();
            for (int i = 0; i < numHash; i++)
            {
                int key = Int32.Parse(netMessage.GetNext());
                string typename = netMessage.GetNext();
                string datainfo = netMessage.GetNext();
                object value = ParserAParameter(typename, datainfo);
                hash.Add(key, value);
            }

            if (targetHashID == "0")
            {
                LexNetwork.CustomProperties.UpdateProperties(hash);
            }
            else
            {
                LexNetwork.GetPlayerByID(targetHashID.ToString()).CustomProperties.UpdateProperties(hash);
            }
            NetworkEventManager.TriggerEvent(LexCallback.HashChanged, new NetEventObject() { stringObj = targetHashID, objData = hash });
        }

        private void ParseDestroy(int sentActorNumber, LexNetworkMessage netMessage)
        {
            if (sentActorNumber == LexNetwork.LocalPlayer.actorID) return;
            int targetViewID = Int32.Parse(netMessage.GetNext());
            LexView lv = LexViewManager.GetViewByID(targetViewID);
            LexViewManager.ReleaseViewID(lv);

        }

        private void ParseInstantiate(int sentActorNumber, LexNetworkMessage netMessage)
        {
            //
//LocalPlayer.actorID, (int)MessageInfo.Instantiate, param.viewID, param.ownerID, param.prefabName, param.isRoomView, position, quaternion);
            if (sentActorNumber == LexNetwork.LocalPlayer.actorID) return;
            //actorNum, Instantiate [int]viewID [int]ownerID [string]prefabName [flaot,float,float] position [float,float,float]quarternion parentviewID [object[...]] params
            int targetViewID = Int32.Parse(netMessage.GetNext());
            int ownerID = Int32.Parse(netMessage.GetNext());
            string prefabName = netMessage.GetNext();
            bool isRoomView = bool.Parse(netMessage.GetNext());
            Vector3 position = StringToVector3(netMessage.GetNext());
            Quaternion quaternion = StringToQuarternion(netMessage.GetNext());
            int numParams = Int32.Parse(netMessage.GetNext());
            object[] iparam = null;
            if (numParams > 0)
            {
                iparam = ParseParametersByString(numParams, netMessage);
            }
            NetworkInstantiateParameter netParam = new NetworkInstantiateParameter(targetViewID, prefabName, ownerID, sentActorNumber, isRoomView, iparam);
            NetObjectPool.PollObject(position, quaternion, netParam);
            //Params
        }

        private void ParseChat(int sentActorNumber, LexNetworkMessage netMessage)
        {
            if (sentActorNumber == LexNetwork.LocalPlayer.actorID) return;
            //actorNum, Chat [string]chat message (needs cleansing)
            string message = netMessage.GetNext();
            message = message.Replace("^^", NET_DELIM);
            NetworkEventManager.TriggerEvent(LexCallback.ChatReceived, new NetEventObject() { stringObj = message });
        }

        private void ParseSyncVar(int sentActorNumber, LexNetworkMessage netMessage)
        {
            //actorNum, SyncVar [int]viewID  [int]numparam ,[object[,,,]] params
            int targetViewID = Int32.Parse(netMessage.GetNext());
            int numParams = Int32.Parse(netMessage.GetNext());
            Debug.Assert(numParams != 0, "Syncing what?");
            var param = ParseParametersByString(numParams, netMessage);
            if (sentActorNumber == LexNetwork.LocalPlayer.actorID) return;
            LexNetwork.instance.SyncVar_Receive(targetViewID, param);
        }

        //  actorNum, RPC[int] viewID[string] FunctionName[object[...]]params
        private void ParseRPC(int sentActorNumber, LexNetworkMessage netMessage)
        {
            int targetViewID = Int32.Parse(netMessage.GetNext());
            string functionName = netMessage.GetNext();
            int numParams = Int32.Parse(netMessage.GetNext());
            if (numParams == 0)
            {
                LexNetwork.instance.RPC_Receive(targetViewID, functionName);
            }
            else
            {
                // var param = ParseParameters(numParams, netMessage);
                var param = ParseParametersByString(numParams, netMessage);
                LexNetwork.instance.RPC_Receive(targetViewID, functionName, param);
            }
        }
        object[] ParseParametersByString(int numParams, LexNetworkMessage netMessage)
        {

            object[] param = new object[numParams];
            for (int i = 0; i < numParams; i++)
            {
                string typeName = netMessage.GetNext();
                string dataInfo = netMessage.GetNext();
                param[i] = ParserAParameter(typeName, dataInfo);
            }
            return param;
        }
        internal static object ParserAParameter(string typename, string dataInfo)
        {            
            switch (typename)
            {
                case "NULL":
                    return null;
                case nameof(Boolean):
                    return bool.Parse(dataInfo);
                case nameof(Int32):
                    return Int32.Parse(dataInfo);
                case nameof(String):
                    return dataInfo;
                case nameof(Double):
                    return double.Parse(dataInfo);
                case nameof(Vector3):
                    return StringToVector3(dataInfo);
                case nameof(Quaternion):
                    return StringToQuarternion(dataInfo);
                case nameof(Single):
                    return float.Parse(dataInfo);
                default:
                    Debug.LogWarning("Unsupported type");
                    return dataInfo;
            }
        }

        private static Vector3 StringToVector3(string sVector)
        {

            int start = sVector.IndexOf('(') + 1;
            int end = sVector.IndexOf(')');
            sVector = sVector.Substring(start, end - start);

            // split the items
            string[] sArray = sVector.Split(',');
            Vector3 result = new Vector3(
                float.Parse(sArray[0]),
                float.Parse(sArray[1]),
                float.Parse(sArray[2]));

            return result;
        }
        private static Quaternion StringToQuarternion(string sVector)
        {
            int start = sVector.IndexOf('(') + 1;
            int end = sVector.IndexOf(')');
            sVector = sVector.Substring(start, end - start);
            // split the items
            string[] sArray = sVector.Split(',');

            // store as a Vector3
            Quaternion result = new Quaternion(
                float.Parse(sArray[0]),
                float.Parse(sArray[1]),
                float.Parse(sArray[2]),
                float.Parse(sArray[3])
                );

            return result;
        }

        static string floatFormat = "0.00000000";
        internal static string Vector3ToString(Vector3 v)
        {
            return string.Format("({0},{1},{2})", v.x.ToString(floatFormat), v.y.ToString(floatFormat), v.z.ToString(floatFormat));
        }
        internal static string QuarternionToString(Quaternion q)
        {
            return string.Format("({0},{1},{2},{3})", q.x.ToString(floatFormat), q.y.ToString(floatFormat), q.z.ToString(floatFormat), q.w.ToString(floatFormat));
        }
    }
}