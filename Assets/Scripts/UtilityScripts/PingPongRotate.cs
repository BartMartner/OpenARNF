using UnityEngine;
using System.Collections;

public class PingPongRotate : MonoBehaviour
{
    public float minAngle;
    public float maxAngle;
    public float speed;
    private float _offset;

    void Awake ()
    {
        _offset = transform.rotation.eulerAngles.z/speed;
    }

    void Update ()
    {
        transform.rotation = Quaternion.Euler(0, 0, minAngle + Mathf.PingPong((Time.time + _offset) * speed, (maxAngle-minAngle)));
	}
}
