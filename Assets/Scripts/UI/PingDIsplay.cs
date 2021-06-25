using UnityEngine;
using Lex;
using UnityEngine.UI;

public class PingDIsplay : MonoBehaviourLex
{
  [SerializeField] Text pingValueText;
  
    void Update()
    {
        pingValueText.text=  LexNetwork.GetPing()+"ms";
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
