using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using Random = UnityEngine.Random;

public class ShopWindow : MonoBehaviour
{
    public RectTransform window;
    public ShopOption[] shopOptions;
    public RectTransform currencyTracker;
    public Text selectedText;
    public Text forgeText;

    private Player _activePlayer;
    private int _selectedOption;
    private ShopInfo _shopInfo;
    private int _activeShopOptions;
    private bool _purchasingItem;
    private bool _postInitializeDelay;

    private List<string> _expandedDialogue;
    private int _currentDialogue;

    private string[] _traitorSlang = new string[]
    {
        "Doohickey",
        "Some Gismo",
        "Something",
        "A Clinkclank",
        "Thingamabob",
        "A Contraption",
        "Some Device",
        "Maybe A Gun?",
        "Gadget",
        "Thingamajig",
        "Whatchamacallit",
        "Dojigger",
        "A Thing",
        "You'll see",
        "Techy Bits",
        "Eh",
        "Not sure",
        "??????",
        "Probably Cool",
        "Worth It",
    };

    private string[] _traitorDesciptions = new string[]
    {
        "A great item. Absolutely useful!",
        "A must have!",
        "Just what you've been needing, I promise.",
        "Wouldn't take D1K1 but a second to whip up",
        "A Phenomenal purchase. Truely breath taking. Really.",
        "You won't beleive this item. Simply wonderful.",
        "Top teir tech, you have my word.",
        "You'd be a fool not to buy this!",
        "This one is special. I've been saving it just for you!",
        "Best. Item. In. The. World.",
        "Unbelievable item. You gotta have it!",
        "Trade me some scrap for this baby, you won't regret it!",
        "You can't handle how awesome this one is!",
        "Get this one and I'll throw in my mix tape. It's fire!",
        "Best gun and/or orb and/or other thing scrap can buy!",
        "I gotta be desperate to let it go at this price",
        "Premium!",
        "This is the last one of these I've got!",
        "I offer a discount for cool dudes like you!",
        "Not sure you can handle this item. It's only for experts.",
        "Top quality artifact!",
        "I see you're a robot of taste!",
    };

