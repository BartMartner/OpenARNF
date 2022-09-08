using UnityEngine;
using System.Collections;

public class MonsterSpawner : MonoBehaviour
{
    public FXType spawnFx;
    public Enemy monsterPrefab;
    public float minRadius = 1;
    public float maxRadius = 1;
    public int quantity = 1;
    public bool randomFacing;
    public bool addToBossFight;
    public bool matchRotation;
    public float dropChanceMod = 1;
    public float dropAmountChance = 1;

    public void SpawnMonster()
    { 
        var _orginalSpawnDirection = Quaternion.Euler(0, 0, Random.Range(0, 360)) * Vector3.up;
        var anglesBetween = 360 / quantity;

        for (int i = 0; i < quantity; i++)
        {
            var spawnDirection = Quaternion.Euler(0, 0, anglesBetween * i) * _orginalSpawnDirection;
            var spawnPosition = transform.position + spawnDirection * Random.Range(minRadius, maxRadius);

            if (spawnFx != FXType.None)
            {
                FXManager.instance.SpawnFX(spawnFx, spawnPosition);
            }

            var parentRoom = GetComponentInParent<Room>();
            var monster = Instantiate(monsterPrefab, spawnPosition, Quaternion.identity, parentRoom ? parentRoom.transform : null) as Enemy;
            monster.transform.position = spawnPosition;

            if(randomFacing)
            {
                monster.transform.rotation = Random.value > 0.5f ? Constants.flippedFacing : Quaternion.identity;
            }
            else
            {
                monster.transform.rotation = matchRotation ? transform.rotation : Quaternion.identity;
            }

            if (dropChanceMod != 1 || dropAmountChance != 1)
            {
                var drops = monster.GetComponentInChildren<SpawnPickUpsOnDeath>();

                if (dropChanceMod <= 0)
                {
                    Destroy(drops);
                }
                else
                {
                    drops.spawnChance *= dropChanceMod;
                    drops.minDrops = (int)Mathf.Clamp(drops.minDrops * dropAmountChance, 1, int.MaxValue);
                    drops.maxDrops = (int)Mathf.Clamp(drops.maxDrops * dropAmountChance, drops.minDrops, int.MaxValue);
                }
            }

            var spawnables = monster.gameObject.GetInterfaces<ISpawnable>();
            foreach (var s in spawnables)
            {
                s.Spawn();
            }            

            if(addToBossFight)
            {
                var bossFight = FindObjectOfType<BossFight>();
                if(bossFight)
                {
                    bossFight.enemies.Add(monster);
                }
            }

            var ponger = monster.GetComponent<PongerBehavior>();
            if(ponger)
            {
                ponger.randomStartingDirection = false;
                ponger.startingDirection.x = spawnDirection.x > 0 ? 1 : -1;
                ponger.startingDirection.y = spawnDirection.y > 0 ? 1 : -1;
            }
        }
    }

    public void OnDrawGizmosSelected()
    {
        Extensions.DrawCircle(transform.position, minRadius);
        Extensions.DrawCircle(transform.position, maxRadius);
    }
}
