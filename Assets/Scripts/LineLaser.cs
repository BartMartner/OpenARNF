using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(LineRenderer))]
[ExecuteInEditMode]
public class LineLaser : MonoBehaviour, IHasTeam, ILaser
{
    public LayerMask damageLayerMask;
    public List<Vector3> positions = new List<Vector3>();
    public SpriteRenderer blast;
    public float scrollSpeed = 0f;
    public float sineMag;
    public float sineFrequency = 2;
    public float sineScroll = 1f;
    public float sineOffset = 0f;
    public Material material;
    public Sprite sprite;
    public Texture2D palette;
    public Color lightColor;

    [Header("Stat Managed")]
    public float damagePerSecond;
    public DamageType damageType;
    public LayerMask collisionLayerMask;
    public float range = 24f;
    public float width = 0.75f;
    public int sortingOrder;
    public string sortingLayerName;
    public float stopTime = 0.25f;

    private LineRenderer _lineRenderer;
    private Material _materialInstance;
    private Material _blastMaterialInstance;
    private HashSet<IDamageable> _hasHit = new HashSet<IDamageable>();
    private RaycastHit2D[] _damCastResults = new RaycastHit2D[8];
    private LineRendererDouble _double;
    private bool _stopping;

    private Team _team;
    public Team team
    {
        get { return _team; }
        set
        {
            _team = value;
            switch(_team)
            {
                case Team.DeathMatch0:
                case Team.DeathMatch1:
                case Team.DeathMatch2:
                case Team.DeathMatch3:
                case Team.Player:
                    damageLayerMask = LayerMask.GetMask("Enemy");
                    break;
                case Team.Enemy:
                    damageLayerMask = LayerMask.GetMask("Player");
                    break;
                default:
                    damageLayerMask = LayerMask.GetMask("Default");
                    break;
            }
        }
    }
    public bool managed { get; set; }
    private IHasOnDestroy _attachedTo;

    private void Awake()
    {
        _lineRenderer = GetComponent<LineRenderer>();
        SetupMaterial();
    }

    private void SetupMaterial()
    {
        if (_materialInstance == null) { _materialInstance = new Material(material); }
        if (_blastMaterialInstance == null) { _blastMaterialInstance = new Material(material); }

        _materialInstance.shader = material.shader;
        _materialInstance.mainTexture = sprite.texture;
        _lineRenderer.sharedMaterial = _materialInstance;
        _lineRenderer.sortingOrder = sortingOrder;
        blast.sharedMaterial = _blastMaterialInstance;
        blast.sortingOrder = sortingOrder + 1;
        
        if (palette)
        {
            if(!_double) _double = GetComponentInChildren<LineRendererDouble>();
            if (_double)
            {
                var r = _double.GetComponent<LineRenderer>();
                r.startColor = lightColor;
                r.endColor = lightColor;
            }
            _blastMaterialInstance.SetTexture("_Palette", palette);
            _materialInstance.SetTexture("_Palette", palette);
        }
    }

