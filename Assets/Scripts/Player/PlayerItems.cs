using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;
using Random = UnityEngine.Random;

public partial class Player
{
    private bool _blueKey;
    public bool blueKey { get { return _blueKey; } }
    private bool _blackKey;
    public bool blackKey { get { return _blackKey; } }
    private bool _scrapBooster;
    private bool _healthBooster;
    public bool healthBooster { get { return _healthBooster; } }
    private bool _energyBoots;
    private bool _empireHelm;

    public void ExperimentalFeatures()
    {
        
    }

    public void MatchItems(bool force = false, SpriteMakerMode spriteMakerMode = SpriteMakerMode.Automatic)
    {
        string spriteMakerKey = _playerSpriteMaker.GetKey();
        int energyWeaponCount = energyWeapons.Count;

#if DEBUG
        ExperimentalFeatures();
#endif

        if (itemsPossessed.Count > 0 || force)
        {
            ResetAbilities();

            bool attackSoundsDirty = false;

            var projectileTypes = new HashSet<ProjectileType>();
            var environmentsToReveal = new HashSet<EnvironmentType>();
            var palettes = new List<Texture2D>();

            int followerIndex = 0;

            List<MajorItemInfo> itemInfos = new List<MajorItemInfo>();
            foreach (var value in itemsPossessed)
            {
                MajorItemInfo itemInfo = null;
                ItemManager.items.TryGetValue(value, out itemInfo);

                if (itemInfo == null)
                {
                    Debug.LogWarning("Player possesses item not found in Constants.traversalItemInfos: " + value.ToString());
                    continue;
                }

                itemInfos.Add(itemInfo);
            }

            itemInfos = itemInfos.OrderBy((i) => i.decalOrder).ToList();

            projectileStats.childEffects.Clear();
            projectileStats.statusEffects.Clear();

            foreach (var itemInfo in itemInfos)
            {
                if (itemInfo.environmentalResistance != 0 && !environmentalResistances.HasFlag(itemInfo.environmentalResistance))
                {
                    environmentalResistances |= itemInfo.environmentalResistance;
                }

                var itemType = itemInfo.type.ToString();

                if (itemInfo.follower)
                {
                    if (!followers.Any(f => f.followerIndex == followerIndex))
                    {
                        var followerPrefab = Resources.Load<Follower>("Followers/" + itemType);
                        var follower = Instantiate(followerPrefab);
                        follower.followerIndex = followerIndex;
                        follower.type = itemInfo.type;
                        follower.player = this;

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

                        follower.transform.position = transform.position;
                        followers.Add(follower);
                    }

                    followerIndex++;
                }

                if (itemInfo.paletteOverride) { palettes.Add(Resources.Load<Texture2D>("Palettes/Player/PlayerPalette" + itemType)); }

                if (itemInfo.armSprite)
                {
                    _playerSpriteMaker.BackArm = itemType;
                    _playerSpriteMaker.FrontArm = itemType;
                }

                if (itemInfo.torsoSprite) { _playerSpriteMaker.Torso = itemType; }
                if (itemInfo.armDecal) { _playerSpriteMaker.ArmDecals.Add(itemType); }
                if (itemInfo.headSprite) { _playerSpriteMaker.Head = itemType; }
                if (itemInfo.legDecal) { _playerSpriteMaker.LegDecals.Add(itemType); }
                if (itemInfo.legSprite) { _playerSpriteMaker.Legs = itemType; }
                if (itemInfo.shoulderPadSprite) { _playerSpriteMaker.ShoulderPad = itemType; }
                if (itemInfo.torsoDecal) { _playerSpriteMaker.TorsoDecals.Add(itemType); }

                if (itemInfo.projectileType != ProjectileType.Generic) { projectileTypes.Add(itemInfo.projectileType); }

                if (itemInfo.isEnergyWeapon)
                {
                    if (!energyWeapons.Any(e => e.item == itemInfo.type))
                    {
                        var newWeapon = Instantiate(Resources.Load("EnergyWeapons/" + itemInfo.type.ToString())) as PlayerEnergyWeapon;
                        newWeapon.Initialize(this);
                        energyWeapons.Add(newWeapon);
                    }
                }
                else if (itemInfo.ignoreTerrain && !itemInfo.follower)
                {
                    projectileStats.ignoreTerrain = true;
                }

                if (itemInfo.applyDamageTypeToProjectile && itemInfo.damageType != 0 && !projectileStats.damageType.HasFlag(itemInfo.damageType))
                {
                    projectileStats.damageType |= itemInfo.damageType;
                }

                if (itemInfo.bounce) { projectileStats.bounce = true; }
                if (itemInfo.penetrativeShot) { projectileStats.penetrative = true; }
                if (itemInfo.homing > projectileStats.homing) { projectileStats.homing = itemInfo.homing; }
                if (itemInfo.homingRadius > projectileStats.homingRadius) { projectileStats.homingRadius = itemInfo.homingRadius; }
                if (itemInfo.projectileChildEffect) { projectileStats.childEffects.Add(itemInfo.projectileChildEffect); }
                if (itemInfo.homingArc > projectileStats.homingArc) { projectileStats.homingArc = itemInfo.homingArc; }
                if (itemInfo.damageGainPerSecond != 0) { projectileStats.damageGainPerSecond += itemInfo.damageGainPerSecond; }
                if (itemInfo.sizePerSecond != 0) { projectileStats.sizePerSecond += itemInfo.sizePerSecond; }
                if (itemInfo.speedMod > 0) { _baseProjectileSpeed *= itemInfo.speedMod; }
                if (itemInfo.arcShots > 0) { arcShots += itemInfo.arcShots; }
                if (itemInfo.fireArc > fireArc) { fireArc = itemInfo.fireArc; }
                if (itemInfo.itemEnergyRegenRate > 0) { itemEnergyRegenRate += itemInfo.itemEnergyRegenRate; }
                if (itemInfo.bonusNanobots > 0) { nanobotsPerSpawn += itemInfo.bonusNanobots; }
                if (itemInfo.blessingTimeMod > 0) { blessingTimeMod *= itemInfo.blessingTimeMod; }
                if (itemInfo.allowHovering)
                {
                    canHover = true;
                    if (itemInfo.hoverTime > hoverTime) { hoverTime = itemInfo.hoverTime; }
                    if (itemInfo.hoverMaxVelocity > hoverMaxVelocity) { hoverMaxVelocity = itemInfo.hoverMaxVelocity; }
                }

                if (itemInfo.projectileStatusEffects != null)
                {
                    for (int i = 0; i < itemInfo.projectileStatusEffects.Length; i++)
                    {
                        var itemEffect = itemInfo.projectileStatusEffects[i];
                        if (itemEffect != null)
                        {
                            var effect = projectileStats.statusEffects.Find(s => s.type == itemEffect.type);
                            if (effect == null)
                            {
                                effect = StatusEffect.CopyOf(itemEffect);
                                projectileStats.statusEffects.Add(effect);
                            }
                            else
                            {
                                effect.Stack(itemEffect);
                            }
                        }
                    }
                }

                pickUpRangeBonus += itemInfo.pickUpRangeBonus;
                regenerationRate += itemInfo.regenerationRate;

                _baseMaxHealth += itemInfo.baseHealthBonus;
                _baseMaxEnergy += itemInfo.baseEnergyBonus;
                _baseMaxSpeed += itemInfo.baseSpeedBonus;
                _baseDamage += itemInfo.baseDamageBonus;
                _baseProjectileSpeed += itemInfo.baseShotSpeedBonus;
                _baseProjectileSizeMod += itemInfo.baseShotSizeBonus;

                if (itemInfo.energyMultiplier != 0) { _itemEnergyMultiplier *= itemInfo.energyMultiplier; }
                if (itemInfo.healthMultiplier != 0) { _itemHealthMultiplier *= itemInfo.healthMultiplier; }
                if (itemInfo.damageMultiplier != 0) { _itemDamageMultiplier *= itemInfo.damageMultiplier; }
                if (itemInfo.shotSizeMultiplier != 0) { _itemShotSizeMultiplier *= itemInfo.shotSizeMultiplier; }
                if (itemInfo.baseAttackMultiplier != 0) { _baseAttackDelay *= itemInfo.baseAttackMultiplier; }

                switch (itemInfo.type)
                {
                    case MajorItem.Arachnomorph:
                        if (!specialMoves.Any(m => m is PlayerArachnomorph))
                        {
                            var spider = Instantiate(Resources.Load("SpecialMoves/Arachnomorph")) as PlayerSpecialMovement;
                            spider.Initialize(this);
                            specialMoves.Add(spider);
                        }
                        break;
                    case MajorItem.Slide:
                        if (!specialMoves.Any(m => m is PlayerSlide))
                        {
                            var slide = Instantiate(Resources.Load("SpecialMoves/Slide")) as PlayerSpecialMovement;
                            slide.Initialize(this);
                            specialMoves.Add(slide);
                        }
                        break;
                    case MajorItem.Dash:
                        if (!specialMoves.Any(m => m is PlayerDash))
                        {
                            var dash = Instantiate(Resources.Load("SpecialMoves/Dash")) as PlayerSpecialMovement;
                            dash.Initialize(this);
                            specialMoves.Add(dash);
                        }
                        break;
                    case MajorItem.PhaseShell:
                        if (!specialMoves.Any(m => m is PlayerPhase))
                        {
                            var phase = Instantiate(Resources.Load("SpecialMoves/Phase")) as PlayerSpecialMovement;
                            phase.Initialize(this);
                            specialMoves.Add(phase);
                        }
                        break;
                    case MajorItem.BuzzsawShell:
                        if (!specialMoves.Any(m => m is PlayerBuzzsawShell))
                        {
                            var hedgehogShell = Instantiate(Resources.Load("SpecialMoves/BuzzsawShell")) as PlayerSpecialMovement;
                            hedgehogShell.Initialize(this);
                            specialMoves.Add(hedgehogShell);
                        }
                        break;
                    case MajorItem.DiveShell:
                        if (!specialMoves.Any(m => m is PlayerDiveKick))
                        {
                            var diveKick = Instantiate(Resources.Load("SpecialMoves/DiveKick")) as PlayerDiveKick;
                            diveKick.Initialize(this);
                            specialMoves.Add(diveKick);
                        }
                        break;
                    case MajorItem.ViridianShell:
                        if (!specialMoves.Any(m => m is PlayerGravityFlip))
                        {
                            var gravityFlip = Instantiate(Resources.Load("SpecialMoves/GravityFlip")) as PlayerGravityFlip;
                            gravityFlip.Initialize(this);
                            specialMoves.Add(gravityFlip);
                        }
                        break;
                    case MajorItem.DoubleJump:
                        maxAirJumps++;
                        break;
                    case MajorItem.Infinijump:
                        maxAirJumps = 10000;
                        break;
                    case MajorItem.JetPack:
                        {
                            if (spiderForm)
                            {
                                hoverParticles.transform.localPosition = _hoverBootsHoverParticlePosition;
                                _localHoverX = _hoverBootsHoverParticlePosition.x;
                            }
                            else
                            {
                                hoverParticles.transform.localPosition = _jetPackHoverParticlePosition;
                                _localHoverX = _jetPackHoverParticlePosition.x;
                            }
                            var p = hoverParticles.main;
                            p.startLifetime = 2;
                        }
                        break;
                    case MajorItem.HoverBoots:
                        if (!itemsPossessed.Contains(MajorItem.JetPack))
                        {
                            hoverParticles.transform.localPosition = _hoverBootsHoverParticlePosition;
                            _localHoverX = _hoverBootsHoverParticlePosition.x;
                            var p = hoverParticles.main;
                            p.startLifetime = 1;
                        }
                        break;
                    case MajorItem.DeadlockBoots:
                        _allowKnockback = false;
                        controller2D.resistConveyorsAndIce = true;
                        break;
                    case MajorItem.PowerJump:
                        maxJumpHeight = 12;
                        timeToJumpApex = (12 / Constants.startingMaxJumpHeight) * Constants.startingJumpTime * 0.9f;
                        if (!jumpEvents.Find((e) => e.name == "JumpBurst"))
                        {
                            var jumpBurst = new GameObject("JumpBurst").AddComponent<BaseEvent>();
                            jumpBurst.onEventStart = new UnityEvent();
                            jumpBurst.onEventStart.AddListener(PowerJump);
                            jumpEvents.Add(jumpBurst);
                        }
                        break;
                    case MajorItem.FragmentShot:
                        projectileStats.fragment = true;
                        projectileStats.fragmentStats = new ProjectileFragmentStats()
                        {
                            sizeMod = 0.66f,
                            damageMod = 0.5f,
                            arc = 90,
                            amount = 2,
                            lifeSpan = 2,
                            recursion = 0,
                        };
                        break;
                    case MajorItem.ExplosiveBolt:
                        attackSound = Resources.Load<AudioClip>("Sounds/Projectiles/ExplosiveBoltShoot");
                        projectileStats.explosion = Explosion.E16x16;
                        _baseExplosionDamage = 1.5f;
                        break;
                    case MajorItem.FireBolt:
                        attackSound = Resources.Load<AudioClip>("Sounds/Projectiles/FireBoltShoot");
                        attackSoundsDirty = true;
                        break;
                    case MajorItem.HeatShell:
                        var heatBurst = hurtBursts.Find((g) => g.name == "HeatBurst");
                        if (heatBurst == null)
                        {
                            var prefab = Resources.Load<SuitBurst>("ItemFX/HeatBurst");
                            heatBurst = Instantiate(prefab);
                            heatBurst.name = "HeatBurst";
                            heatBurst.player = this;
                            heatBurst.transform.parent = transform;
                            heatBurst.transform.localScale = Vector3.one;
                            heatBurst.transform.localPosition = Vector3.zero;
                            heatBurst.gameObject.SetActive(false);
                            hurtBursts.Add(heatBurst);
                        }
                        break;
                    case MajorItem.BrightShell:
                        if (light)
                        {
                            light.sprite = light400;
                        }

                        var brightBurst = hurtBursts.Find((g) => g.name == "BrightBurst");
                        if (brightBurst == null)
                        {
                            var prefab = Resources.Load<SuitBurst>("ItemFX/BrightBurst");
                            brightBurst = Instantiate(prefab);
                            brightBurst.name = "BrightBurst";
                            brightBurst.player = this;
                            brightBurst.transform.parent = transform;
                            brightBurst.transform.localScale = Vector3.one;
                            brightBurst.transform.localPosition = Vector3.zero;
                            brightBurst.gameObject.SetActive(false);
                            hurtBursts.Add(brightBurst);
                        }
                        break;
                    case MajorItem.BurstPauldrons:
                        var eBurst = hurtBursts.Find((g) => g.name == "BigEnergyBurst");
                        if (eBurst == null)
                        {
                            var prefab = Resources.Load<SuitBurst>("ItemFX/BigEnergyBurst");
                            eBurst = Instantiate(prefab);
                            eBurst.name = "BigEnergyBurst";
                            eBurst.player = this;
                            eBurst.transform.parent = transform;
                            eBurst.transform.localScale = Vector3.one;
                            eBurst.transform.localPosition = Vector3.zero;
                            eBurst.gameObject.SetActive(false);
                            hurtBursts.Add(eBurst);
                        }
                        break;
                    case MajorItem.ToxinPauldrons:
                        if (!itemHurtActions.ContainsKey(MajorItem.ToxinPauldrons))
                        {
                            itemHurtActions.Add(MajorItem.ToxinPauldrons, ToxinPauldrons);
                        }
                        break;
                    case MajorItem.ElectroCharge:
                        canCharge = true;
                        _chargeColor = Constants.electroYellow;
                        attackSound = Resources.Load<AudioClip>("Sounds/Projectiles/ElectroBoltShoot");
                        attackSoundX1p5 = Resources.Load<AudioClip>("Sounds/Projectiles/ElectroBoltShootX1p5");
                        attackSoundX2 = Resources.Load<AudioClip>("Sounds/Projectiles/ElectroBoltShootX2");
                        attackSoundsDirty = true;
                        break;
                    case MajorItem.CaveMap:
                        if (!_allRevealed && !_itemsApplied.Contains(MajorItem.CaveMap))
                        {
                            _itemsApplied.Add(MajorItem.CaveMap);
                            if (SaveGameManager.activeGame != null)
                            {
                                environmentsToReveal.Add(EnvironmentType.Cave);
                                environmentsToReveal.Add(EnvironmentType.CoolantSewers);
                            }
                        }
                        break;
                    case MajorItem.FactoryMap:
                        if (!_allRevealed && !_itemsApplied.Contains(MajorItem.FactoryMap))
                        {
                            _itemsApplied.Add(MajorItem.FactoryMap);
                            if (SaveGameManager.activeGame != null)
                            {
                                environmentsToReveal.Add(EnvironmentType.Factory);
                                environmentsToReveal.Add(EnvironmentType.CrystalMines);
                            }
                        }
                        break;
                    case MajorItem.BuriedCityMap:
                        if (!_allRevealed && !_itemsApplied.Contains(MajorItem.BuriedCityMap))
                        {
                            _itemsApplied.Add(MajorItem.BuriedCityMap);
                            if (SaveGameManager.activeGame != null)
                            {
                                environmentsToReveal.Add(EnvironmentType.BuriedCity);                                
                            }
                        }
                        break;
                    case MajorItem.BossMap:
                        if (!_allRevealed && !_itemsApplied.Contains(MajorItem.BossMap))
                        {
                            _itemsApplied.Add(MajorItem.BossMap);
                            RevealBosses();
                        }
                        break;
                    case MajorItem.ArtifactMap:
                        if (!_allRevealed && !_itemsApplied.Contains(MajorItem.ArtifactMap))
                        {
                            _itemsApplied.Add(MajorItem.ArtifactMap);
                            RevealArtifacts();
                        }
                        break;
                    case MajorItem.GlitchMap:
                        if (!_itemsApplied.Contains(MajorItem.GlitchMap) && SaveGameManager.activeGame != null)
                        {
                            _itemsApplied.Add(MajorItem.GlitchMap);
                            environmentsToReveal.Add(EnvironmentType.Glitch);

                            if (!_allRevealed)
                            {
                                var activeGame = SaveGameManager.activeGame;
                                var layout = activeGame.layout;
                                var automapSegmentStates = activeGame.automapSegmentStates;
                                foreach (var roomAbstract in layout.roomAbstracts)
                                {
                                    if (roomAbstract.assignedRoomInfo.environmentType == EnvironmentType.Glitch) continue;
                                    if (roomAbstract.assignedRoomInfo.environmentType == EnvironmentType.BeastGuts && !SaveGameManager.beastGutsUnlocked) continue;

                                    if (Random.value > 0.75f || roomAbstract.assignedRoomInfo.environmentType == EnvironmentType.Glitch)
                                    {
                                        activeGame.roomsDiscovered.Add(roomAbstract.roomID);

                                        for (int x = 0; x < roomAbstract.width; x++)
                                        {
                                            for (int y = 0; y < roomAbstract.height; y++)
                                            {
                                                var position = new Vector2(roomAbstract.gridPosition.x + x, roomAbstract.gridPosition.y - y);
                                                var name = position.ToString();
                                                // Hidden spaces won't be shown by this
                                                if (!automapSegmentStates.ContainsKey(name)) automapSegmentStates[name] = AutomapSegmentState.Discovered;
                                            }
                                        }
                                    }
                                    onGridPositionChanged();
                                }
                            }
                        }
                        break;
                    case MajorItem.MasterMap:
                        if (!_allRevealed && !_itemsApplied.Contains(MajorItem.MasterMap))
                        {
                            _itemsApplied.Add(MajorItem.MasterMap);
                            _allRevealed = true;
                            foreach (var e in Enum.GetValues(typeof(EnvironmentType)).Cast<EnvironmentType>())
                            {
                                if (e == EnvironmentType.Glitch || e == EnvironmentType.GreyBox)
                                {
                                    continue;
                                }

                                if (!SaveGameManager.beastGutsUnlocked && e == EnvironmentType.BeastGuts) { continue; }

                                environmentsToReveal.Add(e);
                            }
                        }
                        break;
                    case MajorItem.ArtificeHelm:
                        artificeMode = true;
                        if (!_itemsApplied.Contains(MajorItem.ArtificeHelm))
                        {
                            _itemsApplied.Add(MajorItem.ArtificeHelm);
                            health = maxHealth;
                            GainScrap(CurrencyType.Gray, 5);
                        }
                        break;
                    case MajorItem.ScrapCache:
                        if (!_itemsApplied.Contains(MajorItem.ScrapCache))
                        {
                            _itemsApplied.Add(MajorItem.ScrapCache);
                            GainScrap(CurrencyType.Gray, 50);
                        }
                        break;
                    case MajorItem.TheRedKey:
                        maxAirJumps++;
                        break;
                    case MajorItem.TheBlueKey:
                        _blueKey = true;
                        break;
                    case MajorItem.TheBlackKey:
                        _blackKey = true;
                        break;
                    case MajorItem.ScrapBooster:
                        _scrapBooster = true;
                        break;
                    case MajorItem.HealthBooster:
                        _healthBooster = true;
                        break;
                    case MajorItem.EnergyBoots:
                        _energyBoots = true;
                        break;
                    case MajorItem.GlitchShell:
                        if (LayoutManager.instance && !_listenersApplied.Contains(MajorItem.GlitchShell))
                        {
                            LayoutManager.instance.onTransitionComplete += ShuffleStats;
                            _listenersApplied.Add(MajorItem.GlitchShell);
                        }

                        if (!_itemsApplied.Contains(MajorItem.GlitchShell))
                        {
                            healthUps++;
                            energyUps++;
                            attackUps++;
                            damageUps++;
                            speedUps++;
                            shotSpeedUps++;
                            _itemsApplied.Add(MajorItem.GlitchShell);
                        }
                        break;
                    case MajorItem.MegaHealth:
                        if (!_itemsApplied.Contains(MajorItem.MegaHealth))
                        {
                            CalculateMaxHealthAndEnergy(false);
                            health = maxHealth;
                            _itemsApplied.Add(MajorItem.MegaHealth);
                        }
                        break;
                    case MajorItem.MegaEnergy:
                        if (!_itemsApplied.Contains(MajorItem.MegaEnergy))
                        {
                            CalculateMaxHealthAndEnergy(false);
                            energy = maxEnergy;
                            _itemsApplied.Add(MajorItem.MegaEnergy);
                        }
                        break;
                    case MajorItem.CognitiveStabilizer:
                        confused = false;
                        break;
                    case MajorItem.HazardShell:
                        if (!immunities.HasFlag(DamageType.Hazard)) { immunities |= DamageType.Hazard; }
                        break;
                    case MajorItem.CrystalShell:
                        var deflector = gameObject.GetComponent<ProjectileDeflector>();
                        if (!deflector) { deflector = gameObject.AddComponent<ProjectileDeflector>(); }
                        deflector.deflectSound = Resources.Load<AudioClip>("Sounds/DeflectImpact");
                        deflector.deflectDamage = 0;
                        deflector.deflectDamage |= DamageType.Cold;
                        deflector.deflectDamage |= DamageType.Electric;
                        deflector.deflectDamage |= DamageType.Explosive;
                        deflector.deflectDamage |= DamageType.Fire;
                        deflector.deflectDamage |= DamageType.Generic;
                        deflector.deflectDamage |= DamageType.Mechanical;
                        deflector.radialDeflection = true;
                        deflector.setTeam = team;

                        AddVulnerability(DamageType.Cold);
                        AddVulnerability(DamageType.Electric);
                        AddVulnerability(DamageType.Explosive);
                        AddVulnerability(DamageType.Fire);
                        AddVulnerability(DamageType.Generic);
                        AddVulnerability(DamageType.Hazard);
                        AddVulnerability(DamageType.Mechanical);
                        AddVulnerability(DamageType.Velocity);
                        break;
                    case MajorItem.HiveBolt:
                        //_onKillEnemy reset by ResetAbilities
                        //if (!_listenersApplied.Contains(MajorItem.HiveBolt))
                        {
                            _onKillEnemy += HiveBolt;
                            //_listenersApplied.Add(MajorItem.HiveBolt);
                        }
                        break;
                    case MajorItem.BoltHelm:
                        if (!_listenersApplied.Contains(MajorItem.BoltHelm))
                        {
                            onBolt += BoltHelm;
                            _listenersApplied.Add(MajorItem.BoltHelm);
                        }
                        break;
                    case MajorItem.HiveShell:
                        if (!_listenersApplied.Contains(MajorItem.HiveShell))
                        {                            
                            onHurt.AddListener(HiveShell);
                            _listenersApplied.Add(MajorItem.HiveShell);
                        }
                        break;
                    case MajorItem.RotaryBolt:
                        rotaryShot = true;
                        break;
                    case MajorItem.EmpireHelm:
                        _empireHelm = true;
                        break;
                }
            }

            if (environmentsToReveal.Count > 0)
            {
                RevealEnvironments(environmentsToReveal);
            }

            if (palettes.Count > 0)
            {
                _palette = _playerSpriteMaker.AveragePalettesHSL(palettes);
            }
            else
            {
                _palette = _originalPalette;
            }

            mainRenderer.material.SetTexture("_Palette", _palette);

            //figure out projectileType
            {
                var pType = Constants.GetPlayerProjectileType(projectileTypes);

                if (pType != projectileStats.type)
                {
                    if (!DeathmatchManager.instance)
                    {
                        ProjectileManager.instance.ClearProjectiles(new ProjectileType[] { projectileStats.type });
                    }
                    projectileStats.type = pType;
                }
            }

            if (attackSoundsDirty)
            {
                _attackSounds.Clear();
                if (attackSound) { _attackSounds[1] = attackSound; }
                if (attackSoundX1p5) { _attackSounds[1.5f] = attackSoundX1p5; }
                if (attackSoundX2) { _attackSounds[2] = attackSoundX2; }
            }

            //TODO: maybe add a bool to check, so this only happens if a new specialMove was added
            specialMoves.Sort((m1, m2) => -m1.priorty.CompareTo(m2.priorty));
        }

        if (AchievementManager.instance)
        {
            if (followers.FindAll(o => o.followerIndex != -1).Count >= 4)
            {
                AchievementManager.instance.WaitTryEarnAchievement(1, AchievementID.OrbBot);
            }

            if (energyWeapons.Count >= 3)
            {
                AchievementManager.instance.WaitTryEarnAchievement(1, AchievementID.GunBot);
            }
        }

        CalculateAllStats();
        UpdateTraversalCapabilites();

        if (SaveGameManager.activeGame != null && SaveGameManager.activeGame.gameMode == GameMode.Spooky)
        {
            _playerSpriteMaker.Head = "Skeleton";
            spriteMakerMode = SpriteMakerMode.Force;
        }

        if (spriteMakerMode == SpriteMakerMode.Force ||
            (spriteMakerMode == SpriteMakerMode.Automatic &&
            (energyWeaponCount != energyWeapons.Count || spriteMakerKey != _playerSpriteMaker.GetKey())))
        {
            _playerSpriteMaker.MakeSprite();
        }
    }

