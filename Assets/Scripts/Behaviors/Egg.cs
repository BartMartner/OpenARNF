using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Damageable))]
public class Egg : PermanentStateObject
{
    public override void SetStateFromSave(int state)
    {
        if (state == 1 && SaveGameManager.activeGame != null &&
            (SaveGameManager.activeGame.gameMode == GameMode.Exterminator ||
             SaveGameManager.activeGame.gameMode == GameMode.BossRush))
        {
            Destroy(gameObject);
        }
    }

    private void Awake()
    {
        var damageable = GetComponent<Damageable>();
        damageable.onEndDeath.AddListener(() =>
        {
            var slot = SaveGameManager.activeSlot;
            var game = SaveGameManager.activeGame;
            if (game != null)
            {
                if (slot.eggsDestroyed < long.MaxValue) { slot.eggsDestroyed++; }
                if (game.gameMode == GameMode.Exterminator || game.gameMode == GameMode.BossRush)
                {
                    SaveState(1);
                }
            }
        });
    }
}
