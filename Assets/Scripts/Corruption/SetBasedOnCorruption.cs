using UnityEngine;
using System.Collections;

public class SetBasedOnCorruption : MonoBehaviour
{
    [Range(0, 7)]
    public float minCheck = 0;
    [Range(0, 7)]
    public float maxCheck = 1;

    public float debugCorruption;

    protected float _actualCorruption;
    protected float _adjustedCorruption;

    public virtual void Awake()
    {
        if (SaveGameManager.activeGame == null)
        {
            _actualCorruption = debugCorruption;
        }
        else
        {
            _actualCorruption = SaveGameManager.activeGame.corruption;
        }

        if (minCheck >= maxCheck)
        {
            maxCheck = 1;
            Debug.LogWarning("SetSpriteBasedOnRoomsVisited attached to " + gameObject.name + " has minCheck amount that's less than its maxCheck. Setting maxCheck to 1");
        }

        var range = maxCheck - minCheck;
        _adjustedCorruption = Mathf.Clamp(_actualCorruption - minCheck, 0, range) / range;
        SetCorruption();
    }

    public virtual void SetCorruption() { }
}
