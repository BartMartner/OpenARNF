using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

public class Laser : MonoBehaviour, IHasTeam, ILaser
{
    private Team _team;
    public Team team
    {
        get
        {
            return _team;
        }

        set
        {
            _team = value;
            if (_damageTriggers == null) { _damageTriggers = GetComponentsInChildren<DamageCreatureTrigger>(); }

            if(_damageTriggers != null)
            {
                foreach (var trigger in _damageTriggers)
                {
                    trigger.team = _team;
                    Constants.SetCollisionForTeam(trigger.collider2D, _team);
                }
            }
        }
    }

    public DamageType damageType;
    public List<StatusEffect> statusEffects;

    public Team setTeam = Team.None;

    public float damage = 1;
    public float stopTime = 0.5f;
    public SpriteRenderer lazerStart;
    public SpriteRenderer lazerMid;
    public SpriteRenderer lazerEnd;
    public Sprite[] startSprites;
    public bool endEndsAtRightBounds;
    public bool fadeOut;
    public bool damageOnStop = true;
    public bool ignoreDoors = true;
    public LayerMask layerMask;
    public float range = 24f;
    public float scrollSpeed = 1;
    public float pixelLock = 2;
    public bool spriteTileMaterial;
    public bool managed { get; set; }
    private DamageCreatureTrigger[] _damageTriggers;
    private float _tiling;
    private float offsetX;
    private float _width;
    private float _defaultWidth;
    private IEnumerator _stopRoutine;

    public IHasOnDestroy _attachedTo;

    private void Start ()
    {
        _damageTriggers = GetComponentsInChildren<DamageCreatureTrigger>();

        //TODO: Figure out what's using this and get rid of it in favor of using the laser manager
        if (setTeam != Team.None) { _team = setTeam; }

        foreach (var damageTrigger in _damageTriggers)
        {
            Constants.SetCollisionForTeam(damageTrigger.collider2D, _team);
            damageTrigger.damage = damage;
            damageTrigger.team = _team;
        }

        if (_defaultWidth == 0) //default width may have been set in assignStats;
        {
            _width = _defaultWidth = lazerMid.transform.localScale.y;
        }
    }

    private void Update()
    {
        foreach (var damageTrigger in _damageTriggers)
        {
            damageTrigger.damage = damage;
            damageTrigger.damageType = damageType;
            damageTrigger.statusEffects = statusEffects;
            damageTrigger.ignoreDoors = ignoreDoors;
            damageTrigger.ignoreAegis = team != Team.Enemy;
            damageTrigger.perSecond = team != Team.Enemy;
        }

        lazerMid.sortingOrder = lazerStart.sortingOrder - 1;
        lazerEnd.sortingOrder = lazerStart.sortingOrder + 1;

        var raycast = Physics2D.Raycast(transform.position, transform.right, range, layerMask);
        Vector3 endPoint = raycast.collider ? (Vector3)raycast.point : transform.position + transform.right * range;
        lazerEnd.transform.localPosition = transform.InverseTransformPoint(endPoint);

        if (endEndsAtRightBounds)
        {
            lazerEnd.transform.localPosition -= Vector3.right * lazerEnd.sprite.bounds.extents.x;
        }

        lazerEnd.transform.localScale = Vector3.one * _width;

        var startWidth = 1f;
        startWidth = Mathf.Clamp(Vector2.Distance(transform.position, endPoint), 0.1f, lazerStart.sprite.bounds.size.x);
        lazerStart.transform.localPosition = Vector3.right * (startWidth / 2);

        var distance = Vector2.Distance(lazerStart.transform.position, endPoint); ;

        var startOffset = 0f;
        startOffset = lazerStart.sprite.bounds.extents.x;
        distance -= startOffset;
        
        var spriteSize = lazerMid.sprite.bounds.size.x;
        _tiling = distance / spriteSize;

        var startSize = 1f;
        startSize = startWidth / lazerStart.sprite.bounds.size.x;
        

        lazerStart.transform.localScale = new Vector3(startSize, _width, 1);

        if (startSize >= 1)
        {
            lazerMid.gameObject.SetActive(true);
            lazerMid.transform.localScale = new Vector3(_tiling, _width, 1);
            lazerMid.transform.localPosition = lazerStart.transform.localPosition + Vector3.right * (distance * 0.5f + startOffset);

            if (spriteTileMaterial)
            {
                lazerMid.material.SetFloat("_RepeatX", _tiling);
            }
            else
            {
                lazerMid.material.mainTextureScale = new Vector2(_tiling, 1);
            }
        }
        else
        {
            lazerMid.gameObject.SetActive(false);
        }

        if (scrollSpeed > 0)
        {            
            offsetX = ((Mathf.Abs(offsetX) + Time.deltaTime * Mathf.Abs(scrollSpeed)) % _tiling) * -Mathf.Sign(scrollSpeed);
            var pUnit = 16 / pixelLock;
            var pixelOffsetX = Mathf.Round(offsetX * pUnit) / pUnit;

            if (spriteTileMaterial)
            {
                lazerMid.material.SetFloat("_OffsetX", pixelOffsetX);
            }
            else
            {
                lazerMid.material.mainTextureOffset = new Vector2(pixelOffsetX, 0);
            }
            
            if(startSprites.Length > 0)
            {
                var frame = (int)((Mathf.Abs(pixelOffsetX) % 1)* startSprites.Length) % startSprites.Length;
                if (frame > 0 && frame < startSprites.Length)
                {
                    lazerStart.sprite = startSprites[frame];
                }
            }
        }
    }

