using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShrineTriggerBounds : ButtonTriggerBounds, IRepairable
{
    public int priority;
    public int m_priority
    {
        get { return priority; }
        set { priority = value; }
    }

    public ShrineInfo shrineInfo;

    private bool _repairing;

    protected override void Awake()
    {
        base.Awake();
        shrineInfo.triggerBounds = this;
    }

    protected override IEnumerator Start()
    {
        if(SaveGameManager.activeGame != null && SaveGameManager.activeGame.shrinesUsed.ContainsKey(shrineInfo.type))
        {
            var timesUsed = SaveGameManager.activeGame.shrinesUsed[shrineInfo.type];
            for (int i = 0; i < timesUsed; i++)
            {
                if (i < shrineInfo.lightAnimators.Length)
                {
                    shrineInfo.lightAnimators[i].Play("Off");
                }
            }

            if (timesUsed >= 3)
            {
                shrineInfo.animator.Play("Disabled");
                enabled = false;
            }

            shrineInfo.timesUsed = timesUsed;
        }

        yield return base.Start();
    }

    public override void OnSubmit()
    {
        if (!_repairing)
        {
            UISounds.instance.Confirm();
            NPCDialogueManager.instance.ShowShrineScreen(shrineInfo);
            base.OnSubmit();
        }
    }

    public bool CanRepair()
    {
        return !NPCDialogueManager.instance.dialogueActive && shrineInfo.timesUsed >= 3;
    }

    public void Repair()
    {
        if (CanRepair())
        {
            UISounds.instance.ScreenFlash();
            shrineInfo.timesUsed = 0;
            if (SaveGameManager.activeGame != null)
            {
                SaveGameManager.activeGame.shrinesUsed[shrineInfo.type] = 0;
                SaveGameManager.instance.Save();
            }
            enabled = true;
            StartCoroutine(RepairRoutine());
        }
    }

    private IEnumerator RepairRoutine()
    {
        _repairing = true;
        TransitionFade.instance.FadeOut(0.25f, Color.white);
        yield return new WaitForSeconds(0.25f);
        for (int i = 0; i < shrineInfo.lightAnimators.Length; i++)
        {
            shrineInfo.lightAnimators[i].Play("Default");
        }
        shrineInfo.animator.Play("Default");
        TransitionFade.instance.FadeIn(2, Color.white);
        yield return new WaitForSeconds(2f);
        _repairing = false;
    }
}
