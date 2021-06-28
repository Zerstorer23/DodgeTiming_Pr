using UnityEngine;
using Lex;
using UnityEngine.UI;

public class PingDIsplay : MonoBehaviourLex
{
  [SerializeField] Text pingValueText;
  
    void FixedUpdate()
    {
        double time = LexNetwork.Time % 10;
        pingValueText.text = LexNetwork.GetPing() + "ms \n" + time.ToString("00.00");
        if (LexNetwork.IsMasterClient)
        {
            pingValueText.color = Color.red;
        }
        else
        {
            pingValueText.color = Color.green;

        }

    }
}
