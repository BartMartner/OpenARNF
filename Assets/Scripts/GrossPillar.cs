using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GrossPillar : MonoBehaviour
{
    public float spawnRange = 8;
    public float spawnTime = 4;
    public Transform[] spawnPoints;
    public Enemy spawnPrefab;
    public int maxEnemies = 8;
    public Transform shootPoint;
    public ProjectileStats projectileStats;

    private Animator _animator;
    private float _spawnTimer;
    private bool _spawning;
    private bool _shooting;
    private List<Enemy> _enemiesSpawned = new List<Enemy>();

    private void Awake()
    {
        _animator = GetComponent<Animator>();
    }

    public void Update()
    {
        var player = PlayerManager.instance.GetClosestPlayer(transform.position);
        if (player && Vector3.Distance(player.position, transform.position) < spawnRange)
        {
            _enemiesSpawned.RemoveAll(e => !e);
            if (!_spawning && _enemiesSpawned.Count < maxEnemies)
            {
                _spawnTimer -= Time.deltaTime;
                if (_spawnTimer < 0)
                {
                    _spawnTimer = spawnTime;
                    StartCoroutine(Spawn());
                }
            }

            if (!_shooting)
            {
                Vector3 angle1, angle2;
                var solutions = ProjectileManager.instance.SolveBallisticArc(shootPoint.position, projectileStats.speed, player.position, projectileStats.gravity, out angle1, out angle2);
                if (solutions > 0)
                {
                    StartCoroutine(Shoot(player.position));
                }
            }
        }
    }

    private void OnDrawGizmosSelected()
    {
        Extensions.DrawCircle(transform.position, spawnRange);
    }


    private IEnumerator Shoot(Vector3 target)
    {
        _shooting = true;
        _animator.SetTrigger("Shoot");
        yield return new WaitForSeconds(0.25f);
        Vector3 angle1, angle2;
        var solutions = ProjectileManager.instance.SolveBallisticArc(shootPoint.position, projectileStats.speed, target, projectileStats.gravity, out angle1, out angle2);
        if (solutions > 0)
        {
            var angle = solutions == 1 || angle2.y < angle1.y ? angle1 : angle2;
            ProjectileManager.instance.Shoot(projectileStats, shootPoint.position, angle);            
        }        
        yield return new WaitForSeconds(0.5f);
        _shooting = false;
    }

    private IEnumerator Spawn()
    {
        _spawning = true;
        _animator.SetTrigger("Spawn");
        yield return new WaitForSeconds(7 / 12f);
        for (int i = 0; i < spawnPoints.Length; i++)
        {
            yield return new WaitForSeconds(0.5f);
            var p = spawnPoints[i].position;
            var e = Instantiate(spawnPrefab, p, Quaternion.identity, transform.parent);
            _enemiesSpawned.Add(e);
        }
        yield return new WaitForSeconds(5 / 12f);
        _spawning = false;
    }
}
