using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CreepSpawner : MonoBehaviour
{
    public CreepStats stats;
    public float frequency = 0.5f;
    public Vector3 direction = Vector3.down;
    public float distance = 1;
    public float startDelay;
    private float _timer;
    private bool _ready;

    private void Awake()
    {
        _timer = frequency;
    }

    private IEnumerator Start()
    {
        if (startDelay > 0) { yield return new WaitForSeconds(startDelay); }
        _ready = true;
    }

    // Update is called once per frame
    private void Update ()
    {
        if (_ready)
        {
            _timer += Time.deltaTime;
            if (_timer > frequency && FXManager.instance.TrySpawnCreep(transform.position, transform.TransformDirection(direction), distance, stats))
            {
                _timer = 0f;
            }
        }
	}
}
