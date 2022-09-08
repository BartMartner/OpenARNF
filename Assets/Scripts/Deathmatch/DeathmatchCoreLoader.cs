using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;

public class DeathmatchCoreLoader : MonoBehaviour
{
    public bool alwaysLoad;

    private void Awake()
    {
        var deathmatchManager = FindObjectOfType<DeathmatchManager>();

        if (alwaysLoad || !deathmatchManager)
        {
            SceneManager.LoadScene("DeathmatchCore", LoadSceneMode.Additive);
        }

        Destroy(gameObject);
    }
}
