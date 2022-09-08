using UnityEngine;
using System.Collections;

public class FlamethrowerFlame : MonoBehaviour, IHasTeam, ILiquidSensitive
{
    public IFlameOrigin owner;
    public bool creaturesOnly;
    public bool hitWalls;
    public bool ignoreDoors;
    public bool shrinkInWater;
    public ParticleSystem waterParticles;
    public Player player;
    private CircleCollider2D _circleCollider;
    private DamageCreatureTrigger _damageCreatureTrigger;
    public SpriteRenderer spriteRenderer { get; private set; }
    public SimpleAnimator simpleAnimator { get; private set; }
    public Animator animator { get; private set; }

    private Team _team;
    public Team team
    {
        get { return _team; }
        set
        {
            _team = value;
            if (_damageCreatureTrigger) { _damageCreatureTrigger.team = _team; }
        }
    }

    public bool inLiquid { get; set; }
    public bool electrifiesWater { get { return false; } }

    private float _origAnimSpeed;

    private void Awake()
    {
        animator = GetComponent<Animator>();
        simpleAnimator = GetComponent<SimpleAnimator>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        _circleCollider = GetComponent<CircleCollider2D>();
        _damageCreatureTrigger = GetComponent<DamageCreatureTrigger>();
        _damageCreatureTrigger.team = _team;
        _origAnimSpeed = simpleAnimator ? simpleAnimator.fps : animator.speed;
    }
        
    public void Spawn(int sortingOrder, float speed, float movingTime, float totalTime, Team team, bool dps)
    {
        this.team = team;
        Constants.SetCollisionForTeam(_circleCollider, _team, creaturesOnly);
        _damageCreatureTrigger.ignoreDoors = ignoreDoors;
        _damageCreatureTrigger.perSecond = dps;
        gameObject.SetActive(true);
        spriteRenderer.sortingOrder = sortingOrder;
        _circleCollider.enabled = true;

        if (simpleAnimator) simpleAnimator.Reset();

        transform.localPosition = Vector3.zero;
        StartCoroutine(FlameOn(speed, movingTime, totalTime));
    }

    public void WallSpawn(FlamethrowerFlame originFlame)
    {
        this.team = team;
        Constants.SetCollisionForTeam(_circleCollider, _team, creaturesOnly);
        _damageCreatureTrigger.ignoreDoors = ignoreDoors;
        if (spriteRenderer) { spriteRenderer.sortingOrder = originFlame.spriteRenderer.sortingOrder; }
        simpleAnimator.Reset();
        simpleAnimator.currentFrame = originFlame.simpleAnimator.currentFrame;
        gameObject.SetActive(true);
        transform.position = originFlame.transform.position;
        _circleCollider.enabled = true;
    }

