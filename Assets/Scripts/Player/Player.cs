//#define ONEHITKILL

using UnityEngine;
using System.Collections;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.Events;
using Rewired;
using cPlayer = Rewired.Player;
using Random = UnityEngine.Random;

public partial class Player : Damageable, IPausable, IHasTeam, IHasVelocity, IReactsToStatusEffect, ILiquidSensitive
{
    public static Player instance;

    private Team _team = Team.Player;
    public Team team
    {
        get
        {
            return _team;
        }
        set
        {
            _team = value;
            projectileStats.team = value;
            _originalProjectileStats.team = value;
        }
    }

    private bool _started;
    public bool started { get { return _started; } }

    private float _xAxis;
    private float _yAxis;
    private float _absXAxis;
    private float _absYAxis;
    public Direction facing = Direction.Right;
    private bool _changingFacing;

    public float maxSpeed = 9.25f;
    private float _animatorRunSpeed = 8.25f;
    public bool knockBack { get; protected set; }
    public bool inAirFromKnockBack { get; protected set; }
    private bool _allowKnockback = true;
    protected Texture2D _originalPalette;
    protected Texture2D _palette;
    public Texture2D palette { get { return _palette; } }

    [Header("Jumping")]
    public float maxJumpHeight = Constants.startingMaxJumpHeight;
    public float minJumpHeight = Constants.startingMinJumpHeight;
    public float timeToJumpApex = Constants.startingJumpTime;
    public float jumpTimeMod = 1;
    private float _gravity;
    public float gravity { get { return _gravity; } }
    private float _maxJumpVelocity;
    public float maxJumpVeloctiy { get { return _maxJumpVelocity; } }
    private float _minJumpVelocity;
    public float minJumpVelocity { get { return _minJumpVelocity; } }

    private IEnumerator _headCheck;

    public bool jumping;
    public bool jumpInputOverride;
    public bool canJump = true;
    public bool canWallJump = false;
    public string jumpString;
    public AudioClip jumpSound;
    public AudioClip landingSound;
    private float _airControlMod;
    private bool _justLanded;
    private bool _spinJumping;
    private int _unGroundedFrames;
    public bool spinJumping
    {
        get { return _spinJumping; }

        set
        {
            if (value != _spinJumping)
            {
                _spinJumping = value;

                if (_spinJumping)
                {
                    if (onSpinJumpStart != null) onSpinJumpStart();
                }
                else
                {
                    if (onSpinJumpEnd != null) onSpinJumpEnd();
                }

                if (!_spinJumping)
                {
                    var topDistance = controller2D.topDistance();
                    var bottomDisance = controller2D.bottomDistance();

                    var offset = controller2D.collisions.above ? controller2D.collisions.below ? Vector2.zero : -_spinJumpLandOffset : controller2D.collisions.below ? _spinJumpLandOffset : Vector2.zero;

                    if ((topDistance + bottomDisance + _spinJumpBoundsSize.y < _originalSize.y) || controller2D.collisions.below && !CanStand(offset))
                    {
                        _crouching = true;
                        animator.SetTrigger("CrouchImmediate");
                        StartCoroutine(JustCrouched());
                    }
                }

                SetCollisionBounds();
            }
        }
    }
    private Vector2 _spinJumpLandOffset;
    public Action onSpinJumpStart;
    public Action onSpinJumpEnd;
    public int maxAirJumps = 0;
    public int currentAirJumps;
    public List<TriggerableEvent> jumpEvents = new List<TriggerableEvent>();

    private Vector2 _spinJumpBoundsSize;

    public bool mirrorMode { get; protected set; }  = false;
    public bool gravityFlipped { get; protected set; }

    [Header("Shooting")]
    public ProjectileStats projectileStats;
    private ProjectileStats _originalProjectileStats;
    public AudioClip attackSound;
    public AudioClip attackSoundX1p5;
    public AudioClip attackSoundX2;
    public int arcShots;
    public float fireArc;
    public int aiming;
    public Transform shootPoint;
    public Transform shootPointUp;
    public Transform shootPointDown;
    public Transform shootPointAngleUp;
    public Transform shootPointAngleDown;
    public Transform shootPointSpin;
    public bool attacking;
    /// <summary>
    /// Used to limit turning
    /// </summary>
    private bool _justAttacked;
    private float _timeSinceLastAttack;
    public float attackDelay = 0.4f;
    private Dictionary<float, AudioClip> _attackSounds;
    private float _damageMultiplier = 1f;
    public float damageMultiplier
    {
        get { return _damageMultiplier; }
    }

    public string attackString
    {
        get { return _confused ? "Jump" : "Attack"; }
    }

    private float _timeSinceLastBolt;
    public bool boltAttacking;
    public Action<ProjectileStats, AimingInfo, int, float> onBolt;

    [Header("Crouching")]
    private bool _crouching;
    public bool crouching
    {
        get { return _crouching; }

        set
        {
            _crouching = value;
            SetCollisionBounds();
        }
    }
    private bool _justCrouched;
    public bool justUncrouched { get; protected set; }
    private bool _crouchAxis;
    public bool crouchAxis { get { return _crouchAxis; } }
    private Vector2 _originalSize;
    private Vector2 _originalCenter;
    private Vector2 _crouchCenter;
    private Vector2 _crouchBoundsSize;

    [Header("Items")]

    /// <summary>
    /// Items currently possessed by the player not including activated items.
    /// </summary>
    public List<MajorItem> itemsPossessed = new List<MajorItem>();

    /// <summary>
    /// Items that have had their effect applied to the player. Used to avoid reapplying pick-up effects
    /// </summary>
    protected HashSet<MajorItem> _itemsApplied = new HashSet<MajorItem>();
    protected HashSet<MajorItem> _listenersApplied = new HashSet<MajorItem>();
    public Action onCollectItem;

    [Header("ActivatedItems")]
    public PlayerActivatedItem activatedItem;

    [Header("SpecialMovement")]
    protected PlayerSpecialMovement _activeSpecialMove;
    public PlayerSpecialMovement activeSpecialMove { get { return _activeSpecialMove; } }
    public List<PlayerSpecialMovement> specialMoves = new List<PlayerSpecialMovement>();

    [Header("Fatigue")]
    public bool fatigued;
    private float _fatigueAmount;
    private IEnumerator _fatigueCoroutine;

    [Header("EnergyWeapons")]
    public List<PlayerEnergyWeapon> energyWeapons = new List<PlayerEnergyWeapon>();
    public Action onSelectedWeaponChanged;
    public AudioClip weaponFail;
    public PlayerEnergyWeapon selectedEnergyWeapon { get { return _selectedEnergyWeapon; } }
    protected PlayerEnergyWeapon _selectedEnergyWeapon;
    protected int _selectedEnergyWeaponIndex;

    [Header("Hovering")]
    public bool canHover;
    public ParticleSystem hoverParticles;
    public AudioSource hoverSound;
    public bool hoverJumping;
    private float _hoverTimer;
    private float _localHoverX;
    //How long the player can hold jump and still get the hover effect
    public float hoverMaxVelocity = 0;
    public float hoverTime = 0f;
    private Vector3 _jetPackHoverParticlePosition = new Vector3(-0.46f, 0, 0);
    private Vector3 _hoverBootsHoverParticlePosition = new Vector3(0.0625f, -0.9375f, 0);

    [Header("Charging")]
    public bool canCharge;
    public AudioSource chargingSound;
    public Animator chargingFX;
    private Color _chargeColor = Constants.blasterGreen;
    private float _maxChargeDamage = 3f;
    private float _maxChargeSize = 2;
    private float _chargeTime = 1.75f;
    private bool _charging;

    [Header("Conditions")]
    [Tooltip("uxInvincible is used to make the player temporarily invincible. It won't override invincibility granted by items or vice versa")]
    public bool uxInvincible;
    public bool paralyzed;
    public bool slowed;
    private bool _confused;
    public bool confused
    {
        get { return _confused; }
        set
        {
            bool immune = environmentalResistances.HasFlag(EnvironmentalEffect.Confusion);
            if (value != _confused || immune && _confused)
            {
                _confused = immune ? false : value;

                if (mainCamera) { mainCamera.SetGlitch(_confused); }
                if (MusicController.instance) { MusicController.instance.SetPitch(_confused ? 0.5f : 1f); }
            }
        }
    }    

    [Header("Currency")]
    public int grayScrap;
    public int redScrap;
    public int greenScrap;
    public int blueScrap;
    public int totalSpecialScrap { get { return redScrap + greenScrap + blueScrap; } }

    [Header("BaseStats")]

    //Health
    private float _baseMaxHealth;
    protected float _startingHealth = Constants.startingHealth;
    public int healthUps;
    public float bonusRegenerationRate;
    public float regenerationRate;
    private float _regenationCounter;
    private float _itemHealthMultiplier = 1;

    //Energy
    public float energy;
    public float maxEnergy;
    private float _baseMaxEnergy;
    protected float _startingEnergy = Constants.startingEnergy;
    public int energyUps;
    public float bonusEnergyRegenRate;
    private float _bonusEnergyRegenCounter;
    public float itemEnergyRegenRate;
    private float _itemEnergyRegenCounter;
    private float _energyRegenCounter;
    private float _energyRegenTime = 5f;
    private float _energyRegenTimeMod = 1f;
    private float _itemEnergyMultiplier = 1;
    private bool _weaponWheelToggle;

    //Damage
    private float _baseDamage = 3.5f;
    public int damageUps;
    private float _baseExplosionDamage = 0f;
    private float _bonusDamage;
    public float bonusDamage
    {
        get { return _bonusDamage; }
        set
        {
            _bonusDamage = value;
            CalculateDamage();
        }
    }
    private float _itemDamageMultiplier = 1;

    //Attack
    private float _baseAttackDelay;
    public int attackUps;
    private int _bonusAttack;
    public int bonusAttack
    {
        get { return _bonusAttack; }
        set
        {
            _bonusAttack = value;
            CalculateAttackDelay();
        }
    }

    private Action<Enemy> _onKillEnemy;

    //Speed
    private float _baseMaxSpeed;
    public int speedUps;
    private float _bonusSpeed;
    public float bonusSpeed
    {
        get { return _bonusSpeed; }
        set
        {
            _bonusSpeed = value;
            CalculateMaxSpeed();
        }
    }
    public float speedRatio;

    //Projectile Speed
    private float _baseProjectileSpeed = 16;
    public float baseProjectileSpeed { get { return _baseProjectileSpeed; } }
    public int shotSpeedUps;
    private float _shotSpeedModifier;
    public float shotSpeedModifier { get { return _shotSpeedModifier; } }
    private float _bonusShotSpeed;
    public float bonusShotSpeed
    {
        get { return _bonusShotSpeed; }
        set
        {
            _bonusShotSpeed = value;
            CalculateShotSpeed();
        }
    }
    public bool rotaryShot { get; private set; }
    private int _rotaryShotCount;

    //Projectile Size
    private float _baseProjectileSizeMod = 1;
    private float _bonusProjectileSizeMod;
    public float bonusShotSizeMod
    {
        get { return _bonusProjectileSizeMod; }
        set
        {
            _bonusProjectileSizeMod = value;
            CalculateShotSpeed();
        }
    }
    private float _itemShotSizeMultiplier = 1;

    public Action onSetStats;

    //Input
    private int _playerId = 0;
    public int playerId
    {
        get { return _playerId; }
        set
        {
            _playerId = value;
            _controller = ReInput.players.GetPlayer(_playerId);
        }
    }

    private cPlayer _controller;
    public cPlayer controller
    {
        get { return _controller; }
        set { _controller = controller; }
    }

    [Header("Other")]
    public Dictionary<MajorItem, Action> itemHurtActions = new Dictionary<MajorItem, Action>();
    public List<SuitBurst> hurtBursts = new List<SuitBurst>();
    new public SpriteRenderer light;
    public Sprite light100;
    public Sprite light200;
    public Sprite light400;
    private float _playTime;
    public float playTime { get { return _playTime; } }
    public float pickUpRangeBonus = 0;
    public Action onRespawn;
    public Action onResetAnimatorAndCollision;

    public bool lowFriction;
    public bool morphing { get; protected set; }
    public bool spiderForm { get; protected set; }
    public float spiderFormSpeed = 4.25f;
    public DropType forceDropType;

    public Transform helmPoint;

    [HideInInspector]
    public List<LatchOnPlayer> attachedLatchers = new List<LatchOnPlayer>();

    private bool _allRevealed;
    
    public bool artificeMode { get; protected set; }

    public override Vector3 position
    {
        get { return spiderForm ? transform.position + (gravityFlipped ? Vector3.up : Vector3.down) : transform.position; }
    }

    public bool teleporting;

    [EnumFlags]
    public EnvironmentalEffect environmentalResistances;
    private BoxCollider2D _boxCollider2D;
    public BoxCollider2D boxCollider2D { get { return _boxCollider2D; } }

    public Vector2 gridPosition;
    private Vector2 _lastGridPosition;
    //Maybe this should be on the camera? Co-op...
    public Action onGridPositionChanged;

    private Material _defaultMaterial;
    private Material _material;
    public Material material
    {
        get { return _material; }
        set
        {
            _material = value;
            _originalPalette = _material.GetTexture("_Palette") as Texture2D;
        }
    }

    [HideInInspector]
    public Controller2D controller2D;
    public bool grounded
    {
        get { return controller2D.bottomEdge.touching; }
    }

    private Vector2 _velocity;
    public Vector2 velocity { get { return _velocity; } set { _velocity = value; } }
    public AudioSource loopingAudio;

