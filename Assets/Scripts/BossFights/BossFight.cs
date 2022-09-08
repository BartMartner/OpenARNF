using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.Events;

public class BossFight : MonoBehaviour
{
    public List<Enemy> enemies;
    public List<GameObject> otherPausables;

    private Room _parentRoom;
    private bool _bossFightActive;
    private Collider2D _collider2D;
    private bool _allBossMonstersDead;
    private List<IPausable> _actualPausables = new List<IPausable>();

    public bool panToBoss = true;
    public float currentHealth;
    public float maxHealth;
    public bool neverEndLockDown;
    public bool doNotRestartMusic;
    public bool useFinalBossMusic;
    public bool useSecretBossMusic;

    public UnityEvent onDestroyFromSave;

    public UnityEvent onBossFightStart;
    public UnityEvent onBossFightEnd;

    private IEnumerator _checkForEndOfFight;

    private Transform _spawnPickUpsTransform;
    private Vector3 _spawnPickUpsPos;

    private void Awake()
    {
        _parentRoom = GetComponentInParent<Room>();
        _collider2D = GetComponent<Collider2D>();
        if (SaveGameManager.activeGame != null && SaveGameManager.activeGame.bossesDefeated.Contains(_parentRoom.roomInfo.boss))
        {
            if (onDestroyFromSave != null)
            {
                onDestroyFromSave.Invoke();
            }

            Destroy(gameObject);
        }

        foreach (var go in otherPausables)
        {
            var p = go.GetComponent<IPausable>();
            if(p != null)
            {
                _actualPausables.Add(p);
            }
        }
    }

    private IEnumerator Start()
    {
        if (LayoutManager.instance)
        {
            while (LayoutManager.instance.transitioning)
            {
                yield return null;
            }
        }

        MusicController.instance.StartFadeOut(1f);
        var player1 = PlayerManager.instance.player1;

        while(!_bossFightActive)
        {
            if (_collider2D.bounds.Contains(player1.transform.position))
            {
                Debug.Log("Starting Boss Fight!");
                _bossFightActive = true;

                PlayerManager.instance.PauseAllPlayers();

                foreach (var e in enemies)
                {
                    e.Pause();
                    e.isBoss = true;
                }

                foreach (var p in _actualPausables)
                {
                    p.Pause();
                }

                _parentRoom.StartLockDown();

                if (onBossFightStart != null)
                {
                    onBossFightStart.Invoke();
                }

                yield return new WaitForSeconds(1f);

                var _originalPosition = MainCamera.instance.transform.position;

                if (panToBoss)
                {
                    MainCamera.instance.activeTracking = false;
                    yield return StartCoroutine(MainCamera.instance.MoveToPosition(enemies[0].position, 10));
                }

                BossFightUI.instance.healthBar.Show(this);
                yield return StartCoroutine(BossFightUI.instance.GetReadySequence());

                if(useSecretBossMusic)
                {
                    MusicController.instance.PlaySecretBossMusic();
                }
                else if (useFinalBossMusic)
                {
                    MusicController.instance.PlayFinalBossMusic();
                }
                else
                {
                    MusicController.instance.PlayBossFightMusic();
                }

                if(panToBoss)
                { 
                    yield return StartCoroutine(MainCamera.instance.MoveToPosition(_originalPosition, 10));
                    MainCamera.instance.activeTracking = true;
                }
                else
                {
                    yield return new WaitForSeconds(1f);
                }

                _checkForEndOfFight = CheckForEndOfFight();
                StartCoroutine(_checkForEndOfFight);

                PlayerManager.instance.UnpauseAllPlayers();

                foreach (var e in enemies)
                {
                    e.Unpause();
                }

                foreach (var p in _actualPausables)
                {
                    p.Unpause();
                }
            }
            else
            {
                yield return null;
            }
        }
    }

    private IEnumerator CheckForEndOfFight()
    {
        while(_bossFightActive)
        {
            CalculateHealth();

            if (currentHealth <= 0 && _allBossMonstersDead)
            {
                yield return new WaitForSeconds(0.5f);
                CalculateHealth(); //if no health was added, end the fight;
                if (currentHealth <= 0)
                {
                    EndBossFight();
                    yield break;
                }
            }

            yield return null;
        }
    }

    private void CalculateHealth()
    {
        var oldMaxHealth = maxHealth;
        currentHealth = 0;
        maxHealth = 0;

        _allBossMonstersDead = true;
        foreach (var mob in enemies)
        {
            if (!mob) continue;
            currentHealth += mob.health;
            maxHealth += mob.maxHealth;
            if(mob.state != DamageableState.Dead)
            {
                _allBossMonstersDead = false;
            }

            if (!_spawnPickUpsTransform)
            {
                var spuod = mob.GetComponent<SpawnPickUpsOnDeath>();
                if (spuod && spuod.spawnPosition)
                {
                    _spawnPickUpsTransform = spuod.spawnPosition;
                    _spawnPickUpsPos = _spawnPickUpsTransform.position;
                }
            }
            else
            {
                _spawnPickUpsPos = _spawnPickUpsTransform.position;
            }
        }

        if (maxHealth < oldMaxHealth) { maxHealth = oldMaxHealth; }
    }

    public void EndBossFight()
    {
        _bossFightActive = false;

        if(_checkForEndOfFight != null)
        {
            StopCoroutine(_checkForEndOfFight);
            _checkForEndOfFight = null;
        }

        BossFightUI.instance.healthBar.Hide();

        if (!neverEndLockDown) { _parentRoom.EndLockDown(); }

        var bossName = _parentRoom.roomInfo.boss;
        Debug.Log(bossName + " Defeated!");

        if (bossName != BossName.None)
        {
            if (SaveGameManager.activeGame != null)
            {
                SaveGameManager.activeGame.bossesDefeated.Add(bossName);
                SaveGameManager.instance.Save();

                if (SaveGameManager.activeGame.gameMode == GameMode.BossRush)
                {
                    var prefab = ResourcePrefabManager.instance.LoadGameObject("PickUps/" + MinorItemType.GlitchScrap).GetComponent<MinorItemPickUp>();
                    var pathFinder = _parentRoom.gridPathFinder;
                    pathFinder.Refresh();
                    var node = pathFinder.GetClosestNode(_spawnPickUpsPos);
                    var position = node != null ? (Vector3)node.position : _spawnPickUpsPos;
                    var minorItem = Instantiate(prefab, position, Quaternion.identity, PickUpManager.instance.transform) as MinorItemPickUp;
                    minorItem.data.globalID = -99;
                }
            }

            if (AchievementManager.instance)
            {
                AchievementManager.instance.BossDefeated(bossName);
            }
        }

        if(onBossFightEnd != null)
        {
            onBossFightEnd.Invoke();
        }

        if (doNotRestartMusic)
        {
            MusicController.instance.StartFadeOut(1);
        }
        else
        {
            MusicController.instance.PlayEnvironmentMusic();
        }
    }

    private void OnDestroy()
    {
        if (BossFightUI.instance)
        {
            BossFightUI.instance.healthBar.Hide();
        }
    }
}
