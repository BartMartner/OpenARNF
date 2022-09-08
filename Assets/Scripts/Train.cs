using UnityEngine;
using System.Collections.Generic;
using System.Collections;

public class Train : MonoBehaviour
{
    public SpriteRenderer[] parts;
    public float spacingMod = 1;
    public List<Vector3> positions = new List<Vector3>();
    public bool lookAt;
    public bool flip;
    public bool spaceBySpriteSize;
    public float deathTime = 2f;
    public FXType deathFX;

    private Vector3 _lastPosition;
    private float _totalDistanceMoved;
    private float[] _spacingLerp;
    private float _totalWidth;
    private float _moveTheshhold;
    private int _positionCount;
    private bool _dying;

    public void Start()
    {
        PreSort();
    }

    public void Update()
    {
        if (!_dying)
        {
            var distanceMoved = Vector3.Distance(transform.localPosition, _lastPosition);
            _totalDistanceMoved += distanceMoved;
            if (_totalDistanceMoved > _moveTheshhold)
            {
                positions.Add(transform.localPosition);
                if (positions.Count > _positionCount)
                {
                    positions.RemoveAt(0);
                }
                _totalDistanceMoved -= _moveTheshhold;
            }

            //perform same logic we'll use in update to establish spacing
            for (int i = 0; i < parts.Length; i++)
            {
                var part = parts[i];
                float trueIndex = _positionCount * (1 - _spacingLerp[i]);
                float remainder = trueIndex % 1;
                var minPosition = positions[Mathf.FloorToInt(trueIndex)];
                var maxPosition = positions[Mathf.CeilToInt(trueIndex)];
                var truePosition = Vector3.Lerp(minPosition, maxPosition, remainder);

                if (flip)
                {
                    part.transform.rotation = (truePosition.x - part.transform.localPosition.x) > 0 ? Quaternion.identity : Constants.flippedFacing;
                }

                part.transform.localPosition = Vector3.MoveTowards(part.transform.localPosition, truePosition, distanceMoved);

                if (lookAt)
                {
                    SpriteRenderer lookTarget;
                    if (i == 0)
                    {
                        lookTarget = GetComponent<SpriteRenderer>();
                    }
                    else
                    {
                        lookTarget = parts[i - 1];
                    }

                    var lookPosition = lookTarget.transform.localPosition + lookTarget.transform.rotation * Vector3.left * (lookTarget.bounds.extents.x + spacingMod);
                    //Debug.DrawLine(lookPosition + Vector3.up, lookPosition + Vector3.down);
                    var targetRotation = Quaternion.FromToRotation(Vector3.right, lookPosition - part.transform.localPosition);
                    part.transform.rotation = targetRotation;
                }
            }

            _lastPosition = transform.localPosition;
        }
    }

    public void PreSort()
    {
        var so = parts.Length;
        GetComponent<SpriteRenderer>().sortingOrder = so;
        positions.Clear();
        _spacingLerp = new float[parts.Length];
        _totalWidth = 0f;
        
        for (int i = 0; i < parts.Length; i++)
        {
            var part = parts[i];
            var spacing = part.bounds.size.x + spacingMod;
            _totalWidth += spacing;
            _spacingLerp[i] = _totalWidth;
        }

        _moveTheshhold = _totalWidth / parts.Length;

        //Normalize values
        for (int i = 0; i < _spacingLerp.Length; i++)
        {
            _spacingLerp[i] = _spacingLerp[i] / _totalWidth;
        }

        //populate initial positions
        _positionCount = (int)(_totalWidth / _moveTheshhold);
        for (int i = 0; i < _positionCount; i++)
        {
            positions.Add(transform.localPosition + Vector3.left * i * _moveTheshhold);
        }
        positions.Reverse();

        //perform same logic we'll use in update to establish spacing
        for (int i = 0; i < parts.Length; i++)
        {
            var part = parts[i];
            float trueIndex = _positionCount * (1-_spacingLerp[i]);
            float remainder = trueIndex % 1;
            var minPosition = positions[Mathf.FloorToInt(trueIndex)];
            var maxPosition = positions[Mathf.CeilToInt(trueIndex)];
            var truePosition = Vector3.Lerp(minPosition, maxPosition, remainder);
            //var position = transform.position + Vector3.left * (i+1) * spacingMod;
            part.transform.localPosition = truePosition;
            so--;
            part.sortingOrder = so;
        }

        _lastPosition = positions[_positionCount - 1];
    }


    public void DestroyChain(bool fromStart)
    {
        if (fromStart)
        {
            StartCoroutine(DestroyFromStart());
        }
        else
        {
            StartCoroutine(DestroyFromEnd());
        }
    }

    public IEnumerator DestroyFromStart()
    {
        _dying = true;
        var interval = deathTime / parts.Length;
        for (int i = 0; i < parts.Length; i++)
        {
            var sprite = parts[i];
            Destroy(sprite.gameObject);
            if (deathFX != FXType.None)
            {
                FXManager.instance.SpawnFX(deathFX, sprite.transform.position);
            }
            yield return new WaitForSeconds(interval);
        }

        Destroy(gameObject);
    }

    public IEnumerator DestroyFromEnd()
    {
        _dying = true;
        var interval = deathTime / parts.Length;
        for (int i = parts.Length - 1; i >= 0; i--)
        {
            var sprite = parts[i];
            Destroy(sprite.gameObject);
            if (deathFX != FXType.None)
            {
                FXManager.instance.SpawnFX(deathFX, sprite.transform.position);
            }
            yield return new WaitForSeconds(interval);
        }

        Destroy(gameObject);
    }
}