using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BeastGutsFinalGates : MonoBehaviour
{
    public Material unlockedMaterial;
    public Material lockedMaterial;
    public GameObject[] triggers;
    public GameObject[] gates;

    public IEnumerator Start()
    {
        yield return null;

        if(SaveGameManager.activeGame != null)
        {
            var gateStates = new List<int>();
            foreach (var kvp in SaveGameManager.activeGame.otherInts)
            {
                if (kvp.Key.Contains("BeastGutsGate")) { gateStates.Add(kvp.Value); }
            }

            for (int i = 0; i < triggers.Length; i++)
            {
                if (i < gateStates.Count && gateStates[i] == 1)
                {
                    triggers[i].SetActive(true);
                    var r = gates[i].GetComponent<SpriteRenderer>();                    
                    r.material = unlockedMaterial;
                }
                else
                {
                    triggers[i].SetActive(false);
                    var r = gates[i].GetComponent<SpriteRenderer>();                    
                    r.material = lockedMaterial;
                }
            }            
        }
    }

    public void CameraShake()
    {
        MainCamera.instance.Shake(0.5f, 0.1f, 3);
    }
}
