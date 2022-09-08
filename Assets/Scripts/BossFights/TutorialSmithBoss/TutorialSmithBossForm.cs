using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TutorialSmithBossForm : MonoBehaviour, IPausable
{
    protected Animator _animator;
    protected AudioSource _audioSource;
    protected TutorialSmithBossController _parentController;

    public float hoverDistance = 0.25f;
    public float hoverSpeed = 0.5f;
    protected bool _hoveringUp;
    protected float _hoverTimer;
    protected float _hoverTime;
    protected Vector3 _originalPosition;
    protected Vector3 _hoverStart;
    protected Vector3 _hoverTarget;

    protected bool _acting;
    public bool acting
    {
        get { return _acting; }
    }

    protected virtual void Awake()
    {
        _animator = GetComponent<Animator>();
        _audioSource = GetComponent<AudioSource>();
        _parentController = GetComponentInParent<TutorialSmithBossController>();
    }

    public void UpdateHovering()
    {
        _hoverTimer += Time.deltaTime;
        if (_hoverTimer < _hoverTime)
        {
            transform.localPosition = Vector3.Slerp(_hoverStart, _hoverTarget, _hoverTimer / _hoverTime);
        }
        else
        {
            _hoveringUp = !_hoveringUp;
            RefreshHovering();
        }
    }

    public void RefreshHovering()
    {
        _hoverTimer = 0;
        _hoverStart = transform.localPosition;
        _hoverTarget = _originalPosition + (_hoveringUp ? Vector3.up : Vector3.down) * hoverDistance;
        _hoverTime = Vector3.Distance(_hoverTarget, _hoverStart) / hoverSpeed;
    }

    public IEnumerator ReturnToCenter()
    {
        while (transform.localPosition != _originalPosition)
        {
            transform.localPosition = Vector3.MoveTowards(transform.localPosition, _originalPosition, 6 * Time.deltaTime);
            yield return null;
        }
    }

    public void Pause()
    {
        enabled = false;
    }

    public void Unpause()
    {
        enabled = true;
    }
}