    private PlayerSpriteMaker _playerSpriteMaker;

    public GameObject playerDeathParticle;

    [HideInInspector]
    public Vector3 lastSafePosition;
    [HideInInspector]
    public bool lastSafeGravity;

    [Header("Followers")]
    public List<Follower> followers = new List<Follower>();
    public int orbitalFollowerCount;
    public int trailFollowerCount;
    public int nanobotsPerSpawn = 2;
    public float blessingTimeMod = 1f;
    private List<Nanobot> _nanobots = new List<Nanobot>();
    public List<Nanobot> nanobots { get { return _nanobots; } }
    private int _nanobotReserve;

    [Header("Camera")]
    public MainCamera mainCamera;
    public float _lookTimer;

    public TraversalCapabilities traversalCapabilities;
    private float _offScreenTime;

    private float _lastEnergy;
    private bool _lockPosition;
    private float _mapCompletion;
    public bool paused { get; private set; }

#if ARCADE
    public int extraLives
    {
        get
        {
            if (SaveGameManager.instance) { return SaveGameManager.instance.credits; }
            return 0;
        }

        set
        {
            if (SaveGameManager.instance) { SaveGameManager.instance.credits = value; }
        }
    }
#else
    public int extraLives { get; private set; }
#endif

    protected override void Awake()
    {
        base.Awake();

        instance = this;
        controller2D = GetComponentInChildren<Controller2D>();
        _boxCollider2D = GetComponent<BoxCollider2D>();
        _originalSize = _boxCollider2D.size;
        _originalCenter = _boxCollider2D.offset;
        _crouchCenter = new Vector2(_originalCenter.x, -0.5f);
        _crouchBoundsSize = new Vector2(_originalSize.x, 2);
        _spinJumpBoundsSize = new Vector2(_originalSize.x, 1.75f);
        _spinJumpLandOffset = new Vector2(0, (_originalSize.y - _spinJumpBoundsSize.y) * 0.5f - _originalCenter.y);
        _playerSpriteMaker = GetComponent<PlayerSpriteMaker>();
        _originalProjectileStats = new ProjectileStats(projectileStats);
        _attackSounds = new Dictionary<float, AudioClip>() { { 1, attackSound } };
        _originalPalette = Resources.Load<Texture2D>("Palettes/Player/PlayerPalette");
        _palette = _originalPalette;
        _controller = ReInput.players.GetPlayer(_playerId);

        if (!mainCamera) { mainCamera = MainCamera.instance; }
    }

    protected override void Start()
    {
        base.Start();

        aegisTime = 0.92f;
        _baseDamage = Constants.startingDamage;
        _baseMaxEnergy = _startingEnergy;
        _baseMaxHealth = _startingHealth;
        _baseMaxSpeed = Constants.startingMaxSpeed;
        _baseAttackDelay = Constants.startingAttackDelay;
        _baseProjectileSpeed = Constants.startingProjectileSpeed;

        if (SaveGameManager.activeGame != null && !DeathmatchManager.instance)
        {
            var activeGame = SaveGameManager.activeGame;
            _itemsApplied = new HashSet<MajorItem>(activeGame.itemsApplied);
            itemsPossessed = activeGame.itemsPossessed.ToList();

            if (activeGame.currentActivatedItem != 0)
            {
                activatedItem = Instantiate(Resources.Load("ActivatedItems/" + activeGame.currentActivatedItem.ToString())) as PlayerActivatedItem;
                activatedItem.Initialize(this);
            }

            healthUps = activeGame.healthUpsCollected;
            energyUps = activeGame.energyUpsCollected;
            speedUps = activeGame.speedUpsCollected;
            attackUps = activeGame.attackUpsCollected;
            damageUps = activeGame.damageUpsCollected;
            shotSpeedUps = activeGame.shotSpeedUpsCollected;

            health = activeGame.playerHealth;
            energy = activeGame.playerEnergy;
            grayScrap = activeGame.grayScrap;
            redScrap = activeGame.redScrap;
            greenScrap = activeGame.greenScrap;
            blueScrap = activeGame.blueScrap;

            _playTime = SaveGameManager.activeGame.playTime;
            SaveGameManager.instance.onSave += OnSave;

            if (activeGame.gameMode == GameMode.MirrorWorld)
            {
                mirrorMode = true;
                gravityFlipped = mirrorMode;
                CollectMajorItem(MajorItem.ViridianShell, false);
            }

            MatchItems();

            for (int i = 0; i < activeGame.nanobots; i++)
            {
                SpawnNanobot(transform.position, true);
            }
        }
        else
        {
            MatchItems();
            health = maxHealth;
            energy = maxEnergy;

            grayScrap = 25;
            redScrap = 5;
            greenScrap = 5;
            blueScrap = 5;
        }

        onCollectItem += OnCollectItem;

        if (mainRenderer && _material)
        {
            mainRenderer.material = material;
        }
        else
        {
            _material = mainRenderer.material;
        }

        _defaultMaterial = _material;
        onGridPositionChanged += RefreshMapCompletion;
        _started = true;
    }

    private void OnCollectItem()
    {
        if (AchievementManager.instance &&
         SaveGameManager.activeGame != null &&
         SaveGameManager.activeGame.collectRate >= 1)
        {
            AchievementManager.instance.WaitTryEarnAchievement(1, AchievementID.ThoroughBot);
        }
    }

    public void OnSave()
    {
        var activeGame = SaveGameManager.activeGame;
        if (activeGame != null)
        {
            if (state == DamageableState.Alive)
            {
                activeGame.playerHealth = health;
                activeGame.playerEnergy = energy;
            }

            activeGame.healthUpsCollected = healthUps;
            activeGame.energyUpsCollected = energyUps;
            activeGame.damageUpsCollected = damageUps;
            activeGame.attackUpsCollected = attackUps;
            activeGame.speedUpsCollected = speedUps;
            activeGame.shotSpeedUpsCollected = shotSpeedUps;
            activeGame.nanobots = _nanobots.Count;

            activeGame.grayScrap = grayScrap;
            activeGame.redScrap = redScrap;
            activeGame.greenScrap = greenScrap;
            activeGame.blueScrap = blueScrap;
            activeGame.playTime = _playTime;
        }
    }

    public void OnDestroy()
    {
        if (SaveGameManager.instance)
        {
            SaveGameManager.instance.onSave -= OnSave;
        }

        if (LayoutManager.instance)
        {
            LayoutManager.instance.onTransitionComplete -= ShuffleStats;
        }

        foreach (var f in followers) { if (f) { Destroy(f.gameObject); } }
        foreach (var e in energyWeapons) { Destroy(e); }
        foreach (var a in specialMoves) { Destroy(a); }
        foreach (var e in jumpEvents) { Destroy(e); }
    }

    public void CalculateJump()
    {
        var modTimeToJumpApex = timeToJumpApex * jumpTimeMod;

        //Makees it difficult to jump out of water with dive shell
        if (LayoutManager.instance && LayoutManager.instance.currentRoom != null &&
            LayoutManager.instance.currentRoom.roomAbstract != null &&
            LayoutManager.instance.currentRoom.roomAbstract.environmentalEffect.HasFlag(EnvironmentalEffect.Underwater) &&
            itemsPossessed.Contains(MajorItem.DiveShell))
        {
            modTimeToJumpApex *= 1.333f;
        }

        if (spiderForm)
        {
            var spiderJumpMod = 0.675f;
            _gravity = -(maxJumpHeight * 2 * spiderJumpMod) / Mathf.Pow(modTimeToJumpApex * spiderJumpMod, 2);
            _maxJumpVelocity = Mathf.Abs(_gravity) * modTimeToJumpApex * spiderJumpMod;
            _minJumpVelocity = Mathf.Sqrt(2 * Mathf.Abs(_gravity) * (minJumpHeight * spiderJumpMod));
        }
        else
        {
            _gravity = -(2 * maxJumpHeight) / Mathf.Pow(modTimeToJumpApex, 2);
            _maxJumpVelocity = Mathf.Abs(_gravity) * modTimeToJumpApex;
            _minJumpVelocity = Mathf.Sqrt(2 * Mathf.Abs(_gravity) * minJumpHeight);
        }

        if (gravityFlipped)
        {
            _gravity *= -1;
            _maxJumpVelocity *= -1;
            _minJumpVelocity *= -1;
        }
    }

    public void CalculateMaxHealthAndEnergy(bool fireCallback = true)
    {
        maxHealth = Mathf.Clamp(_baseMaxHealth + healthUps * Constants.healthTankAmount, 1, 999) * _itemHealthMultiplier;
        maxEnergy = Mathf.Clamp(_baseMaxEnergy + energyUps * Constants.energyModuleAmount, 1, 999) * _itemEnergyMultiplier;
        health = Mathf.Clamp(health, 0, maxHealth);
        energy = Mathf.Clamp(energy, 0, maxEnergy);

        if (fireCallback && onSetStats != null)
        {
            onSetStats();
        }

        if (maxHealth >= 20) AchievementManager.instance.TryEarnAchievement(AchievementID.PowerShield);
    }

    public void CalculateMaxSpeed(bool fireCallback = true)
    {
        if (spiderForm)
        {
            maxSpeed = _baseMaxSpeed * 1.25f + speedUps * Constants.speedModuleAmount;
        }
        else
        {
            maxSpeed = _baseMaxSpeed + speedUps * Constants.speedModuleAmount;
        }

        if (_empireHelm) { maxSpeed += Mathf.Lerp(0, 3, _mapCompletion); }

        if (artificeMode)
        {
            maxSpeed += greenScrap * Constants.speedModuleAmount;
        }

        maxSpeed += _bonusSpeed;

        if (_energyBoots)
        {
            float max, delta;
            delta = (energy / maxEnergy);
            max = maxEnergy / 3f;
            maxSpeed += Mathf.Lerp(0, max, delta);
        }

        maxSpeed = Mathf.Clamp(maxSpeed, 1, 34);

        speedRatio = maxSpeed / Constants.startingMaxSpeed;

        if (maxSpeed >= 17) { AchievementManager.instance.TryEarnAchievement(AchievementID.BuzzsawShell); }

        if (fireCallback && onSetStats != null)
        {
            onSetStats();
        }
    }

    public void CalculateShotSpeed(bool fireCallback = true)
    {
        _shotSpeedModifier = shotSpeedUps * 1.5f;
        var speed = Mathf.Clamp(_baseProjectileSpeed + shotSpeedModifier + _bonusShotSpeed, 0.05f, 99);

        if (_empireHelm) { speed += Mathf.Lerp(0, 3, _mapCompletion); }

        if (_healthBooster)
        {
            float max, delta;
            if (artificeMode)
            {
                delta = (grayScrap / 99);
                max = delta * 8;
            }
            else
            {
                delta = (health / maxHealth);
                max = maxHealth / 4f;
            }
            speed += Mathf.Lerp(0, max, delta);
        }

        projectileStats.speed = speed;

        if (projectileStats.speed >= 22 && AchievementManager.instance)
        {
            AchievementManager.instance.TryEarnAchievement(AchievementID.Phaserang);
        }

        if (fireCallback && onSetStats != null)
        {
            onSetStats();
        }
    }

    public void CalculateShotSize(bool fireCallback = true)
    {
        var sizeMod = _baseProjectileSizeMod + _bonusProjectileSizeMod;

        if (_blackKey)
        {
            var healthDelta = 1 - (health / maxHealth);
            sizeMod += Mathf.Lerp(0, 2, healthDelta);
        }

        if (_scrapBooster)
        {
            var scrapDelta = (grayScrap / 99f);
            sizeMod += Mathf.Lerp(0, 2, scrapDelta);
        }

        projectileStats.size = 1 * sizeMod * _itemShotSizeMultiplier;

        if (fireCallback && onSetStats != null)
        {
            onSetStats();
        }
    }

    public void CalculateDamage(bool fireCallback = true)
    {
        float damage = _baseDamage;
        if (artificeMode)
        {
            _damageMultiplier = Mathf.Clamp(1 + Constants.damageModuleMultiplier * (damageUps + redScrap), 0.1f, 99);
            damage += Constants.damageModuleBonus * (damageUps + redScrap);
        }
        else
        {
            _damageMultiplier = Mathf.Clamp(1 + Constants.damageModuleMultiplier * damageUps, 0.1f, 99);
            damage += Constants.damageModuleBonus * damageUps;
        }

        damage += _bonusDamage;

        if (_empireHelm) { damage += Mathf.Lerp(0, 3, _mapCompletion); }

        if (_blackKey)
        {
            var healthDelta = 1 - (health/maxHealth);
            _damageMultiplier += Mathf.Lerp(0,1, healthDelta);
            damage += Mathf.Lerp(0,8,healthDelta);
        }

        if (_scrapBooster)
        {
            var scrapDelta = (grayScrap / 99f);
            _damageMultiplier += Mathf.Lerp(0, 1, scrapDelta);
            damage += Mathf.Lerp(0, 8, scrapDelta);
        }

        projectileStats.damage = Mathf.Clamp(damage * _itemDamageMultiplier, 0.1f, 999);
        projectileStats.explosionDamage = _baseExplosionDamage * damageMultiplier;

        if(projectileStats.damage >= 8 && AchievementManager.instance)
        {
            if(projectileStats.damageType.HasFlag(DamageType.Explosive))
            {
                AchievementManager.instance.TryEarnAchievement(AchievementID.Kaboomerang);
            }
        }

        if (fireCallback && onSetStats != null)
        {
            onSetStats();
        }
    }