    void Update()
    {
        if (_materialInstance == null ||
            _materialInstance.shader != material.shader ||
            _materialInstance.mainTexture != sprite.texture)
        {
            SetupMaterial();
        }

        var hit = Physics2D.Raycast(transform.position, transform.right, range, collisionLayerMask);
        var point = hit.collider ? hit.point : (Vector2)(transform.position + transform.right * range);
        var beamDelta = point - (Vector2)transform.position;
        var beamDirection = beamDelta.normalized;
        var beamDistance = beamDelta.magnitude;
        var count = Mathf.CeilToInt(beamDistance);
        if (sineMag > 0) { count *= 2; }
        positions.Clear();
        positions.Add(transform.position);
        for (int i = 1; i < count; i++)
        {
            var progress = (float)i / (float)count;
            var p = Vector3.Lerp(transform.position, point, progress);
            if (sineMag > 0)
            {
                var scroll = -sineScroll * Time.time;
                var sinVal = Mathf.Sin((scroll + sineOffset + progress * (beamDistance / sineFrequency)) * 2 * Mathf.PI);
                var mag = sineMag * _lineRenderer.widthCurve.Evaluate(progress);
                p += Vector3.Cross(beamDirection, Vector3.forward) * sinVal * mag;
            }
            positions.Add(p);
        }
        positions.Add(point);
        _lineRenderer.positionCount = count + 1;
        _lineRenderer.SetPositions(positions.ToArray());
        _lineRenderer.widthMultiplier = width;

        if (scrollSpeed > 0) { _lineRenderer.sharedMaterial.SetFloat("_OffsetX", Time.time * -scrollSpeed); }

        if (blast)
        {
            blast.transform.position = point;
            blast.sortingOrder = _lineRenderer.sortingOrder + 1;
        }

        if (_stopping) return;

        RaycastHit2D damHit;
        _hasHit.Clear();
        var previousSetting = Physics2D.queriesHitTriggers;
        Physics2D.queriesHitTriggers = true;
        _damCastResults = new RaycastHit2D[8];

        var rDamage = damagePerSecond;
        if (_team == Team.Player && PlayerManager.instance) { rDamage *= PlayerManager.instance.coOpMod; }

        for (int i = 0; i < positions.Count - 1; i++)
        {
            var delta = positions[i + 1] - positions[i];
            var distance = delta.magnitude;
            var direction = delta.normalized;
            Physics2D.CircleCastNonAlloc(positions[i], width * 0.5f, direction, _damCastResults, distance, damageLayerMask);

            for (int j = 0; j < _damCastResults.Length; j++)
            {
                damHit = _damCastResults[j];
                if (damHit.collider)
                {
                    var damagable = damHit.collider.gameObject.GetComponent<IDamageable>();
                    var hasTeam = damHit.collider.gameObject.GetComponent<IHasTeam>();
                    if (damagable != null && (hasTeam == null || hasTeam.team != team) && !_hasHit.Contains(damagable))
                    {
                        _hasHit.Add(damagable);
                        damagable.Hurt(rDamage * Time.deltaTime, gameObject, damageType, true);
                    }
                }
            }
        }
        Physics2D.queriesHitTriggers = previousSetting;
    }

    public void AssignStats(LaserStats stats)
    {
        _stopping = false;
        team = stats.team;
        width = stats.width;
        range = stats.range;
        collisionLayerMask = stats.layerMask;
        damageType = stats.damageType;
        damagePerSecond = stats.damage;
        _lineRenderer.sortingLayerID = SortingLayer.NameToID(stats.sortingLayerName);
        blast.sortingLayerID = SortingLayer.NameToID(stats.sortingLayerName);
        sortingOrder = stats.sortingOrder;
        stopTime = stats.stopTime;
        SetupMaterial();
    }

    public void AttachTo(IHasOnDestroy attachTo)
    {
        _attachedTo = attachTo;
        transform.SetParent(_attachedTo.transform);
        _attachedTo.onDestroy += ImmediateStop;
    }

    public void Stop()
    {
        if (gameObject.activeInHierarchy)
        {
            StartCoroutine(StopRoutine());
        }
        else
        {
            ImmediateStop();
        }
    }

    public IEnumerator StopRoutine()
    {
        _stopping = true;
        var timer = 0f;
        var startWidth = width;
        while (timer < stopTime)
        {
            timer += Time.deltaTime;
            width = Mathf.Lerp(startWidth, 0, timer / stopTime);
            yield return null;
        }

        ImmediateStop();
        _stopping = false;
    }

    public void ImmediateStop()
    {
        if (_attachedTo != null) { _attachedTo.onDestroy -= ImmediateStop; }

        if (LaserManager.instance && managed)
        {
            LaserManager.instance.Recycle(this);
        }
        else
        {
            gameObject.SetActive(false);
        }
    }

    public void OnDestroy()
    {
        if (_attachedTo != null) { _attachedTo.onDestroy -= ImmediateStop; }
    }
}
