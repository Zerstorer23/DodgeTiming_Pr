using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Lex
{
    public class LexNetwork_ResetHelper : MonoBehaviour
    {
        private static LexNetwork_ResetHelper instance;
        private void Awake()
        {
            instance = this;

        }
        public static void StartRoutine(IEnumerator routine)
        {
            instance.StartCoroutine(routine);
        }
    }
}