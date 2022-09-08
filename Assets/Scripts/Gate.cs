using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Gate : MonoBehaviour, IPathFindingSensitive
{
    private Animator _animator;
    private BoxCollider2D _boxCollider2D;
    private bool _open = false;
    private Vector3 _boundsSize;
    public float speedMod = 1;
    public bool m_pathFindingSenstive = true;
    public bool pathFindingSensitive { get { return m_pathFindingSenstive; } }

    GameObject IPathFindingSensitive.gameObject { get { return gameObject; } }
    public  Action pfTryRefresh { get; set; }

    private Collider2D[] _collider2Ds;
    public Collider2D[] collider2Ds
    {
        get
        {
            if (_collider2Ds == null) { _collider2Ds = GetComponentsInChildren<Collider2D>(); }
            return _collider2Ds;
        }
    }

    public void Awake()
    {
        _animator = GetComponent<Animator>();
        _boxCollider2D = GetComponent<BoxCollider2D>();
        _boundsSize = _boxCollider2D.bounds.size;
    }

    public void Toggle()
    {
        if(_open)
        {
            StopAllCoroutines();
            StartCoroutine(CloseRoutine());
        }
        else
        {
            StopAllCoroutines();
            StartCoroutine(OpenRoutine(0));
        }
    }

    public void ImmediateOpen()
    {        
        _animator.Play("DefaultOpen");
        _open = true;
        _boxCollider2D.enabled = false;
    }

    public void OpenDelayed(float delay) { StartCoroutine(OpenRoutine(delay)); }
    public void Open() { StartCoroutine(OpenRoutine(0)); }
    public void Close() { StartCoroutine(CloseRoutine()); }

    private IEnumerator OpenRoutine(float delay)
    {
        if (!_open)
        {
            _open = true;
            if (delay > 0) { yield return new WaitForSeconds(delay); }
            _animator.speed = speedMod;
            _animator.SetTrigger("Open");
            var time = (12 / 18f) / speedMod;
            yield return new WaitForSeconds(time);
            _boxCollider2D.enabled = false;
            if (pfTryRefresh != null) { pfTryRefresh(); }
        }
    }

    private IEnumerator CloseRoutine()
    {
        if (_open)
        {
            var bounds = _boxCollider2D.bounds;
            bounds.size = _boundsSize;
            while (PlayerManager.instance.IntersectsAnyPlayerBounds(bounds))
            {
                yield return new WaitForSeconds(0.25f);
            }

            _animator.speed = speedMod;
            _animator.SetTrigger("Close");
            _open = false;

            var time = (4 / 18f) / speedMod;
            yield return new WaitForSeconds(time);

            while (PlayerManager.instance.IntersectsAnyPlayerBounds(bounds))
            {
                yield return null;
            }

            _boxCollider2D.enabled = true;
            if (pfTryRefresh != null) { pfTryRefresh(); }
        }
    }
}
