using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GlitchWorldTrigger : MonoBehaviour
{

    private void OnTriggerEnter2D(Collider2D collision)
    {
        var player = collision.GetComponent<Player>();
        if(player && LayoutManager.instance)
        {
            LayoutManager.instance.GlitchToEnvironmentStart(EnvironmentType.Glitch);
        }
    }

}
