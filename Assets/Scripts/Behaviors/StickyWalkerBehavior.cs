using UnityEngine;
using System.Collections;

[RequireComponent(typeof(Controller2D))]
public class StickyWalkerBehavior : BaseMovementBehaviour 
{        
    public float speed = 5f;
    public int justSwitchedDirections;
    public bool down;
    public bool nearDown;
    public bool right;
    public bool everDown;
    private Controller2D _controller2D;

    protected override void Start()
    {
        base.Start();
        _controller2D = GetComponent<Controller2D>();
        _controller2D.TestEdges();
	}

    private void FixedUpdate ()
    {
        var velocity = transform.right * speed;

        if (justSwitchedDirections <=0 || nearDown)
        {
            velocity += -transform.up * -Physics2D.gravity.y;
        }

        _controller2D.Move(velocity * Time.deltaTime);
        _controller2D.TestEdges();

        down = _controller2D.bottomEdge.touching;
        nearDown = _controller2D.bottomEdge.near;
        right = _controller2D.rightEdge.touching && _controller2D.rightEdge.angle % 90 == 0;

        if (everDown)
        {
            if (justSwitchedDirections > 0)
            {
                if(down)
                {
                    justSwitchedDirections--;
                }                
            }
            else
            {
                if (!down)
                {
                    transform.Rotate(new Vector3(0, 0, -90));
                    justSwitchedDirections = 2;
                }
                else if (right)
                {
                    transform.Rotate(new Vector3(0, 0, 90));
                    justSwitchedDirections = 2;
                }

                if(justSwitchedDirections > 0) //shake it out!
                {
                    _controller2D.UpdateRaycastOrigins();
                    _controller2D.Move(transform.up * 0.25f);
                    _controller2D.Move(-transform.up * 0.25f);
                    _controller2D.TestEdges();
                }
            }
        }
        else
        {
            everDown = down;
        }
	}

    public void SetSpeed(float speed)
    {
        this.speed = speed;
    }
}
