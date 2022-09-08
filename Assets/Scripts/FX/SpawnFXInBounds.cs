using UnityEngine;
using System.Collections;

public class SpawnFXInBounds : MonoBehaviour
{
    public FXType fxType;
    public Vector2 size;
    [Range(0f,1f)]
    public float prewarm = 0f;
    public float frequnecy = 1f;
    public bool randomRotation;
    public int minQuantity = 1;
    public int maxQuantity = 1;
    private float _timer = 0f;

    public void Start()
    {
        _timer = frequnecy * prewarm;
    }

    public void Update()
    {
        _timer += Time.deltaTime;
        if (_timer > frequnecy)
        {
            _timer = 0f;
            var amount = Random.Range(minQuantity, maxQuantity+1);
            var bounds = new Bounds(transform.position, size);
            for (int i = 0; i < amount; i++)
            {
                FXManager.instance.SpawnFX(fxType, Extensions.randomInsideBounds(bounds));
            }
        }
    }

    public void OnDrawGizmosSelected()
    {
        Extensions.DrawSquare(new Rect((Vector2)transform.position - size * 0.5f, size), Color.gray);
    }
}
