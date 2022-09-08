using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameObjectGroupToggle : MonoBehaviour
{
    public GameObject[] gameObjects;

    public void SetActive()
    {
        foreach (var go in gameObjects)
        {
            go.SetActive(true);
        }
    }

    public void SetInactive()
    {
        foreach (var go in gameObjects)
        {
            go.SetActive(false);
        }
    }
}
