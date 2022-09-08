using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Rewired;

public class CoOpPlayer : MonoBehaviour, IDamageable, IPausable, IHasTeam
{
    public Player owner { get; private set; }
    public Vector3 spawnOffset
    {
        get
        {
            if (owner && controller != null)
            {
                switch (controller.id)
                {
                    case 2:
                        return owner.transform.up * 0.5f;
                    case 3:
                        return -owner.transform.up * 0.5f;
                    default:
                        return -owner.transform.right * 0.5f;
                }
            }
            else
            {
                return Vector3.zero;
            }
        }
    }

    public Rewired.Player controller { get; set; }

    private LineRenderer _teleportTrail;
    private bool _flashing;
    private bool _spawning;
    private bool _justAttacked = false;
    private SpriteRenderer[] _renderers;
    private Controller2D _controller2D;
    private AudioSource _audioSource;
    private Animator _animator;
    private float _offScreenTime;
    private bool _aegisActive;

    new public Collider2D collider2D { get { return _controller2D.collider2D; } }

    public DamageableState state
    {
        get { return DamageableState.Alive; }
        set { state = value; }
    }

    public bool targetable { get { return true; } }
    public Vector3 position { get { return transform.position; } }

    public Team team
    {
        get
        {
            if (owner)
            {
                return owner.team;
            }
            else
            {
                return Team.Player;
            }
        }

        set
        {
            Debug.LogWarning("Set on CoOpPlayer not implemented.");
        }
    }

    private void Awake()
    {
        _controller2D = GetComponent<Controller2D>();
        _audioSource = GetComponent<AudioSource>();
        _renderers = GetComponentsInChildren<SpriteRenderer>(true);
        _animator = GetComponent<Animator>();
        _teleportTrail = GetComponentInChildren<LineRenderer>();
        _teleportTrail.gameObject.SetActive(false);
    }

    public void Start()
    {
        if (LayoutManager.instance)
        {
            LayoutManager.instance.onTransitionComplete += JumpToPlayer;
        }
    }

    public void JumpToPlayer()
    {
        if (owner) { transform.position = owner.transform.position + spawnOffset; }
    }

    public void Spawn(Player player, Rewired.Player controller)
    {
        owner = player;
        this.controller = controller;
        StartCoroutine(SpawnRoutine());
    }

    public void Despawn(bool destroy)
    {
        StartCoroutine(DespawnRoutine(destroy));
    }

    private IEnumerator SpawnRoutine(float speedMod = 1)
    {
        _spawning = true;
        transform.position = owner.transform.position + spawnOffset;
        _animator.speed = speedMod;
        _animator.Play("Spawn");
        yield return new WaitForSeconds(1/speedMod);
        _animator.speed = 1;
        _spawning = false;
    }

    private IEnumerator DespawnRoutine(bool destroy, float speedMod = 1)
    {
        _spawning = true;
        _animator.Play("Despawn");
        _animator.speed = speedMod;
        yield return new WaitForSeconds(1 / speedMod);
        _animator.speed = 1;
        _spawning = false;
        if (destroy) { Destroy(gameObject); }
    }

    public void Update()
    {
        if(_spawning || !owner || controller == null)
        {
            return;
        }

        if(!_renderers[0].isVisible)
        {
            _offScreenTime += Time.deltaTime;
            if(_offScreenTime > 1f)
            {
                Debug.Log("Trying to teleport!");
                _offScreenTime = 0;
                StartCoroutine(TeleportToPlayer());
                StartCoroutine(AegisFlash(1));
            }
        }
        else
        {
            _offScreenTime = 0;
        }

        if (owner.state != DamageableState.Alive) { Despawn(true); }

        if (controller.GetButtonDown("CoOpTeleport"))
        {
            StartCoroutine(TeleportToPlayer());
        }

        _controller2D.enabled = true;

        float moveXAxis = controller.GetAxis("CoOpMoveHorizontal");
        float moveYAxis = controller.GetAxis("CoOpMoveVertical");
        float shootXAxis = controller.GetAxis("CoOpShootHorizontal");
        float shootYAxis = controller.GetAxis("CoOpShootVertical");
        
        if(owner.confused)
        {
            moveXAxis = -moveXAxis;
            moveYAxis = -moveYAxis;
            shootXAxis = -shootXAxis;
            shootYAxis = -shootYAxis;
        }

        var deadZone = 0.1f;
        var direction = new Vector3(moveXAxis, moveYAxis, 0);
        var dMag = direction.magnitude;
        if (dMag > deadZone)
        {
            //var adjustedDirection = direction.normalized * ((dMag - deadZone) / (1 - deadZone));
            var speed = owner.maxSpeed * 1.25f;
            var movement = direction * speed * Time.deltaTime;
            _controller2D.Move(movement);
        }

        var shotDirection = new Vector3(shootXAxis, shootYAxis);
        var shotMag = shotDirection.magnitude;
        if (!_justAttacked && shotMag > deadZone)
        {
            _audioSource.pitch = 1.5f;
            _audioSource.PlayOneShot(owner.attackSound, 0.75f);
            var stats = new ProjectileStats(owner.projectileStats);
            stats.damage = stats.damage * 0.75f;
            stats.size = stats.size * 0.75f;
            stats.canOpenDoors = false;

            if (owner.arcShots > 0)
            {
                ProjectileManager.instance.ArcShoot(stats, transform.position, shotDirection, owner.arcShots, owner.fireArc);
            }
            else
            {
                ProjectileManager.instance.Shoot(stats, transform.position, shotDirection);
            }
            
            StartCoroutine(JustAttacked());
        }
    }

