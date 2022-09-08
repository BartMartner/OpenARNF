using UnityEngine;
using System.Collections;

public class DestroyBasedOnCorruption : SetBasedOnCorruption
{
    public override void SetCorruption()
    {
        if (_actualCorruption > maxCheck || _actualCorruption < minCheck)
        {
            Destroy(gameObject);
        }
    }
}
