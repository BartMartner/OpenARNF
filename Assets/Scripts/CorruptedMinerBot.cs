using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public class CorruptedMinerBot : MonoBehaviour, IHasOnDestroy, IPausable
{
    public LaserStats laserStats;
    public Transform[] spawnPoints;
    public Action onDestroy { get; set; }
    public SinusoidalPacer pacer;
    public DangerRubble crystalDropPrefab;
    public bool busy;
    public GameObject tentacles;
    public Enemy babyLatcher;
    public AudioClip eyeAppearSound;

    private ILaser _laser;
    private Animator _animator;
    private Room _room;
    private float _actionTimer;
    private float _actionTime = 4f;
    private bool _meatEnabled;

    private void Awake()
    {
        pacer = GetComponent<SinusoidalPacer>();
        _animator = GetComponent<Animator>();
        _room = GetComponentInParent<Room>();
        _actionTimer = _actionTime;
    }

    private void Update()
    {
        if (busy) return;

        _actionTimer -= Time.deltaTime;
        if(_actionTimer < 0)
        {
            var pick = Random.Range(0, _meatEnabled ? 4 : 2);
            var player = PlayerManager.instance.GetClosestPlayer(transform.position);
            switch (pick)
            {
                case 0:
                    StartCoroutine(FireAtPlayer(player.position));
                    break;
                case 1:
                    StartCoroutine(FireLaserAtCeiling(player));
                    break;
                case 2:
                case 3:
                    StartCoroutine(SummonCreature());
                    break;
            }
            _actionTimer = _actionTime;
        }
    }

    public void MeatEye()
    {
        _animator.SetLayerWeight(1, 1);
        var point = transform.position + Vector3.up * 0.125f;
        GibManager.instance.SpawnGibs(GibType.GreenTech, point, 4);
        AudioManager.instance.PlayClipAtPoint(eyeAppearSound, point);
        laserStats.laserType = LaserType.Bioplasma;
        laserStats.damage = 3;
        tentacles.gameObject.SetActive(true);
    }

    public void EnableMeat()
    {
        StartCoroutine(MeatAppearRoutine());
    }

    private IEnumerator MeatAppearRoutine()
    {
        busy = true;
        _meatEnabled = true;
        pacer.enabled = false;
        _animator.SetLayerWeight(2, 1);
        _animator.Play("MeatAppear", 2, 0);
        yield return new WaitForSeconds(24f/18f);
        pacer.enabled = true;
        yield return new WaitForSeconds(2);
        busy = false;
    }

    public IEnumerator FireAtPlayer(Vector3 position)
    {
        busy = true;
        pacer.enabled = false;
        _animator.SetTrigger("Shoot");
        yield return new WaitForSeconds(0.5f);
        var rotation = Quaternion.FromToRotation(Vector3.right, (position - transform.position).normalized);
        _laser = LaserManager.instance.AttachAndFireLaser(laserStats, Vector3.zero, rotation, 0.5f, this);
        yield return new WaitForSeconds(0.5f);
        _animator.SetTrigger("ShootEnd");
        pacer.enabled = true;
        busy = false;
    }

    public IEnumerator FireLaserAtCeiling(Player player)
    {
        busy = true;
        pacer.enabled = false;
        _animator.SetTrigger("Shoot");
        yield return new WaitForSeconds(0.5f);
        Vector3 target = player.position;
        var y = _room.worldBounds.max.y;
        target.y = y - 1;
        var rotation = Quaternion.FromToRotation(Vector3.right, (target - transform.position).normalized);
        _laser = LaserManager.instance.AttachAndFireLaser(laserStats, Vector3.zero, rotation, 0.5f, this);
        Instantiate(crystalDropPrefab, target+Vector3.down, Quaternion.identity, transform.parent);
        yield return new WaitForSeconds(0.5f);
        _animator.SetTrigger("ShootEnd");
        pacer.enabled = true;
        busy = false;
    }

    public IEnumerator SummonCreature()
    {
        busy = true;
        pacer.enabled = false;
        _animator.Play("Spawn");
        yield return new WaitForSeconds(9f/12f);
        var latcher = Instantiate(babyLatcher, spawnPoints[0].position, Quaternion.identity, _room.transform);
        var spawnPickUps = latcher.GetComponent<SpawnPickUpsOnDeath>();
        if(spawnPickUps) { spawnPickUps.spawnChance = 0; }
        yield return new WaitForSeconds(4f / 12f);
        latcher = Instantiate(babyLatcher, spawnPoints[1].position, Quaternion.identity, _room.transform);
        spawnPickUps = latcher.GetComponent<SpawnPickUpsOnDeath>();
        if (spawnPickUps) { spawnPickUps.spawnChance = 0; }
        yield return new WaitForSeconds(7f / 12f);
        pacer.enabled = true;
        busy = false;
    }

    public void OnDestroy()
    {
        if (onDestroy != null) { onDestroy(); }
    }

    public void Pause()
    {
        enabled = false;
    }

    public void Unpause()
    {
        enabled = true;
    }
}
