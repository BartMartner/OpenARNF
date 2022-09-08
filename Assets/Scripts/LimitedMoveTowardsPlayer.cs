using UnityEngine;
using System.Collections;

public class LimitedMoveTowardsPlayer : MonoBehaviour
{ 
    public float speed = 1f;
    public float maxDistance = 1;
    private Vector3 _originalLocalPosition;

    public void Start()
    {
        _originalLocalPosition = transform.localPosition;
    }
	
	void LateUpdate ()
    {
        var direction = (Player.instance.transform.position - transform.position).normalized;
        transform.position += direction * speed * Time.deltaTime;
        var distanceFromOriginal = Vector3.Distance(transform.localPosition, _originalLocalPosition);
        if (distanceFromOriginal > maxDistance)
        {
            direction = (_originalLocalPosition - transform.localPosition).normalized;
            transform.localPosition += direction * (distanceFromOriginal - maxDistance);
        }
    }
}
