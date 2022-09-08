using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GhostHost : MonoBehaviour
{
    public Material palette;
    public Enemy[] ghosts;

    public void OnDeath()
    {
        foreach (var ghost in ghosts)
        {
            ghost.transform.parent = transform.parent;
            ghost.mainRenderer.material = palette;
            var collider = ghost.GetComponent<Collider2D>();
            collider.enabled = true;
            var homing = ghost.GetComponent<HomingChasePlayer>();
            homing.enabled = true;
            var figure8 = ghost.GetComponent<Figure8Movement>();
            Destroy(figure8);
        }
    }
}
