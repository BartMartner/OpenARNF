using UnityEngine;
using System.Collections;

public class RandomStartAnimator : MonoBehaviour
{
    public int subdivisions = 8;
	private IEnumerator Start ()
    {
        var animator = GetComponent<Animator>();
        yield return new WaitForEndOfFrame();
        var state = animator.GetCurrentAnimatorStateInfo(0);
        float divisor = Random.Range(1, subdivisions);
        animator.Play(state.shortNameHash, 0, state.length * 1/divisor);
	}
}
