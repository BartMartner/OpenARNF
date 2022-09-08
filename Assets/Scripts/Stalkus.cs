using Anima2D;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Stalkus : MonoBehaviour, IPausable, IReactsToStatusEffect
{
    public StalkusTentacle[] tentacles;
    public LaserEye[] eyes;
    public Transform body;
    public float speed = 5;
    public IEnumerator _move;
    public Texture2D[] hurtTextures;
    public AudioClip stab;
    public AudioClip step;

    private bool _moving;
    private bool _acting;
    private int currentTentacleIndex = 0;
    private LayerMask _layerMask;
    private Transform closestTarget;
    private Renderer[] _renderers;
    private SpriteMeshInstance[] _spriteMeshInstances;
    private int _eyeCount;
    private float _eyeProgress;
    private bool _justActed;
    private Animator _animator;
    private AudioSource _audioSource;
    private float _moveTime = 0.5f;
    private bool _paused;
    private float _slowMod = 1;

    public void Start()
    {
        _animator = GetComponent<Animator>();
        _audioSource = GetComponentInChildren<AudioSource>();
        _renderers = GetComponentsInChildren<Renderer>();
        _spriteMeshInstances = GetComponentsInChildren<SpriteMeshInstance>();
        var copy = new Material(body.GetComponent<Renderer>().material);
        foreach (var r in _renderers) { r.material = copy; }
        foreach (var s in _spriteMeshInstances) { s.sharedMaterial = copy; }

        _layerMask = LayerMask.GetMask("Default", "Doors");
    }

    // Update is called once per frame
    public void Update()
    {
        var c = eyes.Count((e) => e.state == DamageableState.Alive);
        if (_eyeCount != c)
        {
            _eyeCount = c;
            _eyeProgress = (eyes.Length - _eyeCount) / (float)eyes.Length;
            var index = (int)(_eyeProgress * (hurtTextures.Length-1));
            foreach (var r in _renderers) { r.material.SetTexture("_Palette", hurtTextures[index]); }
            foreach (var s in _spriteMeshInstances) { s.sharedMaterial.SetTexture("_Palette", hurtTextures[index]); }
            speed = Mathf.Lerp(4.5f, 7.75f, _eyeProgress);
            _animator.speed = 1 + _eyeProgress;
            _moveTime += _eyeProgress * 0.5f;
        }

        if (_acting) return;

        if(!_moving)
        {
            closestTarget = PlayerManager.instance.GetClosestPlayerTransform(body.position);
            Vector2 position = closestTarget.position;

            var target = PlayerManager.instance.GetClosestInArc(body.transform.position, body.right, 6, 120);

            if (target != null && !_justActed)
            {
                _justActed = _eyeProgress < 1; //can stab multiple times when no eyes

                var player = target.GetComponent<Player>();

                if (Random.value > _eyeProgress && player && !player.fatigued)
                {
                    StartCoroutine(Laser());
                }
                else
                {
                    StartCoroutine(Stab());
                }
            }
            else
            {
                _justActed = false;
                if (_move != null) { StopCoroutine(_move); }
                if (_eyeCount > 0)
                {
                    var upHit = Physics2D.Raycast(position, Vector2.up, 100, _layerMask);
                    var upPoint = upHit.collider ? upHit.point + Vector2.down * 1.5f : position + Vector2.down;
                    var downHit = Physics2D.Raycast(position, Vector2.down, 100, _layerMask);
                    var downPoint = downHit.collider ? downHit.point + Vector2.up * 1.5f : position + Vector2.up;
                    position = Vector3.Lerp(upPoint, downPoint, Mathf.PingPong(Time.time * 0.2f, 1));
                }
                _move = MoveTorwards(position);
                StartCoroutine(_move);
            }
        }
    }

    public IEnumerator Laser()
    {
        _acting = true;

        body.rotation = closestTarget.position.x > body.position.x ? Quaternion.identity : Constants.flippedFacing;
        yield return new WaitForSeconds(0.25f);

        var target = PlayerManager.instance.GetClosestInArc(body.position, body.right, 12, 120);
        if (target)
        {
            Vector2 position = target.position;

            foreach (var eye in eyes)
            {
                if (eye.state == DamageableState.Alive)
                {
                    if (target)
                    {
                        eye.Shoot(position + Random.insideUnitCircle * 0.75f);
                        yield return new WaitForSeconds(0.115f);
                    }
                }
            }
        }

        yield return new WaitForSeconds(1.5f - _eyeProgress);

        _acting = false;
    }

    public IEnumerator Stab()
    {
        _acting = true;
        var target = PlayerManager.instance.GetClosestInArc(body.position, body.right, 6, 180);
         
        if (target)
        {
            StalkusTentacle currentTentacle = null;
            if (currentTentacle == null) { currentTentacle = tentacles[currentTentacleIndex]; }
            var direction = (target.position - body.position).normalized;
            var tentacleT = currentTentacle.tentacleIK.transform;
            var pullBackStart = currentTentacle.tentacleIK.transform.position;
            var pullBackEnd = body.position - direction * 6;
            var timer = 0f;
            var time = Mathf.Lerp(0.125f, 0.45f, _eyeProgress);
            while (timer < time)
            {
                timer += Time.deltaTime;
                tentacleT.position = Vector3.Lerp(pullBackStart, pullBackEnd, timer / time);
                direction = (target.position - tentacleT.position).normalized;
                currentTentacle.clawIK.transform.position = tentacleT.position + direction * 2;
                yield return null;
            }

            var holdTime = Mathf.Lerp(0f, 0.125f, _eyeProgress);
            if (holdTime > 0) yield return new WaitForSeconds(holdTime);

            _audioSource.PlayOneShot(stab);

            var t = body.transform.position + direction * 6f;
            timer = 0;
            time = 0.125f;
            while (timer < time)
            {
                timer += Time.deltaTime;
                tentacleT.position = Vector3.Lerp(pullBackEnd, t, timer / time);
                currentTentacle.clawIK.transform.position = tentacleT.position + direction * 2;
                yield return null;
            }

            yield return new WaitForSeconds(0.5f);
        }

        _acting = false;
    }

    public IEnumerator MoveTorwards(Vector3 position)
    {
        _moving = true;

        StalkusTentacle currentTentacle = null;
        Vector3 walkTarget = Vector3.zero;
        Vector3 tentacleTarget = Vector3.zero;
        Vector3 originalPosition = Vector3.zero;
        Vector3 footArc = Vector3.zero;
        var stepTimer = 0f;
        var eProgress = (eyes.Length - _eyeCount) / (float)eyes.Length;
        var stepTime = 0.175f + 0.075f * (1-eProgress);
        var direction = (position -body.position).normalized;
        body.rotation = position.x > body.position.x ? Quaternion.identity : Constants.flippedFacing;

        var timer = 0f;

        bool endWalk = false;
        while (!endWalk)
        {
            timer += Time.deltaTime;
            
            if (currentTentacle == null)
            {
                stepTimer = 0f;
                currentTentacle = tentacles[currentTentacleIndex];

                var hits = new List<Vector3>();
                var hit1 = Physics2D.Raycast(body.position, new Vector2(1, 1).normalized, 6, _layerMask);
                if (hit1.collider != null) hits.Add(hit1.point);
                var hit2 = Physics2D.Raycast(body.position, new Vector2(-1, 1).normalized, 6, _layerMask);
                if (hit2.collider != null) hits.Add(hit2.point);
                var hit3 = Physics2D.Raycast(body.position, new Vector2(1, -1).normalized, 6, _layerMask);
                if (hit3.collider != null) hits.Add(hit3.point);
                var hit4 = Physics2D.Raycast(body.position, new Vector2(-1, -1).normalized, 6, _layerMask);
                if (hit4.collider != null) hits.Add(hit4.point);
                originalPosition = (Vector2)currentTentacle.clawIK.transform.position;
                var bestD = 0f;

                foreach (var h in hits)
                {
                    var d = Vector2.SqrMagnitude(h - currentTentacle.transform.position);
                    if (d > bestD)
                    {
                        bestD = d;
                        walkTarget = h;
                    }
                }

                footArc = originalPosition + (walkTarget - originalPosition) * 0.5f;
                footArc = footArc + (body.position - footArc).normalized * 1f;
            }

            stepTimer += Time.deltaTime;
            var progress = stepTimer / stepTime;
            var m1 = Vector3.Lerp(originalPosition, footArc, progress);
            var m2 = Vector3.Lerp(footArc, walkTarget, progress);
            tentacleTarget = body.position + (walkTarget - body.position).normalized;
            currentTentacle.transform.position = Vector3.MoveTowards(currentTentacle.transform.position, tentacleTarget, speed * Time.deltaTime);
            currentTentacle.clawIK.transform.position = Vector3.Lerp(m1, m2, Mathf.SmoothStep(0, 1, progress));

            if (currentTentacle.clawIK.transform.position == walkTarget)
            {
                currentTentacle = null;
                currentTentacleIndex = (currentTentacleIndex + 1) % tentacles.Length;
                if (timer > _moveTime) endWalk = true;
                _audioSource.PlayOneShot(step);
            }

            var actualSpeed = speed * _slowMod;
            body.position = Vector3.MoveTowards(body.position, position, actualSpeed * Time.deltaTime);

            foreach (var t in tentacles)
            {
                var origClaw = t.clawIK.transform.position;
                var tentDir = (body.transform.position - origClaw).normalized;
                t.tentacleIK.transform.position = origClaw + tentDir * 2;
                t.clawIK.transform.position = origClaw;
                t.transform.localRotation = t.tentacleIK.transform.position.y > body.position.y ? Constants.flippedFacing : Quaternion.identity;
            }

            if (body.position == position) endWalk = true;

            yield return null;
        }

        _moving = false;
    }

    public virtual void Pause()
    {
        if (enabled)
        {
            _paused = true;
            enabled = false;
        }
    }

    public virtual void Unpause()
    {
        if (_paused)
        {
            enabled = true;
        }
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
}
