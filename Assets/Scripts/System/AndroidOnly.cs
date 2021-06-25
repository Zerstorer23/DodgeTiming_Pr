using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AndroidOnly : MonoBehaviour
{
    public bool enableOnAndroid = true;
    private void OnEnable()
    {
        if (enableOnAndroid)
        {
            gameObject.SetActive(Application.platform == RuntimePlatform.Android);

        }
        else {

            gameObject.SetActive(Application.platform != RuntimePlatform.Android);
        }
    }

}
