using UnityEngine;
using System.Collections;

public class DualAxisParallax : MonoBehaviour
{
    [Range(0,1)]
    public float xAmount;
    [Range(0, 1)]
    public float yAmount;
    private Vector3 _originalLocalPostion;

    private void Start()
    {
        _originalLocalPostion = transform.localPosition;
    }

    private void LateUpdate()
    {
        var camPosition = PlayerManager.instance.cameraPosition;
        var original = (transform.parent ? transform.parent.TransformPoint(_originalLocalPostion) : _originalLocalPostion);
        transform.position = new Vector3(Mathf.Lerp(original.x, camPosition.x, xAmount), Mathf.Lerp(original.y, camPosition.y, yAmount), _originalLocalPostion.z);
    }
}
