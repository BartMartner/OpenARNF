using UnityEngine;
using System.Collections;

public class RoomBasedRandom : MonoBehaviour
{
    protected XorShift _random;
    protected Room _room;

    public virtual void Awake()
    {
        _room = GetComponentInParent<Room>();
    }

    protected virtual IEnumerator Start()
    {
        if (_room)
        {
            while (_room.random == null) { yield return null; }
            _random = _room.random;
        }

        Randomize();
    }

    public virtual void Randomize() { }
}
