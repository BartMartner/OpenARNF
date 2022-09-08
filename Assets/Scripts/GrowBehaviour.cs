using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GrowBehaviour : MonoBehaviour
{
    public void Grow(Vector3 targetScale)
    {
        StartCoroutine(GrowRoutine(targetScale));
    }

    private IEnumerator GrowRoutine(Vector3 targetScale)
    {
        transform.localScale = Vector3.zero;

        var timer = 0f;
        var time = 0.25f;
        while (timer < time)
        {
            timer += Time.deltaTime;
            transform.localScale = Vector3.Slerp(Vector3.zero, targetScale, timer / time);
            yield return null;
        }

        transform.localScale = targetScale;
    }
}
