using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ExterminatorUI : MonoBehaviour
{
    public Color[] lockColors;
    public Image[] locks;
    public Sprite locked;
    public Sprite unlocked;
    public Sprite buluc;

    private int _lastCapIndex = 0;
    private Player _player;
    private IEnumerator Start()
    {
        var game = SaveGameManager.activeGame;
        if (game != null && game.gameMode != GameMode.Exterminator)
        {
            Destroy(gameObject);
            yield break;
        }

        while (!PlayerManager.instance || !PlayerManager.instance.player1.started)
        {
            yield return null;
        }

        _player = PlayerManager.instance.player1;
        for (int i = 0; i < locks.Length; i++) { locks[i].color = lockColors[i]; }
        _lastCapIndex = _player.GetPlayerCapabilitiesIndex();
        _player.onCollectItem += Refresh;
        Refresh();
    }

    private void Refresh()
    {
        var capIndex = _player.GetPlayerCapabilitiesIndex();

        for (int i = 0; i < locks.Length; i++)
        {
            if (i >= capIndex)
            {
                locks[i].sprite = locked;
            }
            else
            {
                locks[i].sprite = unlocked;
                var color = lockColors[i];
                color.a = 0.5f;
                locks[i].color = color;
            }
        }

        if (capIndex > _lastCapIndex)
        {
            _lastCapIndex = capIndex;
            StartCoroutine(ShowBuluc());
        }
    }

    private IEnumerator ShowBuluc()
    {
        yield return new WaitForSeconds(1);
        while(Time.timeScale != 1 || !PlayerManager.instance.player1.grounded) { yield return null; }

        var dialogueInfo = new DialogueInfo();
        dialogueInfo.NPCName = "Avatar of Buluc Chabtan";
        dialogueInfo.portrait = buluc;

        switch(_lastCapIndex)
        {
            case 1:
                dialogueInfo.dialogue = "We have determined you are capable enough to proceed to Red Sector. Do not fail us.";
                break;
            case 2:
                dialogueInfo.dialogue = "Proceed to Orange Sector.";
                break;
            case 3:
                dialogueInfo.dialogue = "Yellow Sector is now unlocked.";
                break;
            case 4:
                dialogueInfo.dialogue = "We will allow you to enter Green Sector. Speak not of what you see there.";
                break;
            case 5:
                dialogueInfo.dialogue = "Proceed to Blue Sector";
                break;
            case 6:
                dialogueInfo.dialogue = "Indigo Sector unlocked. You have done well.";
                break;
            case 7:
                dialogueInfo.dialogue = "Access to Pink Sector granted. [break] We believe a remnant of the Megabeast has found its way into the Buried City. Destroy this foul thing and stop its incessant breeding.";
                break;
        }
        NPCDialogueManager.instance.ShowDialogueScreen(dialogueInfo);
    }
}
