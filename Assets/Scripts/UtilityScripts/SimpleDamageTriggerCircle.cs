using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SimpleDamageTriggerCircle : MonoBehaviour
{
    public float radius = 1;
    public float damage;
    public bool ignoreAegis;
    public DamageType damageType;
    public LayerMask mask;
    private Collider2D[] _collider2Ds;    
    private bool _previousSetting;

    public void Awake()
    {
        _collider2Ds = DeathmatchManager.instance ? new Collider2D[4] : new Collider2D[2];
    }

    public void FixedUpdate()
    {
        _previousSetting = Physics2D.queriesHitTriggers;
        Physics2D.queriesHitTriggers = true;        
        if (Physics2D.OverlapCircleNonAlloc(transform.position, radius, _collider2Ds, mask) > 0)
        {
            for (int i = 0; i < _collider2Ds.Length; i++)
            {
                if (_collider2Ds[i] != null)
                {
                    var damageable = _collider2Ds[i].gameObject.GetComponent<IDamageable>();
                    if (damageable != null)
                    {
                        //var rDamage = damage;
                        //if (team == Team.Player && PlayerManager.instance) { rDamage *= PlayerManager.instance.coOpDamageMod; }
                        damageable.Hurt(damage, gameObject, damageType, ignoreAegis);
                    }
                }
            }
        }
        Physics2D.queriesHitTriggers = _previousSetting;
    }

    public void OnDrawGizmosSelected()
    {
        Extensions.DrawCircle(transform.position, radius);
    }
}
