using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MegaBeast : MonoBehaviour
{
	public bool fillSlots = true;

    public Animator animator;

	public MegaBeastComponent[] centerSlotPrefabs;
    public MegaBeastComponent[] nearSidePrefabs;
    public MegaBeastComponent[] farSidePrefabs;
    public MegaBeastComponent[] oneUnitPrefabs;
	public MegaBeastComponent[] twoUnitPrefabs;
    public MegaBeastComponent[] fourUnitPrefabs;
    public GameObject[] bgTentacles;

	public SpriteRenderer[] centerSlots;
    public SpriteRenderer[] nearSides;
    public SpriteRenderer[] farSides;
    public SpriteRenderer[] oneUnitSlots;
    public SpriteRenderer[] twoUnitSlots;
    public SpriteRenderer[] fourUnitSlots;

    public float maxTimeBetweenActions = 7;
    public float minTimeBetweenActions = 3;
    private float _timeBetweenActions;

    public BeastGutsPortal beastGutsPortal;

    private Enemy _enemy;
    private List<MegaBeastComponent> _allComponents = new List<MegaBeastComponent>();
    private List<MegaBeastComponent> _actionComponents = new List<MegaBeastComponent>();
    private int[] _actionOrder;
    private int _currentActionIndex;
    private DamageCreatureTrigger _damageCreatureTrigger;
    public float _componentActionTimer;

    public Material altMaterial;

    protected void Awake()
    {
        animator = GetComponent<Animator>();

        _enemy = GetComponent<Enemy>();
        _damageCreatureTrigger = GetComponentInChildren<DamageCreatureTrigger>();

        if (bgTentacles.Length > 0)
        {
            var bgTentaclesInstance = Instantiate(bgTentacles[Random.Range(0, bgTentacles.Length)], transform);
            bgTentaclesInstance.transform.localPosition = Vector3.zero;
        }

        if (fillSlots)
        {
            MegaBeastComponent pick;

            foreach (var c in centerSlots)
            {
                pick = centerSlotPrefabs[Random.Range(0, centerSlotPrefabs.Length)];
                AddMBComponent(Instantiate(pick, c.transform), c);
            }
            
            foreach (var r in farSides)
            {
                pick = farSidePrefabs[Random.Range(0, farSidePrefabs.Length)];
                AddMBComponent(Instantiate(pick, r.transform), r);
            }

            foreach (var r in nearSides)
            {
                pick = nearSidePrefabs[Random.Range(0, nearSidePrefabs.Length)];
                AddMBComponent(Instantiate(pick, r.transform), r);
            }

            foreach (var r in oneUnitSlots)
            {
                pick = oneUnitPrefabs[Random.Range(0, oneUnitPrefabs.Length)];
                AddMBComponent(Instantiate(pick, r.transform), r);
            }

			foreach (var r in twoUnitSlots)
			{
				pick = twoUnitPrefabs[Random.Range(0, twoUnitPrefabs.Length)];
				AddMBComponent(Instantiate(pick, r.transform), r);
			}

            foreach (var r in fourUnitSlots)
            {
                pick = fourUnitPrefabs[Random.Range(0, fourUnitPrefabs.Length)];
                AddMBComponent(Instantiate(pick, r.transform), r);
            }
        }

        if (altMaterial)
        {
            var renderers = GetComponentsInChildren<SpriteRenderer>();
            foreach (var r in renderers)
            {
                if (r.material.name.Contains("MegaBeast")) { r.material = altMaterial; }
            }
        }

        _enemy.onStartDeath.AddListener(OnDeathStart);
        _enemy.onEndDeath.AddListener(OnDeathEnd);
    }

    public IEnumerator Start()
    {
        RefreshActionOrder();

        //Pre Fight Clean-up
        var p = new ProjectileType[]
        {
            ProjectileType.RedBioPlasma,
            ProjectileType.GreenBioPlasma,
            ProjectileType.TumorBullet,
            ProjectileType.EyePlasma,
            ProjectileType.Molotov,
            ProjectileType.SlimeBullet,
        };

        ProjectileManager.instance.ClearProjectiles(p);

        var g = new GibType[]
        {
            GibType.BrownRock,
            GibType.BuriedCityRock,
            GibType.CaveMetal,
            GibType.Concrete,
            GibType.ExplosiveDoorShield,
            GibType.GlitchGib,
            GibType.GreenMeat,
            GibType.PinkMeat,
        };

        GibManager.instance.ClearGibs(g);

        centerSlotPrefabs = null;
        nearSidePrefabs = null;
        farSidePrefabs = null;
        oneUnitPrefabs = null;
        twoUnitPrefabs = null;
        fourUnitPrefabs = null;

        yield return null;

        Resources.UnloadUnusedAssets(); //Calls GC.Collect
    }

    public void OnDeathStart()
    {
        foreach (var c in _allComponents)
        {
            c.StartDeath();
        }

        EnemyManager.instance.KillAllEnemies();
        _damageCreatureTrigger.gameObject.SetActive(false);
        MainCamera.instance.Shake(5, 0.5f, 12);
    }

    public void OnDeathEnd()
    {
        //add code for beast guts environment
        EnemyManager.instance.KillAllEnemies();
        StartCoroutine(HandleDeath());
    }

    private IEnumerator HandleDeath()
    {
        var achievementEarned = AchievementManager.instance.MegaBeastDefeated();

        if(achievementEarned)
        {
            yield return new WaitForSeconds(0.1f);
            while(AchievementScreen.instance.visible)
            {
                yield return new WaitForSeconds(0.1f);
            }
        }

        var allowPortal = SaveGameManager.activeGame == null || 
            (SaveGameManager.activeGame.gameMode != GameMode.BossRush &&
            SaveGameManager.activeGame.gameMode != GameMode.Exterminator);
        
        if (SaveGameManager.beastGutsUnlocked && allowPortal)
        {
            beastGutsPortal.Show();
        }
        else
        {
            TransitionFade.instance.SetCullingMask(LayerMask.GetMask());
            TransitionFade.instance.FadeOut(2, Color.red);
            yield return new WaitForSeconds(2);
            
            GameMode gameMode = GameMode.Normal;
            var activeSlot = SaveGameManager.activeSlot;
            if (activeSlot != null)
            {
                activeSlot.RunCompleted();
                gameMode = SaveGameManager.activeGame.gameMode;
            }

            switch (gameMode)
            {
                case GameMode.Normal:
                    SceneManager.LoadScene("EndScreen01");
                    break;
                case GameMode.Spooky:
                    SceneManager.LoadScene("SpookyEnding");
                    break;
                case GameMode.BossRush:
                    SceneManager.LoadScene("BossRushEndScreen");
                    break;
                case GameMode.Exterminator:
                    SceneManager.LoadScene("ExterminatorEndScreen");
                    break;
                default:
                    SceneManager.LoadScene("Congratulations");
                    break;
            }
        }
    }

    protected void Update()
    {
        _timeBetweenActions = Mathf.Clamp(_enemy.health / _enemy.maxHealth * maxTimeBetweenActions, minTimeBetweenActions, maxTimeBetweenActions);
        //if(!_componentActioning && _actionComponents.Count > 0 && Time.timeScale > 0)
        if (_actionComponents.Count > 0 && Time.timeScale > 0)
        {
            if (_componentActionTimer > _timeBetweenActions)
            {
                var nextActionComponent = _actionComponents[_actionOrder[_currentActionIndex]];
                if(nextActionComponent && nextActionComponent.canAct)
                {
                    _componentActionTimer = 0;
                    //StartCoroutine(StartAndWaitForNextAction(nextActionComponent));
                    StartCoroutine(WaitAndStartNextAction(nextActionComponent));
                }

                _currentActionIndex++;
                if(_currentActionIndex >= _actionComponents.Count)
                {
                    RefreshActionOrder();
                }
            }
            else
            { 
                _componentActionTimer += Time.deltaTime;
            }
        }
    }

    public void RefreshActionOrder()
    {
        _actionComponents.RemoveAll((c) => c == null || c.state != DamageableState.Alive);
        var componentIndices = new List<int>();
        for (int i = 0; i < _actionComponents.Count; i++)
        {
            componentIndices.Add(i);
        }
        _actionOrder = componentIndices.OrderBy(x => Random.value).ToArray();
        _currentActionIndex = 0;
    }

    public IEnumerator WaitAndStartNextAction(MegaBeastComponent component)
    {
        //don't try to start component action if it's still acting
        while (component != null &&
               component.state == DamageableState.Alive &&
               component.eventAction.eventCycleAtive)
        {
            yield return new WaitForSeconds(0.5f);
        }

        if (component != null && component.state == DamageableState.Alive)
        {
            component.TriggerEventAction();
        }
    }

    public IEnumerator StartAndWaitForNextAction(MegaBeastComponent component)
    {
        component.TriggerEventAction();

        while (component != null &&
            component.state == DamageableState.Alive && 
            component.eventAction.eventCycleAtive)
        {
            yield return null;
        }
    }

    public void AddMBComponent(MegaBeastComponent component, SpriteRenderer parentSprite)
    {
        component.transform.localPosition = component.localOffset;
        component.linkedDamageable = _enemy;

        if (parentSprite.transform.localPosition.x > 0)
        {
            component.transform.localRotation = Constants.flippedFacing;
        }

        if (component.eventAction)
        {
            _actionComponents.Add(component);
        }

        _allComponents.Add(component);

        parentSprite.enabled = false;
        component.onEndDeath.AddListener(() => parentSprite.enabled = true);
    }
}
