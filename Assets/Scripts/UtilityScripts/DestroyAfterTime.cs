using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class DestroyAfterTime : MonoBehaviour
{
    public float time;
    public float fadeTime;    

    public IEnumerator Start()
    {
        yield return new WaitForSeconds(time);

        if(fadeTime > 0)
        {
            var spriteRenderers = GetComponentsInChildren<SpriteRenderer>();
            if(spriteRenderers.Length > 0)
            {
                var timer = 0f;
                var originalColors = new List<Color>();
                while(timer < fadeTime)
                {
                    timer += Time.deltaTime;
                    for (int i = 0; i < spriteRenderers.Length; i++)
                    {
                        var r = spriteRenderers[i];
                        if(i >= originalColors.Count)
                        {
                            originalColors.Add(r.color);
                        }

                        r.color = Color.Lerp(originalColors[i], Color.clear, timer / fadeTime);
                    }
                    yield return null;
                }
            }
        }

        Destroy(gameObject);
    }
}
