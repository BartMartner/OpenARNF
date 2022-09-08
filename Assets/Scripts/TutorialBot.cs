using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;
using Random = UnityEngine.Random;

public class TutorialBot : RoomBasedRandom
{
    public Sprite deadFrame;
    public AudioClip poochieFlap;
    public AudioClip slideWhistle;
    public SpriteRenderer spriteRenderer;

    private DialogueTrigger _dialogueTrigger;
    private Animator _animator;
    private bool _hasGlitch;
    private Action _onHide;
    private XorShift _localRandom;

    [HideInInspector]
    public static string[] helpfulTips = new string[]
    {
        //Bullshit
        "If you don't need help, don't talk to me.",
        "Avoid spikes. They'll damage you. If you take too much damage you die forever. Like me.",
        "To wall jump, press... oh god the pain! Please help...",
        "Shoot at meat to destroy it.",
        "Every time you talk to me I seem to die. Please stop talking to me.",
        "Shoot at blocks to discover secrets!",
        "Avoid enemy projectiles. The damage they cause can kill you.",
        "Shoot at boss monsters until they are dead.",
        "What is a robot? A miserable little pile of circuits.",
        "Jump to reach higher areas.",
        "I was one day away from retirement...",
        "Shoot doors to open them. Some doors require special abilities to open",
        "Are you the same one? Are you a different one?",
        "Hot rooms are too hot. Don't let them melt you.",
        "Use items to find out how they work.",
        "Collect health to avoid dying. I wish I had.",
        "I'm sorry Fight, but your tutorial is in another castle.",
        "Don't die.",
        "Maybe you should just die now. This one is going to be hard.",
#if !UNITY_SWITCH
        "A Besethdo Net account is required to play this title. Please connect to the internet to continue.",
#endif
        //Lore
        "Don't trust orbs",
        "Don't trust me",
        "Don't trust yourself",
        "Don't trust the gods",
        //Genuine Tips
        "Shot Speed Modules can also increase the range of certain energy weapons.",
        "Exploit revenant stations to get where you need to go, quickly.",
        "You can summon K1K1 the orb using a second controller.",
        "Avoid dark rooms if you don't have a light source.",
        "Want to jump higher? Hold the button longer dummy. For your jumps.",
        "<bosstip>",
        //Shrines
        "4 times 4 shall open the door.",
        "Every shrine has a holy number.",
        "Shrines expire after three uses.",
        "Multiples of a shrine's holy number yield great rewards.",
        "Tyr can reliably provide temporary boosts to damage and shot size.",
        "Zurvan can restore your health for 5 scrap.",
        "Zurvan can provide you with a map for 10 scrap.",
        "Zurvan can reliably provide you with temporary regeneration.",
        "Hephaestus's alchemy can change two unwanted scrap to what you desire.",
        "Hephaestus is a master of energy. He can make you temporarily unstoppable.",
        "For his holy number squared, Hephaestus can remake you.",
        "For 12 scrap, Hephaestus can alter your special scrap.",
        "Wadjet-Mikail will shield you if you provide her 7 scrap.",
        "Orphiel will spawn nanobots to aid you for 9 scrap.",
        "Need help finding items? Give Zurvan 10 Gray Scrap and 1 Red.",
        "Tyr has a holy number and that number is 3.",
        "Zurvan was the 5th mechanical god to enter the world."
    };

    private string[] _glitchWorld = new string[]
    {
        //"Upon the night, betwixt earth and flesh, the grinding of souls whispers a tale of how the dead do dance.", //Too long
        "I'm dying Fight. Is it blissful? It's like a dream. Do you want to dream too?",
        "I am error. I am death. I am all.",
        "01000100 01100101 01100101 01110010 01100110 01101111 01110010 01100011 01100101",
        "Let's live here together.",
        "It's a secret to everybody.",
        "Goodbye.",
    };

    private string _poochie = "I have to go now. My planet needs me.";