    public void CalculateAllStats()
    {
        CalculateMaxHealthAndEnergy(false);
        CalculateAttackDelay(false);
        CalculateMaxSpeed(false);
        CalculateJump();
        CalculateDamage(false);
        CalculateShotSpeed(false);
        CalculateShotSize(false);

        if (onSetStats != null)
        {
            onSetStats();
        }
    }

    public void ShuffleStats()
    {
        var healthPercent = health / maxHealth;
        var energyPercent = energy / maxEnergy;
        var allCollected = damageUps + attackUps + shotSpeedUps + healthUps + energyUps + speedUps;
        var unEventSplit = Constants.UnevenSplit(allCollected, 6, new MicrosoftRandom());
        damageUps = unEventSplit[0];
        attackUps = unEventSplit[1];
        shotSpeedUps = unEventSplit[2];
        healthUps = unEventSplit[3];
        energyUps = unEventSplit[4];
        speedUps = unEventSplit[5];

        StartMultiColorFlash(0.2f, Constants.moduleColors.Values.ToArray(), 0.5f, true);

        CalculateMaxHealthAndEnergy(false);
        health = maxHealth * healthPercent;
        energy = maxEnergy * energyPercent;

        CalculateAllStats();
    }

    public void CalculateAttackDelay(bool fireCallback = true)
    {
        attackDelay = _baseAttackDelay;
        var chargeMod = 1f;
        var attackUps = artificeMode ? this.attackUps + blueScrap : this.attackUps;
        attackUps += _bonusAttack;

        if (_empireHelm) { attackUps += Mathf.RoundToInt(Mathf.Lerp(0, 3, _mapCompletion)); }

        if (_healthBooster)
        {
            float max, delta;
            if (artificeMode)
            {
                delta = (grayScrap / 99);
                max = delta * 8;
            }
            else
            {
                delta = (health / maxHealth);
                max = maxHealth / 4f;
            }
            attackUps += (int)Mathf.Lerp(0, max, delta);
        }

        for (int i = 0; i < Mathf.Abs(attackUps); i++)
        {
            if (attackUps > 0)
            {
                attackDelay *= 0.88f;
                chargeMod *= 0.88f;
            }
            else
            {
                attackDelay /= 0.88f;
                chargeMod /= 0.88f;
            }
        }

        attackDelay = Mathf.Clamp(attackDelay, 0.05f, 10);
        _chargeTime = 1.75f * chargeMod;

        if (attackDelay < 0.2 && AchievementManager.instance)
        {
            if (projectileStats.damageType.HasFlag(DamageType.Fire))
            {
                AchievementManager.instance.TryEarnAchievement(AchievementID.HellfireCannon);
            }
        }

        if (fireCallback && onSetStats != null)
        {
            onSetStats();
        }
    }

    public float GetYAxis()
    {
        if(_weaponWheelToggle) { return 0; }
        var yAxis = controller.GetAxis("Vertical") * (gravityFlipped ? -1 : 1);
        if (mirrorMode) { yAxis *= -1; }
        return confused ? -yAxis : yAxis;
    }

    public float GetXAxis()
    {
        if (_weaponWheelToggle) { return 0; }
        var xAxis = controller.GetAxis("Horizontal");
        return confused ? -xAxis : xAxis;
    }

    protected override void UpdateAlive()
    {
        if (PlayerManager.instance.trueCoOp && this != PlayerManager.instance.player1)
        {
            if (!_renderers[0].isVisible)
            {
                _offScreenTime += Time.deltaTime;
                if (_offScreenTime > 1f)
                {
                    TeleportToPlayer(PlayerManager.instance.player1);
                }
            }
            else
            {
                _offScreenTime = 0;
            }
        }

        if (!flashing && tempStatMods.Count > 0)
        {
            if (_currentModFlash >= tempStatMods.Count) { _currentModFlash = 0; }
            var currentMod = tempStatMods[_currentModFlash];
            if (currentMod.rank > 0) { StartFlash(1, 0.5f, currentMod.flashColor, 0.25f, true); }
            _currentModFlash++;
        }

        if (energy < 0) { energy = 0; }

        if (_energyBoots)
        {
            if (energy != _lastEnergy) { CalculateMaxSpeed(); }
            _lastEnergy = energy;
        }

        if (artificeMode)
        {
            health = maxHealth;
            if (grayScrap <= 0 && !(invincible || uxInvincible)) { StartDeath(); }
        }
        else
        {
            base.UpdateAlive();
        }

        _playTime += Time.deltaTime;

        if (Time.timeScale == 0)
        {
            return;
        }

        if (fatigued || paralyzed)
        {
            JustGravity();
            if (_fatigueCoroutine == null)
            {
                _fatigueCoroutine = FatigueCoroutine();
                StartCoroutine(_fatigueCoroutine);
            }
            return;
        }

        if (NPCDialogueManager.instance && NPCDialogueManager.instance.dialogueActive) { return; }

        if (health < maxHealth && (regenerationRate + bonusRegenerationRate) > 0)
        {
            _regenationCounter += (regenerationRate + bonusRegenerationRate) * Time.deltaTime;
            if (_regenationCounter > 1)
            {
                GainHealth(1);
                _regenationCounter -= 1;
            }
        }

        if (morphing)
        {
            JustGravity();
            return;
        }

        var activeSlot = SaveGameManager.activeSlot;
        _lockPosition = _controller.GetButton("LockPosition");

        _weaponWheelToggle = _controller.GetButton("WeaponWheel");
        if (_weaponWheelToggle)
        {
            _xAxis = 0;
            _yAxis = 0;
        }
        else
        {
            _xAxis = _controller.GetAxis("Horizontal");
            _xAxis = Mathf.Sin(_xAxis * Mathf.PI * 0.5f);
            _yAxis = _controller.GetAxis("Vertical");
        }

        if (gravityFlipped) { _yAxis = -_yAxis; }
        if (mirrorMode) { _yAxis = -_yAxis; }

        if (_confused)
        {
            _xAxis = -_xAxis;
            _yAxis = -_yAxis;
        }

        _absXAxis = Math.Abs(_xAxis);
        _absYAxis = Math.Abs(_yAxis);
        _crouchAxis = _yAxis < -0.1f && _absYAxis - _absXAxis > 0.1f;
        var moving = Mathf.Abs(_velocity.x) > 0.1f && _xAxis != 0;
        var onGround = controller2D.bottomEdge.touching;

#region GridPosition
        gridPosition = Constants.WorldToLayoutPosition(transform.position);

        if (gridPosition != _lastGridPosition)
        {
            _lastGridPosition = gridPosition;

            if (LayoutManager.instance && SaveGameManager.activeGame != null)
            {
                int minY = (int)Mathf.Clamp((gridPosition.y - 1), 0, LayoutManager.instance.layout.height);
                int maxY = (int)Mathf.Clamp((gridPosition.y + 1), 0, LayoutManager.instance.layout.height);
                int minX = (int)Mathf.Clamp((gridPosition.x - 1), 0, LayoutManager.instance.layout.width);
                int maxX = (int)Mathf.Clamp((gridPosition.x + 1), 0, LayoutManager.instance.layout.width);

                var currentRoom = SaveGameManager.activeGame.layout.GetRoomAtPositon(gridPosition);

                var automapSegmentStates = SaveGameManager.activeGame.automapSegmentStates;

                if (currentRoom != null)
                {
                    for (int y = minY; y <= maxY; y++)
                    {
                        for (int x = minX; x <= maxX; x++)
                        {
                            var position = new Vector2(x, y);
                            var pos = position.ToString();

                            var room = SaveGameManager.activeGame.layout.GetRoomAtPositon(position);
                            bool addRoom = room == currentRoom;

                            if (!addRoom && room != null && (x == gridPosition.x || y == gridPosition.y))
                            {
                                addRoom = currentRoom.HasExistingExitToGridPosition(position.Int2D());
                            }

                            if (addRoom)
                            {
                                if (room != null)
                                {
                                    SaveGameManager.activeGame.roomsDiscovered.Add(room.roomID);
                                }

                                if (position == gridPosition)
                                {
                                    if (!automapSegmentStates.ContainsKey(pos) || automapSegmentStates[pos] != AutomapSegmentState.Visited)
                                    {
                                        automapSegmentStates[pos] = AutomapSegmentState.Visited;

                                        if (activeSlot.mapSpacesUncovered < long.MaxValue)
                                        {
                                            activeSlot.mapSpacesUncovered++;
                                        }
                                    }
                                }
                                else if (!automapSegmentStates.ContainsKey(pos)) // Hidden spaces won't be shown by this
                                {
                                    automapSegmentStates[pos] = AutomapSegmentState.Discovered;
                                }
                            }
                        }
                    }

                    RefreshMapCompletion();
                }
            }

            if (onGridPositionChanged != null)
            {
                onGridPositionChanged();
            }
        }
#endregion

        if (activatedItem != null)
        {
            if (_controller.GetButtonDown("ActivatedItem")) { activatedItem.ButtonDown(); }
            if (_controller.GetButton("ActivatedItem")) { activatedItem.Button(); }
            if (_controller.GetButtonUp("ActivatedItem")) { activatedItem.ButtonUp(); }
            activatedItem.Update();
        }

#region SpecialMoves
        if (_activeSpecialMove != null && _activeSpecialMove.complete) { _activeSpecialMove = null; }

        if (_activeSpecialMove == null && specialMoves.Count > 0 && _controller.GetButtonDown("SpecialMove"))
        {
            foreach (var specialMove in specialMoves)
            {
                if (specialMove.TryToActivate())
                {
                    _activeSpecialMove = specialMove;
                    break;
                }
            }
        }

        if (grounded && spiderForm && _absYAxis > _absXAxis && _yAxis > 0.25f && (_activeSpecialMove == null || !_activeSpecialMove.supressMorph))
        {
            ToggleSpiderForm();
        }
#endregion

        if (_activeSpecialMove == null && onGround && moving)
        {
            animator.speed = Mathf.Abs(_velocity.x) / (spiderForm ? spiderFormSpeed : _animatorRunSpeed);
            if (slowed) { animator.speed *= 0.5f; }
            if (inLiquid)
            {
                animator.speed *= !environmentalResistances.HasFlag(EnvironmentalEffect.Underwater) ? 0.75f : 0.5f;
            }
        }
        else
        {
            animator.speed = 1;
        }

#region Crouching
        //check !onGround prevents getting stuck in the jump animation when Knockback is called while crouched. Hopefully it doesn't cause other problems
        var breakCrouch = !onGround || jumping || (moving && !lowFriction) ||
            (!_lockPosition && (_yAxis > 0.1f || (lowFriction && _absXAxis > _absYAxis)));
        if (!_justCrouched && breakCrouch && crouching && CanStand(Vector3.zero))
        {
            crouching = false;
            StartCoroutine(JustUncrouched());
        }
        else if (!_lockPosition && !_justLanded && _crouchAxis && onGround && !crouching && !spiderForm && (!_activeSpecialMove || _activeSpecialMove.allowCrouching))
        {
            crouching = true;
            animator.SetTrigger("Crouch");
            StartCoroutine(JustCrouched());
        }
#endregion

#region Aiming
        var angleUpButton = _controller.GetButton("AngleUp");
        var angleDownButton = _controller.GetButton("AngleDown");

        if (_confused)
        {
            var originalAD = angleDownButton;
            angleDownButton = angleUpButton;
            angleUpButton = originalAD;
        }

        if (angleUpButton && angleDownButton && !(moving && onGround))
        {
            aiming = 2;
        }
        else if (angleUpButton)
        {
            aiming = 1;
        }
        else if (angleDownButton)
        {
            aiming = -1;
        }
        else
        {
            /*
             *    0
             *   7 1
             *  6   2
             *   5 3
             *    4
             */
            var aimAngle = 2;
            if (_xAxis != 0 || _yAxis != 0)
            {
                var dir = new Vector2(_xAxis, _yAxis).normalized;
                var angle = Vector2.SignedAngle(Vector2.up, dir);
                if (angle < 0) { angle = -angle; }
                else { angle = 360 - angle; }
                var modAngle = angle + 22.5;
                if (modAngle >= 360) { modAngle -= 360; }
                var t = modAngle / 360f;
                aimAngle = (int)(8 * t);
            }

            //if (_absYAxis < (onGround ? 0.25f : 0.2f))
            if (aimAngle == 2 || aimAngle == 6)
            {
                aiming = 0;
            }
            else
            {
                if (aimAngle == 0)
                {
                    aiming = moving && onGround ? 1 : 2;
                }
                else if (aimAngle == 1 || aimAngle == 7)
                {
                    aiming = 1;
                }
                else if (aimAngle == 3 || aimAngle == 5)
                {
                    aiming = -1;
                }
                else if (aimAngle == 4)
                {
                    if(!onGround)
                    {
                        aiming = -2;
                    }
                    else if (_lockPosition)
                    {
                        aiming = -1;
                    }
                    else if (_crouching)
                    {
                        aiming = 0;
                    }
                }
            }
        }

        if (aiming == 2 && grounded && !CanStand(Vector3.zero))
        {
            aiming = 1;
        }
#endregion

#region EnergyWeapons
        UpdateEnergyWeapon();
#endregion

#region Attacking
        bool attackDown = _controller.GetButton(attackString);
        //used to make attack coroutine frame rate independent
        if ((attackDown && (activeSpecialMove == null || activeSpecialMove.allowAttack)))
        {
            _timeSinceLastAttack = _timeSinceLastAttack + Time.deltaTime;
            _timeSinceLastBolt = _timeSinceLastBolt + Time.deltaTime;
        }
        else
        {
            _timeSinceLastAttack = 0f;
            _timeSinceLastBolt = 0f;
        }

        if ((_activeSpecialMove == null || _activeSpecialMove.allowAttack) && attackDown)
        {
            if (!attacking && !_charging)
            {
                if (_spinJumping && (_selectedEnergyWeapon == null || !_selectedEnergyWeapon.allowSpinJumping))
                {
                    spinJumping = false;
                }

                if (_selectedEnergyWeapon == null)
                {
                    BasicAttack();
                }
                else
                {
                    if (energy > 0 && _selectedEnergyWeapon.Usable())
                    {
                        _selectedEnergyWeapon.OnAttackDown();
                    }
                    else
                    {
                        SelectEnergyWeapon(-1);
                        audioSource.PlayOneShot(weaponFail);
                    }
                }
            }

            if (!boltAttacking && onBolt != null)
            {
                StartCoroutine(OnBoltCaller(projectileStats));
            }
        }

        if (_selectedEnergyWeapon != null && _controller.GetButtonUp(attackString))
        {
            _selectedEnergyWeapon.OnAttackUp();
        }
#endregion

        if (_activeSpecialMove == null || _activeSpecialMove.allowJumping)
        {
            HandleJumping();
        }

        if (_activeSpecialMove == null || !_activeSpecialMove.supressMorph)
        {
            if (spiderForm)
            {
                animator.SetLayerWeight(2, 1);
                animator.SetLayerWeight(1, 0);
            }
            else
            {
                animator.SetLayerWeight(1, facing == Direction.Left ? 1 : 0);
                animator.SetLayerWeight(2, 0);
            }
        }

        animator.SetBool("Moving", moving);
        animator.SetBool("OnGround", onGround);
        animator.SetBool("Crouching", _crouching);
        animator.SetFloat("VelocityY", _velocity.y);
        animator.SetBool("Landing", _justLanded);
        animator.SetBool("SpinJumping", _spinJumping);
        animator.SetBool("Fatigued", fatigued);
        animator.SetBool("OnWall", canWallJump);

        //Aiming
        var p5Aim = (attacking || _charging);
        if (moving && onGround)
        {
            switch (aiming)
            {
                case 1:
                    animator.SetFloat("Aiming", 1);
                    break;
                case -1:
                    animator.SetFloat("Aiming", -1);
                    break;
                default:
                    animator.SetFloat("Aiming", p5Aim ? 0.5f : 0);
                    break;
            }
        }
        else
        {
            if (p5Aim && aiming == 0 && !onGround)
            {
                animator.SetFloat("Aiming", 0.5f);
            }
            else
            {
                animator.SetFloat("Aiming", aiming);
            }
        }

        if (controller2D.bottomEdge.justTouched && !jumping)
        {
            currentAirJumps = 0;
            if (!_justLanded && _unGroundedFrames > 6)
            {
                PlayOneShot(landingSound);
                if (controller2D.bottomEdge.angle == 0)
                {
                    StartCoroutine(JustLanded());
                }
            }
        }

        if (controller2D.bottomEdge.touching)
        {
            _unGroundedFrames = 0;
            if(controller2D.CheckBottomCorners(true))
            {
                SetLastSafePosition();
            }
        }
        else
        {
            _unGroundedFrames++;
        }

        if (energy < maxEnergy)
        {
            _energyRegenCounter += Time.deltaTime;
            var time = (_energyRegenTime + energy + Mathf.Clamp(energy-2, 0, 99)) * _energyRegenTimeMod;
            if (_energyRegenCounter > (time))
            {
                _energyRegenCounter = 0;
                GainEnergy(1);
            }

            if (bonusRegenerationRate > 0)
            {
                _bonusEnergyRegenCounter += Time.deltaTime;
                var bonusEnergyRegenTime = 1 / bonusRegenerationRate;
                if (_bonusEnergyRegenCounter > bonusEnergyRegenTime)
                {
                    GainEnergy(1);
                    _bonusEnergyRegenCounter -= bonusEnergyRegenTime;
                }
            }

            if (itemEnergyRegenRate > 0)
            {
                _itemEnergyRegenCounter += Time.deltaTime;
                var itemEnergyRegenTime = 1 / itemEnergyRegenRate;
                if (_itemEnergyRegenCounter > itemEnergyRegenTime)
                {
                    GainEnergy(1);
                    _itemEnergyRegenCounter -= itemEnergyRegenTime;
                }
            }
        }
        else
        {
            _energyRegenCounter = 0;
            _bonusEnergyRegenCounter = 0;
        }

#region Look        
        if (activeSlot == null || activeSlot.lookControls)
        {
            var lookAxis = gravityFlipped ? -_yAxis : _yAxis;

            if (_absYAxis > 0.75f && grounded && !moving && !attacking)
            {
                if (lookAxis > 0)
                {
                    if (_lookTimer < 0) _lookTimer = 0;
                    _lookTimer += Time.deltaTime;
                }
                else if (lookAxis < 0)
                {
                    if (_lookTimer > 0) _lookTimer = 0;
                    _lookTimer -= Time.deltaTime;
                }
            }
            else
            {
                _lookTimer = 0;
            }

            if (_lookTimer > 1.25f || _lookTimer < -1.25f)
            {
                mainCamera.yLookMod = Mathf.MoveTowards(mainCamera.yLookMod, Mathf.Sign(_lookTimer) * 2.25f, Time.deltaTime * 7f);
            }
            else if (mainCamera.yLookMod != 0)
            {
                mainCamera.yLookMod = Mathf.MoveTowards(mainCamera.yLookMod, 0, Time.deltaTime * 7f);
            }
        }
        else if (mainCamera.yLookMod != 0)
        {
            mainCamera.yLookMod = Mathf.MoveTowards(mainCamera.yLookMod, 0, Time.deltaTime * 5f);
        }
#endregion
    }

