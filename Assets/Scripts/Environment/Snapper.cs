using UnityEngine;
using System.Collections;

[RequireComponent(typeof(Animator))]
[RequireComponent(typeof(Collider2D))]
public class Snapper : MonoBehaviour
{
    public float warmUpTime = 0.5f;
    public float animationTime = 0.5f;
    public float coolDownTime = 0.5f;
    private Animator _animator;

    private bool _snapping;

    private void Awake()
    {
        _animator = GetComponent<Animator>();
    }

    public void OnTriggerEnter2D(Collider2D collision)
    {
        if(!_snapping)
        {
            StartCoroutine(Snap());
        }
    }

    private IEnumerator Snap()
    {
        _snapping = true;
        yield return new WaitForSeconds(warmUpTime);
        _animator.SetTrigger("Snap");
        yield return new WaitForSeconds(animationTime + coolDownTime);
        _snapping = false;
    }
}
