using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class ForceBounds : MonoBehaviour
{
    public float minVelocity;
    public float maxVelocity;
    public float oscillationTime;
    public HashSet<IHasVelocity> _effected = new HashSet<IHasVelocity>();

    public AnimationCurve xFallOff;
    public AnimationCurve yFallOff;

    private BoxCollider2D _collider2D;

    public void Awake()
    {
        _collider2D = GetComponent<BoxCollider2D>();
    }

    public void FixedUpdate()
    {
        float velocity = maxVelocity;
        if (_effected.Count > 0 && oscillationTime > 0 && maxVelocity > minVelocity)
        {
            velocity = minVelocity + Mathf.PingPong(Time.time/oscillationTime, maxVelocity - minVelocity);
        }

        var movement = transform.rotation * transform.up * velocity * Time.deltaTime;

        foreach (var e in _effected)
        {
            Vector2 eMovement = movement;
            var delta = Quaternion.Inverse(transform.rotation) * (e.transform.position - transform.position);

            if (xFallOff != null)
            {
                var fallOff = xFallOff.Evaluate(Mathf.Abs(delta.x) / (_collider2D.size.x * 0.5f));
                eMovement *= fallOff;
            }

            if (yFallOff != null)
            {
                var fallOff = yFallOff.Evaluate(Mathf.Abs(delta.y) / (_collider2D.size.y));
                eMovement *= fallOff;
            }

            eMovement = Quaternion.Inverse(transform.rotation) * movement;

            e.velocity += eMovement;
        }

        _effected.Clear();
    }

    private void OnTriggerStay2D(Collider2D collision)
    {
        var hasVelocity = collision.gameObject.GetComponent<IHasVelocity>();
        if (hasVelocity != null)
        {
            _effected.Add(hasVelocity);
        }
    }


    private void OnDrawGizmosSelected()
    {
        if(!_collider2D)
        {
            _collider2D = GetComponent<BoxCollider2D>();
        }

        var marksX = _collider2D.size.x *4;
        var marksY = _collider2D.size.y * 4;
        var markExtents = 0.125f;
        var extentsX = _collider2D.size.x * 0.5f;
        var extentsY = _collider2D.size.y * 0.5f;
        
        for (int x = 0; x <= marksX; x++)
        {
            for (int y = 0; y <= marksY; y++)
            {
                Vector3 pos = transform.position + transform.rotation * (Vector3)_collider2D.offset;
                pos += transform.right * Mathf.Lerp(-extentsX, extentsX, x / marksX);
                pos += transform.up * Mathf.Lerp(-extentsY, extentsY, y / marksY);

                var power = 1f;
                var delta = Quaternion.Inverse(transform.rotation) * (pos-transform.position);
                power *= xFallOff.Evaluate(Mathf.Abs(delta.x) / (_collider2D.size.x *0.5f));
                power *= yFallOff.Evaluate(Mathf.Abs(delta.y) / (_collider2D.size.y));
                var color = Color.Lerp(Color.clear, Color.green, power);
                Debug.DrawLine(pos - transform.up * markExtents, pos + transform.up * markExtents, color);
                Debug.DrawLine(pos - transform.right * markExtents, pos + transform.right * markExtents, color);
            }
        }
        
        
    }
}
