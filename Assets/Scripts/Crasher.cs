using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Crasher : MonoBehaviour
{
    public GameObject secret;

    public void OnTriggerStay2D(Collider2D collision)
    {
        StartCoroutine(Crash());
    }

    public IEnumerator Crash()
    {
        var timer = 0f;
        while(timer < 5)
        {
            timer += Time.unscaledDeltaTime;
            if(Input.GetKeyDown(KeyCode.F8))
            {
                secret.SetActive(true);
                if (SaveGameManager.activeSlot != null)
                {
                    SaveGameManager.activeSlot.spookyFinished = true;
                }
            }
            yield return null;
        }

        SaveGameManager.activeSlot.RunCompleted(); //Call Save        
        yield return new WaitForSeconds(3f);
        SceneManager.LoadScene("Backstory");
    }
}
