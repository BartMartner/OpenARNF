using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public class MegaBeastCoreLocksController : MonoBehaviour, IPausable
{
    public List<MegaBeastCoreLock> coreLocks;
    public GameObject coreShell;
    public GameObject coreBrain;
    public GameObject coreShellBlowApart;

    private float _shootTimer;
    private float _shootDelay;

    public void Awake()
    {
        _shootTimer = 5;
    }

	public void Update ()
    {
        coreLocks.RemoveAll((c) => !c || !c.notDead);
        if (coreLocks.Count > 0)
        {
            foreach (var coreLock in coreLocks)
            {
                if(coreLock.shooting)
                {
                    return;
                }
                else if (_shootTimer > 0)
                {
                    _shootTimer -= Time.deltaTime;
                }
                else
                {
                    _shootTimer = coreLocks.Count * 3;
                    var pick = coreLocks[Random.Range(0, coreLocks.Count)];
                    pick.RandomShot();
                }
            }
        }
        else
        {
            AllLocksDestoyed();
        }
	}

    public void AllLocksDestoyed()
    {
        Debug.Log("All Locks Destroyed!");
        coreShell.gameObject.SetActive(false);
        coreShellBlowApart.gameObject.SetActive(true);
        coreBrain.gameObject.SetActive(true);
        enabled = false;
    }

    public void Pause()
    {
        enabled = false;
    }

    public void Unpause()
    {
        enabled = true;
    }
}
