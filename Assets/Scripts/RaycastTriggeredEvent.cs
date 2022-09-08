using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RaycastTriggeredEvent : BaseEvent
{
    public float coolDown = 1f;
    public bool singleFire;
    public int rays = 1;
    [Range(0, 360)]
    public float arc = 0;
    public float distance = 8;
    public LayerMask layerMask;
    public LayerMask targetLayers;

    private bool _ready;
    private bool _raysActive;

    public IEnumerator Start()
    {
        yield return new WaitForSeconds(1f);
        _ready = true;
        _raysActive = true;
    }

    public void Update()
    {
        if(_raysActive && _ready && rays > 0)
        {
            bool hit = false;

            if (arc != 0)
            {
                for (int i = 0; i < rays; i++)
                {
                    float divisor = arc < 360 ? rays - 1 : rays;
                    float angleMod = (((float)i / divisor) * 2f) - 1f;
                    Vector3 direction = (Quaternion.AngleAxis(angleMod * arc / 2, Vector3.forward) * transform.right).normalized;
                    var result = Physics2D.Raycast(transform.position, direction, distance, layerMask);
                    if(ValidResult(result))
                    {
                        hit = true;
                        break;
                    }
                }
            }
            else
            {
                var result = Physics2D.Raycast(transform.position, transform.right, distance, layerMask);
                hit = ValidResult(result);                
            }

            if(hit)
            {
                StartEvent();
            }
        }
    }

    public bool ValidResult(RaycastHit2D result)
    {
        if (!result.collider) return false;
        if (targetLayers == (targetLayers | (1 << result.collider.gameObject.layer)))
        {
            var damageable = result.collider.gameObject.GetComponent<IDamageable>();
            if (damageable != null && damageable.targetable) { return true; }
        }
        return false;
    }

    protected override IEnumerator EventCycle()
    {
        _raysActive = false;
        yield return base.EventCycle();
        yield return new WaitForSeconds(coolDown);

        if (!singleFire)
        {
            _raysActive = true;
        }
        else
        {
            enabled = false;
        }
    }

    public override void Pause()
    {
        if (enabled)
        {
            _raysActive = false;
        }

        base.Pause();
    }

    public override void Unpause()
    {
        if (!enabled && !_eventCycleActive)
        {
            _raysActive = true;
        }

        base.Unpause();
    }


    public void OnDrawGizmosSelected()
    {
        if (rays <= 0) return;

        var color = new Color(1, 1, 0, 0.5f);
        if (arc != 0)
        {
            for (int i = 0; i < rays; i++)
            {
                float divisor = arc < 360 ? rays - 1 : rays;
                float angleMod = (((float)i / divisor) * 2f) - 1f;
                Vector3 direction = (Quaternion.AngleAxis(angleMod * arc / 2, Vector3.forward) * transform.right).normalized;               
                Debug.DrawLine(transform.position, transform.position + direction * distance, color);                
            }
        }
        else
        {            
            Debug.DrawLine(transform.position, transform.position + transform.right * distance, color);
        }
    }
}
