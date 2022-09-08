using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Credits : MonoBehaviour
{
    private Text _text;
    
    void Awake()
    {
#if !ARCADE
        Destroy(gameObject);
        return;
#else
        _text = GetComponent<Text>();
#endif
    }

#if ARCADE
    // Update is called once per frame
    void Update()
    {
        if (SaveGameManager.instance)
        {
            if (SaveGameManager.instance.saveFileData.freePlay)
            {
                _text.text = "FREE PLAY";
            }
            else
            {
                _text.text = "CREDITS " + SaveGameManager.instance.credits;
            }
        }
    }
#endif
}
