using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Follower : MonoBehaviour, IDamageable, IHasTeam, IHasOnDestroy
{
    private DamageableState _state = DamageableState.Alive;
    public Vector3 position { get { return transform.position; } }

    public virtual DamageableState state
    {
        get
        {
            return _state;
        }

        set { Debug.Log("A follower's DamageableState cannot change"); }
    }

    public bool targetable { get { return true; } }

    public MajorItem type;
    public int followerIndex;
    public int positionNumber;
    public AudioClip hurtSound;
    public Player player;
    public bool skipAssignLayer;
    public Action onDestroy { get; set; }
    public Renderer mainRenderer { get; private set; }
    public FloatEvent onHurt;

    public virtual bool orbital
    {
        get { return false; }
    }

    public virtual Team team
    {
        get
        {
            return (player && !skipAssignLayer) ? player.team : Team.None;
        }

        set
        {
            Debug.LogWarning("Trying to set team on a Follower. This is not supported");
        }
    }

    Transform IHasOnDestroy.transform { get { return transform; } }

    protected AudioSource _audioSource;

    public virtual IEnumerator Start()
    {
        _audioSource = GetComponent<AudioSource>();
        mainRenderer = GetComponentInChildren<Renderer>();

        while (PlayerManager.instance == null && player == null)
        {
            yield return null;
        }

        transform.parent = null;

        if (player == null) { player = PlayerManager.instance.player1; }
        if (!skipAssignLayer)
        {
            gameObject.layer = player.gameObject.layer;
            var damageTriggers = GetComponentsInChildren<DamageCreatureTrigger>();
            var collider2D = GetComponent<Collider2D>();
            foreach (var trigger in damageTriggers)
            {
                trigger.team = player.team;
                Constants.SetCollisionForTeam(trigger.collider2D, player.team, true);
                if (collider2D) { Physics2D.IgnoreCollision(trigger.collider2D, collider2D); }
            }
        }
        SceneManager.MoveGameObjectToScene(gameObject, player.gameObject.scene);
        player.onRespawn += OnRespawn;
    }

    public virtual bool Hurt(float damage, GameObject source = null, DamageType damageType = DamageType.Generic, bool ignoreAegis = false)
    {
        if (damageType == DamageType.Hazard) { return false; }

        var hasTeam = source.GetComponent<IHasTeam>();

        if (hasTeam != null && hasTeam.team == team) { return false; }

        if (_audioSource && hurtSound && !_audioSource.isPlaying)
        {
            _audioSource.PlayOneShot(hurtSound);
        }

        if (onHurt != null) { onHurt.Invoke(damage); }
        return true;
    }

    public void ApplyStatusEffect(StatusEffect statusEffect, Team team) { }
    public void ApplyStatusEffects(IEnumerable<StatusEffect> statusEffects, Team team) { }

    public virtual void OnDestroy()
    {
        if(player) { player.onRespawn -= OnRespawn; }
        if (onDestroy != null) { onDestroy(); }
    }

    public virtual void OnRespawn() { }
}
