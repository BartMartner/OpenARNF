using UnityEngine;
using System.Collections;

public class CrumbleBlock : MonoBehaviour, ISpecialPlatform
{
    public float crumbleTime = 0.5f;
    public float respawnDelay = 3f;
    public float respawnTime = 0.25f;
    public BoxCollider2D collisionBounds;
    public AudioClip touchSound;
    public AudioClip crumbleSound;
    public AudioClip respawnSound;
    private Animator _animator;
    private bool _crumbling;

    public void Awake()
    {
        _animator = GetComponent<Animator>();        
    }

    public void OnTriggerStay2D(Collider2D collider)
    {
        if (!_crumbling && collider.gameObject.layer == LayerMask.NameToLayer("Player"))
        {
            StartCoroutine(Crumble());
        }
    }

    public IEnumerator Crumble()
    {
        _crumbling = true;
        _animator.SetTrigger("Touch");
        AudioManager.instance.PlayClipAtPoint(touchSound, transform.position);       
        yield return new WaitForSeconds(crumbleTime);
        collisionBounds.gameObject.SetActive(false);
        _animator.SetTrigger("Crumble");
        AudioManager.instance.PlayClipAtPoint(crumbleSound, transform.position);
        yield return new WaitForSeconds(respawnDelay-respawnTime);

        var bounds = collisionBounds.bounds;
        bounds.size = new Vector2(1.125f, 1.125f);

        bool obstruction;
        obstruction = true;
        while (obstruction)
        {
            foreach (var player in PlayerManager.instance.players)
            {
                obstruction = bounds.Intersects(player.controller2D.collider2D.bounds);
                if (obstruction) break;
            }
            if (obstruction) { yield return new WaitForSeconds(0.25f); }
        }

        _animator.SetTrigger("Respawn");
        AudioManager.instance.PlayClipAtPoint(respawnSound, transform.position);
        collisionBounds.gameObject.SetActive(true);
        yield return new WaitForSeconds(respawnTime);
        _crumbling = false;
    }
}
