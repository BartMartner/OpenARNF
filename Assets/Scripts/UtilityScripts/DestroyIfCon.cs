using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DestroyIfCon : MonoBehaviour
{
	void Start ()
    {
        if (SaveGameManager.instance.saveFileData != null && SaveGameManager.instance.saveFileData.conventionMode)
        {
            Destroy(gameObject);
        }
        else
        {
            Destroy(this);
        }
    }
}
