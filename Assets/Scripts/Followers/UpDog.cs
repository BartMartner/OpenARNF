using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UpDog : Follower
{
    public GameObject growl;
    public float growlDamage = 6f;
    public SpriteRenderer spriteRenderer;
    private JumpPad _jumpPad;
    private int _jumpAttempts;
    private bool _spawned;
    private Controller2D _controller2D;
    private LayerMask _layerMask;
    private Animator _animator;
    private bool _moving;
    private Vector2 _velocity;
    private bool _gravityFlipped;
    private LineRenderer _teleportTrail;
    private DamageCreatureTrigger _growlDamageTrigger;
    private float _springDelay;

    private void Awake()
    {
        _animator = GetComponentInChildren<Animator>();
        _teleportTrail = GetComponentInChildren<LineRenderer>();
        _teleportTrail.gameObject.SetActive(false);
        _growlDamageTrigger = growl.GetComponent<DamageCreatureTrigger>();
    }

    public override IEnumerator Start()
    {
        yield return base.Start();

        _jumpPad = GetComponentInChildren<JumpPad>();
        _spawned = true;
        _controller2D = GetComponent<Controller2D>();
        _layerMask = LayerMask.GetMask("Default");

        Hide();        

        while (!LayoutManager.instance && player.mainRenderer == null)
        {
            yield return null;
        }

        Spawn(true);
        
        spriteRenderer.material = player.mainRenderer.material;

        if (LayoutManager.instance)
        {
            LayoutManager.instance.onTransitionComplete += OnTransitionComplete;
        }
        else
        {
            Debug.LogWarning("Updog could not find LayoutManager.instance!");
        }
    }

    private void Update()
    {
        var grounded = _controller2D.bottomEdge.touching;
        if (player.controller.GetButton(player.jumpString))
        {
            if (player.grounded)
            {
                _springDelay = 0.65f;
            }
            else if (_springDelay < 0.1f)
            {
                _springDelay = 0.1f;
            }
        }
        if (_springDelay > 0) { _springDelay -= Time.deltaTime; }
        if (player.GetYAxis() <= -0.7f) { _springDelay = 0; }
        _jumpPad.active = grounded && _springDelay > 0;
    }

    public void FixedUpdate()
    {
        _animator.SetBool("Spawned", _spawned);

        if (!_spawned || _jumpPad.applyingForce) { return; }

        var grounded = _controller2D.bottomEdge.touching;
        var delta = player.controller2D.bottomMiddle - _controller2D.bottomMiddle;
        var canTeleport = player.grounded && grounded;
        var tooFarDist = player.controller2D.bottomEdge.angle % 90 == 0 ? 3 : 4;
        var tooFarY = Mathf.Abs(delta.y) >= tooFarDist;
        var needTeleport = delta.magnitude > 8 || _jumpAttempts >= 3 || (player.grounded && grounded && tooFarY);

        if (canTeleport && needTeleport)
        {
            _moving = false;
            TeleportToPlayer(false); //Will ultimately call WaitSpawn
        }
        else
        {
            if (_controller2D.collisions.below)
            {
                _velocity.y = -1; //makes sure _controller2D.collisions.below remains true for moving platforms
            }
            else
            {
                _velocity.y += player.gravity * Time.deltaTime;
            }

            var absDeltaX = Mathf.Abs(delta.x);
            var playerXForcing = Mathf.Abs(player.GetXAxis()) > 0.8f && (player.controller2D.leftEdge.touching || player.controller2D.rightEdge.touching);
            var needToMove = absDeltaX > (playerXForcing ? 1.25f : 2.5f) || (player.grounded && tooFarY);

            if (!_moving && needToMove || !grounded) { _moving = true; }

            if (_moving)
            {
                growl.SetActive(false);
                _animator.SetBool("Moving", true);
                _animator.SetBool("Growling", false);

                var targetDistance = playerXForcing ? 1 : 1.5f;
                if (absDeltaX > targetDistance)
                {
                    _velocity.x = Mathf.Sign(delta.x) * player.maxSpeed;
                }
                else if (_velocity.x != 0)
                {
                    _velocity.x = Mathf.MoveTowards(_velocity.x, 0, player.maxSpeed * 2 * Time.deltaTime);
                }

                var canJump = (grounded && (player.velocity.y <= 0 || _controller2D.rightEdge.near));
                var tooFar = player.gravityFlipped ? delta.y < -(tooFarDist - 1f) : delta.y > (tooFarDist - 1f);
                var shouldJump = tooFar;

                if (shouldJump && canJump)
                {
                    if (_controller2D.collisions.below) { _jumpAttempts++; }
                    _velocity.y = GetJumpVelocity(delta.y);
                }

                if (absDeltaX < targetDistance && grounded) { _moving = false; }

                SetFacing(delta.x > 0 ? Direction.Right : Direction.Left);
            }
            else
            {
                _velocity.x = 0;
                _jumpAttempts = 0;

                var closestEnemy = EnemyManager.instance.GetClosest(transform.position, 5);

                if (closestEnemy && grounded)
                {
                    growl.SetActive(true);
                    _growlDamageTrigger.damage = growlDamage * player.damageMultiplier;
                    var direction = (closestEnemy.position - transform.position).normalized;
                    _animator.SetBool("Growling", true);
                    _animator.SetFloat("GrowlX", Mathf.Abs(direction.x));
                    _animator.SetFloat("GrowlY", player.gravityFlipped ? -direction.y : direction.y);
                    growl.transform.rotation = Quaternion.FromToRotation(Vector3.right, closestEnemy.position - growl.transform.position);
                    SetFacing(closestEnemy.transform.position.x > transform.position.x ? Direction.Right : Direction.Left);
                }
                else
                {
                    _animator.SetBool("Growling", false);
                    growl.SetActive(false);
                    SetFacing(player.facing);
                }
            }

            //_jumpPad.active = grounded && player.GetYAxis() > -0.9f;
            //_jumpPad.active = grounded && player.GetYAxis() > 0.5;
            var jumping = (player.gravityFlipped ? _velocity.y < 0 : _velocity.y > 0);
            if (_controller2D.topEdge.touching && jumping) _velocity.y = 0;

            _controller2D.Move(_velocity * Time.deltaTime);
            _animator.SetBool("Grounded", grounded);
            _animator.SetBool("Moving", _moving);
            _animator.SetFloat("JumpPlatformUp", _jumpPad.active ? 1 : 0);
        }
    }

    public void SetFacing(Direction newFacing)
    {
        transform.rotation = newFacing == Direction.Right ? transform.rotation = Quaternion.identity : Constants.flippedFacing;

        if (_gravityFlipped != player.gravityFlipped)
        {
            _gravityFlipped = player.gravityFlipped;
            var boundsHeight = _controller2D.collider2D.bounds.size.y;
            if (_controller2D.bottomEdge.touching)
            {
                transform.position += Vector3.up * (_gravityFlipped ? boundsHeight : -boundsHeight);
            }
        }

        if (player.gravityFlipped) { transform.rotation *= Quaternion.Euler(0, 180, 180); }
    }

    public float GetJumpVelocity(float height)
    {
        var pGravity = player.gravity;
        var timeToApex = Mathf.Sqrt(-(2 * (height+1)) / pGravity);
        return Mathf.Clamp(Mathf.Abs(pGravity) * timeToApex, 0, player.maxJumpVeloctiy);
    }

    public void Spawn(bool forceCenter)
    {
        StartCoroutine(WaitSpawn(forceCenter));
    }

    public IEnumerator WaitSpawn(bool forceCenter)
    {
        _spawned = false;
        Hide();

        Vector3 position;
        while (!GetSpawnPosition(out position, forceCenter))
        {
            yield return new WaitForSeconds(0.05f);
        }

        StartCoroutine(TeleportTrail(transform.position, position + transform.up * 0.45f));

        transform.position = position;
        SetFacing(player.facing);

        _animator.gameObject.SetActive(true);   
        _animator.SetBool("Growling", false);
        _animator.SetBool("Grounded", true);
        _animator.Play("Spawn");

        yield return new WaitForSeconds(0.25f);

        _controller2D.enabled = true;
        _controller2D.UpdateRaycastOrigins();
        _jumpPad.active = false; //true;
        _jumpAttempts = 0;
        _spawned = true;
    }

    public bool GetSpawnPosition(out Vector3 position, bool forceCenter)
    {
        var size = _controller2D.collider2D.bounds.size;
        var offset = player.transform.up * size.y * 0.5f;
        var direction = -player.transform.up;
        var angle = player.transform.rotation.eulerAngles.z;
        RaycastHit2D hit;
        Collider2D result;
        size *= 0.9f;

        if (!forceCenter)
        {
            position = player.transform.position + player.transform.right;
            hit = Physics2D.Raycast(position, direction, 2, _layerMask);
            if (hit)
            {
                position = hit.point;
                result = Physics2D.OverlapBox(position + offset, size, angle, _layerMask);
                if (!result) { return true; }
            }

            position = player.transform.position - player.transform.right;
            hit = Physics2D.Raycast(position, direction, 2, _layerMask);
            if (hit)
            {
                position = hit.point;
                result = Physics2D.OverlapBox(position + offset, size, angle, _layerMask);
                if (!result) { return true; }
            }
        }

        position = player.transform.position;
        hit = Physics2D.Raycast(position, direction, 2, _layerMask);
        if (hit)
        {
            position = hit.point;
            result = Physics2D.OverlapBox(position + offset, size, angle, _layerMask);
            if (!result) { return true; }
        }

        return false;
    }

    public void TeleportToPlayer(bool forceCenter)
    {
        Debug.Log("TeleportToPlayer called");
        StopAllCoroutines();
        StartCoroutine(DespawnRoutine(() => Spawn(forceCenter)));
    }

    public void Hide()
    {
        _animator.gameObject.SetActive(false);
        growl.SetActive(false);
        _controller2D.enabled = false;
        _jumpPad.active = false;
    }

    private IEnumerator TeleportTrail(Vector3 start, Vector3 end)
    {
        _teleportTrail.gameObject.SetActive(true);

        _teleportTrail.SetPositions(new Vector3[] { start, end });

        var timer = 0f;
        var time = 0.2f;
        var originalStart = start;
        while (timer < time)
        {
            start = Vector3.Lerp(originalStart, end, timer / time);
            _teleportTrail.SetPositions(new Vector3[] { start, end });
            timer += Time.deltaTime;
            yield return null;
        }

        _teleportTrail.gameObject.SetActive(false);
    }

    public IEnumerator DespawnRoutine(Action onFinish)
    {
        while (_jumpPad.applyingForce) yield return null;

        _jumpPad.active = false;
        _spawned = false;

        _animator.SetBool("Growling", false);
        _animator.SetBool("Grounded", true);

        _animator.Play("Despawn");
        yield return new WaitForSeconds(0.25f);

        if(onFinish != null)
        {
            onFinish();
        }
    }

    public override void OnDestroy()
    {
        base.OnDestroy();
        if (LayoutManager.instance)
        {
            LayoutManager.instance.onTransitionComplete -= OnTransitionComplete;
        }
    }

    public void OnTransitionComplete()
    {
        StopAllCoroutines();
        Spawn(true);        
    }
}
