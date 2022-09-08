using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// A child collider that can be on a different Physics layer than its parent
/// </summary>
public class ProjectileSubCollider : MonoBehaviour, IProjectile
{
    public bool alive { get { return _parentProjectile && _parentProjectile.alive; } }
    private Projectile _parentProjectile;
    new public Collider2D collider2D;

    private void Awake()
    {
        _parentProjectile = GetComponentInParent<Projectile>();
        collider2D = GetComponent<Collider2D>();
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.enabled)
        {
            _parentProjectile.HandleCollision(collision);
        }
    }
}
