using System;

namespace Lex
{
    public class MonoBehaviourLexCallbacks :
        MonoBehaviourLex
    {
        private void OnEnable()
        {
            NetworkEventManager.StartListening(LexCallback.PlayerDisconnected, OnPlayerDisconnected);
            NetworkEventManager.StartListening(LexCallback.PlayerJoined, OnPlayerConnected);
            NetworkEventManager.StartListening(LexCallback.OnLocalPlayerJoined, OnLocalPlayerJoined);
            NetworkEventManager.StartListening(LexCallback.MasterClientChanged, OnMasterClientSwitched);
            NetworkEventManager.StartListening(LexCallback.HashChanged, OnHashChanged);
            NetworkEventManager.StartListening(LexCallback.Disconnected, OnDisconnected);
            NetworkEventManager.StartListening(LexCallback.ChatReceived, OnChatReceived);
            NetworkEventManager.StartListening(LexCallback.DB_Received, OnDBReceived);
        }



        private void OnDisable()
        {
            NetworkEventManager.StopListening(LexCallback.PlayerDisconnected, OnPlayerDisconnected);
            NetworkEventManager.StopListening(LexCallback.PlayerJoined, OnPlayerConnected);
            NetworkEventManager.StopListening(LexCallback.OnLocalPlayerJoined, OnLocalPlayerJoined);
            NetworkEventManager.StopListening(LexCallback.MasterClientChanged, OnMasterClientSwitched);
            NetworkEventManager.StopListening(LexCallback.Disconnected, OnDisconnected);
            NetworkEventManager.StopListening(LexCallback.HashChanged, OnHashChanged);
            NetworkEventManager.StopListening(LexCallback.ChatReceived, OnChatReceived);
            NetworkEventManager.StopListening(LexCallback.DB_Received, OnDBReceived);
        }
        private void OnDisconnected(NetEventObject arg0)
        {
            OnDisconnected();
        }
         private void OnHashChanged(NetEventObject arg0)
        {
            //        NetworkEventManager.TriggerEvent(LexCallback.HashChanged, new NetEventObject(LexCallback.HashChanged) { intObj = targetHashID, hashKey = key, hashValue = value });
            string target = arg0.stringObj;
            LexHashTable hashChanged = (LexHashTable)arg0.objData;
            if (target == "0")
            {
                OnRoomSettingsChanged(hashChanged);
            }
            else
            {
                OnPlayerSettingsChanged(LexNetwork.GetPlayerByID(target), hashChanged);
            }
        }
        private void OnMasterClientSwitched(NetEventObject arg0)
        {
            OnMasterClientSwitched(arg0.intObj);
        }
        private void OnPlayerConnected(NetEventObject arg0)
        {
            OnPlayerEnteredRoom((LexPlayer)arg0.objData);
        }
        private void OnPlayerDisconnected(NetEventObject arg0)
        {
            OnPlayerLeftRoom((LexPlayer)arg0.objData);
        }
        private void OnLocalPlayerJoined(NetEventObject arg0)
        {
            OnJoinedRoom();
        }
        private void OnChatReceived(NetEventObject arg0)
        {
            OnChatReceived(arg0.stringObj);
        }
        private void OnDBReceived(NetEventObject arg0)
        {
            OnDBReceived(arg0.stringObj, arg0.objData);
        }
        public virtual void OnDBReceived(string receivedKey, object receivedValue)
        {

        }
        public virtual void OnChatReceived(string message) { 
        
        }
        public virtual void OnDisconnected()
        {

        }

       
        public virtual void OnRoomSettingsChanged(LexHashTable hashChanged)
        {

        }
        public virtual void OnPlayerSettingsChanged(LexPlayer player, LexHashTable hashChanged)
        {

        }

        public virtual void OnMasterClientSwitched(int newMasterActorNr)
        {
        }
     
        public virtual void OnPlayerEnteredRoom(LexPlayer newPlayer)
        {
        }
       
        public virtual void OnPlayerLeftRoom(LexPlayer newPlayer)
        {

        }
        public virtual void OnJoinedRoom()
        {

        }
}

}