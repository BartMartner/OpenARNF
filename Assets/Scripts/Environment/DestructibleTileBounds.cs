using UnityEngine;
using System.Collections;
using CreativeSpore.SuperTilemapEditor;
using System;
using System.Collections.Generic;
using UnityEngine.Events;

[RequireComponent(typeof(BoxCollider2D))]
public class DestructibleTileBounds : Damageable, ISetByDamageType
{
    [Header ("Destructable Tile")]
    public STETilemap tilemap;
    public SpriteRenderer tile;
    public BoxCollider2D trigger;
    public float regenerationTime;
    private Collider2D _tileBounds;
    private Animator _animator;
    private GibOnDeath _gibOnDeath;

    public UnityEvent onRegenerate;

    protected override void Start()
    {
        _tileBounds = tile.GetComponent<Collider2D>();
        _animator = tile.GetComponent<Animator>();
        _gibOnDeath = GetComponent<GibOnDeath>();
        tile.gameObject.SetActive(false);
        tile.color = Color.white;

        if (!tilemap)
        {
            tilemap = transform.parent.parent.GetComponent<STETilemap>();

            if(!tilemap)
            {
                tilemap = GameObject.FindGameObjectWithTag("MainTilemap").GetComponent<STETilemap>();
            }
        }

        destroyOnDeath = false;
    }

    public override void OnImmune(DamageType damageType)
    {
        base.OnImmune(damageType);

        if (health > 0)
        {
            if (tilemap)
            {
                if (_gibOnDeath) { _gibOnDeath.Gib(); }
                DestroyTiles();
            }

            Regenerate();
        }
    }

    public override void EndDeath()
    {
        trigger.enabled = false;
        tile.gameObject.SetActive(false);

        if (tilemap)
        {
            DestroyTiles();
        }

        if (regenerationTime > 0)
        {
            StartCoroutine(RegenerateAfterTime());
        }
        else
        {
            Destroy(gameObject);
        }

        base.EndDeath();        
    }

    private void DestroyTiles()
    {
        if (tilemap)
        {
            tilemap.Erase(tilemap.transform.InverseTransformPoint(transform.position));
            tilemap.UpdateMesh();
            tilemap = null; //so this doesn't get called again
        }

        var slot = SaveGameManager.activeSlot;
        var game = SaveGameManager.activeGame;
        if (slot != null && slot.totalBlocksDestroyed < long.MaxValue && (game == null || game.allowAchievements))
        {
            slot.totalBlocksDestroyed++;
        }
    }

    private IEnumerator RegenerateAfterTime()
    {
        yield return new WaitForSeconds(regenerationTime-1);

        var bounds = _tileBounds.bounds;
        bounds.size = Vector2.one; //if _tileBounds has never been enabled this would be  0,0
        while (PlayerManager.instance.IntersectsAnyPlayerBounds(bounds))
        {
            yield return null;
        }

        _tileBounds.enabled = false;
        tile.gameObject.SetActive(true);
        _animator.SetTrigger("Respawn");

        yield return new WaitForSeconds(1f);

        //var timer = 0f;
        //var transparent = Constants.blasterGreen;
        //transparent.a = 0;

        //while (timer < 1)
        //{
        //    timer += Time.deltaTime;
        //    tile.color = Color.Lerp(transparent, Color.white, timer);
        //    yield return null;
        //}

        while (PlayerManager.instance.IntersectsAnyPlayerBounds(bounds))
        {
            yield return new WaitForSeconds(0.5f);
        }

        Regenerate();
    }

    public void Regenerate()
    {
        _tileBounds.enabled = true;
        health = maxHealth;
        state = DamageableState.Alive;
        tile.color = Color.white;
        tile.gameObject.SetActive(true);
        trigger.enabled = true;

        if(onRegenerate != null)
        {
            onRegenerate.Invoke();
        }
    }

    public void SetByDamageType(DamageType damageType)
    {
        if (damageType == DamageType.Generic || damageType == 0)
        {
            immunities = 0;
        }
        else
        {
            immunities = ~damageType;
        }

        var overrideController = Resources.Load<AnimatorOverrideController>("Animations/DestructibleBlocks/" + (damageType == 0 ? "Generic" : damageType.ToString()) + "Block");

        if(!_gibOnDeath) { _gibOnDeath = GetComponent<GibOnDeath>(); }

        switch(damageType)
        {
            case DamageType.Fire:
                deathFX = FXType.FlameSmall;
                _gibOnDeath.gibType = GibType.PaleMeat;
                break;
            case DamageType.Mechanical:
                deathFX = FXType.BloodSplatSmall;
                _gibOnDeath.gibType = GibType.Meat;
                break;
            case DamageType.Explosive:
                deathFX = FXType.ExplosionSmall;
                _gibOnDeath.gibType = GibType.ExplosiveDoorShield;
                break;
        }

        if(!_animator && tile) _animator = tile.GetComponent<Animator>();
        if(_animator) _animator.runtimeAnimatorController = overrideController;
        gameObject.name = "Destructible" + damageType + "Tile";
    }

    public override void ApplyStatusEffect(StatusEffect statusEffect, Team team) { return; }
    public override void ApplyStatusEffects(IEnumerable<StatusEffect> statusEffects, Team team) { return; }
}