    public void ResetAbilities()
    {
        arcShots = 0;
        fireArc = 0;
        maxAirJumps = 0;
        canHover = false;
        hoverTime = 0;
        hoverMaxVelocity = 0;
        _allowKnockback = true;
        _onKillEnemy = null;
        rotaryShot = false;

        _playerSpriteMaker.ClearItems();
        _playerSpriteMaker.ClearDecals();

        canCharge = false;
        _baseDamage = Constants.startingDamage;
        _baseMaxEnergy = _startingEnergy;
        _baseMaxHealth = _startingHealth;
        _baseMaxSpeed = Constants.startingMaxSpeed;
        _baseAttackDelay = Constants.startingAttackDelay;
        _baseProjectileSpeed = Constants.startingProjectileSpeed;
        _baseProjectileSizeMod = 1;
        _energyRegenTimeMod = 1;
        artificeMode = false;
        _blackKey = false;
        _blueKey = false;
        _scrapBooster = false;
        _healthBooster = false;
        _energyBoots = false;
        _itemHealthMultiplier = 1;
        _itemEnergyMultiplier = 1;
        _itemDamageMultiplier = 1;
        _itemShotSizeMultiplier = 1;

        projectileStats = new ProjectileStats(_originalProjectileStats);
        
        attackSound = Resources.Load<AudioClip>("Sounds/Projectiles/BlasterBoltShoot");
        attackSoundX1p5 = null;
        attackSoundX2 = null;

        _attackSounds.Clear();
        _attackSounds.Add(1, attackSound);

        maxJumpHeight = Constants.startingMaxJumpHeight;
        timeToJumpApex = Constants.startingJumpTime;

        pickUpRangeBonus = 0;
        regenerationRate = 0;
        itemEnergyRegenRate = 0;
        nanobotsPerSpawn = 1;
        blessingTimeMod = 1;
    }

