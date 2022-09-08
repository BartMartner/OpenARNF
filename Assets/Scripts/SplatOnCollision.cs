using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SplatOnCollision : MonoBehaviour
{
    public ParticleSystem part;
    public List<ParticleCollisionEvent> collisionEvents;
    public float minSize = 0.25f;
    public float maxSize = 0.75f;
    public float chance = 0.5f;

    public void Start()
    {
        part = GetComponent<ParticleSystem>();
        collisionEvents = new List<ParticleCollisionEvent>();
    }

    private void OnParticleCollision(GameObject other)
    {
        if (Random.value < chance)
        {
            int numCollisionEvents = part.GetCollisionEvents(other, collisionEvents);

            for (int i = 0; i < numCollisionEvents; i++)
            {
                GibManager.instance.SpawnBloodSplatter(collisionEvents[i].intersection, Random.Range(minSize, maxSize));
            }
        }
    }
}
