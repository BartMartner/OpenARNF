using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.SceneManagement;
#if UNITY_SWITCH
using nn;
using nn.fs;
using nn.account;
#endif

public class SplashScreenWait : MonoBehaviour
{
	// Use this for initialization
	IEnumerator Start ()
    {
        yield return new WaitForSeconds(0.5f);

		while(!SplashScreen.isFinished)
        {
            yield return null;
        }

        SceneManager.LoadScene("Backstory");
	}
}
