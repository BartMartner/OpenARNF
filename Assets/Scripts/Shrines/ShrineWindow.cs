using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Random = UnityEngine.Random;

public class ShrineWindow : MonoBehaviour
{
    public RectTransform window;
    public ScrapDonator[] scrapDonators;
    public RectTransform currencyTracker;
    public GameObject giveButton;

    private int _selectedOption;
    private ShrineInfo _shrineInfo;
    private bool _postInitializeDelay;
    private bool _donatingScrap;

    public bool donatingScrap
    {
        get { return _donatingScrap; }
    }

    private ItemPrice _price;
    private Player _player;

    private float _lastOptionChangeDelay;
    private float _optionChangeDelay;

    public ShrineEffects[] badEffects = new ShrineEffects[]
    {
        new ShrineEffects //Decrease all stats by 1
        {
            outcome = (p) => { return Outcome.Negative; },
            immediateEffect = (player) =>
            {
                foreach (var stat in Constants.allStatModules)
                {
                    player.ModifyMinorItemCollection(stat, -1, false);
                }
                if(player.onCollectItem != null) { player.onCollectItem(); }

                return  "I curse you with weakness in all ways, Fight Smith " + SaveGameManager.fightNumber + ".";
            },
        },

        ShrineEffects.ModifyRandomStat(-1),
        ShrineEffects.AddTempMod(PlayerStatType.Attack, -1),
        ShrineEffects.AddTempMod(PlayerStatType.Damage, -1),
        ShrineEffects.AddTempMod(PlayerStatType.Speed, -1),
        ShrineEffects.AddTempMod(PlayerStatType.ShotSpeed, -1),
        ShrineEffects.healthToOne,
        ShrineEffects.energyToOne,
        ShrineEffects.nothing,
    };

    public ShrineEffects[] neutralEffects = new ShrineEffects[]
    {
        ShrineEffects.shuffleStats,
        ShrineEffects.rerollNonTraversalArtifact,
        ShrineEffects.redistributeArchaicScrap,
        ShrineEffects.statsToScap,
    };

    public ShrineEffects[] goodEffects = new ShrineEffects[]
    {
        ShrineEffects.fullEnergy,
        ShrineEffects.fullHealth,
        ShrineEffects.ModifyRandomStat(1),
        ShrineEffects.ModifyRandomStat(2),
        ShrineEffects.ModifyRandomStat(3),
        ShrineEffects.AddTempMod(PlayerStatType.Attack, 0.1f),
        ShrineEffects.AddTempMod(PlayerStatType.Damage, 0.1f),
        ShrineEffects.AddTempMod(PlayerStatType.Speed, 0.1f),
        ShrineEffects.AddTempMod(PlayerStatType.ShotSpeed, 0.1f),
        ShrineEffects.AddTempMod(PlayerStatType.ShotSize, 0.1f),
        ShrineEffects.increaseAllStats,
    };

    public void OnEnable()
    {
        _player = PlayerManager.instance.player1;
    }

    public void Update()
    {
        if (NPCDialogueManager.instance.skipUpdate) return;

        if (!_donatingScrap)
        {
            if (_optionChangeDelay > 0)
            {
                _optionChangeDelay -= Time.deltaTime;
            }

            if (_player.controller.GetButtonDown("UIDown"))
            {
                _selectedOption = (_selectedOption + 1) % scrapDonators.Length;
                UISounds.instance.OptionChange();
            }
            else if (_player.controller.GetButtonDown("UIUp"))
            {
                _selectedOption = (_selectedOption - 1);
                if (_selectedOption < 0)
                {
                    _selectedOption += scrapDonators.Length;
                }
                UISounds.instance.OptionChange();
            }

            var selectedScrapType = scrapDonators[_selectedOption].scrapType;
            var amount = _price[selectedScrapType];
            var maxScrap = _player.GetScrap(selectedScrapType);
            if (amount > 0 && GetHorizontalButton("UILeft"))
            {
                amount--;
                UISounds.instance.OptionChange();
            }
            else if (amount < maxScrap && GetHorizontalButton("UIRight"))
            {
                amount++;
                UISounds.instance.OptionChange();
            }
            _price[selectedScrapType] = amount;

            if (_price != ItemPrice.zero)
            {
                giveButton.SetActive(true);
                if (_player.controller.GetButtonDown("UISubmit") && !_postInitializeDelay)
                {
                    TryDonating();
                }
            }
            else
            {
                giveButton.SetActive(false);
            }
        }

        for (int i = 0; i < scrapDonators.Length; i++)
        {
            if (scrapDonators[i].isActiveAndEnabled) { scrapDonators[i].Refresh(i == _selectedOption, _price); }
        }
    }

