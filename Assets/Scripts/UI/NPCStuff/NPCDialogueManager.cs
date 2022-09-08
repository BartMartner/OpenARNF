using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NPCDialogueManager : MonoBehaviour
{
    public static NPCDialogueManager instance;
    public NPCDialogueScreen dialogueScreen;
    private Player _player1;

    private bool _skipUdate;
    public bool skipUpdate
    {
        get { return _skipUdate; }
    }

    private bool _dialogueActive;
    public bool dialogueActive
    {
        get { return _dialogueActive; }
    }

    public void Awake()
    {
        if (instance)
        {
            Destroy(gameObject);
            return;
        }
        else
        {
            instance = this;
        }
    }

    public void Start()
    {
        _player1 = PlayerManager.instance.player1;
    }

    private void OnDestroy()
    {
        if (instance == this)
        {
            instance = null;
        }
    }

    public void Update()
    {
        if (_skipUdate) return;

        if (dialogueActive && !dialogueScreen.busy)
        {
            if (dialogueScreen.dialogueQueue.Count > 0 && _player1.controller.GetButtonDown("UISubmit"))
            {
                dialogueScreen.AdvanceQueue();
            }

            if (_player1.controller.GetButtonDown("UICancel"))
            {
                HideDialogueScreen();
            }
        }
    }

    public void ShowShrineScreen(ShrineInfo info)
    {
        _dialogueActive = true;
        _player1.ResetAnimatorAndCollision();
        dialogueScreen.ShowShrine(info);
        StartCoroutine(PostInitializeDelay());
    }

    public void ShowShopScreen(ShopInfo info, Player player)
    {
        _dialogueActive = true;
        player.ResetAnimatorAndCollision();
        dialogueScreen.ShowShop(info, player);
        StartCoroutine(PostInitializeDelay());
    }

    public void ShowDialogueScreen(DialogueInfo info)
    {
        _dialogueActive = true;
        _player1.ResetAnimatorAndCollision();
        dialogueScreen.ShowDialogue(info);
        StartCoroutine(PostInitializeDelay());
    }

    public void HideDialogueScreen(bool silent = false)
    {
        StartCoroutine(WaitFrameHide(silent));
    }

    //If we don't wait for on button up, the player acts based on the input. But if GetButtonUp is used in update, it could hide automatically if the talk to npc button is mapped to B.
    private IEnumerator WaitFrameHide(bool silent)
    {
        _skipUdate = true;
        while (_player1.controller.GetButton("UICancel"))
        {
            yield return null;
        }

        if (!silent) UISounds.instance.Cancel();

        _dialogueActive = false;
        _skipUdate = false;
        dialogueScreen.Hide();
    }

    private IEnumerator PostInitializeDelay()
    {
        _skipUdate = true;
        yield return new WaitForSeconds(0.1f);
        _skipUdate = false;
    }
}

[Serializable]
public class DialogueInfo
{
    public string NPCName;
    [TextArea]    
    public string dialogue;
    public Sprite portrait;
    public GameObject dialoguePrefab;
}

[Serializable]
public class ShopInfo : DialogueInfo
{
    public Animator npcAnimator;
    public ShopType shopType;
    public Transform[] activatedItemSpots;
    public string purchasingDialogue = "As you wish.";
    public string anythingElseDialogue = "Anything else?";
    public string notEnoughDialogue = "You don't have what it takes.";
    public string emptyDialogue = "There's nothing more I can do for you.";
    public List<MajorItem> items;
    public bool hideItemIcons;
    public float grayPriceMod = 1f;
    public float redPriceMod = 1f;
    public float greenPriceMod = 1f;
    public float bluePriceMod = 1f;
}

