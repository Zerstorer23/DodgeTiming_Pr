
using Lex;
using System.Collections;
using UnityEngine;

public class Explosion : MonoBehaviourLex
{
    LexView pv;
    public float delay = 1f;
    private void Awake()
    {
        pv = GetComponent<LexView>();
    }
    private void OnEnable()
    {
        StartCoroutine(WaitAndKill());
    }

    IEnumerator WaitAndKill()
    {
        yield return new WaitForSeconds(delay);
        KillMe();
    }

    public void KillMe()
    {
        if (pv.IsMine)
        {
            LexNetwork.Destroy(pv);
        }
    }

}
