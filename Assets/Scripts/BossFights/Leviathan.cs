using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Leviathan : MonoBehaviour
{
    public Transform topLeftPoint;
    public Transform topRightPoint;
    public Transform bottomLeftPoint;
    public Transform bottomRightPoint;
    public SpriteRenderer body;
    public SpriteRenderer tail;
    public SpriteRenderer hose;
    public Shooter shooter;
    public AdvancedMonsterSpawner spawner;
    public AudioSource crawlLoop;
    private float _cycleSpeed;

    private float _tailHeight;
    private float _topY { get { return topLeftPoint.position.y; } }
    private float _leftX { get { return topLeftPoint.position.x; } }   

    private Enemy _enemy;
    private Animator _animator;

    private bool _acting;
    private Queue<IEnumerator> _queuedActions = new Queue<IEnumerator>();

    private Player _player1;

    private void Awake()
    {
        _tailHeight = tail.sprite.bounds.size.y;
        _animator = GetComponent<Animator>();
        _enemy = GetComponent<Enemy>();
        _animator.Play("TailIdle");

        //_cycleSpeed = 7 / ((24 - 4) / 18);
        _cycleSpeed = 6.3f;

        //wait to appear
        _acting = true; 
        hose.gameObject.SetActive(false);
        body.gameObject.SetActive(false);
        tail.gameObject.SetActive(false);
    }

    public IEnumerator Start()
    {
        _player1 = PlayerManager.instance.player1;
        yield return new WaitForSeconds(2f);
        if (LayoutManager.instance)
        {
            while (LayoutManager.instance.transitioning)
            {
                yield return null;
            }
        }

        while (!_enemy.enabled) { yield return null; }
        StartCoroutine(Appear());
    }

    // Update is called once per frame
    void Update()
    {
        if (_acting || _enemy.state != DamageableState.Alive) return;

        //negative means face left
        var bodyDelta = body.transform.position.x - _player1.position.x;
        body.transform.rotation = Quaternion.Euler(body.transform.position.y == _topY ? 180 : 0, bodyDelta > 0 ? 180 : 0, 0);

        if (_queuedActions.Count <= 0)
        {
            var rand = Random.Range(0, 3);
            switch (rand)
            {
                case 0:
                    _queuedActions.Enqueue(Spit());
                    _queuedActions.Enqueue(RandomCycleAction());
                    break;
                case 1:
                    _queuedActions.Enqueue(Spit());
                    _queuedActions.Enqueue(Wait(0.25f));
                    if (tail.gameObject.activeInHierarchy) { _queuedActions.Enqueue(Spawn()); }
                    _queuedActions.Enqueue(RandomCycleAction());
                    break;
                case 2:
                    _queuedActions.Enqueue(Claw(2.5f));
                    _queuedActions.Enqueue(Wait(0.125f));
                    if (tail.gameObject.activeInHierarchy) { _queuedActions.Enqueue(Spawn()); }
                    _queuedActions.Enqueue(Wait(0.75f));
                    _queuedActions.Enqueue(Spit());
                    _queuedActions.Enqueue(RandomCycleAction());
                    break;
            }
        }

        if (_queuedActions.Count > 0)
        {
            StartCoroutine(_queuedActions.Dequeue());
        }
    }

    public IEnumerator RandomCycleAction()
    {
        return Random.value > 0.5f ? CycleToOpposite() : CycleToSame();
    }

    public void StartAction() { _acting = true; }
    public void EndAction() { _acting = false; }

    private IEnumerator Claw(float time)
    {
        StartAction();
        _animator.Play("ClawStart");
        var modTime = Mathf.Clamp(time, 0.5f, time);
        yield return new WaitForSeconds(modTime);
        _animator.SetTrigger("ClawEnd");
        yield return new WaitForSeconds(1f);
        EndAction();
    }

    private IEnumerator Spit()
    {
        StartAction();
        _animator.Play("Spit");
        yield return new WaitForSeconds(7f / 12f);
        shooter.Shoot();
        yield return new WaitForSeconds(5f / 12f);
        EndAction();
    }

    private IEnumerator Spawn()
    {
        StartAction();
        _animator.Play("TailSpawn");
        yield return new WaitForSeconds(2f/3f);
        spawner.SpawnMonster();
        yield return new WaitForSeconds(1f/3f);
        EndAction();
    }

    private IEnumerator Wait(float time)
    {
        StartAction();
        yield return new WaitForSeconds(time);        
        EndAction();
    }

    private IEnumerator CycleToOpposite()
    {
        StartAction();

        Vector3 bodyTarget;

        if (body.transform.position.y == _topY)
        {
            bodyTarget = body.transform.position.x == _leftX ? bottomRightPoint.position : bottomLeftPoint.position;
        }
        else
        {
            bodyTarget = body.transform.position.x == _leftX ? topRightPoint.position : topLeftPoint.position;
        }

        Vector3 tailTarget;

        if (body.transform.position.y == _topY)
        {
            tailTarget = body.transform.position.x == _leftX ? bottomLeftPoint.position : bottomRightPoint.position;
        }
        else
        {
            tailTarget = body.transform.position.x == _leftX ? topLeftPoint.position : topRightPoint.position;
        }

        yield return StartCoroutine(StartCycle());
        yield return StartCoroutine(HoseCylce(1.25f));
        yield return StartCoroutine(EndCycle(bodyTarget, tailTarget));

        EndAction();
    }

    private IEnumerator Appear()
    {
        StartAction();

        _animator.Play("Default");

        Vector3 bodyTarget;

        var playerDelta = transform.position.x - _player1.position.x;
        bodyTarget = playerDelta < 0 ? bottomLeftPoint.position : bottomRightPoint.position;        

        var bodyYRot = bodyTarget.x == _leftX ? 0 : 180;
        body.transform.rotation = Quaternion.Euler(0, bodyYRot, 0);
        body.transform.position = bodyTarget;

        var bodyOriginalPosition = body.transform.position;        

        body.gameObject.SetActive(true);
        _animator.Play("ClimbOut");

        var timer = 0f;
        var time = 8 / _cycleSpeed;
        while (timer < time)
        {
            timer += Time.deltaTime;
            body.transform.position = Vector3.Lerp(bodyOriginalPosition, bodyTarget, timer / time);
            hose.transform.position = tail.transform.position - tail.transform.up * 8;
            yield return null;
        }

        hose.gameObject.SetActive(false);
        body.transform.position = bodyTarget;

        EndAction();
    }

    private IEnumerator CycleToSame()
    {
        StartAction();

        Vector3 bodyTarget;

        if (body.transform.position.y == _topY)
        {
            bodyTarget = body.transform.position.x == _leftX ? topRightPoint.position : topLeftPoint.position;
        }
        else
        {
            bodyTarget = body.transform.position.x == _leftX ? bottomRightPoint.position : bottomLeftPoint.position;
        }

        Vector3 tailTarget;

        if (body.transform.position.y == _topY)
        {
            tailTarget = body.transform.position.x == _leftX ? bottomLeftPoint.position : bottomRightPoint.position;
        }
        else
        {
            tailTarget = body.transform.position.x == _leftX ? topLeftPoint.position : topRightPoint.position;
        }

        yield return StartCoroutine(StartCycle());
        yield return StartCoroutine(HoseCylce(1.25f));
        yield return StartCoroutine(EndCycle(bodyTarget, tailTarget));

        EndAction();
    }

    private IEnumerator StartCycle()
    {
        hose.gameObject.SetActive(true);
        hose.transform.position = body.transform.position;
        hose.transform.rotation = body.transform.rotation;

        //Body Climbs Up
        var bodyOriginalPosition = body.transform.position;
        var tailOriginalPosition = tail.transform.position;
        Vector3 bodyTarget;
        Vector3 tailTarget = tailOriginalPosition - tail.transform.up * _tailHeight;

        if (bodyOriginalPosition.x == _leftX)
        {
            bodyTarget = bodyOriginalPosition.y == _topY ? bottomLeftPoint.position : topLeftPoint.position;
        }
        else
        {
            bodyTarget = bodyOriginalPosition.y == _topY ? bottomRightPoint.position : topRightPoint.position;
        }

        _animator.Play("TailOut");
        _animator.Play("ClimbIn");

        yield return new WaitForSeconds(4f / 12f); //wait for anim turn

        crawlLoop.volume = 0;
        crawlLoop.Play();

        var timer = 0f;
        var time = 8 / _cycleSpeed;
        while(timer < time)
        {
            timer += Time.deltaTime;
            body.transform.position = Vector3.Lerp(bodyOriginalPosition, bodyTarget, timer/time);
            hose.transform.position = body.transform.position;
            tail.transform.position = Vector3.Lerp(tailOriginalPosition, tailTarget, timer / time);
            crawlLoop.volume = timer / time;
            yield return null;
        }

        body.transform.position = bodyTarget;
        tail.transform.position = tailTarget;

        body.gameObject.SetActive(false);
        tail.gameObject.SetActive(false);
    }

    private IEnumerator HoseCylce(float time)
    {
        var timer = 0f;
        var offset = hose.material.GetFloat("_OffsetY");
        var offsetSpeed = -8 * (_cycleSpeed/8);
        while (timer < time)
        {
            timer += Time.deltaTime;
            offset = (offset +  offsetSpeed * Time.deltaTime) % 1;
            hose.material.SetFloat("_OffsetY", offset);
            yield return null;
        }
    }

    private IEnumerator EndCycle(Vector3 bodyTarget, Vector3 tailTarget)
    {
        _animator.Play("Default");

        var tailXRot = tailTarget.y == _topY ? 180: 0; //X Rotation controls up down
        var tailYRot = tailTarget.x == _leftX ? 0 : 180; //Y Rotation Controls facing left / right
        tail.transform.rotation = Quaternion.Euler(tailXRot, tailYRot, 0);
        tail.transform.position = hose.transform.position - hose.transform.up * 8;

        var bodyXRot = bodyTarget.y == _topY ? 180 : 0;
        var bodyYRot = bodyTarget.x == _leftX ? 0 : 180;
        body.transform.rotation = Quaternion.Euler(bodyXRot, bodyYRot, 0);
        body.transform.position = bodyTarget;

        var bodyOriginalPosition = body.transform.position;
        var tailOriginalPosition = tail.transform.position;

        tail.gameObject.SetActive(true);
        body.gameObject.SetActive(true);
        _animator.Play("ClimbOut");

        var timer = 0f;
        var time = 8 / _cycleSpeed;
        while (timer < time)
        {
            timer += Time.deltaTime;
            body.transform.position = Vector3.Lerp(bodyOriginalPosition, bodyTarget, timer / time);
            tail.transform.position = Vector3.Lerp(tailOriginalPosition, tailTarget, timer / time);
            hose.transform.position = tail.transform.position - tail.transform.up * 8;
            crawlLoop.volume = 1 - (timer / time);
            yield return null;
        }

        crawlLoop.Stop();

        _animator.Play("TailIn");

        hose.gameObject.SetActive(false);
        body.transform.position = bodyTarget;
        tail.transform.position = tailTarget;
    }
}
