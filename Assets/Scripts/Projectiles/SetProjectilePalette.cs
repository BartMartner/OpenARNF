using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Projectile))]
public class SetProjectilePalette : MonoBehaviour
{
    private Projectile _projectile;
    public Material defaultMaterial;
    public Material homingMaterial;

    public void OnEnable()
    {
        if (!_projectile)
        {
            _projectile = GetComponent<Projectile>();
        }

        if (_projectile.renderer)
        {
            _projectile.renderer.material = _projectile.stats.homing > 0 ? homingMaterial : defaultMaterial;
        }
    }
}
