using System.Collections;
using UnityEngine;

public class ToggleBlock : MonoBehaviour, ISpecialPlatform
{
    public float crumbleTime = 0.5f;
    public float respawnTime = 0.25f;
    public AudioClip crumbleSound;
    public AudioClip respawnSound;
    private Animator _animator;    
    private BoxCollider2D _collider;    
    private bool _visible = true;
    public bool visible
    {
        get { return _visible; }
    }


    public void Awake()
    {
        _animator = GetComponent<Animator>();
        _collider = GetComponent<BoxCollider2D>();
    }

    public void Hide()
    {
        if (_visible)
        {
            _visible = false;
            StopAllCoroutines();
            StartCoroutine(Disappear());
        }
    }

    public void Show()
    {
        if (!_visible)
        {
            _visible = true;
            StopAllCoroutines();
            StartCoroutine(Reappear());
        }
    }

    public void HideImmediate()
    {
        StopAllCoroutines();
        _collider.enabled = false;
        _animator.Play("Crumble", 0, 1);
        _visible = false;
    }

    public IEnumerator Disappear()
    {
        _animator.SetTrigger("Crumble");
        AudioManager.instance.PlayClipAtPoint(crumbleSound, transform.position);
        yield return new WaitForSeconds(crumbleTime);
        _collider.enabled = false;
    }

    public IEnumerator Reappear()
    {
        var bounds = _collider.bounds;
        bounds.size = new Vector2(1.125f, 1.125f);        
        while (bounds.Intersects(Player.instance.controller2D.collider2D.bounds))
        {
            yield return new WaitForSeconds(0.25f);
        }

        _animator.SetTrigger("Respawn");
        AudioManager.instance.PlayClipAtPoint(respawnSound, transform.position);
        _collider.enabled = true;
        yield return new WaitForSeconds(respawnTime);
    }
}
