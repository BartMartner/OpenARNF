using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class MegaBeastCoreBrainController : MonoBehaviour
{
    public List<Enemy> brainBits;
    public GameObject egg;

    public void Update()
    {
        if (brainBits.Any((b) => b != null && b.state != DamageableState.Dead))
        {
            return;
        }
        Debug.Log("All Spawners Dead");

        egg.gameObject.SetActive(true);
        enabled = false;
    }
}
