using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
public class SimpleAnimator : MonoBehaviour
{
    public float fps = 12;
    public Sprite[] sprites;
    public SimpleAnimation simpleAnimation;
    public bool randomFrame;
    public bool loop = true;
    public bool clearFrameOnEnd;
    public bool finished;
    public bool reverse;
    private SpriteRenderer _spriteRenderer;
    private float _timer;
    public int currentFrame;
    public AudioClip soundOnStart;

	// Use this for initialization
	void Start ()
    {
        _spriteRenderer = GetComponent<SpriteRenderer>();
        if(randomFrame)
        {
            currentFrame = Random.Range(0, sprites.Length);
            _spriteRenderer.sprite = simpleAnimation ? simpleAnimation.sprites[currentFrame] : sprites[currentFrame];
        }

        if (soundOnStart)
        {
            AudioManager.instance.PlayClipAtPoint(soundOnStart, transform.position);
        }
    }

    // Update is called once per frame
    void Update ()
    {
        var frames = simpleAnimation ? simpleAnimation.sprites : sprites;

        if (frames.Length > 0 && !finished)
        {
            _timer += Time.deltaTime;
            if (_timer > 1 / fps)
            {
                _timer -= (1 / fps);
                currentFrame = reverse ? (currentFrame - 1) : (currentFrame + 1);
                if(currentFrame < 0)
                {
                    if(loop)
                    {
                        currentFrame = frames.Length-1;
                    }
                    else
                    {
                        if (clearFrameOnEnd) { _spriteRenderer.sprite = null; }
                        finished = true;
                        return;
                    }
                }

                if(currentFrame >= frames.Length)
                {
                    if (loop)
                    {
                        currentFrame = currentFrame % frames.Length;
                    }
                    else
                    {
                        if (clearFrameOnEnd) { _spriteRenderer.sprite = null; }
                        finished = true;
                        return;
                    }
                }

                _spriteRenderer.sprite = frames[currentFrame];
            }
        }
	}

    public void Reset()
    {
        _timer = 0;
        if (reverse)
        {            
            currentFrame = simpleAnimation ? simpleAnimation.sprites.Length-1 : sprites.Length;
        }
        else
        {
            currentFrame = 0;
        }

        if (soundOnStart)
        {
            AudioManager.instance.PlayClipAtPoint(soundOnStart, transform.position);
        }

        finished = false;
    }

    public void ResetAndSetFrame()
    {
        Reset();
        var frames = simpleAnimation ? simpleAnimation.sprites : sprites;
        _spriteRenderer.sprite = frames[currentFrame];
    }
}
