using UnityEngine;

public class GlitchScrapPickUp : MinorItemPickUp
{
    
    public SpriteRenderer spriteRenderer;
    public SpriteRenderer lightRenderer;
    public Sprite redSprite;
    public Sprite greenSprite;
    public Sprite blueSprite;
    public Color redLight;
    public Color greenLight;
    public Color blueLight;

    private float _timer;
    private float _time = 0.75f;
    private MinorItemType[] _types = new MinorItemType[] { MinorItemType.BlueScrap, MinorItemType.GreenScrap, MinorItemType.RedScrap };
    private MinorItemType _currentType = MinorItemType.BlueScrap;
    private int _currentTypeIndex;

    public override void Awake()
    {
        base.Awake();
        MatchCurrentType();
    }

    public void Update()
    {
        _timer -= Time.deltaTime;
        if(_timer < 0)
        {
            _timer = _time;
            _currentTypeIndex = (_currentTypeIndex + 1) % _types.Length;
            _currentType = _types[_currentTypeIndex];
            MatchCurrentType();
        }
    }

    public void MatchCurrentType()
    {
        switch(_currentType)
        {
            case MinorItemType.RedScrap:
                spriteRenderer.sprite = redSprite;
                lightRenderer.color = redLight;
                break;
            case MinorItemType.GreenScrap:
                spriteRenderer.sprite = greenSprite;
                lightRenderer.color = greenLight;
                break;
            case MinorItemType.BlueScrap:
                spriteRenderer.sprite = blueSprite;
                lightRenderer.color = blueLight;
                break;
        }
    }

    public override void OnPickUp(Player player)
    {
        bool everCollected = false;

        var activeGame = SaveGameManager.activeGame;
        if (activeGame != null)
        {
            everCollected = activeGame.minorItemTypesCollected.Contains(data.type);

            if (!everCollected)
            {
                activeGame.minorItemTypesCollected.Add(data.type);
            }

            if (activeGame.minorItemIdsCollected.Contains(data.globalID) && data.globalID != -99)
            {
                Debug.LogError("Trying to collect a minor item with a global ID that's already been collected!");
                return;
            }

            if (data.globalID != -99)
            {
                activeGame.minorItemIdsCollected.Add(data.globalID);
            }
        }
        else
        {
            Debug.LogWarning("SaveGameManager.instance.activeGame == null");
        }

        ItemCollectScreen.instance.Show(ItemManager.GetMinorItemInfo(_currentType), everCollected);

        player.CollectMinorItem(_currentType);

        if (onPickUp != null) { onPickUp.Invoke(player); }
        if (pickUpSound) { UISounds.instance.PlayOneShotLowPriority(pickUpSound); }

        Destroy(gameObject);
    }

    public static MinorItemType GetRandomScrap()
    {
        return Random.value > 0.33f ? (Random.value > 0.5f ? MinorItemType.RedScrap : MinorItemType.BlueScrap) : MinorItemType.GreenScrap;
    }
}
