using UnityEngine;
using System.Collections;

public class SpawnFXOnTimer : MonoBehaviour
{
    public FXType fxType;
    public bool randomRotation;
    public bool noSound;
    [Range(0,1)]
    public float prewarm = 0f;
    public float time = 1f;
    public Vector3 offset;
    private float _timer = 0f;
    

    public void Start()
    {
        _timer = time * prewarm;
    }

    public void Update()
    {
        _timer += Time.deltaTime;
        if (_timer > time)
        {
            _timer = 0f;
            FXManager.instance.SpawnFX(fxType, transform.TransformPoint(offset), randomRotation, noSound);
        }
    }

    public void OnDrawGizmosSelected()
    {
        var position = transform.TransformPoint(offset);
        Debug.DrawLine(position + Vector3.up * 0.25f, position + Vector3.down * 0.25f);
        Debug.DrawLine(position + Vector3.left * 0.25f, position + Vector3.right * 0.25f);
    }
}
