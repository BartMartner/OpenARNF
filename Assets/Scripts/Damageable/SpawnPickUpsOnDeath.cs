using UnityEngine;
using System.Collections;

[RequireComponent(typeof(Damageable))]
public class SpawnPickUpsOnDeath : MonoBehaviour
{
    public float spawnChance = 1;
    public float radius;
    public int minDrops = 1;
    public int maxDrops = 3;
    [Range(0,1)]
    public float scrapChance = 0.15f;
    public Transform spawnPosition;
    public bool hotRoomsOnly;
    public DropType[] buffs;

    public void Awake()
    {
        var damagable = GetComponentInChildren<Damageable>();
        damagable.onEndDeath.AddListener(SpawnPickUps);
        if(!spawnPosition)
        {
            spawnPosition = transform;
        }

        var currentRoom = LayoutManager.CurrentRoom;
        if(hotRoomsOnly && (!currentRoom || currentRoom.roomAbstract.environmentalEffect != EnvironmentalEffect.Heat))
        {
            Destroy(this);
        }
    }

    public void SpawnPickUps()
    {
        var enemy = GetComponent<Enemy>();
        if (enemy && enemy.selfDestruct) return;

        var player = PlayerManager.instance.player1;
        if (!player) return;

        float roll = Random.value;
        if (PlayerManager.instance) { roll *= PlayerManager.instance.coOpMod; }

        var game = SaveGameManager.activeGame;
        var exterminator = game != null && game.gameMode == GameMode.Exterminator;
        var bossRosh = game != null && game.gameMode == GameMode.BossRush;

        var champ = buffs != null && buffs.Length > 0;
        if (exterminator && !enemy.isBoss && !champ) { spawnChance *= 0.66f; }

        if (Random.value < spawnChance)
        {
            var healthDiff = player.maxHealth - player.health;
            var energyDiff = player.maxEnergy - player.energy;
            var energyPercent = player.energy / player.maxEnergy;
            var totalDiff = healthDiff + energyDiff;

            int amount = Random.Range(minDrops, maxDrops);
            if(exterminator && !enemy.isBoss && !champ)
            {
                amount = Mathf.CeilToInt(amount * 0.66f);
            }

            var actualScrapChance = (healthDiff > 0 || energyPercent < 0.5f) ? scrapChance : scrapChance * 2f;

            if(bossRosh && enemy.isBoss) //bosses have spawn chance of 1
            {
                actualScrapChance = 0.66f;
                amount = (int)(maxDrops * 2f);
            }
            else if(LayoutManager.instance && LayoutManager.instance.currentEnvironment == EnvironmentType.BeastGuts)
            {
                actualScrapChance = 0;
            }

            bool needBuffDrop = buffs != null && buffs.Length > 0;

            for (int i = 0; i < amount; i++)
            {
                DropType dropType = DropType.None;

                if (needBuffDrop)
                {
                    dropType = buffs[Random.Range(0, buffs.Length)];
                    needBuffDrop = false;
                }
                else if (player.forceDropType != DropType.None)
                {
                    dropType = player.forceDropType;
                }
                else if(SaveGameManager.activeGame != null && SaveGameManager.activeGame.gameMode == GameMode.Spooky)
                {
                    dropType = (Random.value > 0.5f || energyDiff == 0) ? DropType.GrayScrap : DropType.SmallEnergy;
                }
                else if (Random.value < actualScrapChance)
                {
                    dropType = DropType.GrayScrap;
                }
                else if (totalDiff > 0)
                {
                    var rand = Random.value;
                    var healthChance = healthDiff / totalDiff;

                    if (rand < healthChance)
                    {
                        dropType = DropType.SmallHealth;
                    }
                    else
                    {
                        dropType = DropType.SmallEnergy;
                    }
                }

                if (dropType != DropType.None)
                {
                    var position = radius == 0 ? spawnPosition.position : spawnPosition.position + (Vector3)Random.insideUnitCircle * radius;
                    PickUpManager.instance.SpawnPickUp(dropType, position);
                }
            }
        }
    }

    public void OnDrawGizmosSelected()
    {
        Extensions.DrawCircle(spawnPosition ? spawnPosition.position : transform.position, radius);
    }
}
