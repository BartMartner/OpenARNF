using UnityEngine;
using System.Collections;

public class Parallax : MonoBehaviour
{
    [Range(0,1)]
    public float amount;
    private Vector3 _originalLocalPostion;

    private void Start()
    {
        _originalLocalPostion = transform.localPosition;
    }

    private void LateUpdate()
    {
        if (amount == 1)
        {
            var newPostion = PlayerManager.instance.cameraPosition;
            newPostion.z = _originalLocalPostion.z;
            transform.position = newPostion;
        }
        else if (amount != 0)
        {
            var newPostion = Vector3.Lerp(transform.parent.TransformPoint(_originalLocalPostion), PlayerManager.instance.cameraPosition, amount);
            newPostion.z = _originalLocalPostion.z;
            transform.position = newPostion;
        }
    }
}