    public void UpdateEnergyWeapon()
    {
        if (energyWeapons.Count > 0)
        {
            var automap = Automap.instance && Automap.instance.expanded;
            var wheelIndex = automap ? -99 : GetWeaponWheelIndex();

            if (_controller.GetButtonDown("WeaponsCancel"))
            {
                if (_selectedEnergyWeapon != null) SelectEnergyWeapon(-1);
            }
            else if (wheelIndex >= -1)
            {
                if (_selectedEnergyWeaponIndex != wheelIndex || (!_selectedEnergyWeapon && wheelIndex >= 0))
                {
                    SelectEnergyWeapon(wheelIndex);
                }
            }
            else if (_controller.GetButtonDown("PageRight"))
            {
                if (_controller.GetButton("PageLeft"))
                {
                    if (_selectedEnergyWeapon != null) SelectEnergyWeapon(-1);
                }
                else if (_selectedEnergyWeapon == null)
                {
                    SelectEnergyWeapon(0);
                }
                else
                {
                    SelectEnergyWeapon(_selectedEnergyWeaponIndex + 1);
                }
            }
            else if (_controller.GetButtonDown("PageLeft"))
            {
                if (_controller.GetButton("PageRight"))
                {
                    if (_selectedEnergyWeapon != null) SelectEnergyWeapon(-1);
                }
                else if (_selectedEnergyWeapon == null)
                {
                    SelectEnergyWeapon(energyWeapons.Count - 1);
                }
                else
                {
                    SelectEnergyWeapon(_selectedEnergyWeaponIndex - 1);
                }
            }

            if (_selectedEnergyWeapon != null) { _selectedEnergyWeapon.Update(); }
        }
    }

    public int GetWeaponWheelIndex()
    {
        float weaponWheelX, weaponWheelY;
        if (_weaponWheelToggle)
        {
            weaponWheelX = controller.GetAxis("Horizontal");
            weaponWheelY = controller.GetAxis("Vertical");
        }
        else
        {
            weaponWheelX = controller.GetAxis("WeaponsHorizontal");
            weaponWheelY = controller.GetAxis("WeaponsVertical");
        }
        
        var wheel = new Vector2(weaponWheelX, weaponWheelY);
        if (wheel.magnitude > 0.5f || _weaponWheelToggle)
        {
            var eCount = energyWeapons.Count + 1;
            var dir = wheel.normalized;
            var angle = Vector2.SignedAngle(Vector2.up, dir);
            if (angle < 0) { angle = -angle; }
            else { angle = 360 - angle; }
            var sliceSize = 360 / eCount;
            var modAngle = angle + (sliceSize * 0.5f);
            if (modAngle >= 360) { modAngle = modAngle - 360; }
            var t = modAngle / 360f;
            var w = (int)(eCount * t) - 1;
            return w;
        }
        return -99;
    }

    private void TeleportToPlayer(Player player)
    {
        Debug.Log("Trying to teleport!");
        _offScreenTime = 0;
        transform.position = player.transform.position;
        FXManager.instance.SpawnFX(FXType.Teleportation, transform.position, false, false, facing == Direction.Left, gravityFlipped);
        ResetAnimatorAndCollision();
        StartCoroutine(AegisFlash(aegisTime));
    }

    public void SetLastSafePosition()
    {
        if (lastSafePosition != transform.position)
        {
            lastSafePosition = transform.position;
            lastSafeGravity = gravityFlipped;
            //Debug.Log("lastSafeGravity: " + lastSafeGravity);
        }
    }

    private void JustGravity()
    {
        var minVelocityY = Physics2D.gravity.y * 3;
        _velocity.x = 0;
        _velocity.y += _gravity * Time.deltaTime;
        _velocity.y = Mathf.Clamp(_velocity.y, minVelocityY, 100);

        controller2D.Move(_velocity * Time.deltaTime);
    }

    private IEnumerator FatigueCoroutine()
    {
        _fatigueAmount = 1;
        var totalInputRecieved = 0;
        var inputRecieved = false;
        var totalRecovery = 0f;
        var fatiguedTime = 0f;

        if (spiderForm)
        {
            ToggleSpiderForm();
            while (morphing) yield return null;
        }
        else
        {
            ResetAnimatorAndCollision();
        }

        if (_selectedEnergyWeapon != null) { _selectedEnergyWeapon.Stop(); }

        //stop if the player has accumulated enough recovery or inputed 10 buttons and been fatiqued for over 3 seconds
        while ((totalRecovery < 0.25f) && !(inputRecieved && fatiguedTime > 3 && totalInputRecieved > 10))
        {
            inputRecieved = false;
            fatiguedTime += Time.deltaTime;

            var newXAxis = _controller.GetAxis("Horizontal");
            var newYAxis = _controller.GetAxis("Vertical");
            float axisDelta = Mathf.Abs(newXAxis - _xAxis) + Mathf.Abs(newYAxis - _yAxis);
            _xAxis = newXAxis;
            _yAxis = newYAxis;

            if (_fatigueAmount < 1)
            {
                _fatigueAmount += Time.deltaTime * 0.75f;
            }

            if (axisDelta > 0.5f)
            {
                inputRecieved = true;
                totalInputRecieved++;
                totalRecovery += Time.deltaTime;
                _fatigueAmount -= 60 * _fatigueAmount * Time.deltaTime;
            }

            if (_controller.GetAnyButtonDown())
            {
                inputRecieved = true;
                totalInputRecieved++;
                totalRecovery += Time.deltaTime;
                _fatigueAmount -= 60 * _fatigueAmount * Time.deltaTime;
            }

            _fatigueAmount = Mathf.Clamp(_fatigueAmount, 0.4f, 1f);
            animator.SetBool("Fatigued", fatigued);
            animator.SetFloat("Fatigue", _fatigueAmount);

            yield return null;
        }

        _fatigueAmount = 0.4f;
        while (_fatigueAmount > 0)
        {
            _fatigueAmount -= Time.deltaTime * 0.5f;
            animator.SetBool("Fatigued", fatigued);
            animator.SetFloat("Fatigue", _fatigueAmount);
            yield return null;
        }

        fatigued = false;
        _fatigueCoroutine = null;
    }

    /// <summary>
    /// Used for the Mega Beast's tractor beam
    /// </summary>
    public void SetAnimatorInAir()
    {
        animator.SetBool("Moving", false);
        animator.SetBool("OnGround", false);
        animator.SetBool("Crouching", false);
        animator.SetFloat("VelocityY", 1);
        animator.SetBool("Landing", false);
        animator.SetBool("SpinJumping", false);
        animator.SetBool("SpecialMovement", false);
        animator.SetFloat("Aiming", 0);
        animator.SetBool("Fatigued", false);
        animator.SetFloat("Fatigue", 0);
    }

    public void ResetAnimatorAndCollision()
    {
        _crouching = false;
        _spinJumping = false;
        _velocity = Vector2.zero;
        if (spiderForm)
        {
            SetCollisionBounds(new Vector3(0, -1f, 0), new Vector3(0.75f, 0.9f, 1));
        }
        else
        {
            SetCollisionBounds();
        }

        if (hoverParticles) { hoverParticles.Stop(); }
        if (hoverSound) { hoverSound.Stop(); }

        animator.SetLayerWeight(1, 0);
        animator.SetLayerWeight(2, spiderForm ? 1 : 0);
        animator.SetBool("Moving", false);
        animator.SetBool("OnGround", true);
        animator.SetBool("Crouching", false);
        animator.SetFloat("VelocityY", 0);
        animator.SetBool("Landing", false);
        animator.SetBool("SpinJumping", false);
        animator.SetBool("SpecialMovement", false);
        animator.SetFloat("Aiming", 0);
        animator.SetBool("Fatigued", false);
        animator.SetFloat("Fatigue", 0);

        if (onResetAnimatorAndCollision != null)
        {
            onResetAnimatorAndCollision();
        }
    }

