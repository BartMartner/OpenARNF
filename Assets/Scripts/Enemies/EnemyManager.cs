using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[DisallowMultipleComponent]
public class EnemyManager : MonoBehaviour 
{
    public static EnemyManager instance;
    public List<Enemy> enemies = new List<Enemy>();

    public List<ChaseAndOrbit> orbitalEnemies;

	private void Awake () 
    {
        instance = this;
	}

    private void OnDestroy()
    {
        instance = null;
    }

    public void HurtAllEnemies(float damage, DamageType damageType = DamageType.Generic, bool ignoreAegis = true)
    {
        foreach (var enemy in enemies)
        {
            enemy.Hurt(damage, gameObject, damageType, ignoreAegis);
        }
    }
    
    public void StatusEffectAllEnemies(StatusEffect effect, Team team)
    {
        foreach (var enemy in enemies)
        {
            enemy.ApplyStatusEffect(effect, team);
        }
    }

    public void DestroyAllEnemies()
    {
        foreach (var enemy in enemies)
        {
            Destroy(enemy.gameObject);
        }
    }

    public void KillAllEnemies()
    {
        foreach (var enemy in enemies)
        {
            enemy.StartDeath();
        }
    }

    public Enemy GetClosest(Vector3 position, float distance)
    {
        var lowestSqrMagnitude = float.MaxValue;
        Enemy closest = null;

        for (int i = 0; i < enemies.Count; i++)
        {
            var enemy = enemies[i];
            if (enemy.state == DamageableState.Alive && !enemy.notTargetable)
            {
                var ePosition = enemy.position;
                var magnitude = (ePosition - position).sqrMagnitude;
                if (magnitude < lowestSqrMagnitude)
                {
                    lowestSqrMagnitude = magnitude;
                    closest = enemy;
                }
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

    public Enemy GetClosest(Vector3 position, out float lowestSqrMagnitude)
    {
        lowestSqrMagnitude = float.MaxValue;
        Enemy closest = null;
        Enemy enemy;

        for (int i = 0; i < enemies.Count; i++)
        {
            enemy = enemies[i];
            if (enemy.state == DamageableState.Alive)
            {
                var ePosition = enemy.position;
                var magnitude = (ePosition - position).sqrMagnitude;
                if(magnitude < lowestSqrMagnitude)
                {
                    lowestSqrMagnitude = magnitude;
                    closest = enemy;
                }
            }
        }

        return closest;
    }

    public Enemy GetClosestInArc(Vector3 position, Vector3 direction, float distance, float angle)
    {
        var lowestDistance = distance;
        Enemy closest = null;
        Enemy enemy;

        for (int i = 0; i < enemies.Count; i++)
        {
            enemy = enemies[i];

            if (enemy.notTargetable) continue;

            if (enemy.state == DamageableState.Alive)
            {
                var ePosition = enemy.position;
                var eAngle = Mathf.Abs(Quaternion.FromToRotation(direction, (ePosition - position).normalized).eulerAngles.z);
                if(eAngle > 180)
                {
                    eAngle = 360 - eAngle;
                }

                var eDistance = Vector3.Distance(ePosition, position);
                if (eDistance < lowestDistance && eAngle < angle * 0.5f)
                {
                    lowestDistance = eDistance;
                    closest = enemy;
                }
            }
        }

        return closest;
    }

    public void RegisterOrbital(ChaseAndOrbit orbital)
    {
        orbitalEnemies.Add(orbital);
        AssignOrbitalOffsets();
    }

    public void DestroyOrbital(ChaseAndOrbit orbital)
    {
        orbitalEnemies.Remove(orbital);
        AssignOrbitalOffsets();
    }

    private void AssignOrbitalOffsets()
    {
        for (int i = 0; i < orbitalEnemies.Count; i++)
        {
            orbitalEnemies[i].offsetRatio = (float)i / orbitalEnemies.Count;
        }
    }
}
