using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PickUpManager : MonoBehaviour
{
    public static PickUpManager instance;
    public List<PickUp> allDrops = new List<PickUp>();
    public List<ScrapDrop> scrapDrops = new List<ScrapDrop>();

    public BuffPickUp damageBuffPrefab;
    public BuffPickUp attackBuffPrefab;
    public BuffPickUp speedBuffPrefab;
    public HealthDrop healthPrefab;
    public EnergyDrop energyPrefab;
    public ScrapDrop scrapPrefab;

    public bool pickUpsPresent
    {
        get { return allDrops.Count > 0; }
    }

    private void Awake()
    {
        instance = this;
    }

    private void OnDestroy()
    {
        instance = null;
    }

    public void SpawnPickUp(DropType type, Vector3 position)
    {
        switch (type)
        {
            case DropType.SmallHealth:
                Instantiate(healthPrefab, position, Quaternion.identity);
                break;
            case DropType.SmallEnergy:
                Instantiate(energyPrefab, position, Quaternion.identity);
                break;
            case DropType.GrayScrap:
                Instantiate(scrapPrefab, position, Quaternion.identity);
                break;
            case DropType.AttackBuff:
                Instantiate(attackBuffPrefab, position, Quaternion.identity);
                break;
            case DropType.DamageBuff:
                Instantiate(damageBuffPrefab, position, Quaternion.identity);
                break;
            case DropType.SpeedBuff:
                Instantiate(speedBuffPrefab, position, Quaternion.identity);
                break;
        }
    }

    public ScrapDrop GetClosestScrapDrop(Vector3 position, float distance)
    {
        var lowestSqrMagnitude = float.MaxValue;
        ScrapDrop closest = null;

        for (int i = 0; i < scrapDrops.Count; i++)
        {
            var drop = scrapDrops[i];

            var ePosition = drop.transform.position;
            var magnitude = (ePosition - position).sqrMagnitude;
            if (magnitude < lowestSqrMagnitude)
            {
                lowestSqrMagnitude = magnitude;
                closest = drop;
            }
        }

        if (Mathf.Sqrt(lowestSqrMagnitude) < distance)
        {
            return closest;
        }
        else
        {
            return null;
        }
    }

    public void ChangeAllPickUp(DropType dropType)
    {
        foreach (var item in allDrops)
        {
            bool reroll = false;

            var buffDrop = item as BuffPickUp;
            switch(dropType)
            {
                case DropType.GrayScrap:
                    reroll = !(item is ScrapDrop);
                    break;
                case DropType.SmallHealth:
                    reroll = !(item is HealthDrop);
                    break;
                case DropType.SmallEnergy:
                    reroll = !(item is EnergyDrop);
                    break;
                case DropType.AttackBuff:
                    reroll = !buffDrop || buffDrop.statType != PlayerStatType.Attack;
                    break;
                case DropType.DamageBuff:
                    reroll = !buffDrop || buffDrop.statType != PlayerStatType.Damage;
                    break;
                case DropType.SpeedBuff:
                    reroll = !buffDrop || buffDrop.statType != PlayerStatType.Speed;
                    break;
            }

            if (reroll)
            {
                if (FXManager.instance) { FXManager.instance.SpawnFX(FXType.AnimeSplode, item.transform.position); }
                SpawnPickUp(dropType, item.transform.position);
                Destroy(item.gameObject);
            }
        }
    }

    public void PickupToNanobots(Player player)
    {
        foreach (var item in allDrops)
        {
            if (FXManager.instance) { FXManager.instance.SpawnFX(FXType.AnimeSplode, item.transform.position); }
            player.SpawnNanobot(item.transform.position, true);
            Destroy(item.gameObject);
        }
    }

    public PickUp GetClosestPickUp (Vector3 position, float distance)
    {
        var lowestSqrMagnitude = float.MaxValue;
        PickUp closest = null;

        for (int i = 0; i < allDrops.Count; i++)
        {
            var drop = allDrops[i];

            var ePosition = drop.transform.position;
            var magnitude = (ePosition - position).sqrMagnitude;
            if (magnitude < lowestSqrMagnitude)
            {
                lowestSqrMagnitude = magnitude;
                closest = drop;
            }
        }

        if (Mathf.Sqrt(lowestSqrMagnitude) < distance)
        {
            return closest;
        }
        else
        {
            return null;
        }
    }

    public Sprite GetMinorItemSprite(MinorItemType type)
    {
        string path = "Sprites/Pickups/" + type.ToString();
        var sprite = Resources.Load<Sprite>(path);
        if (!sprite) { sprite = Resources.Load<Sprite>(path + "_0"); }
        return sprite;
    }
}