    public void HandleJumping()
    {
        CalculateJump();

        jumpString = _confused ? "Attack" : "Jump";

        var jumpHeld = _controller.GetButton(jumpString);
        var heldOrOverride = jumpInputOverride || jumpHeld;

        //Hover jumping needs to be checked before currentAirJumps is incremented
#region Hover Jumping
        if (canHover)
        {
            if (controller2D.collisions.below)
            {
                _hoverTimer = hoverTime;
            }

            if (hoverJumping)
            {
                var particlePos = hoverParticles.transform.localPosition;
                particlePos.x = aiming == -2 ? 0 : _localHoverX;
                hoverParticles.transform.localPosition = particlePos;

                if (_hoverTimer <= 0 || controller2D.collisions.below || !jumpHeld)
                {
                    hoverParticles.Stop();
                    hoverSound.Stop();
                    hoverJumping = false;
                }
                else
                {
                    _velocity.y += -(_gravity) * 3 * Time.deltaTime;
                    _velocity.y = Mathf.Clamp(_velocity.y, -hoverMaxVelocity, hoverMaxVelocity);
                    _hoverTimer -= Time.deltaTime;
                }
            }
            else if (currentAirJumps >= maxAirJumps &&
                    !canWallJump && 
                    _controller.GetButtonDown(jumpString) &&
                    !controller2D.collisions.below && _hoverTimer > 0)
            {
                hoverParticles.Play();
                hoverSound.Play();
                hoverJumping = true;
            }
        }
#endregion

        var setMin = gravityFlipped ? _velocity.y < _minJumpVelocity : _velocity.y > _minJumpVelocity;
        if (setMin && !heldOrOverride)
        {
            _velocity.y = _minJumpVelocity;
        }

        if (_spinJumping)
        {
            var offset = controller2D.collisions.above ? controller2D.collisions.below ? Vector2.zero : -_spinJumpLandOffset : controller2D.collisions.below ? _spinJumpLandOffset : Vector2.zero;
            if (hoverJumping || (controller2D.collisions.below && !jumping) || (aiming != 0 && CanStand(offset)))
            {                
                spinJumping = false;
            }
        }

        //last bit deals with spin jumping through 2 unit high gaps
        var falling = gravityFlipped ? _velocity.y >= 0 : _velocity.y <= 0;
        if (jumping && falling && (!_spinJumping || _absXAxis < 0.1f || CanStand(_spinJumpLandOffset)))
        {
            jumping = false;
        }

        if (!jumping || currentAirJumps < maxAirJumps)
        {
            if (PlayerManager.instance.allowWallJump && !canWallJump) { canWallJump = !jumpHeld; }
            if(canWallJump)
            {
                var wallD = controller2D.leftEdge.distance;
                canWallJump = _spinJumping && wallD > 0 && wallD < 0.3f &&
                    ((facing == Direction.Right && _xAxis > 0) ||
                    (facing == Direction.Left && _xAxis < 0));
                if(canWallJump && !_flashing)
                {
                    Flash(1, 0.1f, Constants.blasterGreen, 0.1f);
                }
            }

            if (!canJump)
            {
                canJump = (controller2D.bottomEdge.near && !heldOrOverride) || (currentAirJumps < maxAirJumps && !jumpHeld);
                if (canJump && !controller2D.bottomEdge.touching && controller2D.topEdge.touching) //prevents infinijumping through ceiling
                {
                    canJump = false;
                }
            }

            var basicJump = canJump && controller2D.collisions.below && heldOrOverride;
            var airJump = canJump && maxAirJumps > 0 && jumpHeld;
            var wallJump = canWallJump && jumpHeld;

            if (basicJump || airJump || wallJump)
            {
                if (controller2D.collisions.below)
                {
                    currentAirJumps = 0;
                }
                else if (jumpHeld)
                {
                    if (airJump) { currentAirJumps++; }
                    FXManager.instance.SpawnFX(FXType.SmokePuffSmall, controller2D.bottomMiddle, true, false);
                }

                if(jumpEvents.Count > 0)
                {
                    foreach (var e in jumpEvents)
                    {
                        e.StartEvent();
                    }
                }

                PlayOneShot(jumpSound);

                _velocity.y = _maxJumpVelocity;

                canJump = false;
                canWallJump = false;
                jumping = true;

                if (!spiderForm && 
                    (_activeSpecialMove == null || _activeSpecialMove.allowSpinJumping) &&
                    (_xAxis != 0 || !CanStand(Vector3.zero)))
                {
                    spinJumping = true;
                }

                if (crouching) //Setting this calls SetBounds so don't change it if you don't have to!
                {
                    crouching = false;
                }
            }
        }
    }

    public void SelectEnergyWeapon(int index)
    {
        _timeSinceLastAttack = 0;

        if (_selectedEnergyWeapon != null)
        {
            //This allows necroluminant spray to use the ResetLight method without it thinking _selectedEnergyWeapon is NecroLuminantSpray
            var weapon = _selectedEnergyWeapon;
            _selectedEnergyWeapon = null;
            weapon.OnDeselect();
        }

        if (index < 0 || index >= energyWeapons.Count)
        {
            _selectedEnergyWeaponIndex = 0;
            _selectedEnergyWeapon = null;
        }
        else
        {
            _selectedEnergyWeaponIndex = index;
            _selectedEnergyWeapon = energyWeapons[_selectedEnergyWeaponIndex];
            _selectedEnergyWeapon.OnSelect();
        }

        if (onSelectedWeaponChanged != null)
        {
            onSelectedWeaponChanged();
        }
    }

    private void FixedUpdate()
    {
        if (fatigued || state != DamageableState.Alive || Time.timeScale == 0 || (NPCDialogueManager.instance.dialogueActive && NPCDialogueManager.instance.dialogueActive))
        {
            return;
        }

        var onGround = controller2D.bottomEdge.touching;

        if (!onGround)
        {
            if (!_spinJumping && _airControlMod > 0.45f)
            {
                _airControlMod -= Time.fixedDeltaTime * 0.5f;
            }
        }
        else
        {
            _airControlMod = 1f;
        }

        lowFriction = !controller2D.resistConveyorsAndIce && controller2D.bottomEdge.tileParams != null && controller2D.bottomEdge.tileParams.Contains("lowFriction");

        var xThreshold = _absXAxis < _absYAxis ? 0.4f : 0.2; //makes aiming up easier
        var canMove = !_lockPosition && (!_activeSpecialMove || _activeSpecialMove.allowMovement) && (!_crouching || CanStand(Vector3.zero));
        if (canMove && !knockBack && !_crouchAxis && !_justCrouched && _absXAxis > xThreshold &&
            ((_xAxis > xThreshold && facing == Direction.Right) ||
            (_xAxis < -xThreshold && facing == Direction.Left)))
        {
            float targetXVelocity = 0;
            if (!_justLanded)
            {
                var modAxis = _absXAxis > 0.75f ? Mathf.Sign(_xAxis) : _xAxis;
                targetXVelocity = onGround ? modAxis * maxSpeed : modAxis * maxSpeed * _airControlMod;
            }
            _velocity.x = Mathf.Lerp(_velocity.x, targetXVelocity, 0.1f);
        }
        else if (_velocity.x != 0 && (!_activeSpecialMove || _activeSpecialMove.allowDeceleration))
        {
            var deceleration = spinJumping ? 0.075f : (onGround && aiming == 0f) ? 1f : 0.5f;
            if (lowFriction) deceleration *= 0.025f;
            _velocity.x = Mathf.Lerp(_velocity.x, 0, deceleration);
        }

#region Facing
        var canTurn = (!_activeSpecialMove || _activeSpecialMove.allowTurning) &&
            !_justAttacked || (_selectedEnergyWeapon != null && _selectedEnergyWeapon.allowTurning);
        if (canTurn && !_changingFacing)
        {
            if (_xAxis < -0.2f && transform.rotation != Constants.flippedFacing)
            {
                StartCoroutine(ChangeFacing(Direction.Left));
            }
            else if (_xAxis > 0.2f && transform.rotation != Quaternion.identity)
            {
                StartCoroutine(ChangeFacing(Direction.Right));
            }
        }
#endregion

        if (!_activeSpecialMove || _activeSpecialMove.allowGravity)
        {
            _velocity.y += _gravity * Time.deltaTime;
        }

        var minVelocityY = Physics2D.gravity.y * 3;
        _velocity.y = Mathf.Clamp(_velocity.y, minVelocityY, 100);

        if (morphing) { _velocity.x = 0; } //Fix for morphing bug

        var modVelocity = _velocity;

        if (slowed)
        {
            modVelocity.x *= 0.5f;
            if (modVelocity.y > 0) { modVelocity.y *= 0.5f; }
        }

        if(inLiquid)
        {
            if (environmentalResistances.HasFlag(EnvironmentalEffect.Underwater))
            {
                if (grounded)
                {
                    modVelocity.x *= 0.85f;
                }
            }
            else
            { 
                modVelocity.x *= 0.5f;
                modVelocity.y *= Constants.startingWaterJumpMod;
            }
        }
        
        controller2D.Move(modVelocity * Time.deltaTime);

        if (_headCheck == null && controller2D.collisions.above && !hoverJumping)
        {
            _headCheck = HeadCheck();
            StartCoroutine(_headCheck);
        }

        if (controller2D.collisions.below)
        {
            _velocity.y = 0;
        }
    }

    //wait 3 frames in case blocks above get destroyed
    //frame 1, reduced to 0 hit points
    //frame 2, die gets called and tilemap is told to update
    //frame 3, tilemap is updated
    private IEnumerator HeadCheck()
    {
        for (int i = 0; i < 3; i++)
        {
            yield return null;
        }
        if (controller2D.collisions.above) { _velocity.y = 0; }
        _headCheck = null;
    }

    /// <summary>
    /// At your own risk!
    /// </summary>
    public void SetCollisionBounds(Vector3 offset, Vector3 size)
    {
        //Debug.Log("SetCollisionBounds(" + offset + ", " + size);
        _crouching = false;
        _spinJumping = false;
        _boxCollider2D.size = size;
        _boxCollider2D.offset = offset;
        controller2D.UpdateRaycastOrigins();
    }

    public void ResetCollisionBounds(bool allowCrouch = false)
    {
        //Debug.Log("ResetCollisionBounds()");
        _crouching = allowCrouch && _crouchAxis;
        if(_crouching)
        {            
            animator.SetTrigger("CrouchImmediate");
            StartCoroutine(JustCrouched());
        }
        _spinJumping = false;
        _boxCollider2D.size = _originalSize;
        _boxCollider2D.offset = _originalCenter;
        controller2D.UpdateRaycastOrigins();

        var position = transform.position;
        position.y += gravityFlipped ? -2 : 2;
        transform.position = position;
        controller2D.Move(-transform.up * 2);
    }

    private void SetCollisionBounds()
    {
        if (_spinJumping)
        {
            _boxCollider2D.size = _spinJumpBoundsSize;
            _boxCollider2D.offset = Vector2.zero;
        }
        else if (_crouching)
        {
            if (_boxCollider2D.size == _spinJumpBoundsSize)
            {
                var correction = new Vector3(0, (_crouchBoundsSize.y - _spinJumpBoundsSize.y) * 0.5f - _crouchCenter.y, 0);
                transform.position += gravityFlipped ? -correction : correction;
            }
            _boxCollider2D.size = _crouchBoundsSize;
            _boxCollider2D.offset = _crouchCenter;
        }
        else
        {
            if (_boxCollider2D.size == _spinJumpBoundsSize)
            {
                if (controller2D.collisions.below)
                {
                    transform.position += (Vector3)(gravityFlipped ? -_spinJumpLandOffset : _spinJumpLandOffset);
                }

                if(controller2D.collisions.above)
                {
                    transform.position -= (Vector3)(gravityFlipped ? -_spinJumpLandOffset : _spinJumpLandOffset);
                }
            }
            _boxCollider2D.size = _originalSize;
            _boxCollider2D.offset = _originalCenter;
        }

        controller2D.UpdateRaycastOrigins();
    }

    public bool CanStand(Vector2 offset)
    {
        var trueOffset = _originalCenter + offset;
        if(gravityFlipped)
        {
            trueOffset.y *= -1;
        }

        Vector2 point = (Vector2)transform.position + trueOffset;
        Vector2 size = _originalSize;
        size.x -= controller2D.skinWidth + float.Epsilon;
        size.y -= controller2D.skinWidth + float.Epsilon;

        var rect = new Rect(point - size *0.5f, size);
        Extensions.DrawSquare(rect, Color.yellow);

        var result = Physics2D.OverlapBoxAll(point, size, 0, controller2D.collisionMask);
        if (result.Length > 0)
        {
            if (result.Length == 1 && result[0].transform.position.y < transform.position.y)
            {
                return result[0].GetComponent<MovingPlatform>();
            }
            else
            {
                return false;
            }
        }
        else
        {
            return true;
        }
    }

    public override void StartDeath()
    {
        if (_activeSpecialMove != null) { _activeSpecialMove.DeathStop(); }
        if (_selectedEnergyWeapon != null) { _selectedEnergyWeapon.ImmediateStop(); }
        base.StartDeath();
    }

