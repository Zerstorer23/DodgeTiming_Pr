namespace Lex
{
    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;
    using System.Linq;

    public partial class LexNetwork {



        internal void InitServerTime(long time)
        {
            Time = (double)time / 1000; //long is in mills
            LexDebug.Log("서버 시작시간 : " + Time);
        }
        internal int ModifyServerTime(long timeValue)
        {
            double timeInMills = (double)timeValue / 1000;
            int remainingRuns = timeHandler.Append(timeInMills);
            if (remainingRuns <= 0)
            {
                timeInMills = timeHandler.Finalise();
                Time += timeInMills;
                LexDebug.Log("Modified time : " + timeInMills);
            }
            return remainingRuns;
        }

        double lastSentPing = 0;
        double lastCalculatedPing = 0;
        IEnumerator pingRoutine;
        float pingPeriodInSec = 5f;
        private void DoTimeStartUp() {
            if (pingRoutine != null)
            {
                StopCoroutine(pingRoutine);
            }
            pingRoutine = KeepPing();
            StartCoroutine(pingRoutine);
        }

        IEnumerator KeepPing()
        {
            while (gameObject.activeInHierarchy)
            {
                if (IsConnected)
                {

                    instance.SendPing();
                }
                yield return new WaitForSeconds(pingPeriodInSec);
            }
        }
        public void SendPing()
        {
            lastSentPing = Time;
            LexNetworkMessage netMessage = new LexNetworkMessage();
            netMessage.Add(LocalPlayer.actorID);
            netMessage.Add(MessageInfo.ServerRequest);
            netMessage.Add(LexRequest.Ping);
            networkConnector.EnqueueAMessage(netMessage);
        }
        public void ReceivePing(double remoteServerTime)
        {
            lastCalculatedPing = Time - lastSentPing;
            double expectedTime = (remoteServerTime + (lastCalculatedPing * 0.5));
            double difference = expectedTime - Time;
            // Debug.Log("Time diff = " + difference);
            Time = expectedTime;
        }
    }


    public class LexNetwork_TimeHandler
    {
        List<double> receivedTimes = new List<double>();
        int requiredData = 5;

        public int Append(double time)
        {
            receivedTimes.Add(time);
            return Remaining();
        }
        public int Remaining() => requiredData - receivedTimes.Count;
        public void Clear() => receivedTimes.Clear();
        public double Finalise()
        {
            return (receivedTimes.Sum() - receivedTimes.Min() - receivedTimes.Max())
                / (receivedTimes.Count - 2);
        }
    }
}