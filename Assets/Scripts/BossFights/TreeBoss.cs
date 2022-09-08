using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TreeBoss : MonoBehaviour, IPausable
{
    [Header("Debug")]
    public bool fistSlamTrigger;
    public bool noseBlowTrigger;
    public bool tongueTrigger;

    [Header("Death")]
    public GameObject destroyedTree;

    [Header("Top Growth")]
    public Enemy topGrowth;
    public DestroyOffCamera flyPrefab;
    public Transform[] spawnPoints;

    [Header("Nose Blow")]
    public Shooter shooter;

    [Header("Fist Pound")]
    public float cameraShakeTime = 1f;
    public float cameraShakeMagnitude = 0.2f;
    public float cameraShakeSpeed = 6f;

    [Header("TongueTentacle")]
    public Animator tongueTentaclePrefab;
    public Animator tongueInstance;
    public float tongueYOffset;

    private bool _topActing;
    private bool _bottomActing;
    private Enemy _enemy;
    private Animator _animator;
    private bool _paused;

    public void Awake()
    {
        _animator = GetComponent<Animator>();
        _enemy = GetComponent<Enemy>();
        _enemy.onStartDeath.AddListener(OnStartDeath);
        _enemy.onEndDeath.AddListener(OnEndDeath);
    }

    public void Update()
    {
        if (_paused) return;

        if(topGrowth && topGrowth.state == DamageableState.Alive && !_topActing)
        {
            StartCoroutine(WaitSpawnFlies());
        }

        if (!_bottomActing)
        {
            if (fistSlamTrigger)
            {
                fistSlamTrigger = false;
                StartCoroutine(FistPound());
            }
            else if(noseBlowTrigger)
            {
                noseBlowTrigger = false;
                StartCoroutine(NoseBlow());
            }
            else if(tongueTrigger)
            {
                tongueTrigger = false;
                StartCoroutine(TongueAttack());
            }
            else
            {
                StartCoroutine(WaitPickBottomAction());
            }
        }
    }

    public IEnumerator WaitSpawnFlies()
    {
        _topActing = true;
        var waitTime = Mathf.Lerp(5, 8, topGrowth.health / topGrowth.maxHealth);

        while (waitTime > 0)
        {
            if (!_paused)
            {
                waitTime -= Time.deltaTime;
            }
            yield return null;
        }

        if (topGrowth && topGrowth.state == DamageableState.Alive)
        {
            _animator.SetTrigger("SpawnFlies");
            yield return new WaitForSeconds(0.5f);

            var parentRoom = GetComponentInParent<Room>();

            foreach (var p in spawnPoints)
            {
                if (topGrowth && topGrowth.state == DamageableState.Alive)
                {
                    var fly = Instantiate(flyPrefab, p.position, Quaternion.identity, parentRoom ? parentRoom.transform : null);
                    var sine = fly.GetComponent<GrowingSine>();
                    sine.direction.x = transform.rotation == Quaternion.identity ? 0.25f : -0.25f;
                    fly.tolerance = 12;
                    yield return new WaitForSeconds(1);
                }
            }
            _topActing = false;
        }
    }

    public IEnumerator WaitPickBottomAction()
    {
        _bottomActing = true;

        //wait
        var waitTime = Mathf.Lerp(0.75f, 2.5f, _enemy.health / _enemy.maxHealth);

        while (waitTime > 0)
        {
            if (!_paused)
            {
                waitTime -= Time.deltaTime;
            }
            yield return null;
        }

        _bottomActing = false; //set true again when an action is started

        //pick action
        var pick = Random.Range(0, 3);
        switch(pick)
        {
            case 0:
                StartCoroutine(FistPound());
                break;
            case 1:
                StartCoroutine(NoseBlow());
                break;
            case 2:
                StartCoroutine(TongueAttack());
                break;
        }
    }

    public IEnumerator FistPound()
    {
        _bottomActing = true;
        var player = PlayerManager.instance.player1;
        _animator.Play("FistPound",0);
        yield return new WaitForSeconds(43f / 36f);
        MainCamera.instance.Shake(cameraShakeTime, cameraShakeMagnitude, cameraShakeSpeed);

        var counter = 7f / 36f;
        while (counter > 0)
        {
            counter -= Time.deltaTime;
            if (player.controller2D.bottomEdge.touching) player.fatigued = true;
            yield return null;
        }
        _bottomActing = false;
    }

    public IEnumerator NoseBlow()
    {
        _bottomActing = true;
        _animator.Play("Shoot",0);
        yield return new WaitForSeconds(10f / 18f);
        shooter.Shoot();
        yield return new WaitForSeconds(10f / 18f);
        _bottomActing = false;
    }

    public IEnumerator TongueAttack()
    {
        _bottomActing = true;
        _animator.SetTrigger("TongueStart");
        yield return new WaitForSeconds(6f/18f);

        var player = PlayerManager.instance.player1;
        var tonguePos = new Vector2(player.position.x, transform.position.y + tongueYOffset);
        var rayCastResult = Physics2D.Raycast(tonguePos, Vector3.down, 2, LayerMask.GetMask("Door", "Default"));
        var spawnPoint = rayCastResult.point;
        
        yield return new WaitForSeconds(6f/18f);
        
        if (!rayCastResult.collider)
        {
            Debug.LogWarning("WHAT THE FUCK! NO GROUND FOR MY TONGUE!");
        }
        else
        {
            if (rayCastResult.collider.gameObject.layer == LayerMask.NameToLayer("Door") || rayCastResult.collider.GetComponent<Door>())
            {
                spawnPoint.x += Random.value > 0 ? 4 : -4;
            }

            tongueInstance = Instantiate(tongueTentaclePrefab, spawnPoint, Quaternion.identity);
            var d = tongueInstance.GetComponent<ChildDamagable>();
            d.parent = _enemy;
        }

        yield return new WaitForSeconds(0.75f);
        if (tongueInstance) tongueInstance.SetTrigger("End");
        yield return new WaitForSeconds(8/18f);
        if (tongueInstance) Destroy(tongueInstance.gameObject);
        _animator.SetTrigger("TongueEnd");
        yield return new WaitForSeconds(12/18f);
        _bottomActing = false;
    }

    public void OnDrawGizmosSelected()
    {
        var tonguePos = transform.position;
        tonguePos.y += tongueYOffset;
        Debug.DrawLine(tonguePos + Vector3.up, tonguePos - Vector3.up);
        Debug.DrawLine(tonguePos + Vector3.right, tonguePos - Vector3.right);
    }

    public void OnStartDeath()
    {
        _bottomActing = true;
        _topActing = true;
        StopAllCoroutines();
        _animator.SetTrigger("Die");
    }

    public void OnEndDeath()
    {
        destroyedTree.transform.rotation = transform.rotation;
        destroyedTree.gameObject.SetActive(true);
        var f = destroyedTree.GetComponent<SaveFacing>();
        f.SaveState();

        if(tongueInstance)
        {
            tongueInstance.SetTrigger("End");
            Destroy(tongueInstance.gameObject, 8 / 12f);
        }
    }

    public void Pause()
    {
        _paused = true;
    }

    public void Unpause()
    {
        _paused = false;
    }
}