    private static readonly Dictionary<ShopType, string[]> dialogue = new Dictionary<ShopType, string[]>()
    {
        {ShopType.Artificer, new string[]
            {
                "The war times were so long ago. They're practically myth.",
                "I find the meat life fascinating. Who knows exactly how it works. Probably chemicals?",
                "I lived in the forest slums before all this. The avatars said I wasn't pious enough for the city.",
                "Yes. I am quite tall. Thank you.",
                "The shrines are fickle. Gift them gray scrap in accordance to their holy number.",
                "Leave? It seems quite safe here.",
                "No, I don't actually have a surname.",
                "Did you know the god's avatars still have cities in the sky? At least they did...",
                "I'm hiding out. I don't think I'd get another shell if I lost this one.",
                "All of the conduits and processors in the caves were placed centuries ago. They're a mess.",
                "Is it getting crazy out there? Will things ever return to normal?",
                "The Tutorial Smiths? They're part of some esoteric order. They help out in the slums.",
                "Why is the flesh attacking us? What did we ever do to it?",
            }
        },

        {ShopType.GunSmith, new string[]
            {
                "Praise Zurvan! The Navigator. Five sided sultan. Their holy plan will see us through this.",
                "Praise Tyr! The Peacekeeper. First of the gods whose number is 3.",
                "Praise Hephaestus! The Creator. My weapons are built by their designs.",
                "By the three pillars of Law, Order, and Discipline shall Tyr dispatch these meat fiends.",
                "Aside from make guns? I like to clean guns. I like to pray.",
                "Our piety wards us from glitched thought.",
                "I don't trust the Tutorial Smiths. They're all weirdo cultists if you ask me.",
                "I once saw a hole in a wall that appeared to go on forever. Best not to think about.",
                "Why don't I fight? I'm fighting with the power of prayer.",
                "I have to confess, I'm not a very good shot myself.",
                "Make sure you take proper care of your guns. They're precious.",
                "Clearly, The Megabeast hates our freedom. Thus we must destroy it!",
                "The Meat Storm is a test from the gods! All that matters is The Mission.",
                "You can never have too many guns. Really. No limit.",
                "In another life I must've been a Fight Smith",
                "The gods left to serve The Mission. One day they will return.",
            }
        },

        {ShopType.OrbSmith, new string[]
            {
                "By the authority of the Hephaestus-Oprphiel accord I grant life to the round ones.",
                "The orbs have minds that are different than ours. It's technical.",
                "Aside from orbs? I don't know. It's mostly just orbs with me.",
                "I had hoped to visit space before all of this.",
                "What if the orbs built their own cities and shrines one day? Wouldn't that be funny?",
                "I've heard rumors of orbs adopting molemen as pets.",
                "Some of the flesh things appear quite round. I don't know what to think about that.",
                "If you don't have the scrap I need now you can always circle back.",
                "These ARE the droids you're looking for!",
                "Be careful around K1K1 models. They have a life of their own.",
                "Hephaestus's holy number is 6. Orphiel's holy number is 9. Nice.",
                "I don't get many Fight Smiths 'round these parts.",
                "The round ones all serve Orphiel, The Visionary",
                "Blasphemy! The Megabeast is a sphere, not an orb. Big difference!",
                "The Tutorial Smiths say a lot of wild stuff. I wouldn't put too much stock in it.",
                "The Artificers? A bunch of bums if you ask me.",
            }
        },

        {ShopType.TheTraitor, new string[]
            {
                "I'm just here for the scrap. I didn't ask for monsters.",
                "The other molemen seem pretty fed up, huh?",
                "I don't have time for mumbo-jumbo. I'm a moleman of science!",
                "My friend? Oh, that's D1K1. He's just wilin' out.",
                "I've only seen a few other Fight Smiths make it this far.",
                "No offense, but this city has been around longer than any robot city.",
                "Go D1K1! Go D1K1! Go!",
                "Yes, of course this is my real hair!",
                "D1K1 actually does most of the crafting. I'm here for moral support.",
                "I want to get some arcade machines down here. What do you think?",
                "The robots took everything by force long ago, but now the free market has made things fair.",
                "What am I doing with the surplus scrap? Don't you worry about that.",
                "The White Wyrms pre date the Mega Beast. They've been here as long as I can remember.",
            }
        },
    };

    private List<string> chosenTraitorSlang = new List<string>();

    public bool purchasingItem
    {
        get { return _purchasingItem; }
    }

    public void Awake()
    {
        var options = new List<string>(_traitorSlang);

        for (int i = 0; i < 3; i++)
        {
            var pick = options[Random.Range(0, options.Count)];
            options.Remove(pick);
            chosenTraitorSlang.Add(pick);
        }
    }

    public void Update()
    {
        if (NPCDialogueManager.instance.skipUpdate) return;
        if (!_activePlayer) return;

        if (_activeShopOptions > 0 && !_purchasingItem)
        {
            if (_activeShopOptions > 1)
            {
                if (_activePlayer.controller.GetButtonDown("UIDown"))
                {
                    _selectedOption = (_selectedOption + 1) % _activeShopOptions;
                    UISounds.instance.OptionChange();
                    RefreshSelected();
                }
                else if (_activePlayer.controller.GetButtonDown("UIUp"))
                {
                    _selectedOption = (_selectedOption - 1);
                    if (_selectedOption < 0)
                    {
                        _selectedOption += _activeShopOptions;
                    }
                    UISounds.instance.OptionChange();
                    RefreshSelected();
                }
            }
            
            if (_activePlayer.controller.GetButtonDown("UISubmit") && !_postInitializeDelay)
            {
                SelectOption();
            }
        }
    }

    public void SelectOption()
    {
        var shopItem = shopOptions[_selectedOption] as ShopItem;
        if (shopItem)
        {
            var item = shopItem.item;
            if (CanAfford(_activePlayer, item))
            {
                UISounds.instance.Confirm();
                StartCoroutine(PurchaseItem(item));
            }
            else
            {
                NPCDialogueManager.instance.dialogueScreen.SetDialogue(_shopInfo.notEnoughDialogue);
                UISounds.instance.Cancel();
            }
        }
        else
        {
            UISounds.instance.Confirm();
            var d = _expandedDialogue[_currentDialogue];
            _currentDialogue = (_currentDialogue + 1) % _expandedDialogue.Count;            
            NPCDialogueManager.instance.dialogueScreen.SetDialogue(d);
        }
    }