    protected override IEnumerator Start()
    {
        _animator = GetComponentInChildren<Animator>();
        _dialogueTrigger = GetComponentInChildren<DialogueTrigger>();
        _hasGlitch = LayoutManager.instance && LayoutManager.instance.layout.hasGlitchWorld;
        return base.Start();
    }

    public override void Randomize()
    {
        var activeSlot = SaveGameManager.activeSlot;
        var activeGame = SaveGameManager.activeGame;
        _localRandom = new XorShift(_room.roomAbstract.seed);

        if (activeGame != null && activeGame.tutorialComplete)
        {
            Destroy(GetComponentInChildren<ButtonTriggerBounds>().gameObject);
            Destroy(_dialogueTrigger.buttonHint.gameObject);
            Destroy(_dialogueTrigger.gameObject);
            Destroy(_animator);
            spriteRenderer.sprite = deadFrame;
            return;
        }

        if (activeGame != null && activeGame.bossesDefeated.Contains(BossName.GlitchBoss))
        {
            _dialogueTrigger.dialogueInfo.dialoguePrefab = null;
            _dialogueTrigger.dialogueInfo.dialogue = "BOR PHOR PHORBA PHOR PHORBA" +
                "BES CHARIN BAUBA TE PHOR BORPHORBA PHORBABOR" +
                "BAPHORBA PHABRAIE PHORBA PHARBA PHORPHOR" +
                "PHORBA BOBORBORBA PAMPHORBA PHORPHOR PHORBA," +
                "protect N";
            spriteRenderer.sprite = deadFrame;
            _animator.enabled = false;
            if (NPCDialogueManager.instance)
            {
                NPCDialogueManager.instance.dialogueScreen.OnShow += FadeMusic;
                NPCDialogueManager.instance.dialogueScreen.OnHide += OnHideSpooky;
            }
            return;
        }

        if (_hasGlitch)
        {
            _dialogueTrigger.dialogueInfo.dialoguePrefab = null;
            _dialogueTrigger.dialogueInfo.dialogue = _glitchWorld[_localRandom.Range(0, _glitchWorld.Length)];
            _animator.Play("Glitch");
            if (NPCDialogueManager.instance)
            {                
                NPCDialogueManager.instance.dialogueScreen.OnShow += FadeMusic;
                NPCDialogueManager.instance.dialogueScreen.OnHide += OnHideGlitch;
            }
            return;
        }

        if (!SaveGameManager.instance || (activeSlot != null && activeSlot.tutorialSmithVisted))
        {
            _dialogueTrigger.dialogueInfo.dialoguePrefab = null;

            string tip = string.Empty;

            var roll = _localRandom.Value();
            if (roll <= 0.005)
            {
                tip = _poochie;
                _onHide += () => StartCoroutine(Poochie());
            }
            else if (roll <= 0.2f)
            {
                _onHide += AnimatorCast;
                
                var player = PlayerManager.instance.player1;
                List<MajorItem> availableKeys = new List<MajorItem>();
                if(activeSlot != null)
                {
                    foreach (var item in activeSlot.itemsCollected)
                    {
                        if(item == MajorItem.TheBlackKey ||
                            item == MajorItem.TheGreenKey ||
                            item == MajorItem.TheRedKey ||
                            item == MajorItem.TheBlueKey)
                        {
                            availableKeys.Add(item);
                        }
                    }
                }

                var fightNumber = (activeSlot != null ? activeSlot.totalDeaths : 0);
                var option = _localRandom.Range(0, availableKeys.Count > 0 ? 11 : 10);
                switch (option)
                {
                    case 0:
                        tip = "Godspeed, Fight " + fightNumber;
                        _onHide += (() =>
                        {
                            player.ModifyMinorItemCollection(MinorItemType.SpeedModule, 2, true);
                        });
                        break;
                    case 1:
                        tip = "Crawl away from here, Fight " + fightNumber;
                        _onHide += (() =>
                        {
                            player.ModifyMinorItemCollection(MinorItemType.SpeedModule, -2, true);
                            player.fatigued = true;
                        });
                        break;
                    case 2:
                        tip = "A blessing of violence upon you, Fight " + fightNumber;
                        _onHide += (() =>
                        {
                            player.ModifyMinorItemCollection(MinorItemType.DamageModule, 2, true);
                        });
                        break;
                    case 3:
                        tip = "A curse of weakness upon you, Fight " + fightNumber;
                        _onHide += (() =>
                        {
                            player.ModifyMinorItemCollection(MinorItemType.DamageModule, -2, true);
                            player.fatigued = true;
                        });
                        break;
                    case 4:
                        tip = "Wealth to you, Fight " + fightNumber;
                        _onHide += (() =>
                        {
                            player.StartFlash(1, 0.75f, Color.white, 0.5f, true);
                            player.GainScrap(CurrencyType.Gray, 15);
                            player.GainScrap(CurrencyType.Red, 1);
                            player.GainScrap(CurrencyType.Green, 1);
                            player.GainScrap(CurrencyType.Blue, 1);
                        });
                        break;
                    case 5:
                        tip = "A pox upon you, Fight " + fightNumber;
                        _onHide += (() =>
                        {
                            player.ModifyMinorItemCollection(MinorItemType.HealthTank, -1, true);
                            player.ModifyMinorItemCollection(MinorItemType.EnergyModule, -1, true);
                            player.fatigued = true;
                        });
                        break;
                    case 6:
                        tip = "Take care of them, Fight " + fightNumber;
                        _onHide += (() =>
                        {
                            var quantity = _localRandom.Range(3, 6);
                            player.StartFlash(1, 0.5f, Color.white, 0.75f, true);
                            for (int i = 0; i < quantity; i++)
                            {
                                player.SpawnNanobot(player.transform.position);
                            }
                        });
                        break;
                    case 7:
                        tip = "I told you to leave me alone!";
                        _onHide += (() =>
                        {
                            player.Hurt(player.health - 1);
                            player.fatigued = true;
                        });
                        break;
                    case 8:
                        tip = "Feeling Drained?";
                        _onHide += (() =>
                        {
                            player.energy = 0;
                            player.fatigued = true;
                        });
                        break;
                    case 9:
                        tip = "Tired of chasing bullets, Fight " + fightNumber + "?";
                        _onHide += (() =>
                        {
                            player.ModifyMinorItemCollection(MinorItemType.AttackModule, 3, true);
                            player.ModifyMinorItemCollection(MinorItemType.ShotSpeedModule, -6, true);
                            player.fatigued = true;
                        });
                        break;
                    case 10:
                        tip = "The Tarot Will Teach You How To Create A Soul";
                        _onHide += (() =>
                        {
                            player.CollectMajorItem(availableKeys[_localRandom.Range(0, availableKeys.Count)]);    
                        });
                        break;
                }

                if(SaveGameManager.instance)
                {
                    SaveGameManager.instance.Save();
                }
            }
            else
            {
                _onHide += AnimatorDie;
                tip = helpfulTips[_localRandom.Range(0, helpfulTips.Length)];
                if (tip == "<bosstip>")
                {
                    tip = GetBossTip();
                }
            }

            _dialogueTrigger.dialogueInfo.dialogue = tip;
        }
        else
        {
            _onHide += AnimatorDie;
        }

        if (NPCDialogueManager.instance)
        {
            NPCDialogueManager.instance.dialogueScreen.OnHide += OnHide;
        }
    }

