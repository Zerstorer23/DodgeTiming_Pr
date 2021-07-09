using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Lex
{
    using static LexNetwork;
    public class LexDBManager
    {
        /*
         Hash구조와 통일
         */
        Dictionary<string, object> networkDatabase = new Dictionary<string, object>();
        Dictionary<string, Dictionary<string, int>> Leaderboard = new Dictionary<string, Dictionary<string, int>>();
        Dictionary<string, bool> Achievements = new Dictionary<string, bool>();

        public static string DB_UID = "TEST";
        public static bool loggedIntoDB = false;

        internal void HandleDBReceive(LexNetworkMessage netMessage)
        {
            LexDBTable table = (LexDBTable)Int32.Parse(netMessage.GetNext());
            LexDBcode code = (LexDBcode)Int32.Parse(netMessage.GetNext());
            if (code == LexDBcode.LogIn)
            {
                LexDebug.LogWarning("Loggedinto DB");
                HandleLogIn(netMessage);
            }
            else if (code == LexDBcode.Get)
            {
                switch (table)
                {
                    case LexDBTable.Statistics:
                        Handle_ReceiveStatistics(netMessage);
                        break;
                    case LexDBTable.Leaderboards:
                        Handle_ReceiveLeaderboard(netMessage);
                        break;
                    case LexDBTable.Achievements:
                        Handle_ReceiveAchievements(netMessage);
                        break;
                }
            }
        }

        public void HandleLogIn(LexNetworkMessage netMessage)
        {
            int success = Int32.Parse(netMessage.GetNext());
            loggedIntoDB = success > 0;
            NetworkEventManager.TriggerEvent(LexCallback.DB_Received, new NetEventObject() { stringObj = "LOGIN", objData = loggedIntoDB });
      
        }
        public void Handle_ReceiveStatistics(LexNetworkMessage netMessage)
        {
            string key = netMessage.GetNext();
            int value = netMessage.GetNextInt();

            //Update with higher value
            UpdateHigherValue(key, value);
            NetworkEventManager.TriggerEvent(LexCallback.DB_Received,
            new NetEventObject()
            {
                intObj = (int)LexDBTable.Statistics,
                stringObj = key,
                objData = value
            });
        }

        public void Handle_ReceiveLeaderboard(LexNetworkMessage netMessage)
        {

        }
        public void Handle_ReceiveAchievements(LexNetworkMessage netMessage)
        {

        }

        public void UpdateValue(string key, object value)
        {
            if (networkDatabase.ContainsKey(key))
            {
                networkDatabase[key] = value;
            }
            else
            {
                networkDatabase.Add(key, value);
            }
        }
        public void UpdateHigherValue(string key, int value)
        {
            if (networkDatabase.ContainsKey(key))
            {
                if ((int)networkDatabase[key] < value)
                {
                    networkDatabase[key] = value;
                }
                else
                {
                    SetNetworkDB(LexDBcode.Set, key, value);
                }
            }
            else
            {
                networkDatabase.Add(key, value);
            }
        }



        public static void LogInToNetworkDB(string id, string password)
        {
            DB_UID = id;
            LexNetworkMessage message = new LexNetworkMessage(LocalPlayer.actorID,
                   (int)MessageInfo.ServerRequest,
                   (int)LexRequest.DBReference,
                   (int)LexDBTable.Statistics,
                   (int)LexDBcode.LogIn,
                   id, password);
            networkConnector.EnqueueAMessage(message);
        }
        public static void SetNetworkDB(LexDBcode code, string key, object value)
        {
            LexNetworkMessage message = new LexNetworkMessage(LocalPlayer.actorID,
                (int)MessageInfo.ServerRequest,
                (int)LexRequest.DBReference,
                 (int)LexDBTable.Statistics,
                (int)code,
                DB_UID
                );
            /*
             SentID, ServerRequest, DBReferece/  DBCOde:Set,Get,Update,SQL / UID /  [key, type, val] or [SQL]
             */
            switch (code)
            {
                case LexDBcode.Set:
                case LexDBcode.Append:
                case LexDBcode.Get:
                    message.Add(key);
                    message.Add(value);
                    break;
                case LexDBcode.SQL:
                case LexDBcode.LogIn:
                    message.Add(key);
                    break;
            }
            networkConnector.EnqueueAMessage(message);
        }
        public static void RequestNetworkDB(string key)
        {
            LexNetworkMessage message = new LexNetworkMessage(LocalPlayer.actorID,
             (int)MessageInfo.ServerRequest,
             (int)LexRequest.DBReference,
              (int)LexDBTable.Statistics,
             (int)LexDBcode.Get,
             DB_UID,key
             );
            networkConnector.EnqueueAMessage(message);
        }

        public static void AddEventStat(string key, int value)
        {
            LexNetworkMessage message = new LexNetworkMessage(LocalPlayer.actorID,
                 (int)MessageInfo.ServerRequest,
                 (int)LexRequest.DBReference,
                 (int)LexDBTable.Events,
                 (int)LexDBcode.Append,
                 DB_UID, key, value
                 );
            networkConnector.EnqueueAMessage(message);
        }


    }


}
