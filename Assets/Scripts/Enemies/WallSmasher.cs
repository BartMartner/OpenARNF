using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WallSmasher : MonoBehaviour
{
    public BoxCollider2D minSmasher;
    public BoxCollider2D maxSmasher;
    public LayerMask layerMask;
    public float maxConnectingVelocity = 24f;
    public float connectingAcceleration = 24f;
    public float maxSeparatingVelocity = 24f;
    public float separatingAcceleration = 24f;
    public AudioClip launchSound;
    public AudioClip smashSound;
    public AudioClip separateSound;
    public AudioClip returnSound;
    public bool vertical;
    private Vector2 _minDir;
    private Vector2 _maxDir;
    private float _minParallelExtents;
    private float _maxParallelExtents;    

    private AudioSource _audioSource;
    private Animator _minAnimator;
    private Animator _maxAnimator;
    private bool _smashing;
    private LayerMask _playerMask;

    public bool connected
    {
        get
        {
            return minSmasher.transform.position == transform.position && maxSmasher.transform.position == transform.position;
        }
    }

    public void Awake()
    {
        _audioSource = GetComponent<AudioSource>();
        _minAnimator = minSmasher.GetComponent<Animator>();
        _maxAnimator = maxSmasher.GetComponent<Animator>();
    }

    public void Start()
    {
        CorrectPositions();
        _playerMask = LayerMask.GetMask("Player");
    }

    void Update ()
    {
        if(!_smashing && CheckForPlayer())
        {
            StartCoroutine(Smash());
        }
    }

    public bool CheckForPlayer()
    {
        var distance = Vector3.Distance(minSmasher.transform.position, maxSmasher.transform.position);

        Vector2 top = minSmasher.transform.position;
        top.y += minSmasher.bounds.extents.y;
        Vector2 bottom = minSmasher.transform.position;
        bottom.y -= minSmasher.bounds.extents.y;
        
        var hit = Physics2D.Raycast(top, _maxDir, distance, _playerMask);
        if (hit.collider != null)
        {
            var player = hit.collider.GetComponent<Player>();
            if (player && player.targetable) return true;
        };

        hit = Physics2D.Raycast(minSmasher.transform.position, _maxDir, distance, _playerMask);
        if (hit.collider != null)
        {
            var player = hit.collider.GetComponent<Player>();
            if (player && player.targetable) return true;
        };

        hit = Physics2D.Raycast(bottom, _maxDir, distance, _playerMask);
        if (hit.collider != null)
        {
            var player = hit.collider.GetComponent<Player>();
            if (player && player.targetable) return true;
        };

        return false;
    }

    public IEnumerator Smash()
    {
        _smashing = true;

        var delay = 2f;

        var velocity = 0f;

        _minAnimator.Play("Launch");
        _maxAnimator.Play("Launch");
        _audioSource.PlayOneShot(launchSound);

        while (!connected)
        {
            if(velocity < maxConnectingVelocity)
            {
                velocity += connectingAcceleration * Time.deltaTime;
            }

            minSmasher.transform.position = Vector3.MoveTowards(minSmasher.transform.position, transform.position, velocity * Time.deltaTime);
            maxSmasher.transform.position = Vector3.MoveTowards(maxSmasher.transform.position, transform.position, velocity * Time.deltaTime);
            yield return null;
        }

        _minAnimator.Play("Smash");
        _maxAnimator.Play("Smash");
        _audioSource.PlayOneShot(smashSound);

        yield return new WaitForSeconds(delay);

        _minAnimator.Play("Separate");
        _maxAnimator.Play("Separate");
        _audioSource.PlayOneShot(separateSound);
        yield return new WaitForSeconds(2f/12f);

        Vector3 minPos = Physics2D.Raycast(transform.position, _minDir, 100, layerMask).point + _maxDir * _minParallelExtents * 2f;        
        Vector3 maxPos = Physics2D.Raycast(transform.position, _maxDir, 100, layerMask).point + _minDir * _maxParallelExtents * 2f;

        velocity = 0f;
        while (minSmasher.transform.position != minPos || maxSmasher.transform.position != maxPos)
        {
            if (velocity < maxSeparatingVelocity)
            {
                velocity += separatingAcceleration * Time.deltaTime;
            }

            minSmasher.transform.position = Vector3.MoveTowards(minSmasher.transform.position, minPos, velocity * Time.deltaTime);
            maxSmasher.transform.position = Vector3.MoveTowards(maxSmasher.transform.position, maxPos, velocity * Time.deltaTime);
            yield return null;
        }

        _audioSource.PlayOneShot(returnSound);
        _smashing = false;
    }

    private void OnDrawGizmos()
    {
        if (!Application.isPlaying)
        {
            CorrectPositions();
        }
    }

    public void CorrectPositions()
    {
        if (minSmasher && maxSmasher)
        {
            bool moved = false;
            if(vertical)
            {
                _minDir = Vector2.down;
                _maxDir = Vector2.up;
                minSmasher.transform.rotation = Quaternion.Euler(0,0,90);
                maxSmasher.transform.rotation = Quaternion.Euler(0,0,90);
                _minParallelExtents = minSmasher.bounds.extents.y;
                _maxParallelExtents = maxSmasher.bounds.extents.y;
            }
            else
            {
                _minDir = Vector2.left;
                _maxDir = Vector2.right;
                minSmasher.transform.rotation = Quaternion.identity;
                maxSmasher.transform.rotation = Quaternion.identity;
                _minParallelExtents = minSmasher.bounds.extents.x;
                _maxParallelExtents = maxSmasher.bounds.extents.x;
            }
            
            var hit = Physics2D.Raycast(transform.position, _minDir, 100, layerMask);
            if (hit)
            {
                Vector3 newPosition = hit.point + _maxDir * _minParallelExtents * 2f;
                if (minSmasher.transform.position != newPosition)
                {
                    minSmasher.transform.position = newPosition;
                    moved = true;
                }
            }

            hit = Physics2D.Raycast(transform.position, _maxDir, 100, layerMask);
            if (hit)
            {
                Vector3 newPosition = hit.point + _minDir * _maxParallelExtents * 2f;
                if (maxSmasher.transform.position != newPosition)
                {
                    maxSmasher.transform.position = newPosition;
                    moved = true;
                }
            }

            if (!moved)
            {
                var maxPos = maxSmasher.transform.position;
                var minPos = minSmasher.transform.position;
                transform.position = minSmasher.transform.position + (maxSmasher.transform.position - minSmasher.transform.position) * 0.5f;
                minSmasher.transform.position = minPos;
                maxSmasher.transform.position = maxPos;
            }
        }
    }
}