    public bool GetHorizontalButton(string button)
    {
        if(_player.controller.GetButtonDown(button))
        {
            _optionChangeDelay = 0.5f;
            _lastOptionChangeDelay = 0.5f;
            return true;
        }
        else if(_optionChangeDelay <= 0 && _player.controller.GetButton(button))
        {
            _optionChangeDelay = Mathf.Clamp(_lastOptionChangeDelay * 0.65f, 0.05f, 0.5f);
            _lastOptionChangeDelay = _optionChangeDelay;
            return true;
        }
        else
        {
            return false;
        }
    }

    public void TryDonating()
    {
        if (_price.CanAfford(_player))
        {
            UISounds.instance.Confirm();
            StartCoroutine(DonateScrap());
        }
        else
        {
            NPCDialogueManager.instance.dialogueScreen.SetDialogue("Error: You don't have enough scrap");
            UISounds.instance.Cancel();
        }
    }

    public IEnumerator DonateScrap()
    {
        _donatingScrap = true;

        //Determine Positive/Neutral/Negative Effect
        ShrineEffects effect;
        if (!_shrineInfo.effects.TryGetValue(_price, out effect))
        {
            var holyNumber = _shrineInfo.holyNumber;
            var totalSpecialScrap = _price.redScrapCost + _price.greenScrapCost + _price.blueScrapCost;

            if (_price.grayScrapCost == holyNumber && totalSpecialScrap > 0 && totalSpecialScrap <= 3 && _shrineInfo.associatedMinorItem != MinorItemType.None)
            {
                effect = ShrineEffects.ModifyStat(Mathf.Clamp(totalSpecialScrap, 1, 3), _shrineInfo.associatedMinorItem);
            }
            else if(_price.grayScrapCost != 0 && _price.grayScrapCost % holyNumber == 0 && totalSpecialScrap > 0 && _shrineInfo.itemPool.Count > 0)
            {
                effect = new ShrineEffects() { possibleItemRewards = new List<MajorItem>(_shrineInfo.itemPool) };
            }
            else
            {
                var roll = Random.Range(0, 0.666f);

                Debug.Log("original roll");

                roll += (_price.grayScrapCost / 99f) * 0.25f;
                roll += (_price.redScrapCost / 4) * 0.25f;
                roll += (_price.greenScrapCost / 4) * 0.25f;
                roll += (_price.blueScrapCost / 4) * 0.25f;

                var holy = (_price.grayScrapCost != 0 && _price.grayScrapCost % holyNumber == 0);
                var unholy = _price.grayScrapCost % 4 == 0;

                if (totalSpecialScrap != 0 && totalSpecialScrap % holyNumber == 0)
                {
                    roll = 1;
                }
                else if (holy)
                {
                    roll = Mathf.Clamp(roll, 0.334f, 1);
                }

                if ((totalSpecialScrap == 0 && _price.grayScrapCost <= 2)  || (unholy && !holy))
                {
                    effect = badEffects[Random.Range(0, badEffects.Length)];
                }
                else if (roll < 0.666f)
                {
                    effect = neutralEffects[Random.Range(0, neutralEffects.Length)];
                }
                else
                {
                    effect = goodEffects[Random.Range(0, goodEffects.Length)];
                }
            }
        }

        MajorItem itemReward = MajorItem.None;
        if (effect.possibleItemRewards != null)
        {
            var possibleItems = CleanItemPool(effect.possibleItemRewards);

            //Use shrine's pool as a backup
            if (possibleItems.Count == 0) { possibleItems = CleanItemPool(_shrineInfo.itemPool); }

            itemReward = (possibleItems.Count > 0) ? possibleItems[Random.Range(0, possibleItems.Count)] : MajorItem.None;
        }

        //Set Positive/Neutral/Negative Dialogue
        var outcome = Outcome.Neutral;
        if (_price == ItemPrice.zero)
        {
            outcome = Outcome.Negative;
            NPCDialogueManager.instance.dialogueScreen.SetDialogue(_shrineInfo.offerNothing);
        }
        else
        {
            outcome = effect.outcome == null ? Outcome.Neutral : effect.outcome(_player);
            if (itemReward != MajorItem.None) outcome = Outcome.Positive;

            switch (outcome)
            {
                case Outcome.Positive:
                    NPCDialogueManager.instance.dialogueScreen.SetDialogue(_shrineInfo.positiveEffect);
                    break;
                case Outcome.Neutral:
                    NPCDialogueManager.instance.dialogueScreen.SetDialogue(_shrineInfo.neutralEffect);
                    break;
                case Outcome.Negative:
                    NPCDialogueManager.instance.dialogueScreen.SetDialogue(_shrineInfo.negativeEffect);
                    break;
            }
        }

        //Save Shrine Use
        if(SaveGameManager.activeGame != null)
        {
            var shrinesUsed = SaveGameManager.activeGame.shrinesUsed;
            if (!shrinesUsed.ContainsKey(_shrineInfo.type))
            {
                shrinesUsed.Add(_shrineInfo.type, 0);
            }

            shrinesUsed[_shrineInfo.type]++;

            _shrineInfo.timesUsed = shrinesUsed[_shrineInfo.type];
        }
        else
        {
            _shrineInfo.timesUsed++;
        }

        if (_shrineInfo.timesUsed <= _shrineInfo.lightAnimators.Length)
        {
            _shrineInfo.lightAnimators[_shrineInfo.timesUsed-1].SetTrigger("TurnOff");
        }

        //Start currency tracker bounce
        var timer = 0f;
        while (timer < 0.25f)
        {
            timer += Time.unscaledDeltaTime;
            currencyTracker.localScale = Vector3.Lerp(Vector3.one, Vector3.one * 1.1f, timer / 0.35f);
            yield return null;
        }

        UISounds.instance.Purchase();
        _price.chargePlayer(_player);

        //TrackDonated Scrap
        var activeSlot = SaveGameManager.activeSlot;
        if (activeSlot != null)
        {
            foreach (CurrencyType scrapType in Enum.GetValues(typeof(CurrencyType)))
            {
                if (!activeSlot.scrapDonated.ContainsKey(scrapType)) { activeSlot.scrapDonated.Add(scrapType, 0); }
                if (activeSlot.scrapDonated[scrapType] < long.MaxValue) { activeSlot.scrapDonated[scrapType] += _price[scrapType]; }
            }
        }

        timer = 0f;
        while (timer < 0.5f)
        {
            timer += Time.unscaledDeltaTime;
            currencyTracker.localScale = Vector3.Lerp(Vector3.one * 1.1f, Vector3.one, timer / 0.35f);
            yield return null;
        }
        //End currency tracker bounce

        currencyTracker.localScale = Vector3.one;

        window.gameObject.SetActive(false);

        if (_player.state != DamageableState.Alive)
        {
            NPCDialogueManager.instance.dialogueScreen.Hide();
            yield break;
        }

        var animationTime = 1f;
        var shrineAnimator = _shrineInfo.animator;

        if (shrineAnimator)
        {
            shrineAnimator.Play("EffectStart");
            yield return null;
            animationTime = shrineAnimator.GetCurrentAnimatorStateInfo(0).length - Time.deltaTime;
        }

        yield return new WaitForSeconds(animationTime);

        //effect start animation complete

        string result = effect.immediateEffect == null ? string.Empty : effect.immediateEffect(_player);

        if(result == string.Empty && effect.possibleItemRewards != null)
        {
            result = itemReward != MajorItem.None ? "May this relic aid the mission." : "I have nothing to give that you do not already have.";
        }

        if (outcome == Outcome.Negative)
        {
            _player.PlayOneShot(_player.hurtSound);
            _player.StartFlash(5, 0.2f, Constants.damageFlashColor, 0.5f, false);
            if (effect.teleportationEffect == null) { _player.fatigued = true; }
        }

        NPCDialogueManager.instance.dialogueScreen.SetDialogue(result);

        if(itemReward != MajorItem.None)
        {
            yield return new WaitForSeconds(2f);
            MajorItemInfo itemInfo = new MajorItemInfo() { fullName = "Error", description = "Error" };
            ItemManager.items.TryGetValue(itemReward, out itemInfo);

            if (itemInfo.isActivatedItem && _player.activatedItem)
            {
                Vector3 openSpot = _player.transform.position;
                var activeItems = FindObjectsOfType<ActivatedItemPickUp>().ToList();
                foreach (var t in _shrineInfo.activatedItemSpots)
                {
                    if (!activeItems.Find((i) => i.transform.position == t.position))
                    {
                        openSpot = t.position;
                        break;
                    }
                }
                var currentRoom = LayoutManager.CurrentRoom ?? FindObjectOfType<Room>();
                _player.DropEquippedActiveItem(openSpot, currentRoom, false);
            }

            if (ItemCollectScreen.instance) { ItemCollectScreen.instance.Show(itemInfo); }
            _player.CollectMajorItem(itemReward);
            yield return new WaitForSeconds(0.25f);
            while (ItemCollectScreen.instance.visible) { yield return null; }
        }
        else
        {
            var buttonsPressed = 0;
            while (timer < 2)
            {
                timer += Time.unscaledDeltaTime;
                if (_player.controller.GetAnyButtonDown()) { buttonsPressed++; }
                yield return null;
            }
            while (!_player.controller.GetAnyButtonDown() && buttonsPressed < 5) { yield return null; }
        }

        yield return null; //wait to see if an achievement was triggered by item collection or something else;
        while (AchievementScreen.instance.visible) { yield return null; }

        if (shrineAnimator)
        {
            var state = _shrineInfo.timesUsed >= 3 ? "EffectEndFinal" : "EffectEnd";
            shrineAnimator.Play(state);
            yield return null; // wait for play
            animationTime = shrineAnimator.GetCurrentAnimatorStateInfo(0).length;
        }
        else
        {
            animationTime = 1f;
        }

        yield return new WaitForSeconds(animationTime); //wait for animation and to avoid input to close the ItemCollectScreen from buying items or closing the dialogue

        bool achievement = false;
        var scrapDonated = activeSlot != null ? activeSlot.scrapDonated[CurrencyType.Gray] : 0;

        if (scrapDonated >= 27 && AchievementManager.instance.TryEarnAchievement(AchievementID.LilTyr)) { achievement = true; }
        if (scrapDonated >= 125 && AchievementManager.instance.TryEarnAchievement(AchievementID.LilZurvan)) { achievement = true; }
        if (scrapDonated >= 216 && AchievementManager.instance.TryEarnAchievement(AchievementID.LilPhaestus)) { achievement = true; }
        if (scrapDonated >= 343 && AchievementManager.instance.TryEarnAchievement(AchievementID.LilWadjet)) { achievement = true; }
        if (scrapDonated >= 512 && AchievementManager.instance.TryEarnAchievement(AchievementID.LilBuluc)) { achievement = true; }
        if (scrapDonated >= 729 && AchievementManager.instance.TryEarnAchievement(AchievementID.LilOrphy)) { achievement = true; }

        if (achievement)
        {
            yield return null;
            while (AchievementScreen.instance.visible)
            {
                yield return null;
            }
        }

        _donatingScrap = false;
        NPCDialogueManager.instance.dialogueScreen.SetDialogue("Do you have more to offer?");

        //figure out teleportation
        if (effect.teleportationEffect != null)
        {
            NPCDialogueManager.instance.HideDialogueScreen();
            _shrineInfo.triggerBounds.enabled = false;
            effect.teleportationEffect(_player);
        }
        else if (_shrineInfo.timesUsed >= 3)
        {
            NPCDialogueManager.instance.HideDialogueScreen();
            _shrineInfo.triggerBounds.enabled = false;
        }
        else
        {
            Initialize(_shrineInfo);
        }

        if (SaveGameManager.instance) SaveGameManager.instance.Save();
    }