    private void OnEnable()
    {
        if (_damageTriggers != null)
        {
            foreach (var d in _damageTriggers) { d.enabled = true; }
        }
    }

    public void ImmediateStop()
    {
        if (_attachedTo != null) { _attachedTo.onDestroy -= ImmediateStop; }

        if(_stopRoutine != null)
        {
            StopCoroutine(_stopRoutine);
            _stopRoutine = null;
        }

        _width = _defaultWidth;
        lazerStart.color = Color.white;
        lazerMid.color = Color.white;
        lazerEnd.color = Color.white;

        if (LaserManager.instance && managed)
        {
            LaserManager.instance.Recycle(this);
        }
        else
        {
            gameObject.SetActive(false);
        }
    }

    public void Stop()
    {
        //checking this due to weird null ref exception
        if (this) 
        {
            if (gameObject.activeSelf && enabled && _stopRoutine == null)
            {
                if (!damageOnStop)
                {
                    foreach (var d in _damageTriggers) { if (d) { d.enabled = false; } }
                }
                _stopRoutine = fadeOut ? FadeAndStop() : ShrinkAndStop();
                StartCoroutine(_stopRoutine);
            }
            else
            {
                ImmediateStop();
            }
        }
    }

    public IEnumerator FadeAndStop()
    {
        var alpha = 1f;
        var timer = stopTime;
        Color c;

        while (timer > 0)
        {
            timer -= Time.deltaTime;
            alpha = timer / stopTime;

            c = Color.white;
            c.a = alpha;

            lazerStart.color = c;
            lazerMid.color = c;
            lazerEnd.color = c;
            yield return null;
        }

        ImmediateStop();
    }

    public IEnumerator ShrinkAndStop()
    {
        while(_width > 0)
        {
            _width -= (Time.deltaTime * _defaultWidth) / stopTime;
            yield return null;
        }

        ImmediateStop();
    }

    public void SetDefaultWidth(float widthOverride)
    {
        _width = _defaultWidth = widthOverride;
    }

    public void AssignStats(LaserStats stats)
    {
        enabled = true; //sometimes this gets set false somehow...
        SetDefaultWidth(stats.width);
        lazerEnd.transform.localPosition = Vector3.zero;
        lazerEnd.transform.localScale = Vector3.zero;
        lazerStart.transform.localScale = new Vector3(0, _width, 1);
        lazerMid.transform.localScale = new Vector3(0, _width, 1);
        lazerStart.color = Color.white;
        lazerMid.color = Color.white;
        lazerEnd.color = Color.white;

        team = stats.team;
        damage = stats.damage;
        stopTime = stats.stopTime;
        damageType = stats.damageType;

        statusEffects = stats.statusEffects;
        layerMask = stats.layerMask;
        SetSortingOrder(stats.sortingLayerName, stats.sortingOrder);
        fadeOut = stats.stopType == LaserStopType.Fade;
        ignoreDoors = stats.ignoreDoors;
        range = stats.range;

        if(_stopRoutine != null)
        {
            Debug.LogError("_stopRouting for laser " + gameObject.name + 
                " still had a value when Assign Stats was called. Stopping and setting _stopRoutine null.");
            StopCoroutine(_stopRoutine);
            _stopRoutine = null;
        }
    }

    public void AttachTo(IHasOnDestroy attachTo)
    {
        _attachedTo = attachTo;
        transform.SetParent(_attachedTo.transform);
        _attachedTo.onDestroy += ImmediateStop;
    }

    public void SetSortingOrder(string sortingLayer, int sortingOrder)
    {
        lazerStart.sortingLayerID = lazerMid.sortingLayerID = lazerEnd.sortingLayerID = SortingLayer.NameToID(sortingLayer);
        lazerStart.sortingOrder = sortingOrder + 1;
        //Set in update
        //lazerMid.sortingOrder = sortingOrder;
        //lazerEnd.sortingOrder = sortingOrder + 2;
    }

    public void OnDestroy()
    {
        if (_attachedTo != null) { _attachedTo.onDestroy -= ImmediateStop; }
    }
}
