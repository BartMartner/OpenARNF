using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DangerRubble : MonoBehaviour
{
    public float preDropWait = 1f;
    public Vector3 speed = Vector3.zero;
    public LayerMask layerMask;

    private Enemy _enemy;
    private float _timer = 0;

    public void Awake()
    {
        _enemy = GetComponent<Enemy>();
    }

    public void Update ()
    {
        speed += (Vector3)Physics2D.gravity * Time.deltaTime;
        speed.y = Mathf.Clamp(speed.y, Physics2D.gravity.y*2, 0);
        transform.position += speed * Time.deltaTime;
        _timer += Time.deltaTime;

        if(_timer > preDropWait)
        {
            if (Physics2D.Raycast(transform.position, Vector3.down, 0.5f, layerMask))
            {
                _enemy.StartDeath();
            }
        }
	}
}
