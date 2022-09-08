using UnityEngine;
using System.Collections;

public class DisableAfterTime : MonoBehaviour
{
    public float time;
    public Behaviour[] behaviorsToDisable;    

	IEnumerator Start ()
    {
        yield return new WaitForSeconds(time);
        for (int i = 0; i < behaviorsToDisable.Length; i++)
        {
            behaviorsToDisable[i].enabled = false;
        }
    }
}