    public override void EndDeath()
    {
        base.EndDeath();

        StopAllCoroutines();
        _headCheck = null;
        _fatigueCoroutine = null;

        if (hoverParticles) { hoverParticles.Stop(); }
        if (hoverSound) { hoverSound.Stop(); }
        if (chargingSound) { chargingSound.Stop(); }
        if (chargingFX) { chargingFX.gameObject.SetActive(false); }

        animator.enabled = false;

        StartCoroutine(DeathRoutine());
    }

    public virtual IEnumerator DeathRoutine()
    {
        Time.timeScale = 0.1f;
        var timer = 0f;
        var time = 1.5f;

        statusEffects.Clear();

        if (NPCDialogueManager.instance && NPCDialogueManager.instance.dialogueActive)
        {
            NPCDialogueManager.instance.HideDialogueScreen(true);
        }

        mainRenderer.enabled = true;

        mainRenderer.material.SetColor("_FlashColor", Constants.blasterGreen);

        SaveGameData activeGame = SaveGameManager.activeGame;
        if (activeGame != null && activeGame.activeSaveRoomPositions.Count <= 0)
        {
            activeGame.playerDead = true;
            SaveGameManager.instance.Save(false, true);
        }

        while (timer < time)
        {
            var progress = (time - timer) / time;
            Time.timeScale = Mathf.Clamp(progress, 0.05f, 1f);
            timer += Time.unscaledDeltaTime;
            mainRenderer.material.SetFloat("_FlashAmount", progress);

            yield return null;
        }
        Time.timeScale = 1;

        DamageLatchers(10000, DamageType.Generic);

        mainRenderer.enabled = false;
        for (int i = 0; i < 3; i++)
        {
            var particleCount = 8f;
            var startingDistance = 0.25f;
            for (int j = 0; j < particleCount; j++)
            {
                var progress = (j / particleCount) * 360 * Mathf.Deg2Rad;
                var particle = Instantiate(playerDeathParticle);
                particle.transform.parent = transform;
                particle.transform.localPosition = new Vector3(Mathf.Sin(progress), Mathf.Cos(progress)) * startingDistance;
            }
            yield return new WaitForSeconds(0.33f);
        }

        yield return new WaitForSeconds(1);

        var validSaveRoom = LayoutManager.instance && activeGame != null && activeGame.activeSaveRoomPositions.Count > 0;

#if ARCADE
        if(!validSaveRoom && extraLives <= 0 && ContinueScreen.instance)
        {
            if (SaveGameManager.instance.saveFileData.freePlay)
            {
                DeathScreen.instance.BackToStartScreen();
                yield break;
            }
            else
            {
                Debug.Log("Show Continue!");
                ContinueScreen.instance.Show();
                yield break;
            }
        }
#endif

        if (extraLives > 0)
        {
            UseExtraLife();
        }
        else if (SaveGameManager.instance)
        {
#if ONEHITKILL
            if(LayoutManager.instance)
            {
                LayoutManager.instance.RespawnInCurrentRoom();
            }
            else
#endif
            if (validSaveRoom)
            {
                LayoutManager.instance.RespawnAtLastSaveRoom();
            }
            else
            {
                SaveGameManager.instance.onSave -= OnSave;
                var slot = SaveGameManager.activeSlot;
                
                if (slot != null && (activeGame == null || activeGame.allowAchievements))
                {
                    if (slot.currentStreak > 0) { slot.currentStreak = 0; }
                    if (slot.currentStreak > short.MinValue) { slot.currentStreak--; }
                    slot.totalDeaths++;

                    if (AchievementManager.instance.TryEarnAchievement(AchievementID.RevenantStation))
                    {
                        while (AchievementScreen.instance.visible) { yield return null; }
                    }
                }

                SaveGameManager.instance.Save();
                DeathScreen.instance.Show();
            }
        }
        else
        {
            DeathScreen.instance.Show();
        }
    }

    public void UseExtraLife()
    {
        extraLives = extraLives - 1;
        Respawn(false);
        var invincibleTime = 3f;
        StartCoroutine(AegisFlash(invincibleTime));
        StartCoroutine(Aegis(invincibleTime));
        SetLastSafePosition();
    }

    public virtual void Respawn(bool fatigue = true)
    {
        StopAllCoroutines();

        knockBack = false;
        inAirFromKnockBack = false;
        _justCrouched = false;
        justUncrouched = false;
        _justLanded = false;
        _justAttacked = false;
        _softAegisActive = false;
        _aegisActive = false;
        _charging = false;
        _flashing = false;
        morphing = false;        
        gravityFlipped = mirrorMode;
        spiderForm = false;

        inLiquid = false;
        confused = false;
        slowed = false;
        invincible = false;
        hoverJumping = false;
        notTargetable = false;

        _changingFacing = false;
        facing = Direction.Right;
        transform.rotation = Quaternion.identity;

        attacking = false;
        boltAttacking = false;
        mainRenderer.enabled = true;
        controller2D.enabled = true;
        animator.enabled = true;

        _headCheck = null;
        _fatigueCoroutine = null;
        if (state != DamageableState.Alive)
        {
            if (artificeMode)
            {
                health = maxHealth;
                if (grayScrap < 3)
                {
                    grayScrap = 3;
                }
            }
            else
            {
                health = maxHealth * 0.5f;
            }
            state = DamageableState.Alive;
            
            fatigued = fatigue;
        }
        else
        {
            fatigued = false;
        }

        foreach (var effect in statusEffects.Values)
        {
            effect.Remove();
        }
        statusEffects.Clear();

        var lasers = GetComponentsInChildren<Laser>();
        foreach (var l in lasers) { l.ImmediateStop(); }

        if (chargingFX)
        {
            var r = chargingFX.GetComponent<SpriteRenderer>();
            if(r) r.enabled = true;
        }

        if(onRespawn != null)
        {
            onRespawn();
        }

        ResetLight();
        ResetAnimatorAndCollision();
        _activeSpecialMove = null;
    }

    public virtual bool HurtKnocback(float damage, GameObject source = null, DamageType damageType = DamageType.Generic, bool ignoreAegis = false, bool knockBack = true)
    {
#if ONEHITKILL
        damage = 10000000;
#endif
        bool result;

        if (PlayerManager.instance.trueCoOp &&
        DeathmatchManager.instance == null &&
        PlayerManager.instance.player1 != this)
        {
            result = PlayerManager.instance.player1.Hurt(damage, source, damageType, ignoreAegis);
        }
        else
        {
            result = base.Hurt(damage, source, damageType, ignoreAegis);
        }

        if (result)
        {
            StartCoroutine(AegisFlash(aegisTime));
            //don't do knockBack or hurtBursts if ignoreAegis is true
            if (!ignoreAegis)
            {
                if (_activeSpecialMove == null || _activeSpecialMove.allowKnockback)
                {
                    Vector3 direction;
                    if (source)
                    {
                        direction = (Vector3.up + (source.transform.position.x > transform.position.x ? Vector3.left : Vector3.right)).normalized;
                    }
                    else
                    {
                        direction = (Vector3.up - transform.right).normalized;
                    }

                    if (knockBack && _allowKnockback && (!LayoutManager.instance || !LayoutManager.instance.transitioning))
                    {
                        StartCoroutine(Knockback(direction));
                    }
                }

                //it's okay if ignoreAegis makes this get called over and over as StartBurst checks if _bursting
                if (hurtBursts.Count > 0)
                {
                    var pick = hurtBursts[UnityEngine.Random.Range(0, hurtBursts.Count)];
                    pick.StartBurst();
                }

                foreach (var kvp in itemHurtActions) { kvp.Value(); }
            }
        }

        return result;
    }

    public override bool Hurt(float damage, GameObject source = null, DamageType damageType = DamageType.Generic, bool ignoreAegis = false)
    {
        if (uxInvincible) return false;
        return HurtKnocback(damage, source, damageType, ignoreAegis, true);
    }

    public override void HandleDamage(float damageAmount)
    {
        if(artificeMode)
        {
            int scrapDamage = (int)Mathf.Clamp(damageAmount, 0, 3);
            grayScrap = Mathf.Clamp(grayScrap - scrapDamage, 0, 99);
        }
        else
        {
            base.HandleDamage(damageAmount);
        }
    }

    public void GainEnergy(float amount)
    {
        StartCoroutine(Flash(1, 0.2f, Constants.damageTypeColors[DamageType.Electric], 0.5f));

        if (PlayerManager.instance.trueCoOp && this != PlayerManager.instance.player1)
        {
            PlayerManager.instance.player1.GainEnergy(amount);
            return;
        }

        energy += amount;
        energy = Mathf.Clamp(energy, 0, maxEnergy);
    }

    public void GainHealth(float amount)
    {
        if (state == DamageableState.Alive)
        {
            StartCoroutine(Flash(1, 0.2f, Constants.blasterGreen, 0.5f));
            if (PlayerManager.instance.trueCoOp && this != PlayerManager.instance.player1)
            {
                PlayerManager.instance.player1.GainHealth(amount);
                return;
            }

            health += amount;
            if (health > maxHealth) { health = maxHealth; }
        }
    }

    public void GainScrap(CurrencyType type, float amount)
    {
        if(PlayerManager.instance.trueCoOp && this != PlayerManager.instance.player1)
        {
            PlayerManager.instance.player1.GainScrap(type, amount);
            return;
        }

        switch (type)
        {
            case CurrencyType.Gray:
                grayScrap = Mathf.Clamp((int)(grayScrap + amount), 0, 99);
                break;
            case CurrencyType.Red:
                redScrap = Mathf.Clamp((int)(redScrap + amount), 0, 99);
                break;
            case CurrencyType.Green:
                greenScrap = Mathf.Clamp((int)(greenScrap + amount), 0, 99);
                break;
            case CurrencyType.Blue:
                blueScrap = Mathf.Clamp((int)(blueScrap + amount), 0, 99);
                break;
        }

        if (AchievementManager.instance)
        {
            if (grayScrap >= 50)
            {
                AchievementManager.instance.TryEarnAchievement(AchievementID.ArtificeHelm);
                if (grayScrap >= 99) { AchievementManager.instance.TryEarnAchievement(AchievementID.ScrapCache); }
            }

            if(redScrap >= 3 && greenScrap >= 3 && blueScrap >= 3)
            {
                AchievementManager.instance.TryEarnAchievement(AchievementID.ArtificeBeam);
            }
        }

        var activeSlot = SaveGameManager.activeSlot;
        if (activeSlot != null)
        {
            if(!activeSlot.scrapCollected.ContainsKey(type))
            {
                activeSlot.scrapCollected.Add(type, 0);
            }

            if (activeSlot.scrapCollected[type] < long.MaxValue)
            {
                activeSlot.scrapCollected[type] += (int)amount;
            }
        }
    }

    public int GetScrap(CurrencyType type)
    {
        switch(type)
        {
            case CurrencyType.Red:
                return redScrap;
            case CurrencyType.Blue:
                return blueScrap;
            case CurrencyType.Green:
                return greenScrap;
            case CurrencyType.Gray:
            default:
                return grayScrap;
        }
    }

    private IEnumerator AegisFlash(float time)
    {
        if (_renderers == null) { yield break; }

        var timer = 0f;
        var flashTime = 0.05f;
        while (timer < time)
        {
            foreach (var renderer in _renderers)
            {
                renderer.enabled = false;
            }
            yield return new WaitForSeconds(flashTime);
            timer += flashTime;

            foreach (var renderer in _renderers)
            {
                renderer.enabled = true;
            }
            yield return new WaitForSeconds(flashTime);
            timer += flashTime;
        }

        foreach (var renderer in _renderers)
        {
            renderer.enabled = true;
        }
    }

    public virtual IEnumerator Knockback(Vector3 direction)
    {
        inAirFromKnockBack = true;
        knockBack = true;
        var timer = 0f;
        while (timer < 0.2f)
        {
            timer += Time.deltaTime;
            controller2D.Move(direction * 14f * Time.deltaTime);
            yield return null;
        }
        knockBack = false;

        while (!controller2D.bottomEdge.touching) { yield return null; }
        inAirFromKnockBack = false;
    }

    private IEnumerator JustCrouched()
    {
        _justCrouched = true;
        yield return new WaitForSeconds(0.1f);
        _justCrouched = false;
    }

    private IEnumerator JustUncrouched()
    {
        justUncrouched = true;
        yield return new WaitForSeconds(0.1f);
        justUncrouched = false;
    }

    private IEnumerator JustLanded()
    {
        _justLanded = true;
        yield return new WaitForSeconds(1 / 9f);
        _justLanded = false;
    }

    public IEnumerator ChangeFacing(Direction newFacing)
    {
        transform.rotation = newFacing == Direction.Right ? transform.rotation = Quaternion.identity : Constants.flippedFacing;

        if (gravityFlipped)
        {
            transform.rotation *= Quaternion.Euler(0, 180, 180);
        }

        if (crouching)
        {
            _changingFacing = true;
            yield return new WaitForSeconds(0.15f);
        }
        facing = newFacing;
        _changingFacing = false;
    }

