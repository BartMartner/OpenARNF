using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TrackPoint : MonoBehaviour
{
    public TrackPoint prevTrackPoint;
    public TrackPoint nextTrackPoint;

    private void OnDrawGizmos()
    {
        if(nextTrackPoint != null)
        {
            Debug.DrawLine(transform.position, nextTrackPoint.transform.position);
        }
    }
}