    public IEnumerator PurchaseItem(MajorItemInfo itemInfo)
    {
        _purchasingItem = true;
        _activePlayer.uxInvincible = true;
        RefreshSelected();

        NPCDialogueManager.instance.dialogueScreen.SetDialogue(_shopInfo.purchasingDialogue);

        var timer = 0f;
        while (timer < 0.25f)
        {
            timer += Time.unscaledDeltaTime;
            currencyTracker.localScale = Vector3.Lerp(Vector3.one, Vector3.one * 1.1f, timer / 0.35f);
            yield return null;
        }

        UISounds.instance.Purchase();

        var halfOff = _activePlayer.itemsPossessed.Contains(MajorItem.ChromeDome);
        var itemPrice = new ItemPrice(itemInfo, _shopInfo, halfOff);
        itemPrice.chargePlayer(_activePlayer);

        var slot = SaveGameManager.activeSlot;
        var game = SaveGameManager.activeGame;
        if (slot != null && (game == null || game.allowAchievements))
        {
            foreach (CurrencyType scrapType in Enum.GetValues(typeof(CurrencyType)))
            {
                if (!slot.scrapSpent.ContainsKey(scrapType)) { slot.scrapSpent.Add(scrapType, 0); }
                if (slot.scrapSpent[scrapType] < long.MaxValue) { slot.scrapSpent[scrapType] += itemPrice[scrapType]; }
                if (slot.totalItemsPurchased < uint.MaxValue) { slot.totalItemsPurchased++; }
            }
        }

        timer = 0f;
        while (timer < 0.5f)
        {
            timer += Time.unscaledDeltaTime;
            currencyTracker.localScale = Vector3.Lerp(Vector3.one * 1.1f, Vector3.one, timer / 0.35f);
            yield return null;
        }

        currencyTracker.localScale = Vector3.one;

        window.gameObject.SetActive(false);

        if (_shopInfo.npcAnimator) { _shopInfo.npcAnimator.SetTrigger("StartForge"); }

        yield return new WaitForSeconds(1f);

        if (itemInfo.isActivatedItem && _activePlayer.activatedItem)
        {
            Vector3 openSpot = _activePlayer.transform.position;
            var activeItems = FindObjectsOfType<ActivatedItemPickUp>().ToList();
            foreach (var t in _shopInfo.activatedItemSpots)
            {
                if (!activeItems.Find((i) => i.transform.position == t.position))
                {
                    openSpot = t.position;
                    break;
                }
            }
            var currentRoom = LayoutManager.CurrentRoom ?? FindObjectOfType<Room>();
            _activePlayer.DropEquippedActiveItem(openSpot, currentRoom, false);
        }

        ItemCollectScreen.instance.Show(itemInfo);
        _activePlayer.CollectMajorItem(itemInfo.type);

        while(ItemCollectScreen.instance.visible)
        {
            yield return null;
        }

        if (_shopInfo.npcAnimator)
        {
            _shopInfo.npcAnimator.SetTrigger("EndForge");
        }

        yield return new WaitForSeconds(0.5f); //to avoid input to close the ItemCollectScreen from buying items or closing the dialogue

        bool achievement = false;
        long red, green, blue;
        if (slot != null && slot.scrapSpent.TryGetValue(CurrencyType.Red, out red) && red >= 3 && slot.scrapSpent.TryGetValue(CurrencyType.Green, out green) && green >= 3 &&
            slot.scrapSpent.TryGetValue(CurrencyType.Blue, out blue) && blue >= 3 && AchievementManager.instance.TryEarnAchievement(AchievementID.TheTraitor))
        {
            achievement = true;
        }

        if(LayoutManager.instance && LayoutManager.instance.ShopsOutOfStock() >= 3 && AchievementManager.instance.TryEarnAchievement(AchievementID.AllyBot))
        {
            achievement = true;
        }

        if(achievement)
        {
            yield return null;
            while (AchievementScreen.instance.visible)
            {
                yield return null;
            }
        }

        _activePlayer.uxInvincible = false;

        yield return new WaitForSeconds(0.5f); //to avoid input to close the ItemCollectScreen from buying items or closing the dialogue
        _purchasingItem = false;

        if (_activePlayer.state != DamageableState.Alive)
        {
            NPCDialogueManager.instance.dialogueScreen.Hide();
            yield break;
        }

        NPCDialogueManager.instance.dialogueScreen.SetDialogue(_shopInfo.anythingElseDialogue);
        window.gameObject.SetActive(true);
        Initialize(_shopInfo, _activePlayer);
    }

