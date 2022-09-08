using UnityEngine;
using System.Collections;

[RequireComponent(typeof(Rigidbody2D))]
public class SoundOnCollide : MonoBehaviour
{
    private Rigidbody2D _rigidbody2D;

    public AudioClip[] audioClips;
    public float velocityThreshold = 9;

    private void Awake()
    {        
        _rigidbody2D = GetComponent<Rigidbody2D>();
    }

    public void OnCollisionEnter2D(Collision2D collision)
    {
        if(_rigidbody2D.velocity.magnitude >= velocityThreshold)
        {
            if (audioClips.Length > 0)
            {
                var clip = audioClips[Random.Range(0, audioClips.Length)];
                AudioManager.instance.PlayClipAtPoint(clip, transform.position);
            }
            else
            {
                Debug.LogWarning(name + "'s SoundOnCollide doesn't have any audioClips assigned!");
            }
        }
    }
}
