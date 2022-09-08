using UnityEngine;
using System.Collections;

public class SpawnOnDeath : MonoBehaviour
{
    public GameObject objectToSpawn;
    public bool onEndDeath;
    public bool matchRotation;
    private Damageable _damagable;

    public void Awake()
    {
        if(!gameObject)
        {
            Debug.LogError("Spawnable Not Assigned on " + gameObject.name);
        }

        _damagable = GetComponentInChildren<Damageable>();
        if(onEndDeath)
        {
            _damagable.onEndDeath.AddListener(SpawnObject);
        }
        else
        {
            _damagable.onStartDeath.AddListener(SpawnObject);
        }
    }

    public void SpawnObject()
    {
        var o = Instantiate(objectToSpawn, transform.position, matchRotation ? transform.rotation : Quaternion.identity) as GameObject;
        var spawnable = o.GetComponent<ISpawnable>();
        var parentRoom = GetComponentInParent<Room>();

        if(parentRoom)
        {
            o.transform.parent = parentRoom.transform;
        }

        if(spawnable != null)
        {
            spawnable.Spawn();
        }
    }
}
