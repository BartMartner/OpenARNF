using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GlitchBGTexture : MonoBehaviour
{
    private Renderer _renderer;

	// Use this for initialization
	void Start ()
    {
        _renderer = GetComponent<Renderer>();
        _renderer.sortingLayerName = "FarBackground";
    }
}
