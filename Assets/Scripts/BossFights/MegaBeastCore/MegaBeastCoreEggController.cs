using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using Random = UnityEngine.Random;
using UnityEngine.SceneManagement;

public class MegaBeastCoreEggController : MonoBehaviour
{
    public LaserSpinner spinner;
    public List<LaserLineGroup> laserLineGroups;
    public float maxInterval = 5f;
    public float minInterval = 1f;
    public TimedShooter shooter;
    public SpriteRenderer silohuette;
    public Sprite tutSmith;
    public Enemy falseBeastProjPrefab;
    public List<GameObject> bgObjects;
    public GameObject glitchQuad;

    private float _timer;
    private float _interval;
    private Animator _animator;
    private List<int> _laserOrder;
    private int _currentLaserPattern = 0;
    private bool _lasersFiring;
    private bool _useTut;
    private bool _spawn;
    private Room _room;

    private void Awake()
    {
        _animator = GetComponent<Animator>();
        _laserOrder = new List<int>();
        for (int i = 0; i < laserLineGroups.Count+1; i++)
        {
            _laserOrder.Add(i);
        }
        _laserOrder.Shuffle();
        _interval = Random.Range(minInterval, maxInterval);

        _useTut = SaveGameManager.activeGame != null && SaveGameManager.activeGame.bossesDefeated.Contains(BossName.GlitchBoss);
        if(_useTut)
        {
            silohuette.transform.position -= Vector3.up * 0.25f;
        }
    }

    public IEnumerator Start()
    {
        _room = LayoutManager.instance && LayoutManager.instance.currentRoom ? LayoutManager.instance.currentRoom : FindObjectOfType<Room>();
        if (_useTut)
        {
            yield return new WaitForSeconds(0.5f);
            var e = GetComponent<Enemy>();
            if(e) { e.health = e.maxHealth * 2; }
            foreach (var bg in bgObjects) { bg.SetActive(false); }
            glitchQuad.SetActive(true);
        }
    }

    public void Update()
    {
        if (!_lasersFiring)
        {
            if (_timer < _interval)
            {
                _timer += Time.deltaTime;
            }
            else
            {
                _interval = Random.Range(minInterval, maxInterval);
                _timer = 0;
                if (_useTut)
                {
                    _spawn = !_spawn;

                    if (_spawn)
                    {
                        SpawnFalseBeastProjector();
                    }
                    else
                    {
                        StartCoroutine(ActivateRandomLaser());
                    }
                }
                else
                {
                    StartCoroutine(ActivateRandomLaser());
                }
            }
        }
    }

    public void LateUpdate()
    {
        if(_useTut)
        {
            silohuette.sprite = tutSmith;
            silohuette.transform.localScale = Vector3.one * 0.75f;
        }
    }

    public void OnStartDeath()
    {
        StopAllCoroutines();
        shooter.enabled = false;
        spinner.Stop();
        foreach (var group in laserLineGroups)
        {
            foreach (var line in group.lines)
            {
                line.Stop();
            }
        }
        _lasersFiring = true; //prevents update
        StartCoroutine(HandleDeath());
    }
    
    public IEnumerator HandleDeath()
    {
        if (EnemyManager.instance)
        {
            EnemyManager.instance.HurtAllEnemies(10000, DamageType.Generic, true);
        }

        if (PlayerManager.instance)
        {
            foreach (var p in PlayerManager.instance.players)
            {
                p.invincible = true;
                p.fatigued = true;
            }
        }
        
        MusicController.instance.StartFadeOut(1f);
        MainCamera.instance.Shake(6, 0.5f, 12);
        MainCamera.instance.MoveCamera(transform.position, 5);

        TransitionFade.instance.SetCullingMask(LayerMask.GetMask());
        TransitionFade.instance.FadeIn(0.25f, Color.white);

        yield return new WaitForSeconds(2 + 1/6f);

        TransitionFade.instance.FadeOut(2, Color.white);

        var timer = 0f;
        bool breakOut = false;
        while (timer < 3)
        {
            timer += Time.deltaTime;
            yield return null;
            if (LayoutManager.instance && LayoutManager.instance.transitioning)
            {
                breakOut = true;
            }
        }

        if(AchievementManager.instance.MegaBeastCoreDefeated())
        {
            yield return new WaitForSeconds(0.1f);
            while (AchievementScreen.instance.visible)
            {
                yield return new WaitForSeconds(0.1f);
            }
        }

        GameMode gameMode = GameMode.Normal;
        if(SaveGameManager.activeSlot != null)
        {
            SaveGameManager.activeSlot.RunCompleted();
            gameMode = SaveGameManager.activeGame.gameMode;
        }

        if (_useTut)
        {
            SeedHelper.StartSpookyMode(false);
        }
        else if (breakOut)
        {
            TransitionFade.instance.FadeIn(2, Color.white);
            yield break;
        }
        else
        {
            switch (gameMode)
            {
                case GameMode.Normal:
                    SceneManager.LoadScene("EndScreen02");
                    break;
                case GameMode.Exterminator:
                    SceneManager.LoadScene("ExterminatorEndScreen");
                    break;
                case GameMode.MegaMap:
                    SceneManager.LoadScene("MegaMapEndScreen");
                    break;
                case GameMode.Spooky:
                    SceneManager.LoadScene("SpookyEnding");
                    break;
                default:
                    SceneManager.LoadScene("Congratulations");
                    break;
            }
        }
    }

	public IEnumerator ActivateRandomLaser()
    {
        _lasersFiring = true;

        while(shooter.shooting)
        {
            yield return null;
        }
        shooter.enabled = false;

        _animator.SetTrigger("StartCharge");

        yield return new WaitForSeconds(1);

        var pattern = _laserOrder[_currentLaserPattern];
        if(pattern >= laserLineGroups.Count)
        {
            spinner.Activate();
        }
        else
        {
            var group = laserLineGroups[pattern];
            foreach (var line in group.lines)
            {
                line.Activate();
            }
        }

        yield return new WaitForSeconds(3);
        _animator.SetTrigger("EndCharge");

        _currentLaserPattern++;

        if(_currentLaserPattern > _laserOrder.Count-1)
        {
            _currentLaserPattern = 0;
            _laserOrder.Shuffle();
        }

        shooter.enabled = true;
        _lasersFiring = false;
    }

    public void SpawnFalseBeastProjector()
    {
        var position = transform.position;
        var d = Random.insideUnitSphere.normalized;
        position += d * (4f + Random.value * 3f);
        var e = Instantiate<Enemy>(falseBeastProjPrefab, position, Quaternion.identity, _room.transform);
        var spawnPickups = e.GetComponent<SpawnPickUpsOnDeath>();
        if (spawnPickups) { spawnPickups.spawnChance = 0.1f; }
        FXManager.instance.SpawnFX(FXType.AnimeSplode, position);
    }
}

[Serializable]
public class LaserLineGroup
{
    public List<LaserLine> lines;
}

