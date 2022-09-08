using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class JumpWithPlayer : MonoBehaviour
{
    public bool jumping;
    public float maxJumpHeight = Constants.startingMaxJumpHeight;
    public float minJumpHeight = 0.5f;
    public float timeToJumpApex = Constants.startingJumpTime;
    public float delay = 0.1f;

    private Vector2 _velocity;
    public Vector2 velocity
    {
        get { return _velocity; }
    }

    public UnityEvent onJumpStart;
    public UnityEvent onJumpEnd;

    private float _gravity;
    private float _maxJumpVelocity;
    private float _minJumpVelocity;
    private Controller2D _controller2D;
    private Player _player1;

    void Start ()
    {
        _controller2D = GetComponent<Controller2D>();
        _player1 = PlayerManager.instance.player1;
	}
	
	void Update ()
    {
        if (_player1.state == DamageableState.Alive)
        {
            if(!jumping && _player1.controller.GetButton(_player1.jumpString))
            {
                Debug.Log("Jump");
                StartCoroutine(Jump());
            }
        }
    }

    public void CalculateJump()
    {
        _gravity = -(2 * maxJumpHeight) / Mathf.Pow(timeToJumpApex, 2);
        _maxJumpVelocity = Mathf.Abs(_gravity) * timeToJumpApex;
        _minJumpVelocity = Mathf.Sqrt(2 * Mathf.Abs(_gravity) * minJumpHeight);
    }

    private IEnumerator Jump()
    {
        jumping = true;

        yield return new WaitForSeconds(delay);

        CalculateJump();

        onJumpStart.Invoke();

        _velocity.y = _maxJumpVelocity;

        while(_player1.controller.GetButton(_player1.jumpString) && !_controller2D.topEdge.touching)
        {
            _velocity.y += _gravity * Time.deltaTime;
            _controller2D.Move(_velocity * Time.deltaTime);
            yield return null;
        }

        if (velocity.y > _minJumpVelocity)
        {
            _velocity.y = _minJumpVelocity;
        }

        while(!_controller2D.bottomEdge.touching)
        {
            _velocity.y += _player1.gravity * Time.deltaTime;
            _controller2D.Move(_velocity * Time.deltaTime);
            yield return null;
        }

        onJumpEnd.Invoke();

        _velocity = Vector2.zero;
        jumping = false;
    }
}