    public string GetBossTip()
    {
        BossName boss = BossName.None;
        if (!LayoutManager.instance)
        {
            boss = LayoutManager.instance.layout.bossesAdded[_localRandom.Range(0, LayoutManager.instance.layout.bossesAdded.Count)];
        }

        switch (boss)
        {
            case BossName.FleshAdder:
            case BossName.Cephalodiptera:
                return "Some meat beasts can alter their metabolism to conduct powerful charge attacks.";
            case BossName.Sluggard:
                return "Sluggard looks more frightening than he really is. Just stand your ground and blast him.";
            case BossName.MouthMeat:
                return "Some meat beast split apart when you shoot them. Take your time fighting them.";
            case BossName.MouthMeatSenior:
                return "While fighting powerful meat beasts, be careful to avoid hazards like spikes and Dentatas.";
            case BossName.MetalPatriarch:
                return "Some meat beast can take over robots. I hope they don't find the Metal Patriarchs.";
            default:
                return "Shoot at bosses to kill them.";
        }
    }

    public void AnimatorDie()
    {
        _animator.Play("Die");
    }

    public void AnimatorCast()
    {
        _animator.Play("Cast");
    }

    public void OnHide()
    {
        if(_onHide != null)
        {
            _onHide();
        }

        if (NPCDialogueManager.instance)
        {
            NPCDialogueManager.instance.dialogueScreen.OnHide -= OnHide;
            NPCDialogueManager.instance.dialogueScreen.OnShow -= FadeMusic;
        }

        Destroy(GetComponentInChildren<ButtonTriggerBounds>());
        Destroy(GetComponentInChildren<DialogueTrigger>());

        if (SaveGameManager.activeGame != null)
        {
            SaveGameManager.activeSlot.tutorialSmithVisted = true;
            SaveGameManager.activeGame.tutorialComplete = true;
            SaveGameManager.instance.Save();
        }
    }

