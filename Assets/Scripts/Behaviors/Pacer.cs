using UnityEngine;
using System.Collections;

[RequireComponent(typeof(Controller2D))]
public class Pacer : BaseMovementBehaviour
{
    public float speed = 5f;
    private Controller2D _controller2D;
    private Vector3 _lastPosition;
    public int stuckCount;
    private bool _skipGravity;
    private Vector3 _velocity;
    public Vector3 velocity
    {
        get { return _velocity; }
    }

    protected void Awake()
    {
        _controller2D = GetComponent<Controller2D>();
    }

    public void FixedUpdate()
    {
        _velocity = Vector3.zero;

        if (!_skipGravity)
        {
            _velocity = transform.up * Physics2D.gravity.y;
        }

        if (!_controller2D.collisions.below)
        {
            _controller2D.Move(_velocity * Time.deltaTime); //just fall
            return;
        }

        _velocity += transform.right * speed * _slowMod;
        _controller2D.Move(_velocity * Time.deltaTime);

        if (_lastPosition == transform.position || !_controller2D.CheckForGroundAhead(0.25f, 0.5f))
        {
            stuckCount++;
        }
        else
        {
            stuckCount = 0;
        }

        if (_controller2D.collisions.right || stuckCount > 4)
        {
            var eulerRotation = transform.rotation.eulerAngles;
            transform.rotation = Quaternion.Euler(eulerRotation.x, eulerRotation.y == 0 ? 180 : 0, eulerRotation.z);
        }

        _lastPosition = transform.position;
    }

    public void ToggleGravity()
    {
        _skipGravity = !_skipGravity;
    }
}
