using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Teleporter : ButtonTriggerBounds
{
    public Animator animator;
    private Player _player;

    protected override IEnumerator Start()
    {
        yield return base.Start();

        _player = PlayerManager.instance.player1;

        if (_player.teleporting)
        {
            _player.enabled = false;
            animator.Play("OpenIdle");
            yield return new WaitForSeconds(1f);
            animator.Play("Close");
            yield return new WaitForSeconds(3/18f);
            yield return StartCoroutine(_player.FadeColor(1f, Constants.blasterGreen, 1, false));
            _player.enabled = true;
            _player.teleporting = false;
        }
        else if(SaveGameManager.activeGame != null)
        {
            var game = SaveGameManager.activeGame;
            var location = Constants.WorldToLayoutPosition(transform.position).Int2D();
            if (game.discoveredTeleporters.Contains(location))
            {
                animator.Play("ActiveIdle");
            }
            else
            {
                SaveGameManager.activeGame.discoveredTeleporters.Add(location);
                while (LayoutManager.instance.transitioning) { yield return null; }
                yield return new WaitForSeconds(0.5f);
                animator.Play("Activate");

                if (AchievementManager.instance && game.gameMode == GameMode.Normal && game.discoveredTeleporters.Count >= 3)
                {
                    AchievementManager.instance.WaitTryEarnAchievement(1f, AchievementID.PersonalTeleporter);
                }
            }
        }
    }

    public override void OnSubmit()
    {
        if (SaveGameManager.instance)
        {
            UISounds.instance.Confirm();
            Automap.instance.OpenGridSelect(SaveGameManager.activeGame.discoveredTeleporters, SelectSpace);
            FadeOut();
        }
        base.OnSubmit();
    }

    public void SelectSpace(Int2D gridSpace)
    {
        StartCoroutine(Teleport(gridSpace));
    }

    public IEnumerator Teleport(Int2D gridSpace)
    {
        animator.Play("Open");
        _player.ResetAnimatorAndCollision();
        _player.teleporting = true;
        _player.enabled = false;
        yield return new WaitForSeconds(1f);
        yield return StartCoroutine(_player.FadeColor(1, Constants.blasterGreen, 1, true));
        LayoutManager.instance.TeleportToPosition(gridSpace, TransitionFadeType.Teleporter);
    }
}
