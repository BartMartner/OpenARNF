using UnityEngine;
using System.Collections;
using UnityEngine.Events;

[RequireComponent(typeof(SpriteRenderer))]
public class PickUp : MonoBehaviour
{
    public AudioClip pickUpSound;
    public PlayerEvent onPickUp;
    public bool allowCoOp;
    protected SpriteRenderer _spriteRenderer;

    public virtual void Awake()
    {
        if (onPickUp == null) onPickUp = new PlayerEvent();
        gameObject.layer = LayerMask.NameToLayer("PlayerOnly");
        var collider = GetComponent<Collider2D>();
        if(!collider)
        {
            Debug.LogError("Pick up " + gameObject.name + " at " + transform.position + " does not have a Collider2D");
        }

        collider.isTrigger = true;

        _spriteRenderer = GetComponent<SpriteRenderer>();
    }

    public virtual void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.layer == LayerMask.NameToLayer("Player"))
        {
            var player = collision.GetComponent<Player>();
            if (player)
            {
                OnPickUp(player);
                return;
            }

            if (allowCoOp)
            {
                var kiki = collision.GetComponent<CoOpPlayer>();
                if (kiki)
                {
                    OnPickUp(kiki.owner);
                    return;
                }
            }

            Debug.LogWarning("Something without a player script attached is on the player layer");
        }
    }

    public virtual void OnPickUp(Player player)
    {
        if (onPickUp != null)
        {
            onPickUp.Invoke(player);
        }

        if (pickUpSound)
        {
            AudioManager.instance.PlayClipAtPoint(pickUpSound, transform.position, 1, 1, 256);
        }

        Destroy(gameObject);
    }
}
