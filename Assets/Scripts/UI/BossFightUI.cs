using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class BossFightUI : MonoBehaviour
{
    public static BossFightUI instance;
    public BossHealthBar healthBar;
    public Text getReady;
    public AudioClip getReadySound;
    private AudioSource _audioSource;
    public bool getReadyVisible { get; private set; }

    public void Awake()
    {
        instance = this;
        _audioSource = GetComponent<AudioSource>();
    }

    public void OnDestroy()
    {
        if (instance == this) { instance = null; }
    }

    public IEnumerator GetReadySequence()
    {
        getReady.gameObject.SetActive(true);
        getReadyVisible = true;
        _audioSource.PlayOneShot(getReadySound);
        yield return new WaitForSeconds(2);
        getReady.gameObject.SetActive(false);
        getReadyVisible = false;
    }
}
