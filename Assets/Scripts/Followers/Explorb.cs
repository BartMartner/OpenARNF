using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Explorb : Follower
{    
    public float offsetRatio;
    public float spotDistance = 10;

    private DamageCreatureTrigger _damageTrigger;

    private float _distance = 2f;
    //time it takes for a full rotation;
    private float _rotationTime = 3f;

    public AudioClip spotSound;

    private List<MinorItemPickUp> _minorItems;
    private Transform _minorItem;
    private float _timer;
    private Animator _animator;
    private float _velocity;
    private Vector3 _targetPosition;
    private bool _noMove;

    public override bool orbital
    {
        get { return true; }
    }

    private void Awake()
    {
        _animator = GetComponent<Animator>();
    }

    public override IEnumerator Start()
    {
        yield return base.Start();

        var room = FindObjectOfType<Room>();

        if (room)
        {
            _minorItems = new List<MinorItemPickUp>(room.GetComponentsInChildren<MinorItemPickUp>(true));
        }

        if (LayoutManager.instance)
        {
            LayoutManager.instance.onTransitionComplete += () => StartCoroutine(FindMinorItems());
        }
    }

    public override void OnDestroy()
    {
        base.OnDestroy();
        if (LayoutManager.instance)
        {
            LayoutManager.instance.onTransitionComplete -= ()=> StartCoroutine(FindMinorItems());
        }
    }

    public IEnumerator FindMinorItems()
    {
        yield return new WaitForSeconds(1f);

        var currentRoom = LayoutManager.CurrentRoom;
        if (currentRoom)
        {
            _minorItems = new List<MinorItemPickUp>(currentRoom.GetComponentsInChildren<MinorItemPickUp>(true));
        }
    }

    public void Update()
    {
        if(_noMove)
        {
            return;
        }

        if (player.orbitalFollowerCount > 0)
        {
            offsetRatio = (float)positionNumber / player.orbitalFollowerCount;
        }

        var offsetAngle = (offsetRatio) * 360;
        offsetAngle += (Time.time % _rotationTime) / _rotationTime * 360;
        
        if(!_minorItem && _minorItems != null && _minorItems.Count > 0)
        {
            _minorItems.RemoveAll(i => !i);
            if(_minorItems.Count > 0)
            {
                var m = _minorItems[0].transform;
                if(Vector3.Distance(transform.position, m.position) < 10)
                {
                    _minorItem = m;
                    _audioSource.PlayOneShot(spotSound);
                    _animator.SetTrigger("Spot");
                    StartCoroutine(NoMove(1f));
                }
            }
        }

        if (!_minorItem)
        {
            _targetPosition = player.transform.position + Quaternion.Euler(0, 0, offsetAngle) * Vector3.up * _distance;
        }
        else
        {
            _targetPosition = _minorItem.position + Quaternion.Euler(0, 0, offsetAngle) * Vector3.up * _distance;
        }

        if (transform.position != _targetPosition)
        {
            var direction = (_targetPosition - transform.position).normalized;
            var distance = Vector3.Distance(_targetPosition, transform.position);

            if (distance > 0.25f)
            {
                if (_velocity == 0)
                {
                    _velocity = player.maxSpeed * 0.8f;
                }

                _velocity += distance * distance * Time.deltaTime;
                _velocity = Mathf.Clamp(_velocity, 0, distance * 5);
                transform.position += direction * _velocity * Time.deltaTime;
            }
            else
            {
                _velocity -= distance * Time.deltaTime;

                if (_velocity < 1)
                {
                    _velocity = 1;
                }

                transform.position = Vector3.MoveTowards(transform.position, _targetPosition, _velocity * Time.deltaTime);
            }
        }
        else
        {
            _velocity = 0;
        }
    }

    private IEnumerator NoMove(float time)
    {
        _noMove = true;
        yield return new WaitForSeconds(time);
        _noMove = false;
    }
}
