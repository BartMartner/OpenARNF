using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StickyCreepSpawner : MonoBehaviour
{
    public CreepStats stats;
    public float frequency = 0.5f;    
    public float distance = 1;
    public float startDelay;
    private float _timer;
    private bool _ready;
    private BoundsCheck _boundsCheck;

    private void Awake()
    {
        _timer = frequency;
        _boundsCheck = GetComponent<BoundsCheck>();
    }

    private IEnumerator Start()
    {
        if (startDelay > 0) { yield return new WaitForSeconds(startDelay); }
        _ready = true;
    }

    // Update is called once per frame
    private void Update()
    {
        if (_ready)
        {
            _timer += Time.deltaTime;
            if (_timer > frequency)
            {
                Vector3 direction = Vector3.zero;
                if (_boundsCheck.bottomEdge.touching) { direction = Vector3.down; }
                else if (_boundsCheck.topEdge.touching) { direction = Vector3.up; }

                if (_boundsCheck.rightEdge.touching) { direction += Vector3.right; }
                else if (_boundsCheck.leftEdge.touching) { direction += Vector3.left; }
                
                if (FXManager.instance.TrySpawnCreep(transform.position, transform.TransformDirection(direction), distance, stats))
                {
                    _timer = 0f;
                }
            }
        }
    }
}
