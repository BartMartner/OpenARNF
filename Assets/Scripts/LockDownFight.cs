using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LockDownFight : MonoBehaviour
{
    private Room _parentRoom;
    private bool _fightActive;
    private bool _allBossMonstersDead;

    private IEnumerator Start()
    {
        _parentRoom = GetComponentInParent<Room>();

        if (LayoutManager.instance)
        {
            while (LayoutManager.instance.transitioning)
            {
                yield return null;
            }

            while (_parentRoom.roomAbstract == null)
            {
                yield return null;
            }

        }

        if (SaveGameManager.activeGame != null && SaveGameManager.activeGame.lockDownRoomsCleared.Contains(_parentRoom.roomAbstract.roomID))
        {
            EnemyManager.instance.DestroyAllEnemies();
            //var spawns = _parentRoom.GetComponentsInChildren<MonsterSpawnPoint>();
            //foreach (var s in spawns)
            //{
            //    s.enabled = false;
            //    Destroy(s.gameObject);
            //}
            //spawns = null;
            Destroy(gameObject);
            yield break;
        }

        Debug.Log("Starting Lock Down Fight!");
        _fightActive = true;

        _parentRoom.StartLockDown();

        StartCoroutine(CheckForEndOfFight());
    }

    private IEnumerator CheckForEndOfFight()
    {
        while (_fightActive)
        {
            CheckForDeadMonsters();

            if (_allBossMonstersDead)
            {
                yield return new WaitForSeconds(0.5f);

                CheckForDeadMonsters(); //if no monsters were added, end the fight;

                if (_allBossMonstersDead)
                {
                    EndLockDownFight();
                }
            }

            yield return null;
        }
    }

    private void CheckForDeadMonsters()
    {
        _allBossMonstersDead = true;
        foreach (var mob in EnemyManager.instance.enemies)
        {
            if (mob.state != DamageableState.Dead)
            {
                _allBossMonstersDead = false;
                break;
            }
        }
    }

    public void EndLockDownFight()
    {
        _fightActive = false;
        BossFightUI.instance.healthBar.Hide();
        _parentRoom.EndLockDown();

        if (_parentRoom.roomAbstract != null)
        {
            var roomID = _parentRoom.roomAbstract.roomID;

            if (SaveGameManager.activeGame != null)
            {
                SaveGameManager.activeGame.lockDownRoomsCleared.Add(roomID);
                SaveGameManager.instance.Save();
            }
        }
    }

    private void OnDestroy()
    {
        if (BossFightUI.instance)
        {
            BossFightUI.instance.healthBar.Hide();
        }
    }
}