    public AimingInfo GetAimingInfo()
    {
        var direction = transform.right;
        var origin = shootPoint.position;
        bool nearWall = false;

        if (!spiderForm)
        {
            if (spinJumping)
            {
                origin = shootPointSpin.position;
                direction = (shootPointSpin.position - transform.position).normalized;
            }
            else
            {
                switch (aiming)
                {
                    case -2:
                        direction = Vector3.down;
                        origin = shootPointDown.position;
                        break;
                    case -1:
                        direction = (transform.right + Vector3.down).normalized;
                        origin = shootPointAngleDown.position;
                        break;
                    case 1:
                        direction = (transform.right + Vector3.up).normalized;
                        origin = shootPointAngleUp.position;
                        break;
                    case 2:
                        direction = Vector3.up;
                        origin = shootPointUp.position;
                        break;
                    default:
                        if (crouching)
                        {
                            origin.y += gravityFlipped ? 0.3125f : -0.3125f; //down 5 pixels to make it under tiles
                        }
                        break;
                }
            }
        }

        if (gravityFlipped)
        {
            direction.y *= -1;
        }

        if (controller2D.rightEdge.near)
        {
            var p = origin - direction * 0.5f;
            RaycastHit2D hit = Physics2D.Raycast(p, direction, 1f, Constants.defaultMask);
            if (hit.collider)
            {
                nearWall = true;
                origin = (Vector3)hit.point - direction * 0.5f;
            }
        }

        if (_crouching)
        {
            origin.y += gravityFlipped ? 0.625f : -0.625f;
        }
        else if (spiderForm)
        {
            origin.y += gravityFlipped ? 1.125f : -1.125f;
        }

        if (rotaryShot && !canCharge && !_selectedEnergyWeapon)
        {
            _rotaryShotCount = (_rotaryShotCount + 1) % 4;
            var dirMod = 0;
            if(_rotaryShotCount == 0) { dirMod = 1; }
            else if(_rotaryShotCount == 2) { dirMod = -1; }
            var perpDir = (Vector3)Vector2.Perpendicular(direction);
            origin += perpDir * (0.125f) * dirMod;
        }

        var info = new AimingInfo(origin, direction) { nearWall = nearWall, };
        return info;
    }

    public void BasicAttack()
    {
        if (canCharge && (SaveGameManager.activeSlot == null || SaveGameManager.activeSlot.shotCharging))
        {
            StartCoroutine(ChargeAttack());
        }
        else
        {
            PlayOneShot(attackSound);
            StartCoroutine(Attack(projectileStats, attackDelay));
        }
    }

    public IEnumerator DummyAttack(float attackDelay)
    {
        attacking = true;
        _justAttacked = true;
        yield return null;
        _justAttacked = false;

        var timer = _timeSinceLastAttack;
        while (timer < attackDelay)
        {
            timer += _controller.GetButton(attackString) ? Time.deltaTime : Time.deltaTime * 2;
            yield return null;
        }
        _timeSinceLastAttack = timer - attackDelay;

        attacking = false;
    }


    public IEnumerator OnBoltCaller(ProjectileStats stats)
    {
        boltAttacking = true;

        var aimingInfo = GetAimingInfo();
        CalculatePreAttack(stats);

        onBolt(stats, aimingInfo, arcShots, fireArc);

        var timer = _timeSinceLastBolt;
        while (timer < attackDelay)
        {
            timer += _controller.GetButton(attackString) ? Time.deltaTime : Time.deltaTime * 2;
            yield return null;
        }
        _timeSinceLastBolt = timer - attackDelay;

        boltAttacking = false;
    }

    public IEnumerator Attack(ProjectileStats stats, float attackDelay)
    {
        attacking = true;
        _justAttacked = true;
        yield return null;
        _justAttacked = false;

        var aimingInfo = GetAimingInfo();
        CalculatePreAttack(stats);

        if (arcShots > 0)
        {
            ProjectileManager.instance.ArcShoot(stats, aimingInfo.origin, aimingInfo.direction, arcShots, fireArc);
        }
        else
        {
            ProjectileManager.instance.Shoot(stats, aimingInfo);
        }

        var timer = _timeSinceLastAttack;
        while (timer < attackDelay)
        {
            timer += _controller.GetButton(attackString) ? Time.deltaTime : Time.deltaTime * 2;
            yield return null;
        }
        _timeSinceLastAttack = timer - attackDelay;

        attacking = false;
    }

    public void CalculatePreAttack(ProjectileStats stats)
    {
        if (_healthBooster) { CalculateAttackDelay(false); }
        CalculateShotSpeed(false);
        CalculateShotSize(false);
        CalculateDamage(false);

        if (onSetStats != null) { onSetStats(); }

        //calculate PreShotInvisTime
        stats.preShotInvisTime = 0.48f / stats.speed;
        if (_onKillEnemy == null)
        {
            stats.onHurt = null;
        }
        else
        {
            stats.onHurt = OnHurtEnemy;
        }

        if (stats.size >= 4 && AchievementManager.instance)
        {
            AchievementManager.instance.TryEarnAchievement(AchievementID.SwellBolt);
        }
    }

    public void OnHurtEnemy(IDamageable damageable)
    {
        if (_onKillEnemy != null)
        {
            var enemy = damageable.gameObject.GetComponent<Enemy>();
            if (enemy && enemy.health <= 0) { StartCoroutine(CheckForKill(enemy)); }
        }
    }

    public IEnumerator CheckForKill(Enemy enemy)
    {
        yield return null; //wait a frame
        if (enemy.state != DamageableState.Alive) { _onKillEnemy(enemy); }
    }

    public IEnumerator ChargeAttack()
    {
        var timer = 0f;
        var flashTimer = 0f;
        bool cFlashing = false;
        _charging = true;

        while (_controller.GetButton(attackString))
        {
            if (timer > 0.05f && !chargingFX.gameObject.activeInHierarchy)
            {
                chargingFX.gameObject.SetActive(true);
                chargingFX.logWarnings = false;
                chargingSound.Play();
            }

            if (timer < _chargeTime)
            {
                timer += Time.deltaTime;
            }
            else
            {
                timer = _chargeTime;
            }

            var progress = timer / _chargeTime;
            flashTimer += Time.deltaTime;
            var aimingInfo = GetAimingInfo();
            chargingFX.transform.position = aimingInfo.origin + aimingInfo.direction * 0.5f;
            chargingFX.SetFloat("Charge", progress);
            chargingSound.pitch = Mathf.Lerp(1f, 5, progress);
            if (flashTimer > Mathf.Lerp(0.25f, 0.05f, progress))
            {
                flashTimer = 0f;
                if (cFlashing)
                {
                    cFlashing = false;
                    foreach (var renderer in _renderers)
                    {
                        if (renderer != null)
                        {
                            renderer.material.SetFloat("_FlashAmount", 0);
                        }
                    }
                }
                else
                {
                    cFlashing = true;
                    foreach (var renderer in _renderers)
                    {
                        if (renderer != null)
                        {
                            renderer.material.SetColor("_FlashColor", _chargeColor);
                            renderer.material.SetFloat("_FlashAmount", progress * 0.5f);
                        }
                    }
                }
            }
            yield return null;
        }

        foreach (var renderer in _renderers)
        {
            if (renderer != null)
            {
                renderer.material.SetFloat("_FlashAmount", 0);
            }
        }

        if (_spinJumping)
        {
            spinJumping = false;
        }

        _charging = false;

        var chargeRatio = timer / _chargeTime;
        var damageMod = 1 + Mathf.Clamp(chargeRatio * _maxChargeDamage - 1, 0, _maxChargeDamage - 1);
        var sizeMod = 1 + Mathf.Clamp(chargeRatio * _maxChargeSize - 1, 0, _maxChargeSize - 1);
        var chargeStats = new ProjectileStats(projectileStats);
        chargeStats.damage *= damageMod;
        chargeStats.size *= sizeMod;

        if (chargeRatio > 0.5f)
        {
            chargeStats.penetrative = true;
        }

        chargingSound.Stop();
        chargingFX.gameObject.SetActive(false);

        var sound = _attackSounds.OrderBy(k => Mathf.Abs(k.Key - chargeStats.size)).First().Value;
        PlayOneShot(sound);

        StartCoroutine(Attack(chargeStats, Mathf.Clamp(attackDelay - timer, 0, attackDelay)));        
    }

    public void CollectMinorItem(MinorItemType itemType)
    {
        if (itemType == MinorItemType.GlitchModule)
        {
            var list = Constants.allStatModules.ToList();

            var minusOne = list[Random.Range(0, list.Count)];
            list.Remove(minusOne);
            ModifyMinorItemCollection(minusOne, -1, false, false);
            var boost1 = list[Random.Range(0, list.Count)];
            ModifyMinorItemCollection(boost1, 1, false, false);
            var boost2 = list[Random.Range(0, list.Count)];
            ModifyMinorItemCollection(boost2, 1, false, false);

            var colors = new Color32[] { Constants.moduleColors[boost1], Constants.moduleColors[boost2] };
            StartMultiColorFlash(0.5f, colors, 0.75f,true);

            if (onCollectItem != null)
            {
                onCollectItem();
            }
        }
        else
        {
            ModifyMinorItemCollection(itemType, 1, true);
        }

        SaveGameManager.instance.Save();
    }

    public void ModifyMinorItemCollection(MinorItemType itemType, int amount, bool callOnCollect, bool flash = true)
    {
        if (itemType == MinorItemType.GlitchScrap) { itemType = GlitchScrapPickUp.GetRandomScrap(); }

        bool stat = false;
        PlayerStatType statType = PlayerStatType.Health;

        switch (itemType)
        {
            case MinorItemType.HealthTank:
                stat = true;
                statType = PlayerStatType.Health;
                healthUps = healthUps + amount;
                CalculateMaxHealthAndEnergy();
                health = maxHealth;
                if (artificeMode)
                {
                    GainScrap(CurrencyType.Gray, amount * 11);
                }
                break;
            case MinorItemType.EnergyModule:
                stat = true;
                statType = PlayerStatType.Energy;
                energyUps = energyUps + amount;
                CalculateMaxHealthAndEnergy();
                if (amount > 0)
                {
                    energy = maxEnergy;
                }
                break;
            case MinorItemType.DamageModule:
                stat = true;
                statType = PlayerStatType.Damage;
                damageUps = damageUps + amount;
                CalculateDamage();
                break;
            case MinorItemType.AttackModule:
                stat = true;
                statType = PlayerStatType.Attack;
                attackUps = attackUps + amount;
                CalculateAttackDelay();
                break;
            case MinorItemType.SpeedModule:
                stat = true;
                statType = PlayerStatType.Speed;
                speedUps = speedUps + amount;
                CalculateMaxSpeed();
                break;
            case MinorItemType.ShotSpeedModule:
                stat = true;
                statType = PlayerStatType.ShotSpeed;
                shotSpeedUps = shotSpeedUps + amount;
                CalculateShotSpeed();
                break;
            case MinorItemType.RedScrap:
                GainScrap(CurrencyType.Red, amount);
                if (artificeMode) CalculateDamage();
                break;
            case MinorItemType.BlueScrap:
                GainScrap(CurrencyType.Blue, amount);
                if (artificeMode) CalculateAttackDelay();
                break;
            case MinorItemType.GreenScrap:
                GainScrap(CurrencyType.Green, amount);
                if (artificeMode) CalculateMaxSpeed();
                break;
            default:
                Debug.LogError("Player Gained Unrecognized MinorItemType");
                break;
        }

        Color32 color;
        if (flash && Constants.moduleColors.TryGetValue(itemType, out color))
        {
            StartFlash(amount, 0.2f, color, 0.75f, true);
        }

        if (stat)
        {
            var buff = tempStatMods.FirstOrDefault(b => b.statType == statType && b.rank > 0);
            if (buff != null) { buff.SetToMaxDuration(); }
        }

        if (callOnCollect && onCollectItem != null) { onCollectItem(); }
    }

    public void CollectMajorItem(MajorItem itemType, bool matchItems = true)
    {
        MajorItemInfo info;
        if (ItemManager.items.TryGetValue(itemType, out info))
        {
            PlayerManager.instance.ItemCollected(itemType);
            if (!itemsPossessed.Contains(itemType))
            {
                if (info.isActivatedItem)
                {
                    activatedItem = Instantiate(Resources.Load("ActivatedItems/" + info.type.ToString())) as PlayerActivatedItem;
                    activatedItem.Initialize(this);

                    if (SaveGameManager.activeGame != null)
                    {
                        SaveGameManager.activeGame.currentActivatedItem = info.type;
                    }
                }
                else
                {
                    itemsPossessed.Add(itemType);
                    if (matchItems) MatchItems();
                }

                var activeGame = SaveGameManager.activeGame;
                if (activeGame != null)
                {
                    activeGame.playerStart = transform.position;
                    activeGame.itemsPossessed = itemsPossessed.ToList();
                    activeGame.itemsApplied = new HashSet<MajorItem>(_itemsApplied);

                    //activeGame.layout.itemOrder is null in BossRush
                    if (ItemManager.items[itemType].isTraversalItem && activeGame.layout != null && activeGame.layout.itemOrder != null)
                    {
                        var index = activeGame.layout.itemOrder.IndexOf(itemType);
                        if (index > 0)
                        {
                            while (activeGame.traversalItemCollectTimes.Count <= index)
                            {
                                activeGame.traversalItemCollectTimes.Add(0);
                            }
                            activeGame.traversalItemCollectTimes[index] = activeGame.playTime;
                        }
                    }

                    SaveGameManager.instance.Save();
                }

                if (onCollectItem != null)
                {
                    onCollectItem();
                }
            }
            else
            {
                Debug.LogWarning("Trying to collect " + itemType.ToString() + " and player already has the maximum amount!");
            }
        }
        else
        {
            Debug.LogWarning("Trying to collect " + itemType.ToString() + " and no item info exists for it!");
        }
    }

