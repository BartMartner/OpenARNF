using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DialogueTrigger : ButtonTriggerBounds
{
    public DialogueInfo dialogueInfo;

    public override void OnSubmit()
    {
        UISounds.instance.Confirm();
        NPCDialogueManager.instance.ShowDialogueScreen(dialogueInfo);
        base.OnSubmit();
    }
}
