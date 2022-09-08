using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Darkness : MonoBehaviour
{
    public SpriteRenderer darkness;

    public void SetLighting(float amount, Color color)
    {
        var ambientLight = color * amount;
        ambientLight.a = 1;
        darkness.color = ambientLight;
    }
}
