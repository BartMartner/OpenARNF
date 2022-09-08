using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SearchBurstPulse : MonoBehaviour
{
	// Use this for initialization
	IEnumerator Start ()
    {
        var spriteRenderer = GetComponent<SpriteRenderer>();
        var color = spriteRenderer.color;
        var timer = 0f;
        while(timer < 0.66f)
        {
            timer += Time.deltaTime;
            transform.localScale = Vector3.Lerp(Vector3.zero, Vector3.one * 2, timer / 0.66f);
            yield return null;
        }
        
        timer = 0;
        while(timer < 0.33f)
        {
            timer += Time.deltaTime;
            spriteRenderer.color = Color.Lerp(color, Color.clear, timer / 0.33f);
            yield return null;
        }

        Destroy(gameObject);
	}

    public void OnTriggerEnter2D(Collider2D other)
    {
        if (enabled)
        {
            var destructible = other.gameObject.GetComponent<DestructibleTileBounds>();
            if (destructible != null) { destructible.OnImmune(DamageType.Generic); }
        }
    }
}
