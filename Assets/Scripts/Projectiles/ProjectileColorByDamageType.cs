using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[RequireComponent(typeof(Projectile))]
public class ProjectileColorByDamageType : MonoBehaviour
{
    private Projectile _projectile;
    private List<DamageType> _damageTypes;
    private SpriteRenderer[] _spriteRenderers;

    public void OnEnable()
    {
        _projectile = GetComponent<Projectile>();
        _spriteRenderers = GetComponentsInChildren<SpriteRenderer>();
        _damageTypes = new List<DamageType>(_projectile.stats.damageType.GetFlags());
        _damageTypes.Remove(0);
        _damageTypes.Remove(DamageType.Generic);

        if(_projectile.stats.homing > 0)
        {
            foreach (var r in _spriteRenderers)
            {
                r.color = Constants.damageTypeColors[DamageType.Mechanical];
            }
            return;
        }

        if (_damageTypes.Count == 0)
        {
            foreach (var r in _spriteRenderers)
            {
                r.color = Constants.blasterGreen;
            }
            return;
        }

        if(_damageTypes.Count == 1)
        {
            Color32 color = Color.white;
            if (Constants.damageTypeColors.TryGetValue(_damageTypes[0], out color))
            {
                foreach (var r in _spriteRenderers)
                {
                    r.color = color;
                }
            }
            return;
        }

        StartCoroutine(SetColor());
    }

    public IEnumerator SetColor()
    {
        while(_projectile.alive)
        {
            foreach (var damageType in _damageTypes)
            {
                Color32 color = Color.white;
                if (Constants.damageTypeColors.TryGetValue(damageType, out color))
                {
                    foreach (var r in _spriteRenderers)
                    {
                        r.color = color;
                    }
                    yield return new WaitForSeconds(0.5f/_damageTypes.Count);
                }
            }
        }
    }
}
