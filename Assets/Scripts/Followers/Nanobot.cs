using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Nanobot : MonoBehaviour, IHasTeam
{
    public float offsetRatio;

    private DamageCreatureTrigger _damageTrigger;

    private float _distance = 2f;
    //time it takes for a full rotation;
    private float _rotationTime = 2f;

    private Enemy _enemy;
    private bool _coolDown;
    private float _velocity;
    private Vector3 _targetPosition;

    public Player player;

    public Team team = Team.Player;
    Team IHasTeam.team
    {
        get { return team; }
        set { team = value; }
    }

    private void Awake()
    {
        _damageTrigger = GetComponentInChildren<DamageCreatureTrigger>();
    }

    public virtual IEnumerator Start()
    {
        while (player == null)
        {
            player = PlayerManager.instance.player1;
            yield return null;
        }

        transform.parent = null;
        SceneManager.MoveGameObjectToScene(gameObject, player.gameObject.scene);
        player.AddNanobot(this);

        _damageTrigger.onDamage.AddListener(OnDamageEnemey);
    }

    public void OnDamageEnemey()
    {
        ProjectileManager.instance.SpawnExplosion(transform.position, Explosion.E16x16, team, player.projectileStats.damage * 1.2f, true);
        FXManager.instance.PlaySplodeSound();
        Destroy(gameObject);
        player.OnNanobotDie();
    }

    public void Update()
    {
        _damageTrigger.damage = player.projectileStats.damage;

        if(!_enemy)
        {
            var offsetAngle = (offsetRatio) * 360;

            offsetAngle += (Time.time % _rotationTime) / _rotationTime * 360;

            _targetPosition = player.transform.position + Quaternion.Euler(0, 0, offsetAngle) * Vector3.up * _distance;            

            if (transform.position != _targetPosition)
            {
                var direction = (_targetPosition - transform.position).normalized;
                var distance = Vector3.Distance(_targetPosition, transform.position);

                if (distance > 0.25f)
                {
                    if (_velocity == 0)
                    {
                        _velocity = player.maxSpeed * 0.8f;
                    }

                    _velocity += distance * distance * Time.deltaTime;
                    _velocity = Mathf.Clamp(_velocity, 0, distance * 5);
                    transform.position += direction * _velocity * Time.deltaTime;
                }
                else
                {
                    _velocity -= distance * Time.deltaTime;

                    if (_velocity < 1)
                    {
                        _velocity = 1;
                    }

                    transform.position = Vector3.MoveTowards(transform.position, _targetPosition, _velocity * Time.deltaTime);
                }
            }
            else
            {
                _velocity = 0;
            }

            if (!_coolDown)
            {
                _enemy = EnemyManager.instance.GetClosest(transform.position, 4);
            }
        }
        else
        {
            transform.position = Vector3.MoveTowards(transform.position, _enemy.position, 9 * Time.deltaTime);

            if(_enemy.state != DamageableState.Alive)
            {
                _enemy = null;
                StartCoroutine(CoolDown());
            }
        }
    }

    private void OnDestroy()
    {
        if(player)
        {
            player.RemoveNanobot(this);
        }
    }

    private IEnumerator CoolDown()
    {
        _coolDown = true;
        yield return new WaitForSeconds(4f);
        _coolDown = false;
    }
}
