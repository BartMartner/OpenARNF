using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;

public class AdvancedMonsterSpawner : MonoBehaviour
{
    public FXType spawnFx;
    public Enemy monsterPrefab;
    public float minRadius = 1;
    public float maxRadius = 1;
    public float overTime = 0;
    public int quantity = 1;
    public bool matchRotation;
    public bool randomFacing;
    public bool randomOrder;
    public bool addToBossFight;
    public bool alwaysSpawnFullQuantity;
    public float dropChanceMod = 1;
    public float dropAmountChance = 1;
    public int maxMonsters;
    public int monstersSpawned;
    public List<Transform> spawnPoints;

    public EnemyEvent onSpawnEnemy = new EnemyEvent();

    public float warmUpTime;
    public UnityEvent onEventWarmUp;

    public bool maxedOut
    {
        get { return monstersSpawned > maxMonsters; }
    }

    public void SpawnMonster()
    {
        if (maxMonsters <= 0 || monstersSpawned < maxMonsters)
        {
            StartCoroutine(Spawn());
        }
    }

    private IEnumerator Spawn()
    {
        if (onEventWarmUp != null)
        {
            onEventWarmUp.Invoke();
        }

        if (warmUpTime > 0)
        {
            yield return new WaitForSeconds(warmUpTime);
        }

        var _orginalSpawnDirection = Quaternion.Euler(0, 0, Random.Range(0, 360)) * Vector3.up;
        var anglesBetween = 360 / quantity;

        Vector3 spawnPosition;
        Vector3 spawnDirection;
        List<int> randomSpawnIndices = new List<int>(); ;

        if (randomOrder)
        {
            for (int i = 0; i < quantity; i++)
            {
                randomSpawnIndices.Add(i);
            }
            randomSpawnIndices = randomSpawnIndices.OrderBy(x => Random.value).ToList();
        }

        for (int i = 0; i < quantity; i++)
        {
            if (spawnPoints.Count > 0)
            {
                spawnPosition = spawnPoints[i % spawnPoints.Count].position;
                spawnDirection = randomFacing ? Random.value > 0.5 ? Vector3.left : Vector3.right : Vector3.right;
            }
            else
            {
                if (randomOrder)
                {
                    spawnDirection = Quaternion.Euler(0, 0, anglesBetween * randomSpawnIndices[i]) * _orginalSpawnDirection;
                }
                else
                {
                    spawnDirection = Quaternion.Euler(0, 0, anglesBetween * i) * _orginalSpawnDirection;
                }

                spawnPosition = transform.position + spawnDirection * Random.Range(minRadius, maxRadius);
            }

            if (spawnFx != FXType.None)
            {
                FXManager.instance.SpawnFX(spawnFx, spawnPosition);
            }

            var parentRoom = GetComponentInParent<Room>();
            var monster = Instantiate(monsterPrefab, spawnPosition, matchRotation ? transform.rotation : Quaternion.identity, parentRoom ? parentRoom.transform : null) as Enemy;

            if(dropChanceMod != 1 || dropAmountChance != 1) 
            {
                var drops = monster.GetComponentInChildren<SpawnPickUpsOnDeath>();

                if (drops)
                {
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
            }

            if (randomFacing)
            {
                monster.transform.rotation = Random.value > 0.5f ? Constants.flippedFacing : Quaternion.identity;
            }

            monstersSpawned++;
            monster.onEndDeath.AddListener(() => monstersSpawned--);

            var spawnable = monster.gameObject.GetComponent<ISpawnable>();
            if (spawnable != null)
            {
                spawnable.Spawn();
            }

            if(onSpawnEnemy != null)
            {
                onSpawnEnemy.Invoke(monster);
            }

            if (addToBossFight)
            {
                var bossFight = FindObjectOfType<BossFight>();
                if (bossFight)
                {
                    bossFight.enemies.Add(monster);
                }
            }

            var ponger = monster.GetComponent<PongerBehavior>();
            if (ponger)
            {
                ponger.randomStartingDirection = false;
                ponger.startingDirection.x = spawnDirection.x > 0 ? 1 : -1;
                ponger.startingDirection.y = spawnDirection.y > 0 ? 1 : -1;
            }

            if(maxMonsters > 0 && monstersSpawned > maxMonsters && !alwaysSpawnFullQuantity)
            {
                yield break;
            }

            if (overTime > 0)
            {
                yield return new WaitForSeconds(overTime / quantity);
            }
        }
    }

    public void OnDrawGizmosSelected()
    {
        if (maxRadius > 0)
        {
            Extensions.DrawCircle(transform.position, minRadius);
            Extensions.DrawCircle(transform.position, maxRadius);
        }
        else
        {
            Debug.DrawLine(transform.position + transform.up, transform.position - transform.up);
            Debug.DrawLine(transform.position + transform.up, transform.position - transform.up);
            Debug.DrawLine(transform.position + transform.right, transform.position - transform.right);
            Debug.DrawLine(transform.position + transform.right, transform.position - transform.right);
        }
    }
}