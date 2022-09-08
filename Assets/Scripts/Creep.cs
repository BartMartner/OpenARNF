using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public class Creep : MonoBehaviour
{
    public CreepStats stats;
    new public SpriteRenderer light;
    public SpriteRenderer splat;
    public bool recycle = true;
    
    private float _lifeTimer;
    private bool _skipUpdate;
    private bool _dying;
    private ParticleSystem _steam;
    private DamageCreatureTrigger _damageBounds;
    private TransformPulse _lightPulse;
    
    private ColorPingPong _colorPingPong;

    public void Awake()
    {
        _steam = GetComponentInChildren<ParticleSystem>();
        _damageBounds = GetComponentInChildren<DamageCreatureTrigger>();
        splat = GetComponentInChildren<SpriteRenderer>();
        _colorPingPong = GetComponentInChildren<ColorPingPong>();
        _lightPulse = light.GetComponent<TransformPulse>();
    }

    public void Spawn(Vector3 position, Quaternion rotation, CreepStats newStats)
    {
        _dying = false;
        _lifeTimer = 0;
        stats = newStats;
        _damageBounds.damage = stats.damage;
        _colorPingPong.color1 = stats.color1;
        _colorPingPong.color2 = stats.color2;
        light.color = stats.color1;

        _damageBounds.perSecond = stats.perSecond;
        _damageBounds.ignoreAegis = stats.perSecond;

        var steamColor = _steam.colorOverLifetime;
        var grad = new Gradient();
        grad.SetKeys(new GradientColorKey[] { new GradientColorKey(stats.color1, 0f), new GradientColorKey(stats.color2, 1f) }, new GradientAlphaKey[] { new GradientAlphaKey(1, 0), new GradientAlphaKey(0, 1) });
        steamColor.color = grad;

        var ceilScale = Mathf.Clamp(Mathf.CeilToInt((newStats.lightRadius / 50)) * 50, 50, 400);
        float scaleMod = newStats.lightRadius / ceilScale;
        string path = "Sprites/Lights/Light" + ceilScale + "x" + ceilScale;

        light.sprite = ResourcePrefabManager.instance.LoadSprite(path);
        light.transform.localScale = Vector3.one * scaleMod;

        _lightPulse.Refresh();

        transform.position = position;

        gameObject.SetActive(true);

        _damageBounds.team = stats.team;
        Constants.SetCollisionForTeam(_damageBounds.collider2D, _damageBounds.team, true);

        transform.localScale = Vector3.zero;
        transform.rotation = rotation;
        _steam.transform.rotation = Quaternion.identity;
        splat.transform.rotation = Quaternion.Euler(0, 0, Random.Range(0, 3) * 90);
        StartCoroutine(Grow());
    }

    public void Update()
    {
        if (!_skipUpdate && !_dying)
        {
            _lifeTimer += Time.deltaTime;
            if (_lifeTimer > stats.lifeTime)
            {
                StartCoroutine(Die());
            }
        }
    }

    private IEnumerator Grow()
    {
        _skipUpdate = true;
        var timer = 0f;
        transform.localScale = Vector3.zero;
        while (timer < stats.growTime)
        {
            timer += Time.deltaTime;
            transform.localScale = Vector3.Lerp(Vector3.zero, Vector3.one, timer/stats.growTime);
            yield return null;
        }

        _steam.Play();
        _damageBounds.enabled = true;
        _skipUpdate = false;
    }

    public void Kill()
    {
        if (!_dying)
        {
            StartCoroutine(Die());
        }
    }

    private IEnumerator Die()
    {
        _dying = true;
        _steam.Stop();
        _damageBounds.enabled = false;

        var timer = 0f;
        while(timer < stats.dieTime)
        {
            timer += Time.deltaTime;
            transform.localScale = Vector3.Lerp(Vector3.one, Vector3.zero, timer/ stats.dieTime);
            yield return null;
        }

        if (recycle)
        {
            _dying = false;
            gameObject.SetActive(false);
        }
        else
        {
            Destroy(gameObject);
        }
    }
}

[Serializable]
public class CreepStats
{
    public float growTime = 1;
    public float dieTime = 1;
    public float lifeTime = 1;
    public float damage = 1;
    public float lightRadius = 100;
    public bool perSecond;
    public Team team = Team.Enemy;
    public Color32 color1 = new Color32(192, 255, 64, 128);
    public Color32 color2 = new Color32(128, 255, 0, 96);

    public CreepStats(CreepStats original)
    {
        growTime = original.growTime;
        dieTime = original.dieTime;
        lifeTime = original.lifeTime;
        damage = original.damage;
        lightRadius = original.lightRadius;
        perSecond = original.perSecond;
        team = original.team;
        color1 = original.color1;
        color2 = original.color2;
    }
}

