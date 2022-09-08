using UnityEngine;
using System.Collections;
using UnityEngine.Events;

public class Bouncer : BaseMovementBehaviour
{
    public float horizontalSpeed = 5f;
    public float hopHeight = 3;
    public float timeToHopApex = 1;
    public UnityEvent onBounce;

    private Controller2D _controller2D;
    private Vector2 velocity;
    private float _gravity;
    private float _jumpVelocity;
    private float _bounceMod = 1;
    private bool _justBounced;

    private void Awake()
    {
        _controller2D = GetComponent<Controller2D>();
    }
	
	public void Update ()
    {
        CalculateJump();

        velocity.x = transform.right.x * horizontalSpeed * _slowMod;

        if(!_controller2D.bottomEdge.touching)
        {
            _bounceMod = 1;
            velocity.y += _gravity * Time.deltaTime * _bounceMod;
        }
        else if(velocity.y < 0)
        {
            velocity.y = 0;
        }

        _controller2D.Move(velocity * Time.deltaTime);

        if(_controller2D.rightEdge.touching)
        {
            var eulerRotation = transform.rotation.eulerAngles;
            transform.rotation = Quaternion.Euler(eulerRotation.x, eulerRotation.y == 0 ? 180 : 0, eulerRotation.z);
        }

        if(_controller2D.topEdge.touching)
        {
            _bounceMod = -1;
        }

        if (!_justBounced && (_controller2D.rightEdge.justTouched || _controller2D.leftEdge.justTouched || _controller2D.bottomEdge.touching || _controller2D.topEdge.justTouched))
        {
            StartCoroutine(Bounce());
        }
    }

    public IEnumerator Bounce()
    {
        _justBounced = true;
        velocity.y = _jumpVelocity * _bounceMod * _slowMod;
        if(onBounce != null)
        {
            onBounce.Invoke();
        }
        yield return new WaitForSeconds(0.1f);
        _justBounced = false;
    }

    public void CalculateJump()
    {
        _gravity = -(2 * hopHeight) / Mathf.Pow(timeToHopApex, 2);
        _jumpVelocity = Mathf.Abs(_gravity) * timeToHopApex;
    }
}
