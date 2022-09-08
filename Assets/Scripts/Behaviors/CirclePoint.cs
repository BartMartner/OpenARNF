using UnityEngine;
using System.Collections;

public class CirclePoint : BaseMovementBehaviour, ISpawnable
{
    public float angularSpeed = 360f;
    public float speed = 3;
    public float distance = 2;
    private Vector3 _rotatePoint;

    public void Awake()
    {
        SetPositonAsRotatePoint();
    }
    
    public void Update()
    {
        if (Vector3.Distance(transform.localPosition, _rotatePoint) < distance)
        {
            var direction = transform.localPosition == _rotatePoint ? Vector3.up : (transform.localPosition - _rotatePoint).normalized;
            transform.localPosition += direction * Time.deltaTime * speed * _slowMod;
        }
        var realPosition = transform.parent ? transform.parent.TransformPoint(_rotatePoint) : _rotatePoint;
        transform.RotateAround(realPosition, Vector3.forward, angularSpeed * _slowMod * Time.deltaTime);
        transform.localRotation = Quaternion.identity;
    }

    public void SetPositonAsRotatePoint()
    {
        _rotatePoint = transform.localPosition;
    }

    public void Spawn()
    {
        SetPositonAsRotatePoint();
    }
}
