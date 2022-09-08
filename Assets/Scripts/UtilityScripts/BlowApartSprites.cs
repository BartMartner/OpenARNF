using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class BlowApartSprites : MonoBehaviour
{
    public float time = 6f;
    public float splosiveForce = 6f;
    public float gravity = 6f;
    public float flashInterval = 1/60f;
    private List<SpriteRenderer> _spriteRenderers;
    private List<Vector3> _velocities;
    private float _timer;

    private void Awake()
    {
        transform.parent = null;
    }

    void Start ()
    {
        _spriteRenderers = new List<SpriteRenderer>(GetComponentsInChildren<SpriteRenderer>());
        _spriteRenderers.Remove(GetComponent<SpriteRenderer>());
        _velocities = new List<Vector3>();
        foreach (var sprite in _spriteRenderers)
        {
            var velocity = (sprite.transform.position - transform.position).normalized * splosiveForce;
            _velocities.Add(velocity);
        }

        if(flashInterval > 0)
        {
            StartCoroutine(Flash());
        }
	}
		
	void Update ()
    {
        if (_timer < time)
        {
            _timer += Time.deltaTime;

            for (int i = 0; i < _spriteRenderers.Count; i++)
            {
                var sprite = _spriteRenderers[i];
                var velocity = _velocities[i];
                velocity += Vector3.down * gravity * Time.deltaTime;
                sprite.transform.position += velocity * Time.deltaTime;
                _velocities[i] = velocity;
            }
        }
        else
        {
            Destroy(gameObject);
        }
	}

    private IEnumerator Flash()
    {
        while(_timer < time)
        {
            foreach (var sprite in _spriteRenderers)
            {
                sprite.enabled = !sprite.enabled;
            }

            yield return new WaitForSeconds(flashInterval);
        }
    }
}