    public bool CanAfford(Player player, MajorItemInfo item)
    {
        if (!player) return false;
        var halfOff = player.itemsPossessed.Contains(MajorItem.ChromeDome);
        var itemPrice = new ItemPrice(item, _shopInfo, halfOff);
        return itemPrice.CanAfford(player);
    }

    public void Initialize(ShopInfo shopInfo, Player player)
    {
        _purchasingItem = false;
        _activePlayer = player;
        _shopInfo = shopInfo;
        _selectedOption = 0;
        _expandedDialogue = dialogue[_shopInfo.shopType].ToList();
        _expandedDialogue.Shuffle();
        _currentDialogue = 0;

        var itemInfos = new List<MajorItemInfo>();
        foreach (var item in _shopInfo.items)
        {
            MajorItemInfo info;
            if (ItemManager.items.TryGetValue(item, out info) && 
                !PlayerManager.instance.itemsCollected.Contains(item) &&
                itemInfos.Count < 3)
            {
                itemInfos.Add(info);
            }
        }

        _activeShopOptions = 0;

        for (int i = 0; i < shopOptions.Length; i++)
        {
            var shopItem = shopOptions[i] as ShopItem;
            if(!shopItem) 
            {
                _activeShopOptions++;
                shopOptions[i].gameObject.SetActive(true);
            }
            else
            {
                var index = i - 1;
                if (index < itemInfos.Count)
                {
                    _activeShopOptions++;
                    var info = itemInfos[index];
                    shopItem.AssignItem(info, _shopInfo);
                    shopItem.gameObject.SetActive(true);
                }
                else
                {
                    shopItem.gameObject.SetActive(false);
                }
            }
        }

        if(_activeShopOptions > 1)
        {
            window.gameObject.SetActive(true);
            RefreshSelected();
        }
        else
        {
            NPCDialogueManager.instance.dialogueScreen.SetDialogue(_shopInfo.emptyDialogue);
            NPCDialogueManager.instance.dialogueScreen.exitBox.gameObject.SetActive(true);
            window.gameObject.SetActive(false);

            var activeGame = SaveGameManager.activeGame;
            if (activeGame != null && !activeGame.shopsEmptied.Contains(LayoutManager.instance.currentRoom.roomAbstract.roomID))
            {
                activeGame.shopsEmptied.Add(LayoutManager.instance.currentRoom.roomAbstract.roomID);
            }
        }

        StartCoroutine(PostInitializeDelay());
    }

    private IEnumerator PostInitializeDelay()
    {
        _postInitializeDelay = true;
        yield return new WaitForSeconds(0.1f);
        _postInitializeDelay = false;
    }

    public void RefreshSelected()
    {
        for (int i = 0; i < shopOptions.Length; i++)
        {
            if (shopOptions[i].isActiveAndEnabled)
            {
                shopOptions[i].ToggleSelected(i == _selectedOption && !_purchasingItem);
            }
        }

        forgeText.text = _selectedOption == 0 ? "Talk" : "Forge";

        var shopItem = shopOptions[_selectedOption] as ShopItem;
        if (shopItem)
        {
            if(_shopInfo.shopType == ShopType.TheTraitor)
            {
                selectedText.text = chosenTraitorSlang[_selectedOption-1];
                NPCDialogueManager.instance.dialogueScreen.SetDialogue(_traitorDesciptions[Random.Range(0, _traitorDesciptions.Length)]);
            }
            else
            {
                selectedText.text = shopItem.item.fullName;
                var description = string.IsNullOrEmpty(shopItem.item.itemPageDescription) ? shopItem.item.description : shopItem.item.itemPageDescription;
                NPCDialogueManager.instance.dialogueScreen.SetDialogue(description);
            }
        }
        else
        {
            selectedText.text = string.Empty;
            NPCDialogueManager.instance.dialogueScreen.SetDialogue(_shopInfo.dialogue);
        }
    }
}

