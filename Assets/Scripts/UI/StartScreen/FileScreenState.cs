using UnityEngine;
using System.Collections;

public class FileScreenState : ScreenState
{
    public TitleScreenState titleScreenState;
    public GameStartState gameStartState;
    public FileSlotUI[] fileSlots;
    public CanvasGroup deleteSaveButton;
    public GameObject deleteSavePrompt;
    public int highlightedSlot;
    public bool deleteSaveHighlighted;
    public bool deleteSaveMode;
    public GameObject deleteConfirmPopup;

    private bool _skipUpdate;

    public override void OnEnable()
    {
        for (int i = 0; i < fileSlots.Length; i++)
        {
            fileSlots[i].SetSlotData(i);
        }

        base.OnEnable();
    }

    public override void ReadyUpdate()
    {
        if (_skipUpdate) return;

        if(!deleteSaveMode)
        {            
            if (_controller.GetButtonDown("UIUp") || _controller.GetButtonDown("UIDown"))
            {
                UISounds.instance.OptionChange();
                deleteSaveHighlighted = !deleteSaveHighlighted;
            }
        }

        if (!deleteSaveHighlighted)
        {
            if (_controller.GetButtonDown("UIRight"))
            {
                UISounds.instance.OptionChange();
                highlightedSlot = (highlightedSlot + 1) % fileSlots.Length;
            }
            else if (_controller.GetButtonDown("UILeft"))
            {
                UISounds.instance.OptionChange();
                highlightedSlot = (highlightedSlot - 1);
                if (highlightedSlot < 0)
                {
                    highlightedSlot += fileSlots.Length;
                }
            }
        }

        RefreshHighlightedSlot();

        if (deleteSaveMode)
        {
            if (_controller.GetButtonDown("UISubmit"))
            {
                UISounds.instance.Confirm();
                DeleteConfirm();
            }

            if (_controller.GetButtonDown("UICancel"))
            {
                UISounds.instance.Cancel();
                DeleteSaveModeCancel();
            }
        }
        else
        {
            if (_controller.GetButtonDown("UISubmit"))
            {
                UISounds.instance.Confirm();
                if (deleteSaveHighlighted)
                {
                    DeleteSaveMode();
                }
                else
                {
                    FileSlotSelected(highlightedSlot);
                }
            }

            if (_controller.GetButtonDown("UICancel"))
            {
                UISounds.instance.Cancel();
                GoToState(titleScreenState);
            }
        }
    }

    public void DeleteSaveMode()
    {
        deleteSaveButton.gameObject.SetActive(false);
        deleteSavePrompt.gameObject.SetActive(true);
        deleteSaveMode = true;
        deleteSaveHighlighted = false;
    }

    public void DeleteSaveModeCancel()
    {
        deleteSaveButton.gameObject.SetActive(true);
        deleteSavePrompt.gameObject.SetActive(false);
        deleteSaveMode = false;
        deleteSaveHighlighted = false;
    }

    public void DeleteSave()
    {
        Debug.Log("Deleting Slot " + highlightedSlot);
        SaveGameManager.instance.DeleteSlot(highlightedSlot);
        for (int i = 0; i < fileSlots.Length; i++)
        {
            fileSlots[i].SetSlotData(i);
        }
    }

    public void DeleteConfirm()
    {
        StartCoroutine(WaitForDeleteConfirm());
    }

    public IEnumerator WaitForDeleteConfirm()
    {
        _skipUpdate = true;
        deleteConfirmPopup.gameObject.SetActive(true);

        yield return null;

        bool _finished = false;
        while (!_finished)
        {
            if (_controller.GetButtonDown("UISubmit"))
            {
                _finished = true;
                UISounds.instance.Confirm();
                DeleteSave();
            }
            else if (_controller.GetButtonDown("UICancel"))
            {
                UISounds.instance.Cancel();
                _finished = true;
            }
            yield return false;
        }

        _skipUpdate = false;
        deleteConfirmPopup.gameObject.SetActive(false);
    }


    public void RefreshHighlightedSlot()
    {
        for (int i = 0; i < fileSlots.Length; i++)
        {
            if (deleteSaveHighlighted || i != highlightedSlot)
            {
                fileSlots[i].canvasGroup.alpha = 0.33f;
            }
            else
            {
                fileSlots[i].canvasGroup.alpha = 1;
            }
        }

        deleteSaveButton.alpha = deleteSaveHighlighted ? 1 : 0.33f;
    }

    public void FileSlotSelected(int index)
    {
        Debug.Log("FileSlot " + index + " Selected!");
        gameStartState.SetSelectedSlot(index);
        GoToState(gameStartState);
    }

    protected override IEnumerator GoToStateCoroutine(ScreenState newState)
    {
        yield return StartCoroutine(Hide());

        var activeSlot = SaveGameManager.activeSlot;

        if (activeSlot != null && activeSlot.needAchievementUpdate && newState == gameStartState)
        {
            Debug.Log("Syncing Achievements!");
            activeSlot.needAchievementUpdate = false;

            var megaBeastKills = activeSlot.megaBeastKills;
            ushort coreKills = 0;
            activeSlot.bossKills.TryGetValue(BossName.MegaBeastCore, out coreKills);
            ushort tutSmithKills = 0;
            activeSlot.bossKills.TryGetValue(BossName.GlitchBoss, out tutSmithKills);
            var minVictories = (uint)(megaBeastKills + coreKills + tutSmithKills);

            if (activeSlot.victories < minVictories)
            {
                activeSlot.victories = minVictories;
            }

            if (AchievementManager.instance.CheckForNewAchievements())
            {
                yield return null;
                while (AchievementScreen.instance.visible)
                {
                    yield return new WaitForSeconds(0.5f);
                }
            }
        }

        //AchievementManager.instance.SyncAchievements();

        gameObject.SetActive(false);
        newState.gameObject.SetActive(true);
    }
}
