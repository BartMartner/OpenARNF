using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SetRepeatByLossyScale : MonoBehaviour
{
	void Start ()
    {
        var renderer = GetComponent<Renderer>();
        renderer.material = new Material(renderer.material);
        renderer.material.SetFloat("_RepeatX", transform.lossyScale.x);
        renderer.material.SetFloat("_RepeatY", transform.lossyScale.y);
    }
}