    private IEnumerator FlameOn(float speed, float movingTime, float totalTime)
    {
        var timer = 0f;
        var startingPosition = transform.position;
        var currentPosition = transform.position; //to break parent tranforms movement
        var startingLocal = transform.localPosition;        
        var originalStartingPosition = startingPosition;
        var delta = Vector3.zero;
        var direction = (owner.target - transform.parent.position).normalized;
        bool hitWall = false;
        float timeMod;

        while (timer < movingTime && !hitWall)
        {
            timeMod = GetTimeMod();
            var progress = timer / movingTime;
            _circleCollider.radius = Mathf.Lerp(0.25f, 1, progress);
            direction = (owner.target - transform.parent.position).normalized;
            delta += direction * Time.deltaTime * speed;
            var facing = direction.x > 0 ? Quaternion.identity : Constants.flippedFacing;
            transform.rotation = Quaternion.FromToRotation(facing * Vector3.right, direction) * facing;
            startingPosition = owner.origin.TransformPoint(startingLocal);
            var targetPosition = startingPosition + delta;

            if (hitWalls)
            {
                var hit = Physics2D.Raycast(transform.position, direction, _circleCollider.radius, Constants.defaultMask);
                if (hit.collider)
                {
                    hitWall = true;
                    var deflectDirection = Vector3.Cross(hit.normal, Vector3.forward);
                    if (Physics2D.OverlapPoint(transform.position + deflectDirection * _circleCollider.radius, Constants.defaultMask))
                    {
                        deflectDirection = -deflectDirection;
                        StartCoroutine(WallFlameOn(direction, deflectDirection, timer, speed, movingTime, totalTime));
                    }
                    else
                    {
                        StartCoroutine(WallFlameOn(direction, deflectDirection, timer, speed, movingTime, totalTime));
                        var flame = owner.GetFlame();
                        flame.WallSpawn(this);
                        flame.StartCoroutine(flame.WallFlameOn(direction, -deflectDirection, timer, speed, movingTime, totalTime));
                    }
                    yield break;
                }
            }

            if (!player || !player.spinJumping)
            {
                var absY = Mathf.Abs(direction.y);
                var unmovedTarget = originalStartingPosition + delta;
                var modProg = Mathf.Clamp01(progress - absY);
                var antiModProg = Mathf.Clamp01(1 - (progress - absY));
                targetPosition.y = (unmovedTarget.y * modProg + targetPosition.y * antiModProg);
                currentPosition = Vector3.MoveTowards(currentPosition, targetPosition, speed * 1.5f * Time.deltaTime);
            }
            else
            {
                var trueTarget = Vector3.Lerp(startingPosition, owner.target, progress);
                var spinSpeed = speed * (1.5f + (3 * (1-progress)));
                currentPosition = Vector3.MoveTowards(currentPosition, trueTarget, spinSpeed * Time.deltaTime);
            }

            transform.position = currentPosition;

            timer += Time.deltaTime * timeMod;
            yield return null;
        }

        _circleCollider.enabled = false;

        while (timer < totalTime)
        {
            timeMod = GetTimeMod();
            currentPosition += transform.up * 2.5f * Time.deltaTime;
            transform.position = currentPosition;
            timer += Time.deltaTime * timeMod;
            yield return null;
        }

        gameObject.SetActive(false);
    }

    public float GetTimeMod()
    {
        if (inLiquid && shrinkInWater)
        {
            if (simpleAnimator) { simpleAnimator.fps = _origAnimSpeed * 2; }
            if (animator) { animator.speed = _origAnimSpeed * 2; }
            if (waterParticles && !waterParticles.isPlaying)
            {
                waterParticles.Clear();
                waterParticles.Play();
            }
            return 2;
        }
        else
        {
            if (simpleAnimator) { simpleAnimator.fps = _origAnimSpeed; }
            if (animator) { animator.speed = _origAnimSpeed; }
            if (waterParticles) waterParticles.Stop();
            return 1;
        }
    }

    private IEnumerator WallFlameOn(Vector3 originalDirection, Vector3 direction, float timer, float speed, float movingTime, float totalTime)
    {
        var currentPosition = transform.position; //to break parent tranforms movement
        RaycastHit2D hit;

        while (timer < movingTime)
        {
            var progress = timer / movingTime;
            _circleCollider.radius = Mathf.Lerp(0.25f, 1, progress);
            var facing = direction.x > 0 ? Quaternion.identity : Constants.flippedFacing;
            transform.rotation = Quaternion.FromToRotation(facing * Vector3.right, direction) * facing;

            hit = Physics2D.Raycast(transform.position, originalDirection, _circleCollider.radius, Constants.defaultMask);
            if (!hit.collider) direction = originalDirection;

            hit = Physics2D.Raycast(transform.position, direction, _circleCollider.radius, Constants.defaultMask);
            if (hit.collider)
            {
                direction = Vector3.Cross(hit.normal, Vector3.forward);
                if (Physics2D.OverlapPoint(transform.position + direction * _circleCollider.radius, Constants.defaultMask))
                {
                    direction = -direction;
                }   
            }

            currentPosition += direction * speed * Time.deltaTime;
            transform.position = currentPosition;

            timer += Time.deltaTime;
            yield return null;
        }

        _circleCollider.enabled = false;

        while (timer < totalTime)
        {
            currentPosition += direction * 2.5f * Time.deltaTime;
            transform.position = currentPosition;
            timer += Time.deltaTime;
            yield return null;
        }

        gameObject.SetActive(false);
    }

    public bool OnEnterLiquid(Water water)
    {
        inLiquid = shrinkInWater;
        return shrinkInWater;
    }

    public void OnExitLiquid() { inLiquid = false; }
}