    public List<MajorItem> CleanItemPool(List<MajorItem> items)
    {
        var cleanedPool = new List<MajorItem>();
        for (int i = 0; i < items.Count; i++)
        {
            var item = items[i];
            if (PlayerManager.instance.itemsCollected.Contains(item)) { continue; }

            if (SaveGameManager.activeSlot != null)
            {
                var reqAchieve = ItemManager.items[item].requiredAchievement;
                if (reqAchieve != AchievementID.None && !SaveGameManager.activeSlot.achievements.Contains(reqAchieve)) { continue; }
            }

            if (SaveGameManager.activeGame != null && SaveGameManager.activeGame.layout.allNonTraversalItemsAdded.Contains(item)) { continue; }

            cleanedPool.Add(item);
        }

        return cleanedPool;
    }

    public void Initialize(ShrineInfo shrineInfo)
    {
        _price = new ItemPrice(0, 0, 0, 0);
        _shrineInfo = shrineInfo;
        _shrineInfo.Initialize();
        _selectedOption = 0;

        window.gameObject.SetActive(true);

        for (int i = 0; i < scrapDonators.Length; i++)
        {
            if (scrapDonators[i].isActiveAndEnabled) { scrapDonators[i].Refresh(i == _selectedOption, _price); }
        }

        StartCoroutine(PostInitializeDelay());
    }

    private IEnumerator PostInitializeDelay()
    {
        _postInitializeDelay = true;
        yield return new WaitForSeconds(0.1f);
        _postInitializeDelay = false;
    }
}

