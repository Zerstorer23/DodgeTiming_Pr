namespace Lex
{
    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;
    using System.Linq;

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