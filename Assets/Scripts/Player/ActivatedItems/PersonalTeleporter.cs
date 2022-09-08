using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "PersonalTeleporter", menuName = "Player Activated Items/Personal Teleporter", order = 1)]
public class PersonalTeleporter : PlayerActivatedItem
{
    private bool _spent;

    public override void Initialize(Player player)
    {
        base.Initialize(player);
    }

    public override void ButtonDown()
    {
        if(Usable())
        {
            var activeGame = SaveGameManager.activeGame;
            if (activeGame != null)
            {
                _player.energy -= energyCost;

                if (InSpecialArea())
                {
                    if (activeGame.bossesDefeated.Contains(BossName.GlitchBoss))
                    {
                        if (activeGame.bossesDefeated.Contains(BossName.MegaBeastCore))
                        {
                            _spent = true;
                            SeedHelper.StartSpookyMode(false);
                        }
                        else
                        {
                            LayoutManager.instance.GlitchToEnvironmentStart(LayoutManager.instance.layout.environmentOrder[0]);
                        }
                    }
                    else
                    {
                        LayoutManager.instance.GlitchToEnvironmentStart(EnvironmentType.Glitch);
                        _spent = true;
                    }
                    return;
                }
                                
                var discovered = SaveGameManager.activeGame.discoveredTeleporters;
                Int2D gridSpace = discovered[0];
                var playerPos = _player.gridPosition.Int2D();
                var closestDistance = Int2D.Distance(playerPos, gridSpace);
                for (int i = 1; i < discovered.Count; i++)
                {
                    var distance = Int2D.Distance(playerPos, discovered[i]);
                    if (distance < closestDistance)
                    {
                        closestDistance = distance;
                        gridSpace = discovered[i];
                    }
                }
                _player.StartCoroutine(Teleporter(gridSpace));
            }
        }
    }

    public bool InSpecialArea()
    {
        return LayoutManager.instance &&
                (LayoutManager.instance.currentEnvironment == EnvironmentType.BeastGuts ||
                LayoutManager.instance.currentEnvironment == EnvironmentType.Glitch);
    }

    public override void Update()
    {
        base.Update();
        if (_spent)
        {
            _player.activatedItem = null;
            if (SaveGameManager.activeGame != null)
            {
                SaveGameManager.activeGame.currentActivatedItem = MajorItem.None;
                SaveGameManager.instance.Save();
            }
        }
    }

    public IEnumerator Teleporter(Int2D gridSpace)
    {
        _player.ResetAnimatorAndCollision();
        _player.teleporting = true;
        _player.enabled = false;
        yield return _player.StartCoroutine(_player.FadeColor(1, Constants.blasterGreen, 0.5f, true));
        LayoutManager.instance.TeleportToPosition(gridSpace, TransitionFadeType.Teleporter);
    }

    public override bool Usable()
    {
        if (_spent) return false;
        var usable = base.Usable();
        return usable && (InSpecialArea() || (SaveGameManager.activeGame != null && SaveGameManager.activeGame.discoveredTeleporters.Count > 0));
    }
}
