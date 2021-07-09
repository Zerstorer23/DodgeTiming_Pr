using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using static Lex.LexNetwork_MessageHandler;


namespace Lex
{
    public partial class LexNetwork : MonoBehaviourLexCallbacks
    {
        public void CheckConnection(InputField inputField)
        {
            string ip = inputField.text;
            networkConnector.ipAddress = ip;
            ConnectUsingSettings();
        }
        public void CheckSQL() {
            LexDBManager.RequestNetworkDB("Kills");
        }

        public void DoLogIn()
        {
            LexDBManager.LogInToNetworkDB("TEST", "asd");
        }
    }
}

