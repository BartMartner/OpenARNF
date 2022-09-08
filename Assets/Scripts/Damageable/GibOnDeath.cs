using UnityEngine;
using System.Collections;

[RequireComponent(typeof(Damageable))]
public class GibOnDeath : MonoBehaviour
{
    public GibType gibType;
    public int amount = 6;
    public float lifeSpan = 12;
    public float force = 10;
    public float bloodSplatter;
    public bool glitchSplatter;
    new public Collider2D collider2D;

    public void Start()
    {
        if (!collider2D)
        {
            collider2D = GetComponent<Collider2D>();
        }

        var damagable = GetComponent<Damageable>();
        if (damagable)
        {
            damagable.onEndDeath.AddListener(Gib);
        }
    }

    public void Gib()
    {
        if (amount > 0)
        {
            if (collider2D)
            {
                Rect area = new Rect();
                area.center = collider2D.transform.position;
                area.width = collider2D.bounds.extents.x * 2;
                area.height = collider2D.bounds.extents.y * 2;
                GibManager.instance.SpawnGibs(gibType, area, amount, force, lifeSpan);
            }
            else
            {
                GibManager.instance.SpawnGibs(gibType, transform.position, amount, force, lifeSpan);
            }
        }

        if (bloodSplatter > 0)
        {
            if (collider2D)
            {
                GibManager.instance.SpawnBloodSplatter(collider2D.transform.position, bloodSplatter, glitchSplatter);
            }
            else
            {
                GibManager.instance.SpawnBloodSplatter(transform.position, bloodSplatter, glitchSplatter);
            }
        }
    }
}
