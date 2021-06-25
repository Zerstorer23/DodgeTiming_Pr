namespace Lex
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.IO;
    using System.Net;
    using System.Net.Sockets;
    using System.Text;
    using System.Threading;

    using UnityEngine;

    public class LexNetworkConnection
    {
        string ipAddress = "127.0.0.1";
        int portNumber = 9000;
        static int BUFFER = 32 * 1024;
        private static Mutex sendMutex = new Mutex();
        private static Mutex receiveMutex = new Mutex();

        Queue<string> receivedQueue = new Queue<string>();
        Queue<LexNetworkMessage> sendQueue = new Queue<LexNetworkMessage>();
        Thread listenThread;
        Thread sendThread;
        Socket mySocket;
        LexNetwork_MessageHandler messageHandler = new LexNetwork_MessageHandler();


        public bool stayConnected = true;

        public LexNetworkConnection()
        {
        }

        // Start is called before the first frame update

        public bool Connect()
        {
            mySocket = new Socket(
                  AddressFamily.InterNetwork,
                  SocketType.Stream,
                  ProtocolType.Tcp
                  );
            mySocket.ReceiveBufferSize = 32 * 1024;
            IPAddress addr = IPAddress.Parse(ipAddress);
            IPEndPoint iep = new IPEndPoint(addr, portNumber);
            try
            {
                Debug.Log("Connecting...");
                mySocket.Connect(iep);
                stayConnected = true;
            }
            catch (Exception e)
            {
                Debug.Log(e.Message);
                Debug.Log(e.StackTrace);
                return false;
            }
            Debug.Log("Connection made!");

            listenThread = new Thread(new ThreadStart(ListenMessage));
            listenThread.IsBackground = true;
            listenThread.Start();
            Debug.Log("Listening...");

            sendThread = new Thread(new ThreadStart(SendMessage));
            sendThread.IsBackground = true;
            sendThread.Start();
            Debug.Log("Writing...");
            return true;
        }
        public void SendMessage()
        {//ㅁㄴㅇㄹㄴ ㅎㅇㅀㅇㅀ
         //program1-
         //[20길]3#3#3#123123ㄹ3//20
            while (stayConnected)
            {
                //MUTEX
                sendMutex.WaitOne();
                while (sendQueue.Count > 0 && stayConnected)
                {
                    string message = MergeMessages();
                    SendAMessage(message);
                    //무한루프에 주의        
                }
                sendMutex.ReleaseMutex();
                //MUTEX
            }
        }
        public void EnqueueAMessage(LexNetworkMessage netMessage)
        {
            //MUTEX
            sendMutex.WaitOne();
            sendQueue.Enqueue(netMessage);
            sendMutex.ReleaseMutex();
            //MUTEX
        }
        string MergeMessages()
        {
            string message = sendQueue.Dequeue().Build();
            while (sendQueue.Count > 0)
            {
                string nextMessage = sendQueue.Peek().Build();
                if ((message.Length + nextMessage.Length + 1) < BUFFER)
                {
                    message += nextMessage;
                    sendQueue.Dequeue();
                }
            }
            return message;
        }


        private void SendAMessage(string str)
        {
            try
            {
                byte[] byData = Encoding.UTF8.GetBytes(str + '\0');
                mySocket.Send(byData);
            }
            catch (Exception e)
            {
                Debug.Log(e.Message);
                Debug.Log(e.StackTrace);
            }
        }

        private void ListenMessage()
        {
            byte[] packet = new byte[BUFFER];
            while (stayConnected)
            {
                int received;
                try
                {
                    received = mySocket.Receive(packet);
                }
                catch (Exception e)
                {
                    Debug.LogWarning(e.Message);
                    Debug.LogWarning(e.StackTrace);
                    stayConnected = false;
                    receiveMutex.WaitOne();
                    receivedQueue.Enqueue(FlagDisconnected());
                    receiveMutex.ReleaseMutex();
                    return;
                }
                string str = Encoding.UTF8.GetString(packet, 0, received);
             //   Debug.Log("string length " + str.Length);
             //   LexNetwork.PrintStringToCode(str);
                receiveMutex.WaitOne();
                receivedQueue.Enqueue(str);
                receiveMutex.ReleaseMutex();
                // Debug.Log(receivedQueue.Count + "/ 수신한 메시지:" + str);

            }

        }
        string FlagDisconnected()
        {
            LexNetworkMessage netMessage = new LexNetworkMessage();
            //actorID , MessageInfo , callbackType, params
            netMessage.Add("0");
            netMessage.Add(MessageInfo.ServerCallbacks);
            netMessage.Add(LexCallback.Disconnected);
            return netMessage.Build();
        }
        //MainThread에서만 GameObject조작가능
        //그래서queue에 저장후  따로 Update에서 호출하도록함
        public void DequeueReceivedBuffer()
        {
            //mutex
            receiveMutex.WaitOne();
            while (receivedQueue.Count > 0)
            {
                string message = receivedQueue.Dequeue();
                messageHandler.HandleMessage(message);//
            }//분리하는게 좋을ㄷㅡㅅ
            receiveMutex.ReleaseMutex();
        }

        public void Disconnect()
        {
            stayConnected = false;
            mySocket.Close();//소켓 닫기
        }


    }


    public enum DataType
    {
        STRING, INT, DOUBLE, FLOAT, VECTOR3, QUARTERNION
    }
}