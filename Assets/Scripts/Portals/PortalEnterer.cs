using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PortalEnterer : MonoBehaviour
{
    public PortalExiter exiter;

    public void OnExit()
    {
        Destroy(exiter);
        Destroy(gameObject);
    }
}