    public void RemoveItem(MajorItem item, bool matchItems = true, bool updateTraversal = true)
    {
        if (itemsPossessed.Contains(item))
        {
            Debug.Log("Removing Item: " + item);

            itemsPossessed.Remove(item);
            var itemInfo = ItemManager.items[item];

            immunities = 0;
            vulnerabilities = 0;

            if (itemInfo.follower)
            {
                var follower = followers.FirstOrDefault((f) => f.type == itemInfo.type);
                if (follower)
                {
                    followers.Remove(follower);
                    Destroy(follower.gameObject);
                    ReorderFollowers();
                }
            }

            if (itemInfo.isEnergyWeapon)
            {
                var weapon = energyWeapons.Find(e => e.item == itemInfo.type);
                if (weapon)
                {
                    energyWeapons.Remove(weapon);
                    if (_selectedEnergyWeapon == weapon)
                    {
                        _selectedEnergyWeaponIndex = 0;
                        _selectedEnergyWeapon = null;
                    }
                    else if (_selectedEnergyWeapon)
                    {
                        _selectedEnergyWeaponIndex = energyWeapons.IndexOf(_selectedEnergyWeapon);
                    }

                    Destroy(weapon);
                }
            }

            itemHurtActions.Remove(item);
            _itemsApplied.Remove(item);

            switch (itemInfo.type)
            {
                case MajorItem.Arachnomorph:
                    {
                        var special = specialMoves.Find((m) => m is PlayerArachnomorph);
                        if (_activeSpecialMove = special)
                        {
                            _activeSpecialMove.DeathStop();
                            _activeSpecialMove = null;
                        }
                        if (spiderForm) { ToggleSpiderForm(); }
                        specialMoves.Remove(special);
                        Destroy(special);
                    }
                    break;
                case MajorItem.Slide:
                    {
                        var special = specialMoves.Find((m) => m is PlayerSlide);
                        if (_activeSpecialMove = special)
                        {
                            _activeSpecialMove.DeathStop();
                            _activeSpecialMove = null;
                        }
                        specialMoves.Remove(special);
                        Destroy(special);
                    }
                    break;
                case MajorItem.Dash:
                    {
                        var special = specialMoves.Find((m) => m is PlayerDash);
                        if (_activeSpecialMove = special)
                        {
                            _activeSpecialMove.DeathStop();
                            _activeSpecialMove = null;
                        }
                        specialMoves.Remove(special);
                        Destroy(special);
                    }
                    break;
                case MajorItem.PhaseShell:
                    {
                        var special = specialMoves.Find((m) => m is PlayerPhase);
                        if (_activeSpecialMove = special)
                        {
                            _activeSpecialMove.DeathStop();
                            _activeSpecialMove = null;
                        }
                        specialMoves.Remove(special);
                        Destroy(special);
                    }
                    break;
                case MajorItem.DiveShell:
                    {
                        var special = specialMoves.Find((m) => m is PlayerDiveKick);
                        if (_activeSpecialMove = special)
                        {
                            _activeSpecialMove.DeathStop();
                            _activeSpecialMove = null;
                        }
                        specialMoves.Remove(special);
                        Destroy(special);
                    }
                    break;
                case MajorItem.ViridianShell:
                    {
                        var special = specialMoves.Find((m) => m is PlayerGravityFlip);
                        if (_activeSpecialMove = special)
                        {
                            _activeSpecialMove.DeathStop();
                            _activeSpecialMove = null;
                        }
                        if (gravityFlipped) { FlipGravity(); }
                        specialMoves.Remove(special);
                        Destroy(special);
                    }
                    break;
                case MajorItem.PowerJump:
                    var burst = jumpEvents.Find((e) => e.name == "JumpBurst");
                    if (burst != null)
                    {
                        jumpEvents.Remove(burst);
                        Destroy(burst.gameObject);
                    }
                    break;
                case MajorItem.FragmentShot:
                    projectileStats.fragment = true;
                    projectileStats.fragmentStats = new ProjectileFragmentStats()
                    {
                        sizeMod = 0.66f,
                        damageMod = 0.5f,
                        arc = 90,
                        amount = 2,
                        lifeSpan = 2,
                        recursion = 0,
                    };

                    break;
                case MajorItem.ExplosiveBolt:
                    attackSound = Resources.Load<AudioClip>("Sounds/Projectiles/ExplosiveBoltShoot");
                    projectileStats.explosion = Explosion.E16x16;
                    _baseExplosionDamage = 1.5f;
                    break;
                case MajorItem.FireBolt:
                    attackSound = Resources.Load<AudioClip>("Sounds/Projectiles/FireBoltShoot");
                    break;
                case MajorItem.HeatShell:
                    var heatBurst = hurtBursts.Find((g) => g.name == "HeatBurst");
                    if (heatBurst != null)
                    {
                        hurtBursts.Remove(heatBurst);
                        Destroy(heatBurst.gameObject);
                    }
                    break;
                case MajorItem.BrightShell:
                    var brightBurst = hurtBursts.Find((g) => g.name == "BrightBurst");
                    if (brightBurst != null)
                    {
                        hurtBursts.Remove(brightBurst);
                        Destroy(brightBurst.gameObject);
                    }
                    break;
                case MajorItem.BurstPauldrons:
                    var eBurst = hurtBursts.Find((g) => g.name == "BigEnergyBurst");
                    if (eBurst != null)
                    {
                        hurtBursts.Remove(eBurst);
                        Destroy(eBurst.gameObject);
                    }
                    break;
                case MajorItem.ElectroCharge:
                    canCharge = false;
                    _chargeColor = Constants.blasterGreen;
                    break;
                case MajorItem.ArtificeHelm:
                    artificeMode = false;
                    break;
                case MajorItem.TheBlueKey:
                    _blueKey = false;
                    break;
                case MajorItem.TheBlackKey:
                    _blackKey = false;
                    break;
                case MajorItem.ScrapBooster:
                    _scrapBooster = false;
                    break;
                case MajorItem.HealthBooster:
                    _healthBooster = false;
                    break;
                case MajorItem.EnergyBoots:
                    _energyBoots = false;
                    break;
                case MajorItem.DeadlockBoots:
                    _allowKnockback = true;
                    controller2D.resistConveyorsAndIce = false;
                    break;
                case MajorItem.GlitchShell:
                    if (LayoutManager.instance)
                    {
                        LayoutManager.instance.onTransitionComplete -= ShuffleStats;
                        _listenersApplied.Remove(MajorItem.GlitchShell);
                    }

                    healthUps--;
                    energyUps--;
                    attackUps--;
                    damageUps--;
                    speedUps--;
                    shotSpeedUps--;
                    break;
                case MajorItem.HazardShell:
                    immunities &= ~DamageType.Hazard;
                    break;
                case MajorItem.CrystalShell:
                    var deflector = GetComponent<ProjectileDeflector>();
                    Destroy(deflector);
                    RemoveVulnerability(DamageType.Cold);
                    RemoveVulnerability(DamageType.Electric);
                    RemoveVulnerability(DamageType.Explosive);
                    RemoveVulnerability(DamageType.Fire);
                    RemoveVulnerability(DamageType.Generic);
                    RemoveVulnerability(DamageType.Hazard);
                    RemoveVulnerability(DamageType.Mechanical);
                    RemoveVulnerability(DamageType.Velocity);
                    break;
                case MajorItem.HiveBolt:
                    _onKillEnemy -= HiveBolt;
                    //_listenersApplied.Remove(MajorItem.HiveBolt);
                    break;
                case MajorItem.BoltHelm:
                    onBolt -= BoltHelm;
                    _listenersApplied.Remove(MajorItem.BoltHelm);
                    break;
                case MajorItem.HiveShell:
                    onHurt.RemoveListener(HiveShell);
                    _listenersApplied.Remove(MajorItem.HiveShell);
                    break;
                case MajorItem.RotaryBolt:
                    rotaryShot = false;
                    break;
                case MajorItem.EmpireHelm:
                    _empireHelm = true;
                    break;
            }

            if (onCollectItem != null) onCollectItem();
            if (matchItems) { MatchItems(true); }
            else if (updateTraversal) { UpdateTraversalCapabilites(); }
        }
    }
    
