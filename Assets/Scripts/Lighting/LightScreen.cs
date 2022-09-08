using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LightScreen : MonoBehaviour
{
    private Renderer _renderer;

	// Use this for initialization
	void Start ()
    {
        _renderer = GetComponent<Renderer>();
        _renderer.sortingLayerName = "Foreground1";
        _renderer.sortingOrder = 1000;
	}
	
	// Update is called once per frame
	void Update () {
		
	}
}
