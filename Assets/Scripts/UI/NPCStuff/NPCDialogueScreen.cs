using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class NPCDialogueScreen : MonoBehaviour
{
    public const int characterLimit = 100;
    public Image NPCPortrait;
    public Text NPCName;
    public AnimateText dialogue;
    public ShopWindow shopWindow;
    public ShrineWindow shrineWindow;
    public GameObject moreBox;
    public GameObject exitBox;
    public Sprite noPortrait;
    public GameObject loadedPrefab;
    public Action OnShow;
    public Action OnHide;
    public Queue<string> dialogueQueue = new Queue<string>();

    public bool busy
    {
        get { return shopWindow.purchasingItem || shrineWindow.donatingScrap; }
    }

    public void ShowShop(ShopInfo info, Player player)
    {
        gameObject.SetActive(true);
        SetDialogue(info.dialogue);
        NPCName.text = info.NPCName;
        shrineWindow.gameObject.SetActive(false);
        shopWindow.gameObject.SetActive(true);
        exitBox.SetActive(false);
        moreBox.SetActive(false);

        if (info.portrait == null)
        {
            NPCPortrait.sprite = noPortrait;
        }
        else
        {
            NPCPortrait.sprite = info.portrait;
        }

        shopWindow.Initialize(info, player);
    }

    public void ShowShrine(ShrineInfo info)
    {
        gameObject.SetActive(true);
        SetDialogue(info.dialogue);
        NPCName.text = info.NPCName;
        shopWindow.gameObject.SetActive(false);
        shrineWindow.gameObject.SetActive(true);
        exitBox.SetActive(false);
        moreBox.SetActive(false);

        if (info.portrait == null)
        {
            NPCPortrait.sprite = noPortrait;
        }
        else
        {
            NPCPortrait.sprite = info.portrait;
        }

        shrineWindow.Initialize(info);
    }

    public void ShowDialogue(DialogueInfo info)
    {
        gameObject.SetActive(true);
        if (info.dialoguePrefab)
        {
            SetDialoguePrefab(info.dialoguePrefab);
        }
        else
        {
            SetDialogue(info.dialogue);
        }
        NPCName.text = info.NPCName;
        shopWindow.gameObject.SetActive(false);
        shrineWindow.gameObject.SetActive(false);
        exitBox.SetActive(true);

        if(info.portrait == null)
        {
            NPCPortrait.sprite = noPortrait;
        }
        else
        {
            NPCPortrait.sprite = info.portrait;
        }

        if(OnShow != null)
        {
            OnShow.Invoke();
        }
    }

    public void SetDialoguePrefab(GameObject prefab)
    {
        Destroy(loadedPrefab);
        dialogue.gameObject.SetActive(false);
        moreBox.SetActive(false);
        dialogueQueue.Clear();
        loadedPrefab = Instantiate<GameObject>(prefab);
        loadedPrefab.transform.parent = dialogue.transform.parent;
        loadedPrefab.transform.localScale = Vector3.one;
        var rectTransform = loadedPrefab.GetComponent<RectTransform>();
        var dialogueRect = dialogue.GetComponent<RectTransform>();
        rectTransform.pivot = dialogueRect.pivot;
        rectTransform.anchorMax = dialogueRect.anchorMax;
        rectTransform.anchorMin = dialogueRect.anchorMin;
        rectTransform.offsetMax = dialogueRect.offsetMax;
        rectTransform.offsetMin = dialogueRect.offsetMin;
    }

    public void SetDialogue(string text)
    {
        //replace keys
        if (SaveGameManager.activeSlot != null)
        {
            text = text.Replace("[FightNumber]", SaveGameManager.activeSlot.totalDeaths.ToString());
        }

        Destroy(loadedPrefab);
        dialogueQueue.Clear();
        dialogue.gameObject.SetActive(true);
        if (text.Length < characterLimit)
        {
            dialogue.SetText(text, Mathf.Clamp(text.Length * 0.03f, 0, 2));
            moreBox.SetActive(false);
        }
        else
        {
            var words = text.Split(' ');
            var currentText = words[0];
            if (words.Length > 1)
            {
                for (int i = 1; i < words.Length; i++)
                {
                    var word = words[i];
                    if(word == "[break]")
                    {
                        dialogueQueue.Enqueue(currentText);
                        currentText = string.Empty;
                        continue;
                    }

                    if (currentText.Length + word.Length + 1 < characterLimit)
                    {
                        if(currentText != string.Empty) { currentText += ' '; }
                        currentText += word;
                    }
                    else
                    {
                        dialogueQueue.Enqueue(currentText);
                        currentText = word;
                    }
                }
            }

            //Enque last one
            dialogueQueue.Enqueue(currentText);

            var t = dialogueQueue.Dequeue();
            dialogue.SetText(t, Mathf.Clamp(t.Length * 0.03f, 0, 2));
            moreBox.SetActive(true);
        }
    }

    public void AdvanceQueue()
    {
        if (dialogueQueue.Count > 0)
        {
            var t = dialogueQueue.Dequeue();
            dialogue.SetText(t, Mathf.Clamp(t.Length * 0.03f, 0, 2));
        }

        moreBox.SetActive(dialogueQueue.Count > 0);
    }

    public void Hide()
    {
        Destroy(loadedPrefab);
        gameObject.SetActive(false);
        dialogueQueue.Clear();
        dialogue.ClearText();

        if (OnHide != null)
        {
            OnHide();
        }
    }
}
