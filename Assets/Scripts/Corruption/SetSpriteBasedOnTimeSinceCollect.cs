using UnityEngine;
using System.Collections;

[RequireComponent(typeof(SpriteRenderer))]
public class SetSpriteBasedOnTimeSinceCollect : MonoBehaviour
{
    public Sprite[] sprites;
    private SpriteRenderer _spriteRenderer;

    public int triggerItem;
    public float startTime;
    public float endTime;
    public float debugTimeSince;
    public bool realTime = true;

    private int _currentIndex;
    protected float _actualTimeSince;

    public void Awake()
    {
        if (sprites.Length <= 0)
        {
            Debug.LogWarning("SetSpriteBasedOnRoomsAndItems attached to " + gameObject.name + " has an empty sprites array");
            enabled = false;
            return;
        }

        _spriteRenderer = GetComponent<SpriteRenderer>();

        SetSprite();
        enabled = realTime;
    }

    public void Update() { SetSprite(); }

    public void SetSprite()
    {
        var index = triggerItem - 1;
        if (SaveGameManager.activeGame != null && 
            index < SaveGameManager.activeGame.traversalItemCollectTimes.Count &&
            SaveGameManager.activeGame.traversalItemCollectTimes[index] > 0)
        {
            var playTime = PlayerManager.instance.player1.playTime;
            _actualTimeSince = playTime - SaveGameManager.activeGame.traversalItemCollectTimes[index];
        }
        else
        {
            _actualTimeSince = debugTimeSince;
        }

        _currentIndex = Mathf.RoundToInt(Mathf.Lerp(0, sprites.Length - 1, (_actualTimeSince-startTime) / (endTime-startTime)));
        _spriteRenderer.sprite = sprites[_currentIndex];

        if (_actualTimeSince > endTime) { enabled = false; }
    }
}
