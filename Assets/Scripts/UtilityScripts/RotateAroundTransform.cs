using UnityEngine;
using System.Collections;

public class RotateAroundTransform : MonoBehaviour
{
    public Transform target;
    public float speed = 360f;
    public bool lockLocalRotation = true;
    public float acceleration = 0f;
    public float minSpeed = -360;
    public float maxSpeed = 360;

    public void Start()
    {
        if(!target)
        {
            target = transform.parent;
        }
    }

    public void Update()
    {
        if(acceleration != 0)
        {
            speed = Mathf.Clamp(speed + acceleration * Time.deltaTime, minSpeed, maxSpeed);
        }

        transform.RotateAround(target.position, Vector3.forward, speed * Time.deltaTime);

        if (lockLocalRotation)
        {
            transform.localRotation = Quaternion.identity;
        }
    }
}
