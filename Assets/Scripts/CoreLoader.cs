using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;

public class CoreLoader : MonoBehaviour
{
    public bool alwaysLoad;

    private void Awake()
    {
        var layoutManager = FindObjectOfType<LayoutManager>();

        if (alwaysLoad || !layoutManager)
        {
            SceneManager.LoadScene("Core", LoadSceneMode.Additive);
        }

        Destroy(gameObject);
    }
}
