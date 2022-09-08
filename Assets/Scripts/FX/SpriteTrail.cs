using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class TrailSprite
{
    public SpriteRenderer spriteRender;
    public Vector3 spawnPosition;
    public float trueFadeTime;
}

public class SpriteTrail : MonoBehaviour
{
    public SpriteRenderer parentRenderer;
    public float spawnDistance = 0.1f;
    public float fadeTime = 1f;
    public int maxTrails = 10;
    public int sortOrderMod = 0;
    private bool _stopAndDeactivate;
    private Vector3 _lastPosition;
    private List<TrailSprite> _trailSprites;

    void Awake()
    {
        if (!parentRenderer)
        {
            parentRenderer = GetComponent<SpriteRenderer>();
        }

        _trailSprites = new List<TrailSprite>();
        _lastPosition = transform.position;
    }

    void Update()
    {
        if (!_stopAndDeactivate && Vector3.SqrMagnitude(transform.position - _lastPosition) > spawnDistance * spawnDistance)
        {
            SpawnTrailSprite();
            _lastPosition = transform.position;
        }

        int activeTrails = 0;
        foreach (var trailSprite in _trailSprites)
        {
            if (trailSprite.spriteRender.gameObject.activeInHierarchy)
            {
                activeTrails++;
                trailSprite.spriteRender.sprite = parentRenderer.sprite;
                trailSprite.spriteRender.transform.position = trailSprite.spawnPosition;
                trailSprite.spriteRender.material = parentRenderer.material;
                var color = trailSprite.spriteRender.color;
                color.a -= Time.deltaTime / trailSprite.trueFadeTime;
                trailSprite.spriteRender.color = color;
                if (color.a <= 0)
                {
                    trailSprite.spriteRender.gameObject.SetActive(false);
                }
            }
        }

        if (_stopAndDeactivate && activeTrails == 0)
        {
            _stopAndDeactivate = false;
            gameObject.SetActive(false);
        }
    }

    public TrailSprite NewTrailSprite()
    {
        var s = new GameObject().AddComponent<SpriteRenderer>();
        s.transform.parent = transform;
        s.transform.localScale = Vector3.one;
        s.gameObject.SetActive(false);
        s.name = "TrailSprite" + _trailSprites.Count;
        var trailSprite = new TrailSprite();
        trailSprite.spriteRender = s;
        _trailSprites.Add(trailSprite);
        return trailSprite;
    }
    
    public void Copy(SpriteTrail st)
    {
        fadeTime = st.fadeTime;
        sortOrderMod = st.sortOrderMod;
        maxTrails = st.maxTrails;
        spawnDistance = st.spawnDistance;
    }

    public void Start()
    {
        enabled = true;
        gameObject.SetActive(true);
        _stopAndDeactivate = false;
    }

    public void Stop()
    {
        _stopAndDeactivate = true;
    }

    public void SpawnTrailSprite()
    {
        if (parentRenderer.color.a <= 0)
        {
            return;
        }

        TrailSprite inactive = null;
        foreach (var trailSprite in _trailSprites)
        {
            if (!trailSprite.spriteRender.gameObject.activeInHierarchy)
            {
                inactive = trailSprite;
            }
        }

        if (inactive == null)
        {
            if (_trailSprites.Count < maxTrails)
            {
                inactive = NewTrailSprite();
            }
            else
            {
                return;
            }
        }

        inactive.spriteRender.gameObject.SetActive(true);
        inactive.spriteRender.color = parentRenderer.color;
        inactive.spriteRender.sortingOrder = parentRenderer.sortingOrder + sortOrderMod;
        inactive.spawnPosition = transform.position;
        inactive.spriteRender.transform.localRotation = Quaternion.identity;
        inactive.trueFadeTime = fadeTime / parentRenderer.color.a;
    }
}
