using UnityEngine;
using System.Collections;

public class LockPlayerOnCollide : MonoBehaviour
{
    private Vector3 _lastPosition;
    public bool playerPresent;

    public void Awake()
    {
        var effector = GetComponent<PlatformEffector2D>();
        if(effector && effector.useColliderMask)
        {
            Debug.LogWarning(gameObject.name + " has an effector using a collider mask which may break the LockPlayerOnCollide script attached to it");
        }
    }

    public void LateUpdate()
    {
        if (playerPresent)
        {
            Player.instance.transform.position += transform.position - _lastPosition;
        }

        _lastPosition = transform.position;
    }

    public void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.layer == LayerMask.NameToLayer("Player"))
        {
            Debug.Log("Player Present");
            playerPresent = true;
        }
    }

    public void OnCollisionExit2D(Collision2D collision)
    {
        if (collision.gameObject.layer == LayerMask.NameToLayer("Player"))
        {
            Debug.Log("Player Exit");
            playerPresent = false;
        }
    }
}
