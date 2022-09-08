using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GrowingSine : BaseMovementBehaviour 
{	
	public float speed = 2f;
	public float startingAmplitude = 0.5f;
	public float finalAmplitude = 5f;
	public float startingPeriod = 1;
	public float finalPeriod = 10;
	public float lerpTime = 6;
	public Vector3 direction;

	private float _currentAmplitude;
	private float _currentPeriod;
	private float _lerpTimer;
	private float _sineTime = 0;
	private float _sine;
	private float _lastSine;

	public bool useFacing;

	// Update is called once per frame
	void Update () 
	{
		_lerpTimer += Time.deltaTime;

		if (_lerpTimer < lerpTime)
		{
			_currentAmplitude = Mathf.Lerp(startingAmplitude, finalAmplitude, _lerpTimer/lerpTime);
			_currentPeriod = Mathf.Lerp (startingPeriod, finalPeriod, _lerpTimer / lerpTime);
		}
		else
		{
			_currentAmplitude = finalAmplitude;
			_currentPeriod = finalPeriod;
		}

		_lastSine = _sine;

		if (_currentAmplitude > 0 && _currentPeriod > 0)
		{
			_sineTime += Time.deltaTime / _currentPeriod;
			if (_sineTime > 1)
			{
				_sineTime = 0;
			}

			_sine = Mathf.Sin (_sineTime * (2 * Mathf.PI));
		}
		else
		{
			_sine = 0;
		}

		var movement = (direction * speed * Time.deltaTime) + Vector3.Cross(direction, Vector3.forward) * (_sine - _lastSine) * _currentAmplitude;
		transform.position += movement;

		if (useFacing)
		{
			transform.rotation = movement.x > 0 ? Quaternion.identity : Constants.flippedFacing;
		}
	}

	public void OnDrawGizmosSelected()
	{
		var time = 0f;
		var lastPoint = transform.position;
		var sineTime = 0f;
		var sine = 0f;
		var lastSine = 0f;
		var frame = 1 / 60f;
		var currentPeriod = 0f;
		var currentAmplitude = 0f;

		while (time < lerpTime+finalPeriod)
		{
			time += frame;

			if (_lerpTimer < lerpTime)
			{
				currentAmplitude = Mathf.Lerp(startingAmplitude, finalAmplitude, time/lerpTime);
				currentPeriod = Mathf.Lerp (startingPeriod, finalPeriod, time / lerpTime);
			}
			else
			{
				currentAmplitude = finalAmplitude;
				currentPeriod = finalPeriod;
			}

			lastSine = sine;

			if (currentAmplitude > 0 && currentPeriod > 0)
			{
				sineTime += frame / currentPeriod;
				if (sineTime > 1)
				{
					sineTime = 0;
				}

				sine = Mathf.Sin (sineTime * (2 * Mathf.PI));
			}
			else
			{
				sine = 0;
			}

			var movement = (direction * speed * frame) + Vector3.Cross(direction, Vector3.forward) * (sine - lastSine) * currentAmplitude;

			Debug.DrawLine(lastPoint, lastPoint + movement);
			lastPoint = lastPoint + movement;
		}
	}
}
