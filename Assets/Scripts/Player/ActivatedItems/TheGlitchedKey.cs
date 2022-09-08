using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "TheGlitchedKey", menuName = "Player Activated Items/The Glitched Key", order = 1)]
public class TheGlitchedKey : PlayerActivatedItem
{
    private bool _activated;
    public override void ButtonDown()
    {
        base.ButtonDown();
        if (!_activated)
        {
            _activated = true;
            if (LayoutManager.instance)
            {
                var activeGame = SaveGameManager.activeGame;
                if (LayoutManager.instance.currentEnvironment == EnvironmentType.Glitch)
                {
                    if (activeGame != null && activeGame.bossesDefeated.Contains(BossName.MegaBeastCore))
                    {
                        SeedHelper.StartSpookyMode(false);
                    }
                    else
                    {
                        LayoutManager.instance.GlitchToEnvironmentStart(LayoutManager.instance.layout.environmentOrder[0]);
                    }
                }
                else
                {
                    if (activeGame != null && activeGame.bossesDefeated.Contains(BossName.GlitchBoss))
                    {
                        SeedHelper.StartSpookyMode(false);
                    }
                    else
                    {
                        LayoutManager.instance.GlitchToEnvironmentStart(EnvironmentType.Glitch);
                    }
                }
            }
        }
    }

    public override void Update()
    {
        base.Update();
        if (_activated)
        {
            _player.activatedItem = null;
            if (SaveGameManager.activeGame != null)
            {
                SaveGameManager.activeGame.currentActivatedItem = MajorItem.None;
                SaveGameManager.instance.Save();
            }
        }
    }
}
