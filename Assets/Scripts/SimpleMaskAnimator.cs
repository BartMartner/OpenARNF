using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(SpriteMask))]
public class SimpleMaskAnimator : MonoBehaviour
{
    public float fps = 12;
    public SimpleAnimation simpleAnim;
    public bool randomFrame;
    public bool loop = true;
    public bool clearFrameOnEnd;
    public bool finished;
    public bool softDisable;
    public bool reverse;
    private SpriteMask _spriteMask;
    private float _timer;
    public int currentFrame;

    private void Awake()
    {
        _spriteMask = GetComponent<SpriteMask>();
    }

    private void Start()
    {
        if (randomFrame)
        {
            currentFrame = Random.Range(0, simpleAnim.sprites.Length);
            if (!softDisable) { _spriteMask.sprite = simpleAnim.sprites[currentFrame]; }
        }
    }

    private void Update()
    {
        if (simpleAnim.sprites.Length > 0 && !finished)
        {
            _timer += Time.deltaTime;
            if (_timer > 1 / fps)
            {
                _timer -= (1 / fps);
                currentFrame = reverse ? (currentFrame - 1) : (currentFrame + 1);
                if (currentFrame < 0)
                {
                    if (loop)
                    {
                        currentFrame = simpleAnim.sprites.Length-1;
                    }
                    else
                    {
                        if (clearFrameOnEnd) { _spriteMask.sprite = null; }
                        currentFrame = 0;
                        finished = true;
                        return;
                    }
                }

                if (currentFrame >= simpleAnim.sprites.Length)
                {
                    if (loop)
                    {
                        currentFrame = currentFrame % simpleAnim.sprites.Length;
                    }
                    else
                    {
                        if (clearFrameOnEnd) { _spriteMask.sprite = null; }
                        currentFrame = simpleAnim.sprites.Length - 1;
                        finished = true;
                        return;
                    }
                }

                if (!softDisable) { _spriteMask.sprite = simpleAnim.sprites[currentFrame]; }
            }
        }
    }

    public void Reset()
    {
        _timer = 0;
        currentFrame = reverse ? simpleAnim.sprites.Length-1 : 0;
        finished = false;
        softDisable = false;
    }

    public void SetFrame()
    {
        if (_spriteMask && simpleAnim && simpleAnim.sprites != null && simpleAnim.sprites.Length > 0)
        {
            currentFrame = Mathf.Clamp(currentFrame, 0, simpleAnim.sprites.Length - 1);
            _spriteMask.sprite = simpleAnim.sprites[currentFrame];
        }
    }
}

