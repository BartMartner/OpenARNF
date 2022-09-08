using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DestroyOffCamera : MonoBehaviour 
{
	public float tolerance = 0f;
	public bool xAxis = true;
	public bool yAxis = true;
	public float warmUp = 1f;

	private float _warmUpTimer;

	// Update is called once per frame
	void LateUpdate () 
	{
		if (_warmUpTimer < warmUp)
		{
			_warmUpTimer += Time.deltaTime;
		}
		else if (MainCamera.instance.OffCamera(transform.position, tolerance, xAxis, yAxis))
		{
			Destroy(gameObject);
		}
	}
}