    public TraversalCapabilities UpdateTraversalCapabilites()
    {
        traversalCapabilities = new TraversalCapabilities();
        traversalCapabilities.baseJumpHeight = maxJumpHeight;
        traversalCapabilities.jumps = 1 + maxAirJumps;
        traversalCapabilities.hoverJumpHeight = hoverMaxVelocity * hoverTime;
        traversalCapabilities.waterJumpMod = Constants.startingWaterJumpMod;

        var count = itemsPossessed.Count;
        for (int i = 0; i < count; i++)
        {
            var itemInfo = ItemManager.items[itemsPossessed[i]];
            if (!itemInfo.isTraversalItem) continue;
            if (itemInfo.damageType != 0) { traversalCapabilities.damageTypes |= itemInfo.damageType; ; }
            if (itemInfo.environmentalResistance != 0) { traversalCapabilities.environmentalResistance |= itemInfo.environmentalResistance; }
            if (itemInfo.ignoreTerrain) { traversalCapabilities.shotIgnoresTerrain = true; }

            switch (itemInfo.type)
            {
                case MajorItem.UpDog:
                    var jumpHeightMod = 13 - Constants.startingMaxJumpHeight;
                    traversalCapabilities.baseJumpHeight += jumpHeightMod;
                    break;
                case MajorItem.DiveShell:
                    traversalCapabilities.waterJumpMod = 1f;
                    break;
                case MajorItem.Arachnomorph:
                    traversalCapabilities.canTraverseElevatedSmallGaps = true;
                    traversalCapabilities.canTraverseGroundedSmallGaps = true;
                    break;
                case MajorItem.BuzzsawShell:
                case MajorItem.Slide:
                    traversalCapabilities.canTraverseGroundedSmallGaps = true;
                    break;
                case MajorItem.PhaseShell:
                    traversalCapabilities.canPhaseThroughWalls = true;
                    break;
                case MajorItem.ViridianShell:
                    traversalCapabilities.canReverseGravity = true;
                    break;
            }
        }

        return traversalCapabilities;
    }
}
