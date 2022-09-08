using UnityEngine;
using System.Collections;

public class MoveTowardsTransform : MonoBehaviour
{
    public Transform target;
    public float speed = 5f;

    public void Start()
    {
        if (!target)
        {
            target = transform.parent;
        }
    }

    public void Update()
    {
        var direction = (target.position - transform.position).normalized;
        transform.position += direction * speed * Time.deltaTime;
    }
}
