using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using UnityEngine;

public class Chat_DC_Client : MonoBehaviour
{
    string ipAddress = "127.0.0.1";
    int portNumber = 9917;
    static int BUFFER = 32 * 1024;
    public readonly static string NET_DELIM = "#";
    public readonly static string NET_SIG = "LEX";
    private static Mutex mutex = new Mutex();
    Queue<string> sendQueue = new Queue<string>();
    Thread sendThread;
    Socket mySocket;
    bool stayConnected = true;
    public bool connectionStatus = false;

    int reconnectionDelay = 5;
    public bool doDebug = false;

    // Start is called before the first frame update

    public bool StartClient()
    {
        requestReconnection = false;
        stayConnected = true;
        sendThread = new Thread(new ThreadStart(SendMessage));
        sendThread.IsBackground = true;
        sendThread.Start();
        return true;
    }

    bool TryConnect()
    {
        IPAddress addr = IPAddress.Parse(ipAddress);
        IPEndPoint iep = new IPEndPoint(addr, portNumber);
        try
        {
            mySocket.Connect(iep);
            if(doDebug)Debug.Log("<color=#00ff00>Connection made to server!</color>");
            return true;
        }
        catch (Exception e)
        {
            if (doDebug) Debug.LogWarning(e.Message);
            if (doDebug) Debug.LogWarning(e.StackTrace);
            return false;
        }
    }


    void WaitForConnection()
    {
        while (!connectionStatus && stayConnected)
        {
            connectionStatus = TryConnect();
            if (!connectionStatus && doDebug) Debug.LogWarning("Fail reconnect wait " + reconnectionDelay);
            Thread.Sleep(reconnectionDelay * 1000);
        }
    }

    public void SendMessage()
    {//ㅁㄴㅇㄹㄴ ㅎㅇㅀㅇㅀ
     //program1-
     //[20길]3#3#3#123123ㄹ3//20
        mySocket = new Socket(
             AddressFamily.InterNetwork,
             SocketType.Stream,
             ProtocolType.Tcp
             );//소켓 생성
               //인터페이스 결합(옵션)
               //연결
        WaitForConnection();

        while (connectionStatus)
        {
            //MUTEX
            mutex.WaitOne();
            while (sendQueue.Count > 0)
            {
                string message = PollMessage();
                if (doDebug) Debug.Log("Send message " + message);
                try
                {
                    SendAMessage(message);
                //무한루프에 주의
                }
                catch (Exception)
                {

                    connectionStatus = false;
                }
            }
            mutex.ReleaseMutex();
            try
            {
                connectionStatus = !(mySocket.Poll(1, SelectMode.SelectRead) && mySocket.Available == 0);

            }
            catch {
                connectionStatus = false;
            }
            if (!connectionStatus)
            {
                Disconnect();
                requestReconnection = true;
            }
            //MUTEX
        }
    }

    public void EnqueueAMessage(string netMessage)
    {
        //MUTEX
        if (!connectionStatus) return;
        mutex.WaitOne();
        netMessage = netMessage.Replace(NET_DELIM, " ");
        sendQueue.Enqueue(NET_SIG + NET_DELIM + netMessage);
        mutex.ReleaseMutex();
        //MUTEX
    }
    string PollMessage()
    {
        string message = sendQueue.Dequeue();
        if (message.Length >= BUFFER)
        {
            message = message.Substring(0, 256);
        }
        return message;
    }
    private void OnApplicationQuit()
    {
        Disconnect();
    }
    private void OnDisable()
    {
        Disconnect();
    }
    private void FixedUpdate()
    {
        if (requestReconnection) {
            Debug.Log("Starting connection");
            StartClient();
        }
    }
    private void SendAMessage(string str)
    {

        byte[] packet = new byte[BUFFER];
        MemoryStream ms = new MemoryStream(packet);
        BinaryWriter bw = new BinaryWriter(ms);
        bw.Write(str);
        bw.Close();
        ms.Close();
        mySocket.Send(packet);

    }

   public bool requestReconnection = false;
    public void Disconnect()
    {
        Debug.Log("Closing socket");
        stayConnected = false;
        mySocket.Close();//소켓 닫기
    }


}
