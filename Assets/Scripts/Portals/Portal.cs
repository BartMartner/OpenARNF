using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Portal : MonoBehaviour
{
    public Portal connectedPortal;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (!connectedPortal) return;

        if (!collision.gameObject.GetComponent<PortalExiter>())
        {
            collision.gameObject.AddComponent<PortalExiter>();
            collision.transform.position = connectedPortal.transform.TransformPoint(transform.InverseTransformPoint(collision.transform.position));    
        }

        //    var exiter = Instantiate(collision.gameObject).AddComponent<PortalExiter>();
        //    var enterer = collision.gameObject.AddComponent<PortalEnterer>();
        //    exiter.transform.position = connectedPortal.transform.TransformPoint(transform.InverseTransformPoint(enterer.transform.position));
        //    enterer.exiter = exiter;
        //    exiter.enterer = enterer;
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (!connectedPortal) return;

        Destroy(collision.GetComponent<PortalExiter>());

        //var enterer = collision.GetComponent<PortalEnterer>();
        //if(enterer != null)
        //{
        //    enterer.OnExit();
        //}
    }

}
