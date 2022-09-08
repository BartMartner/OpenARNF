using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ColumnSpread : MonoBehaviour, IHasTeam
{
    public GameObject columnPrefab;
    public float spacing = 1;
    public float quantity = 3;
    public float frequency = 0.2f;
    public float rayLength = 2.5f;
    public bool left = true;
    public bool right = true;
    public bool triggerSpread;
    public bool destroyOnFinish = true;
    public LayerMask layerMask;

    private Team _team;
    public Team team
    {
        get
        {
            return _team;
        }

        set
        {
            _team = value;
            Constants.SetCollisionForTeam(gameObject, _team, true);
        }
    }

    public void Update()
    {
        if(triggerSpread)
        {
            triggerSpread = false;
            StartCoroutine(SpreadColumns());
        }
    }

    public IEnumerator SpreadColumns()
    {
        for (int i = 0; i < quantity+1; i++)
        {
            if(i== 0 || (!left && !right))
            {
                TryToSpawnColumn(transform.position);
            }
            else
            {
                if(left)
                {
                    TryToSpawnColumn(transform.position + Vector3.left * i * spacing);
                }
                
                if (right)
                {
                    TryToSpawnColumn(transform.position + Vector3.right * i * spacing);
                }
            }

            yield return new WaitForSeconds(frequency);
        }


        if (destroyOnFinish)
        {
            yield return new WaitForSeconds(2);
            Destroy(gameObject);
        }        
    }

    public void TryToSpawnColumn(Vector2 point)
    {
        var raycastDown = Physics2D.Raycast(point + Vector2.up, Vector2.down, rayLength, layerMask);
        if (raycastDown.collider)
        {
            Vector3 delta = raycastDown.point - (Vector2)transform.position;
            var rayCastThrough = Physics2D.Raycast(transform.position, delta.normalized, delta.magnitude * 0.9f, layerMask);
            if (!rayCastThrough.collider)
            {
                CreateColumn(raycastDown.point);
            }
        }
    }

    public void CreateColumn(Vector2 point)
    {
        var column = Instantiate(columnPrefab, transform).transform;
        var damageTrigger = column.GetComponent<DamageCreatureTrigger>();
        if (damageTrigger)
        {
            Constants.SetCollisionForTeam(damageTrigger.collider2D, _team, true);
        }
        else
        {
            column.gameObject.layer = gameObject.layer;
        }
        column.localPosition = transform.InverseTransformPoint(point);
    }
}
