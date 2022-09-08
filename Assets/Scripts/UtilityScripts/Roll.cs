using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Roll : MonoBehaviour
{
    public float speed = 15;

	// Update is called once per frame
	void Update ()
    {
        if (speed != 0)
        {
            var rotation = transform.rotation.eulerAngles;
            rotation.z += speed * Time.deltaTime;
            transform.rotation = Quaternion.Euler(rotation);
        }
	}

    public void SetSpeed(float newSpeed)
    {
        speed = newSpeed;
    }
}
