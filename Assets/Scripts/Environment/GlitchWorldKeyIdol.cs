using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GlitchWorldKeyIdol : MonoBehaviour
{
    public GameObject bossFight;
    public AudioClip diamondAppear;
    public AudioClip diamondDisappear;

    public Animator red;
    public Animator green;
    public Animator blue;
    public Animator black;

    private bool _startingFight;
    private SaveGameData _activeGame;
    private Animator _animator;
    private AudioSource _audioSource;

    private void Start()
    {
        _animator = GetComponent<Animator>();
        _audioSource = GetComponent<AudioSource>();

        if (SaveGameManager.activeGame != null)
        {
            _activeGame = SaveGameManager.activeGame;
            red.gameObject.SetActive(_activeGame.redKeyLit);
            green.gameObject.SetActive(_activeGame.greenKeyLit);
            blue.gameObject.SetActive(_activeGame.blueKeyLit);
            black.gameObject.SetActive(_activeGame.blackKeyLit);
        }
        else
        {
            red.gameObject.SetActive(false);
            green.gameObject.SetActive(false);
            blue.gameObject.SetActive(false);
            black.gameObject.SetActive(false);
        }
    }

    void Update()
    {
        if (_startingFight) return;

        if(_activeGame == null)
        {
            _activeGame = SaveGameManager.activeGame;
            return;
        }

        if(!_activeGame.redKeyLit && _activeGame.itemsCollected.Contains(MajorItem.TheRedKey))
        {
            _activeGame.redKeyLit = true;
            SaveGameManager.instance.Save();
            StartCoroutine(KeyAppear(red, MajorItem.TheRedKey));
        }

        if (!_activeGame.greenKeyLit && _activeGame.itemsCollected.Contains(MajorItem.TheGreenKey))
        {
            _activeGame.greenKeyLit = true;
            SaveGameManager.instance.Save();
            StartCoroutine(KeyAppear(green, MajorItem.TheGreenKey));
        }

        if (!_activeGame.blueKeyLit && _activeGame.itemsCollected.Contains(MajorItem.TheBlueKey))
        {
            _activeGame.blueKeyLit = true;
            SaveGameManager.instance.Save();
            StartCoroutine(KeyAppear(blue, MajorItem.TheBlueKey));
        }

        if (!_activeGame.blackKeyLit && _activeGame.itemsCollected.Contains(MajorItem.TheBlackKey))
        {
            _activeGame.blackKeyLit = true;
            SaveGameManager.instance.Save();
            StartCoroutine(KeyAppear(black, MajorItem.TheBlackKey));
        }

        if (!bossFight.activeInHierarchy && _activeGame.redKeyLit && _activeGame.greenKeyLit && _activeGame.blueKeyLit && _activeGame.blackKeyLit)
        {
            StartCoroutine(StartFight());
        }
    }

    public IEnumerator KeyAppear(Animator animator, MajorItem item)
    {
        animator.gameObject.SetActive(true);
        animator.Play("Appear");
        _audioSource.PlayOneShot(diamondAppear);
        yield return new WaitForSeconds(1f);
    }

    public IEnumerator StartFight()
    {
        _startingFight = true;
        yield return new WaitForSeconds(1f);
        LayoutManager.instance.currentRoom.StartLockDown();

        yield return new WaitForSeconds(1f);
        _audioSource.PlayOneShot(diamondDisappear);
        red.Play("Disappear");
        yield return new WaitForSeconds(1f);
        _audioSource.PlayOneShot(diamondDisappear);
        green.Play("Disappear");
        yield return new WaitForSeconds(1f);
        _audioSource.PlayOneShot(diamondDisappear);
        blue.Play("Disappear");
        yield return new WaitForSeconds(1f);
        _audioSource.PlayOneShot(diamondDisappear);
        black.Play("Disappear");
        yield return new WaitForSeconds(1f);

        _animator.Play("FadeOut");
        yield return new WaitForSeconds(1f);
        
        bossFight.SetActive(true);
        Destroy(gameObject);
    }
}

