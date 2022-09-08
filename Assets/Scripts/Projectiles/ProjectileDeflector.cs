using UnityEngine;
using System.Collections;
using UnityEngine.Events;

public class ProjectileDeflector : MonoBehaviour
{
    [EnumFlags] public DamageType deflectDamage = DamageType.Generic;
    public bool radialDeflection;
    public UnityEvent onDeflect;
    public Team setTeam;
    public AudioClip deflectSound;
    [Tooltip("Set true if this deflector is attached to a damagable and should deflect all attacks until it's been destroyed")]
    public bool deflectAllUntilDead;

    //just to show enabled as an option in the editor
    public void Start() { }

    public void OnDeflect()
    {
        if(onDeflect != null)
        {
            onDeflect.Invoke();
        }

        if (deflectSound) { AudioManager.instance.PlayClipAtPoint(deflectSound, transform.position); }
    }
}