    private IEnumerator TeleportToPlayer()
    {
        yield return StartCoroutine(DespawnRoutine(false, 2));
        StartCoroutine(TeleportTrail(transform.position, owner.transform.position + spawnOffset));
        yield return StartCoroutine(SpawnRoutine(2));
    }

    private IEnumerator TeleportTrail(Vector3 start, Vector3 end)
    {
        _teleportTrail.gameObject.SetActive(true);

        _teleportTrail.SetPositions(new Vector3[] { start, end });

        var timer = 0f;
        var originalStart = start;
        while (timer < 0.25f)
        {
            start = Vector3.Lerp(originalStart, end, timer / 0.25f);
            _teleportTrail.SetPositions(new Vector3[] { start, end });
            timer += Time.deltaTime;
            yield return null;
        }

        _teleportTrail.gameObject.SetActive(false);
    }

    private IEnumerator JustAttacked()
    {
        _justAttacked = true;
        yield return new WaitForSeconds(owner.attackDelay);
        _justAttacked = false;
        _audioSource.pitch = 1f;
    }

    public void SetMaterial(Material material)
    {
        foreach (var r in _renderers) { r.material = material; }
    }

    public bool Hurt(float damage, GameObject source = null, DamageType damageType = DamageType.Generic, bool ignoreAegis = false)
    {
        if (_aegisActive) return false;

        var hurt = owner.HurtKnocback(damage, gameObject, damageType, ignoreAegis, false);

        if (hurt)
        {
            StartCoroutine(AegisFlash(owner.aegisTime));

            if (!_flashing)
            {
                StartCoroutine(Flash(1, 0.1f, Constants.damageFlashColor, 0.5f));
            }
        }

        return hurt;
    }

    private IEnumerator AegisFlash(float time)
    {
        var timer = 0f;        
        var flashTime = 0.05f;

        _aegisActive = true;

        while (timer < time)
        {
            foreach (var renderer in _renderers)
            {
                renderer.enabled = false;
            }

            yield return new WaitForSeconds(flashTime);
            timer += flashTime;

            foreach (var renderer in _renderers)
            {
                renderer.enabled = true;
            }

            yield return new WaitForSeconds(flashTime);
            timer += flashTime;
        }

        _aegisActive = false;

        foreach (var renderer in _renderers)
        {
            renderer.enabled = true;
        }
    }

    public virtual IEnumerator Flash(int flashes, float time, Color color, float amount)
    {
        if (_renderers == null || _renderers.Length == 0)
        {
            yield break;
        }

        _flashing = true;
        var flashCounter = 0;

        while (flashCounter < flashes)
        {
            flashCounter++;

            foreach (var renderer in _renderers)
            {
                if (renderer != null)
                {
                    renderer.material.SetColor("_FlashColor", color);
                    renderer.material.SetFloat("_FlashAmount", amount);
                }
            }
            yield return new WaitForSeconds(time);

            foreach (var renderer in _renderers)
            {
                if (renderer != null)
                {
                    renderer.material.SetFloat("_FlashAmount", 0);
                }
            }
            yield return new WaitForSeconds(time);
        }

        foreach (var renderer in _renderers)
        {
            if (renderer != null)
            {
                renderer.material.SetColor("_FlashColor", color);
                renderer.material.SetFloat("_FlashAmount", 0);
            }
        }
        _flashing = false;
    }

    public void Pause()
    {
        enabled = false;
        _animator.speed = 0;
    }

    public void Unpause()
    {
        enabled = true;
        _animator.speed = 1;
    }

    private void OnDestroy()
    {
        if (LayoutManager.instance)
        {
            LayoutManager.instance.onTransitionComplete -= JumpToPlayer;
        }

        if(PlayerManager.instance)
        {
            PlayerManager.instance.CalculateCoOpDamageMod();
        }
    }

    public void ApplyStatusEffect(StatusEffect statusEffect, Team team)
    {
        owner.ApplyStatusEffect(statusEffect, team);
    }

    public void ApplyStatusEffects(IEnumerable<StatusEffect> statusEffects, Team team)
    {
        owner.ApplyStatusEffects(statusEffects, team);
    }
}