    public void OnHideGlitch()
    {
        if (NPCDialogueManager.instance)
        {
            NPCDialogueManager.instance.dialogueScreen.OnHide -= OnHideGlitch;
        }

        Destroy(GetComponentInChildren<ButtonTriggerBounds>());
        Destroy(GetComponentInChildren<DialogueTrigger>());
        _animator.Play("Cast");

        if (SaveGameManager.activeGame != null)
        {
            SaveGameManager.activeSlot.tutorialSmithVisted = true;
            SaveGameManager.activeGame.tutorialComplete = true;
            SaveGameManager.instance.Save();
        }

        if (LayoutManager.instance)
        {
            LayoutManager.instance.GlitchToEnvironmentStart(EnvironmentType.Glitch);
        }
    }

    public void OnHideSpooky()
    {
        if (NPCDialogueManager.instance)
        {
            NPCDialogueManager.instance.dialogueScreen.OnHide -= OnHideSpooky;
        }

        Destroy(GetComponentInChildren<ButtonTriggerBounds>());
        Destroy(GetComponentInChildren<DialogueTrigger>());
        _animator.Play("Cast");

        if (SaveGameManager.activeGame != null)
        {
            SaveGameManager.activeSlot.tutorialSmithVisted = true;
            SaveGameManager.activeGame.tutorialComplete = true;
            SaveGameManager.instance.Save();
        }

        SeedHelper.StartSpookyMode(false);
    }

    private void FadeMusic()
    {
        MusicController.instance.StartFadeOut(0.5f);
    }

    private void OnDestroy()
    {
        if (NPCDialogueManager.instance)
        {
            NPCDialogueManager.instance.dialogueScreen.OnHide -= OnHideGlitch;
            NPCDialogueManager.instance.dialogueScreen.OnHide -= OnHide;
            NPCDialogueManager.instance.dialogueScreen.OnShow -= FadeMusic;
        }
    }

    public IEnumerator Poochie()
    {
        var renderer = GetComponentInChildren<SpriteRenderer>();
        var audioSource = GetComponentInChildren<AudioSource>();

        if (slideWhistle) audioSource.PlayOneShot(slideWhistle);
        
        while(renderer.isVisible)
        {
            if (poochieFlap) audioSource.PlayOneShot(poochieFlap);
            transform.position += Vector3.up * Random.Range(2,4f);
            transform.rotation = transform.rotation * Quaternion.Euler(0, 0, Random.Range(-45,45));
            yield return new WaitForSeconds(Random.Range(0.3f, 0.5f));            
        }

        Destroy(gameObject);
    }
}
