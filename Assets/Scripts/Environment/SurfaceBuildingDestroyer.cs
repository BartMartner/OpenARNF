using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SurfaceBuildingDestroyer : PermanentStateObject
{
    private Animator _animator;
    private SpriteRenderer _spriteRenderer;
    private int _state = 0;
    private bool _ready;

    public void Awake()
    {
        _spriteRenderer = GetComponent<SpriteRenderer>();
        _animator = GetComponent<Animator>();
        if(SaveGameManager.activeGame != null && Constants.postBeastModes.Contains(SaveGameManager.activeGame.gameMode))
        {
            Destroy(this); //remove script
            return;
        }
    }

    public IEnumerator Start()
    {
        yield return new WaitForSeconds(2f);
        _ready = true;
    }

    public void Update()
    {
        if (!_ready) return;

        if (_state != 1 && _spriteRenderer.isVisible)
        {           
            _state = 1;
            StartCoroutine(WaitDestroy());
        }
    }

    private IEnumerator WaitDestroy()
    {
        yield return new WaitForSeconds(Random.Range(1, 2.5f));
        while (Mathf.Abs(transform.position.x - MainCamera.instance.transform.position.x) > 150) yield return null;
        SaveState(_state);
        _animator.SetTrigger("Destroy");
        yield return new WaitForSeconds(0.5f);
        MainCamera.instance.Shake(0.75f, 0.1f, 4);
    }

    public override void SetStateFromSave(int state)
    {
        _state = state;
        if(state == 1)
        {
            _animator.Play("Destroyed");
            enabled = false;
        }
    }
}
