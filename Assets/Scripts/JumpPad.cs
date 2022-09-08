using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class JumpPad : BoundsCheck, ISpecialPlatform
{
    public float distance = 10;    
    private HashSet<Player> _players = new HashSet<Player>();
    public bool applyingForce { get; private set; }
    public bool active = true;
    public UnityEvent onJump;

    protected override void Awake()
    {
        collisionMask = LayerMask.GetMask("Player");
        base.Awake();
        TestEdges();        
    }

    public override void FixedUpdate()
    {
        if (active)
        {
            UpdateRaycastOrigins();
            TestTop();
        }
    }

    public override void OnTouch(RaycastHit2D hit, Direction direction)
    {
        if (active && direction == Direction.Up)
        {            
            var player = hit.transform.GetComponent<Player>();
            if (!player || _players.Contains(player)) return;
            if (player.morphing || player.inAirFromKnockBack || player.justUncrouched || player.activeSpecialMove != null) return;
            if (player.gravityFlipped ? player.velocity.y <= 0 : player.velocity.y >= 0) return;

            var lineDir = (player.controller2D.bottomLeft - player.controller2D.bottomRight).normalized;
            float distance = Vector3.Cross(lineDir, hit.point - player.controller2D.bottomRight).magnitude;

            if (distance > 0.25f) return;

            StartCoroutine(ApplyForce(player));            
        }
    }

    private IEnumerator ApplyForce(Player player)
    {
        applyingForce = true;
        _players.Add(player);
        player.jumping = true;
        player.jumpInputOverride = true;
        player.currentAirJumps = 0;

        if (onJump != null)
        {
            onJump.Invoke();
        }

        var timeToApex = Mathf.Sqrt(Mathf.Abs(2 * distance / player.gravity));
        var maxJumpVelocity = Mathf.Abs(player.gravity * timeToApex);

        Debug.Log("time = " + timeToApex);
        Debug.Log("jumpVel = " + maxJumpVelocity);

        var velocity = transform.up * maxJumpVelocity;

        var playerVelocity = player.velocity;
        playerVelocity.y = velocity.y;
        player.velocity = playerVelocity;

        var timer = 0f;
        while(timer < timeToApex)
        {
            if (player.controller2D.topEdge.touching)
            {
                player.jumpInputOverride = false;
                yield return new WaitForSeconds(0.5f); //Waiting 1/2 second prevents player from repeatedly jumping off updog in 3 unity high hall
                break;
            }
            timer += Time.deltaTime;
            yield return null;
        }
        player.jumpInputOverride = false;

        _players.Remove(player);
        applyingForce = false;
    }
}
