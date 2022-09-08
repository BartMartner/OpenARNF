using UnityEngine;
using System.Collections;

[RequireComponent(typeof(BoundsCheck))]
public class PongerBehavior : BaseMovementBehaviour
{
    public bool justSwitchedDirections;
    public float speed = 5f;
    public Vector3 startingDirection = new Vector3(1,1,0);
    public bool randomStartingDirection = true;
    private Vector3 _direction;
    private BoundsCheck _boundsCheck;

    protected override void Start()
    {
        base.Start();
        _boundsCheck = GetComponent<BoundsCheck>();
        if (randomStartingDirection)
        {
            _direction = new Vector3();
            _direction.x = Random.value > 0.5f ? 1 : -1;
            _direction.y = Random.value > 0.5f ? 1 : -1;
        }
        else
        {
            _direction = startingDirection;
        }
        _direction.Normalize();
    }

    private void Update()
    {
        if (justSwitchedDirections)
        {
            justSwitchedDirections = !(_boundsCheck.bottomEdge.touching || _boundsCheck.leftEdge.touching || _boundsCheck.rightEdge.touching || _boundsCheck.topEdge.touching);
        }

        if (!justSwitchedDirections)
        {
            if (_boundsCheck.topEdge.touching)
            {
                _direction.y = -1;
                justSwitchedDirections = true;
            }
            else if (_boundsCheck.bottomEdge.touching)
            {
                _direction.y = 1;
                justSwitchedDirections = true;
            }
            else
            {
                _direction.y = _direction.y < 0 ? -1 : 1;
            }

            if (_boundsCheck.rightEdge.touching)
            {
                _direction.x = -1;
                justSwitchedDirections = true;
            }
            else if (_boundsCheck.leftEdge.touching)
            {
                _direction.x = 1;
                justSwitchedDirections = true;
            }
            else
            {
                _direction.x = _direction.x < 0 ? -1 : 1;
            }
        }        

        _direction.Normalize();
        transform.position += transform.rotation * _direction * Time.deltaTime * speed * _slowMod;
    }
}
