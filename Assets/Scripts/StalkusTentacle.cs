using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Anima2D;

public class StalkusTentacle : MonoBehaviour
{
    public Bone2D[] bones;
    public Transform clawBone;
    public IkCCD2D tentacleIK;
    public IkCCD2D clawIK;
}
