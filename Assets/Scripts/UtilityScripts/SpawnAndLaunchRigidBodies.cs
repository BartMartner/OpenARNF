using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.Events;

public class SpawnAndLaunchRigidBodies : MonoBehaviour, IPermitEvent
{
    public Rigidbody2D rigidBodyPrefab; //Legacy
    public Rigidbody2D[] rigidBodyPrefabs;
    [Range(1,50)]
    public int quantity;
    public float arc = 15;
    public float force = 5;
    public float gravityScale = 1;
    public int limit;

    public List<GameObject> spawned = new List<GameObject>();

    public void SpawnAndLaunch()
    {
        if (limit > 0)
        {
            spawned.RemoveAll((g) => !g);
            if (spawned.Count >= limit)
            {
                return;
            }
        }

        for (int i = 0; i < quantity; i++)
        {
            float angleMod = (((float)i / (quantity - 1f)) * 2f) - 1f;
            Vector3 shotDirection = (Quaternion.AngleAxis(angleMod * arc / 2, Vector3.forward) * transform.right).normalized;
            var rotation = transform.right.x > 0 ? Quaternion.identity : Constants.flippedFacing;
            var room = transform.GetComponentInParent<Room>();
            var prefab = rigidBodyPrefabs != null && rigidBodyPrefabs.Length > 0 ? rigidBodyPrefabs[Random.Range(0, rigidBodyPrefabs.Length)] : rigidBodyPrefab;
            var rb = Instantiate(prefab, transform.position, rotation, room ? room.transform : transform.parent) as Rigidbody2D;
            rb.gravityScale = gravityScale;
            rb.mass = 1;
            rb.AddForce(shotDirection * force, ForceMode2D.Impulse);

            if (limit > 0)
            {
                spawned.Add(rb.gameObject);
                var spawner = rb.gameObject.GetComponent<AdvancedMonsterSpawner>();
                if(spawner)
                {
                    spawner.onSpawnEnemy.AddListener((m) => spawned.Add(m.gameObject));
                }
            }
        }
    }


    public void OnDrawGizmosSelected()
    {
        for (int i = 0; i < quantity; i++)
        {
            float angleMod = (((float)i / (quantity - 1f)) * 2f) - 1f;
            Vector2 shotDirection = (Quaternion.AngleAxis(angleMod * arc / 2, Vector3.forward) * transform.right).normalized;
            
            float time = 3f;
            var timeDelta = 1f / 4f;

            Vector2 start = transform.position;
            Vector2 direciton = shotDirection * force;

            while (time > 0)
            {
                time -= timeDelta;
                direciton += gravityScale * Physics2D.gravity * timeDelta;

                var end = start + direciton * timeDelta;
                Debug.DrawLine(start, end, Color.red);
                start = end;
            }
        }
    }

    public bool PermitEvent()
    {
        if (limit > 0)
        {
            spawned.RemoveAll((g) => !g);
            return spawned.Count < limit;
        }

        return true;
    }
}
