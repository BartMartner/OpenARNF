using CreativeSpore.SuperTilemapEditor;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[AddComponentMenu("Water", 10)]
[DisallowMultipleComponent]
public class Water : MonoBehaviour
{
    public Vector2 size;
    public bool allowSplash = true;
    public bool conductsElectric = true;
    public PaletteCycling electricCycling;
    public FXType splashFX = FXType.Splash32;
    public DamageCreatureTrigger damageCreatureTrigger;
    private HashSet<ILiquidSensitive> _inactive = new HashSet<ILiquidSensitive>();
    private HashSet<ILiquidSensitive> _presentObj = new HashSet<ILiquidSensitive>();
    private BoxCollider2D _collider2D;
    private SpriteRenderer[] _renderers;
    private bool _electrified;
    private bool _moves;

    private void Awake()
    {
        SetSize(size);
        _moves = GetComponent<VerticalPacer>() || GetComponent<VerticalSmoothPacer>();
    }

    public void SetSize(Vector2 newSize)
    {
        size = newSize;
        _collider2D = GetComponent<BoxCollider2D>();
        _renderers = GetComponentsInChildren<SpriteRenderer>();
        _collider2D.size = newSize;

        var dCollider = damageCreatureTrigger.GetComponent<BoxCollider2D>();
        Physics2D.IgnoreCollision(_collider2D, dCollider);
        dCollider.size = newSize;

        foreach (var r in _renderers) { r.size = newSize; }
    }

    protected virtual void Update()
    {
        bool electric = false;

        foreach (var obj in _presentObj)
        {
            obj.inLiquid = true;
            if (obj.electrifiesWater && conductsElectric) { electric = true; }
            if (obj != null && obj.gameObject && !obj.gameObject.activeInHierarchy) _inactive.Add(obj);
        }

        foreach (var obj in _inactive) { ObjectExit(obj); }
        _inactive.Clear();

        if (electric != _electrified) Electrify(electric);
    }

    public void Splash(Collider2D collision)
    {
        if (collision.CompareTag("IgnoreWater")) return;
        if (LayoutManager.instance && LayoutManager.instance.transitioning) return;

        var d = collision.Distance(_collider2D);

        if (_moves)
        {
            if (collision.gameObject.GetComponent<TilemapChunk>() ||
                collision.gameObject.GetComponentInParent<Door>()) { return; }

            var r = collision.attachedRigidbody;
            if (r && (r.IsSleeping() || (!r.isKinematic && r.velocity.magnitude <= 1))) return;

            var v = collision.gameObject.GetComponent<IHasVelocity>();
            if (v != null && v.velocity.magnitude <= 1) return;
        }

        if (Mathf.Abs(d.pointA.y - _collider2D.bounds.max.y) < 8f / 16f)
        {
            var point = d.isOverlapped ? d.pointA : (Vector2)collision.transform.position;
            point.y = _collider2D.bounds.max.y;
            FXManager.instance.SpawnFX(splashFX, point);
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (allowSplash && collision.gameObject) { Splash(collision); }
        var objs = collision.gameObject.GetInterfaces<ILiquidSensitive>();
        foreach (var obj in objs)
        {
            if (obj.OnEnterLiquid(this)) { _presentObj.Add(obj); }
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (allowSplash) { Splash(collision); }
        var obj = _presentObj.FirstOrDefault(o => o.gameObject == collision.gameObject);
        if (obj != null) { ObjectExit(obj); }
    }

    public void ObjectExit(ILiquidSensitive obj)
    {
        _presentObj.Remove(obj);
        obj.OnExitLiquid();
        obj.inLiquid = false;
    }

    private void OnDestroy()
    {
        foreach (var obj in _presentObj) { if (obj != null) { obj.inLiquid = false; } }
        _presentObj.Clear();
    }

    public void Electrify(bool on)
    {
        if (conductsElectric)
        {
            _electrified = on;
            if (electricCycling) { electricCycling.enabled = on; }
            damageCreatureTrigger.gameObject.SetActive(on);
        }
    }
}
