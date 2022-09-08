using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public abstract class AdvancedAI : MonoBehaviour, IReactsToStatusEffect
{
    public GravStarAgent _agent;
    public float maxHuntSpeed = 8f;
    public float maxPatrolSpeed = 1.5f;
    public float maxRange = 12f;
    public float minRange = 4f;
    public float preferredRange = 8f;    
    public float huntPathingTime = 2f;
    public float engagedPathingTime = 0.5f;
    public float hearingRange = 3;

    protected Room _room;
    private Damageable _damageable;
    protected float _huntPathTimer;
    protected Transform _closestPlayer;
    protected float _speedFactor; //used only for animation
    protected Animator _animator;    
    protected float _slowMod = 1;

    //attacking
    public float reactionTime = 0.25f;
    protected bool _allowInAirAttack;
    protected bool _allowFacingAttack;
    protected LayerMask _playerMask;
    protected LayerMask _lineOfSight;
    protected bool _canAttack;
    protected bool _targetInSight;
    protected bool _attackCooldown;
    protected float _reactionCounter;
    protected Vector3 _lastPosition;

    protected bool _attacking;
    protected bool _coroutineControl;

    //patroling
    public Transform[] patrolTransforms;
    public float patrolIdle = 1f;
    private float _patrolIdleTimer;
    protected List<Vector3> _patrolPoints;
    protected int _currentPatrolPoint;
    private float _warmUpDelay = 1f;

    //0 patrol, 1 hunt
    protected AIMode mode;

    protected virtual void Awake()
    {
        _damageable = GetComponent<Damageable>();
        _agent = GetComponent<GravStarAgent>();
        _agent.enabled = false;
        _playerMask = LayerMask.GetMask("Player");
        _lineOfSight = LayerMask.GetMask("Player", "Default");
        _animator = GetComponent<Animator>();
        _room = GetComponentInParent<Room>();
        if (!_room)
        {
            Debug.LogError("AdvancedAI requires the game object it's attached to be the child of a transform with the Room scripts attached.");
            enabled = false;
            return;
        }

        var collider2D = GetComponent<Collider2D>();
    }    

    public void FixedUpdate()
    {
        if (!WarmUpCheck()) { return; }

        if (!_agent.pathFinder.ready)
        {
            _agent.pathFinder.TryRefresh();
            return;
        }

        if (_coroutineControl || _attacking || _agent.doNotInterupt) return;

        PreAttackNavigation();
        AttackCheck();
        PostAttackNavigation();
        _lastPosition = transform.position;
    }

    private void Update()
    {
        SetAnimatorFlags();
    }

    public bool WarmUpCheck()
    {
        if (_warmUpDelay > 0)
        {
            _warmUpDelay -= Time.deltaTime;
            return false;
        }
        else
        {
            _agent.gravStarPathFinder = _room.gravityPathFinder;
            _agent.enabled = true;
            return true;
        }
    }

    #region PreAttackNavigation
    public virtual void PreAttackNavigation()
    {
        _closestPlayer = PlayerManager.instance.GetClosestPlayerTransform(transform.position, ValidateClosestPlayer);

        //get nodes based on mode
        switch (mode)
        {
            case AIMode.Hunt:
                HuntModeUpdate();
                break;
            case AIMode.Patrol:
                PatrolModeUpdate();
                break;
        }
    }

    public virtual bool ValidateClosestPlayer(Player player)
    {
        if (!DeathmatchManager.instance) return true;
        var closestNode = _room.gravityPathFinder.GetClosestNode(player.position);
        var xDelta = Mathf.Abs(closestNode.position.x - player.position.x);
        return xDelta < 1.5f;
    }

    public void SetHuntMode()
    {
        mode = AIMode.Hunt;
    }

    public void HuntModeUpdate()
    {
        _agent.stats.maxSpeed = maxHuntSpeed;

        if (_huntPathTimer > 0 && _agent.hasPath)
        {
            _huntPathTimer -= Time.deltaTime;
        }
        else if (_closestPlayer)
        {
            List<GravStarNode> nodes;

            var playerDistance = _closestPlayer ? Vector3.Distance(_closestPlayer.position, transform.position) : 99f;
            _huntPathTimer = playerDistance < maxRange ? engagedPathingTime : huntPathingTime;

            //current position or destination valid?
            var currentTarget = _agent.destination.HasValue ? _agent.destination.Value : (Vector2)transform.position;
            var distance = Vector2.Distance(currentTarget, _closestPlayer.position);
            if (distance > minRange && distance < minRange &&
              (CheckForTarget(currentTarget, 1, false) || CheckForTarget(currentTarget, -1, false)))
            {
                return;
            }
            
            if (playerDistance < maxRange)
            {
                nodes = _room.gravityPathFinder.FindNodes(AttackPosNodeCheck);
            }
            else
            {
                nodes = _room.gravityPathFinder.FindNodes(HuntingNodeCheck);
            }

            if (nodes != null && nodes.Count > 0)
            {
                var sorted = nodes.OrderBy(BestNodeRange).ToArray();
                //look at the top third after sorting by distance from preferred
                var node = sorted[Random.Range(0, Mathf.CeilToInt(sorted.Length * 0.33f))]; //newTarget

                _agent.MoveTo(node.position);
                //If the can't path to player go back to patroling
                if (!_agent.hasPath)
                {
                    Debug.Log("Returning to Patrol");
                    GetPatrolNodes();
                    mode = AIMode.Patrol;
                }
            }
        }
    }

    public float BestNodeRange(GravStarNode node)
    {
        var pDelta = Mathf.Abs(Vector3.Distance(node.position, _closestPlayer.position) - preferredRange);
        var distance = Vector3.Distance(node.position, transform.position);
        return distance + pDelta;
    }

    protected virtual bool AttackPosNodeCheck(GravStarNode node)
    {
        var distance = Vector2.Distance(_closestPlayer.position, node.position);

        if (distance < maxRange && distance > minRange)
        {
            var p = node.position;
            p.y += _agent.nodeOffset;
            return CheckForTarget(p, 1, false) || CheckForTarget(p, -1, false);
        }
        else
        {
            return false;
        }
    }

    protected abstract bool CheckForTarget(Vector3 fromPos, float facing, bool setAim = true);

    protected virtual bool HuntingNodeCheck(GravStarNode node)
    {
        var delta = (Vector2)_closestPlayer.position - node.position;
        var distance = delta.magnitude;
        return distance < maxRange && distance > minRange;
    }

    public void PatrolModeUpdate()
    {
        _agent.stats.maxSpeed = maxPatrolSpeed;

        if (!_agent.hasPath)
        {
            if (_patrolIdleTimer > 0)
            {
                _patrolIdleTimer -= Time.deltaTime;
            }
            else
            {
                _patrolIdleTimer = patrolIdle;
                if (_patrolPoints == null) { GetPatrolNodes(); }
                _agent.MoveTo(_patrolPoints[_currentPatrolPoint]);
                _currentPatrolPoint = (_currentPatrolPoint + 1) % _patrolPoints.Count;
            }
        }
    }

    public void GetPatrolNodes()
    {
        _patrolPoints = new List<Vector3>();

        if (patrolTransforms != null && patrolTransforms.Length > 0)
        {
            foreach (var t in patrolTransforms) { _patrolPoints.Add(t.position); }
            return;
        }

        var nPos = transform.position;
        nPos.y -= _agent.nodeOffset;
        var startingNode = _room.gravityPathFinder.GetClosestNode(nPos);
        GravStarNode rightNode = null;
        var n = startingNode;
        while (n != null)
        {
            rightNode = n;
            if (rightNode.rightEdge) { break; }
            var neighbor = n.neighbors.FirstOrDefault(nb => nb.y == n.indexY && nb.x == n.indexX + 1);
            n = _room.gravityPathFinder.NodeAtIndex(neighbor.x, neighbor.y);
        }

        GravStarNode leftNode = null;
        n = startingNode;
        while (n != null)
        {
            leftNode = n;
            if (leftNode.leftEdge) { break; }
            var neighbor = n.neighbors.FirstOrDefault(nb => nb.y == n.indexY && nb.x == n.indexX - 1);
            n = _room.gravityPathFinder.NodeAtIndex(neighbor.x, neighbor.y);
        }

        _patrolPoints.Add(leftNode.position);
        _patrolPoints.Add(rightNode.position);
    }
    #endregion

    #region AttackCheck
    public virtual void AttackCheck()
    {
        _targetInSight = _attacking || CheckForTarget(transform.position, transform.right.x) ||
            (mode == AIMode.Hunt && CheckForTarget(transform.position, -transform.right.x));

        if(!_targetInSight || _attackCooldown || 
          (!_allowInAirAttack && _agent.airState != AirState.Grounded && !_attacking))
        {
            _reactionCounter = 0;
            _canAttack = false;
        }
        else
        {
            _reactionCounter += Time.deltaTime;
            _canAttack = _reactionCounter > reactionTime;
            if (_canAttack && !_attacking) { Attack(); }
        }
    }

    public abstract void Attack();
    #endregion

    #region PostAttackNavigation
    public virtual void PostAttackNavigation()
    {
        _agent.lockRotation = _closestPlayer && _targetInSight;

        if (_agent.lockRotation)
        {
            if (mode != AIMode.Hunt) { mode = AIMode.Hunt; }

            if (!_attacking || _allowFacingAttack)
            {
                if (_closestPlayer.position.x > transform.position.x && transform.rotation != Quaternion.identity)
                {
                    transform.rotation = Quaternion.identity;
                }
                else if (_closestPlayer.position.x < transform.position.x && transform.rotation != Constants.flippedFacing)
                {
                    transform.rotation = Constants.flippedFacing;
                }
            }
        }

        var delta = transform.position - _lastPosition;
        var sign = delta.x != 0 && Mathf.Sign(delta.x) != transform.right.x ? -1 : 1;
        
        if (_agent.navigating && _agent.currentMoveSpeed != 0)
        {
            _speedFactor = _agent.currentMoveSpeed > maxHuntSpeed * 0.75 ? 1 : 0.5f;
            _speedFactor *= sign;
        }
        else if (_speedFactor * sign == 1 || _speedFactor * sign == 0.5f) //give a frame to reset to 0 to prevent stutter
        {
            _speedFactor -=  sign * 0.01f; //close enough to blendtree var to not change animation
        }
        else
        {
            _speedFactor = 0;
        }
    }
    #endregion

    public bool Move(Vector3 moveAmount)
    {
        float directionY = Mathf.Sign(moveAmount.y);
        bool collided = false;

        if (directionY < 0)
        {
            var raySpacing = 0.25f;
            var bounds = _damageable.collider2D.bounds;
            var bottomLeft = new Vector2(bounds.min.x, bounds.min.y);
            var bottomRight = new Vector2(bounds.max.x, bounds.min.y);
            var verticalRayCount = Mathf.RoundToInt(Mathf.Clamp(bounds.size.x / raySpacing, 2, int.MaxValue));
            var verticalRaySpacing = bounds.size.x / (verticalRayCount - 1);
            float rayLength = Mathf.Abs(moveAmount.y);

            Vector2 rayOrigin;
            RaycastHit2D hit;

            for (int i = 0; i < verticalRayCount; i++)
            {
                rayOrigin = bottomLeft;
                rayOrigin += Vector2.right * (verticalRaySpacing * i + moveAmount.x);
                hit = Physics2D.Raycast(rayOrigin, Vector2.down, rayLength, BaseAgent.defaultMask);

                if (hit)
                {
                    moveAmount.y = (hit.distance) * directionY;
                    rayLength = hit.distance;
                    collided = true;
                }
            }
        }

        transform.position += moveAmount;
        return collided;
    }

    public virtual void SetAnimatorFlags()
    {
        _animator.SetFloat("Speed", _speedFactor);
        _animator.SetBool("InAir", _agent.airState != AirState.Grounded);
        _animator.SetInteger("AirState", (int)_agent.airState);
    }

    public void OnAddEffect(StatusEffect effect)
    {
        if (effect.type == StatusEffectsType.Slow) { _slowMod = effect.amount; }
    }

    public void OnRemoveEffect(StatusEffect effect)
    {
        if (effect.type == StatusEffectsType.Slow) { _slowMod = 1; }
    }

    public void OnStackEffect(StatusEffect effect)
    {
        if (effect.type == StatusEffectsType.Slow) { _slowMod = Mathf.Clamp(_slowMod * effect.amount, 0.33f, 1); }
    }

    private void OnDestroy()
    {
        
    }
}