    public void ReorderFollowers()
    {
        int followersChecked = 0;
        orbitalFollowerCount = 0;
        trailFollowerCount = 0;

        foreach (var follower in followers)
        {
            if (follower.followerIndex != -1)
            {
                follower.followerIndex = followersChecked;
            }
            followersChecked++;

            if (follower.orbital)
            {
                follower.positionNumber = orbitalFollowerCount;
                orbitalFollowerCount++;
            }
            else if (follower is TrailFollower)
            {
                follower.positionNumber = trailFollowerCount;
                trailFollowerCount++;
            }
        }
    }

    public void AddVulnerability(DamageType damageType)
    {
        if(!vulnerabilities.HasFlag(damageType)) { vulnerabilities |= damageType; }
    }

    public void RemoveVulnerability(DamageType damageType)
    {
        if (vulnerabilities.HasFlag(damageType)) { vulnerabilities &= ~damageType; }
    }

    public bool RerollNontraversalArtifact()
    {
        //implement artifact reroll
        var nonTraversal = itemsPossessed.Where((i) => !ItemManager.items[i].isTraversalItem).ToList();

        if(nonTraversal.Count == 0)
        {
            return false;
        }

        var itemToReroll = nonTraversal[Random.Range(0, nonTraversal.Count)];
        var possibleItems = ItemManager.items.Values.Where((i) => !nonTraversal.Contains(i.type) && i.nontraversalPool).ToList();

        if (LayoutManager.instance && LayoutManager.instance.layout != null)
        {
            var layout = LayoutManager.instance.layout;
            possibleItems.RemoveAll((i) => layout.allNonTraversalItemsAdded.Contains(i.type));
        }

        if (possibleItems.Count > 0)
        {
            var newItem = possibleItems[Random.Range(0, possibleItems.Count)];
            RemoveItem(itemToReroll, false);
            CollectMajorItem(newItem.type);
            Debug.Log("Rerolled " + itemToReroll + " into " + newItem);
        }

        return true;
    }

    public void ResetLight()
    {
        var renderer = light.GetComponent<Renderer>();
        if (renderer)
        {
            renderer.enabled = true;
        }

        if(itemsPossessed.Contains(MajorItem.BrightShell))
        {
            light.sprite = light400;
        }
        else if(_selectedEnergyWeapon is NecroluminantSpray)
        {
            light.sprite = light200;
        }
        else
        {
            light.sprite = light100;
        }

        light.color = new Color32(214, 255, 231, 255);
    }

    public void SpawnNanobot(Vector3 position, bool ignoreBonus = false)
    {
        var count = nanobotsPerSpawn < 1 || ignoreBonus ? 1 : nanobotsPerSpawn;

        for (int i = 0; i < count; i++)
        {
            if (_nanobots.Count <= 48)
            {
                var nanobotPrefab = Resources.Load<Nanobot>("Followers/Nanobot");
                var bot = Instantiate(nanobotPrefab, position, Quaternion.identity);
                bot.player = this;
                bot.team = team;
            }
            else
            {
                var n = _nanobots.First();
                n.transform.position = position;
                _nanobotReserve++;
            }
        }

        if(_nanobots.Count >= 18)
        {
            AchievementManager.instance.TryEarnAchievement(AchievementID.PhantasmalOrbs);
        }
    }

    public void OnNanobotDie()
    {
        if(_nanobotReserve > 0)
        {
            _nanobotReserve--;
            SpawnNanobot(position, true);
        }
    }

    public void AddNanobot(Nanobot nanobot)
    {
        _nanobots.Add(nanobot);
        RefreshNanobots();

        if(_nanobots.Count + _nanobotReserve >= 81)
        {
            AchievementManager.instance.TryEarnAchievement(AchievementID.OrphielsAltar);
        }
    }

    public void RemoveNanobot(Nanobot nanobot)
    {
        _nanobots.Remove(nanobot);
        RefreshNanobots();
    }

    private void RefreshNanobots()
    {
        for (int i = 0; i < _nanobots.Count; i++)
        {
            _nanobots[i].offsetRatio = (float)i / (float)_nanobots.Count;
        }
    }

    public void RevealBosses()
    {
        if (SaveGameManager.activeGame != null)
        {
            var activeGame = SaveGameManager.activeGame;
            var layout = activeGame.layout;
            var automapSegmentStates = SaveGameManager.activeGame.automapSegmentStates;
            foreach (var roomAbstract in layout.roomAbstracts)
            {
                if (roomAbstract.assignedRoomInfo.roomType == RoomType.BossRoom || roomAbstract.assignedRoomInfo.roomType == RoomType.MegaBeast)
                {
                    if(roomAbstract.assignedRoomInfo.environmentType == EnvironmentType.Glitch || (roomAbstract.assignedRoomInfo.boss == BossName.MegaBeastCore && !SaveGameManager.beastGutsUnlocked))
                    {
                        continue;
                    }

                    activeGame.roomsDiscovered.Add(roomAbstract.roomID);

                    for (int x = 0; x < roomAbstract.width; x++)
                    {
                        for (int y = 0; y < roomAbstract.height; y++)
                        {
                            var position = new Vector2(roomAbstract.gridPosition.x + x, roomAbstract.gridPosition.y - y);
                            var name = position.ToString();
                            if (!automapSegmentStates.ContainsKey(name)) // Hidden spaces won't be shown by this
                            {
                                automapSegmentStates[name] = AutomapSegmentState.Discovered;
                            }
                        }
                    }
                }
            }

            onGridPositionChanged();
        }
    }

    public void RevealArtifacts()
    {
        if (SaveGameManager.activeGame != null)
        {
            var layout = SaveGameManager.activeGame.layout;
            var automapSegmentStates = SaveGameManager.activeGame.automapSegmentStates;
            foreach (var roomAbstract in layout.roomAbstracts)
            {
                if (roomAbstract.majorItem != MajorItem.None && roomAbstract.assignedRoomInfo.environmentType != EnvironmentType.Glitch)
                {
                    SaveGameManager.activeGame.roomsDiscovered.Add(roomAbstract.roomID);

                    for (int x = 0; x < roomAbstract.width; x++)
                    {
                        for (int y = 0; y < roomAbstract.height; y++)
                        {
                            var position = new Vector2(roomAbstract.gridPosition.x + x, roomAbstract.gridPosition.y - y);
                            var name = position.ToString();
                            if (!automapSegmentStates.ContainsKey(name)) // Hidden spaces won't be shown by this
                            {
                                automapSegmentStates[name] = AutomapSegmentState.Discovered;
                            }
                        }
                    }
                }
            }

            onGridPositionChanged();
        }
    }

    public void RevealEnvironments(ICollection<EnvironmentType> envTypes)
    {
        if (SaveGameManager.activeGame != null)
        {
            var layout = SaveGameManager.activeGame.layout;
            var automapSegmentStates = SaveGameManager.activeGame.automapSegmentStates;
            foreach (var roomAbstract in layout.roomAbstracts)
            {
                if (envTypes.Contains(roomAbstract.assignedRoomInfo.environmentType))
                {
                    SaveGameManager.activeGame.roomsDiscovered.Add(roomAbstract.roomID);

                    for (int x = 0; x < roomAbstract.width; x++)
                    {
                        for (int y = 0; y < roomAbstract.height; y++)
                        {
                            var position = new Vector2(roomAbstract.gridPosition.x + x, roomAbstract.gridPosition.y - y);
                            var name = position.ToString();
                            if (!automapSegmentStates.ContainsKey(name)) // Hidden spaces won't be shown by this
                            {
                                automapSegmentStates[name] = AutomapSegmentState.Discovered;
                            }
                        }
                    }
                }
            }

            onGridPositionChanged();
        }
    }

    public void Pause()
    {
        paused = true;
        enabled = false;
        animator.speed = 0;

        foreach (var f in followers)
        {
            var pausibles = f.gameObject.GetInterfaces<IPausable>();
            foreach (var p in pausibles)
            {
                p.Pause();
            }

            if(_activeSpecialMove != null)
            {
                _activeSpecialMove.OnPause();
            }
        }
    }

    public void Unpause()
    {
        paused = false;
        enabled = true;
        animator.speed = 1;

        foreach (var f in followers)
        {
            var pausibles = f.gameObject.GetInterfaces<IPausable>();
            foreach (var p in pausibles)
            {
                p.Unpause();
            }

            if (_activeSpecialMove != null)
            {
                _activeSpecialMove.OnUnpause();
            }
        }
    }

    public void DamageLatchers(float damage, DamageType damageType)
    {
        if (attachedLatchers.Count <= 0) return;

        foreach (var l in attachedLatchers)
        {
            if (l)
            {
                l.Hurt(damage, gameObject, damageType);
            }
        }
    }

    public void ToggleSpiderForm()
    {
        if (!spiderForm)
        {
            hoverParticles.transform.localPosition = _hoverBootsHoverParticlePosition;
            _localHoverX = _hoverBootsHoverParticlePosition.x;
            StartCoroutine(SpiderMorphIn());
        }
        else if(CanStand(-_originalCenter))
        {
            if(itemsPossessed.Contains(MajorItem.JetPack))
            {
                hoverParticles.transform.localPosition = _jetPackHoverParticlePosition;
                _localHoverX = _jetPackHoverParticlePosition.x;
            }
            StartCoroutine(SpiderMorphOut());
        }
    }

    public void SetSpiderCollisionBounds()
    {
        SetCollisionBounds(new Vector3(0, -1f, 0), new Vector3(0.75f, 0.9f, 1));
    }

    public IEnumerator SpiderMorphIn()
    {
        spiderForm = true;
        morphing = true;
        controller2D.resistConveyorsAndIce = true;
        animator.SetLayerWeight(2, 1);
        animator.SetLayerWeight(1, 0);

        animator.SetBool("MorphingIn", true);
        yield return new WaitForSeconds(6f/16f);
        animator.SetBool("MorphingIn", false);        

        controller2D.resistConveyorsAndIce = itemsPossessed.Contains(MajorItem.DeadlockBoots);
        _crouching = false;
        SetSpiderCollisionBounds();
        morphing = false;
        CalculateJump();
        CalculateMaxSpeed();
    }

    public IEnumerator SpiderMorphOut()
    {   
        morphing = true;
        controller2D.resistConveyorsAndIce = true;
        animator.SetBool("MorphingOut", true);
        yield return new WaitForSeconds(6f/16f);
        animator.SetBool("MorphingOut", false);

        controller2D.resistConveyorsAndIce = itemsPossessed.Contains(MajorItem.DeadlockBoots);
        animator.SetLayerWeight(1, facing == Direction.Left ? 1 : 0);
        animator.SetLayerWeight(2, 0);
        ResetCollisionBounds(true);
        morphing = false;
        spiderForm = false;
        CalculateJump();
        CalculateMaxSpeed();
    }

    public void FlipGravity()
    {
        gravityFlipped = !gravityFlipped;
        StartCoroutine(ChangeFacing(facing));
        CalculateJump();
    }
    
    public ActivatedItemPickUp DropEquippedActiveItem(Vector3 position, Room parentRoom, bool setJustSpawned = true)
    {        
        if (activatedItem)
        {
            var roomID = parentRoom && parentRoom.roomAbstract != null ? parentRoom.roomAbstract.roomID : string.Empty;
            var activeGame = SaveGameManager.activeGame;

            if (activeGame != null)
            {
                var looseItemData = new LooseItemData()
                {
                    roomID = parentRoom.roomAbstract != null ? parentRoom.roomAbstract.roomID : string.Empty,
                    position = position,
                    item = activatedItem.item,
                };

                List<LooseItemData> looseItems;
                if (activeGame.looseItems.TryGetValue(roomID, out looseItems))
                {
                    looseItems.Add(looseItemData);
                }
                else
                {
                    looseItems = new List<LooseItemData>() { looseItemData };
                }
                activeGame.looseItems[roomID] = looseItems;
                SaveGameManager.instance.Save();
            }

            return activatedItem.Unequip(position, parentRoom ? parentRoom.transform : transform.parent, setJustSpawned);
        }
        else
        {
            return null;
        }
    }

    public int GetPlayerCapabilitiesIndex()
    {
        var layout = LayoutManager.instance ? LayoutManager.instance.layout : null;
        int index = 0;
        if(layout != null)
        {
            for (int i = 0; i < layout.itemOrder.Count; i++)
            {
                if(itemsPossessed.Contains(layout.itemOrder[i]))
                {
                    index++;
                }
                else
                {
                    break;
                }
            }
        }
        return index;
    }

    public void OnAddEffect(StatusEffect effect)
    {
        if (effect.type == StatusEffectsType.Fatigue)
        {
            fatigued = true;
            _fatigueAmount = 1;
        }
    }

    public void OnRemoveEffect(StatusEffect effect) { }

    public void OnStackEffect(StatusEffect effect)
    {
        if (effect.type == StatusEffectsType.Fatigue) { _fatigueAmount = 1; }
    }

    public override bool OnEnterLiquid(Water water) { inLiquid = true; return inLiquid; }
    public override void OnExitLiquid() { inLiquid = false; }

    private void RefreshMapCompletion()
    {
        if (Automap.instance && SaveGameManager.activeGame != null)
        {
            var count = (float)SaveGameManager.activeGame.automapSegmentStates.Values.Count(a => a != AutomapSegmentState.Hidden);
            _mapCompletion =  count / Automap.instance.segmentCount;
        }
    }
}

public struct AimingInfo
{
    public bool nearWall;
    public Vector3 direction;
    public Vector3 origin;

    public AimingInfo(Vector3 origin, Vector3 direction)
    {
        nearWall = false;
        this.origin = origin;
        this.direction = direction;
    }
}