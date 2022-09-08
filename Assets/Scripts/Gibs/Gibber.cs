using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Gibber : MonoBehaviour
{
    public GibType gibType;
    public int amount = 6;
    new public Collider2D collider2D;

    public void Start()
    {
        if (!collider2D)
        {
            collider2D = GetComponent<Collider2D>();
        }
    }

    public void Gib()
    {
        if (collider2D)
        {
            Rect area;
            area = collider2D ? new Rect(0, 0, collider2D.bounds.extents.x * 2, collider2D.bounds.extents.y * 2) : new Rect(0, 0, 1, 1);
            area.center = transform.position;
            GibManager.instance.SpawnGibs(gibType, area, amount, 12);
        }
        else
        {
            GibManager.instance.SpawnGibs(gibType, transform.position, amount);
        }
    }
}
