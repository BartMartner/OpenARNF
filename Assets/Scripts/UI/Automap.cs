//#define SHOWFULLMAP
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using System.Linq;
using System;

public class Automap : MonoBehaviour
{
    public static Automap instance;

    public static Vector2 roomUnit = new Vector2(7, 5);
    public Player player;
    public RectTransform mapContainer;
    public RectTransform playerDot;
    public RectTransform questionMarks;
    public Sprite itemUncollected;
    public Sprite itemCollected;
    public Sprite boss;
    public Sprite save;
    public Sprite shop;
    public Sprite shrine;
    public Sprite teleporter;
    public Sprite megaBeast;
    public Sprite beastRemnant;
    public Sprite bossRushStart;
    public Sprite beastGutsGateLock;
    public Sprite beastGutsPreBoss;
    public Sprite highlight;
    private Dictionary<string, Image> _mapSegments = new Dictionary<string, Image>();
    public int segmentCount { get { return _mapSegments.Count; } }
    private Dictionary<int, Image> _minorItems = new Dictionary<int, Image>();
    private Dictionary<MajorItem, Image> _majorItems = new Dictionary<MajorItem, Image>();
    private Dictionary<string, Image> _roomIcons = new Dictionary<string, Image>();
    private Dictionary<string, EnvironmentType> _envTypes = new Dictionary<string, EnvironmentType>();
    private Dictionary<string, Dictionary<int, Image>> _exits = new Dictionary<string, Dictionary<int, Image>>();
    private Dictionary<string, int> _exterminatorSegmentColorIndex;
    private HashSet<string> _unrevealableSegments = new HashSet<string>();
    private HashSet<string> _waterSegments = new HashSet<string>();
    private HashSet<string> _darkSegments = new HashSet<string>();
    private HashSet<string> _heatSegments = new HashSet<string>();
    private HashSet<string> _confuseSegments = new HashSet<string>();
    private List<Image> highlightSquares = new List<Image>();
    public List<Sprite> autoMapTiles;
    public Color[] exterminatorColors;

    private RoomLayout _layout;
    private bool _expanded;
    public bool expanded { get { return _expanded; } }
    private Vector3 _originalPosition;
    private Vector3 _originalSize;
    private RectTransform _rectTransform;
    private float _visibleLayoutWidth;

    private List<Int2D> _selectableGridSpaces;
    private bool _gridSelectMode;
    public bool  gridSelectMode { get { return _gridSelectMode; } }
    private Image _highlightedImage;
    private Int2D _highlightedPos;
    private float _selectionDelay;
    private Action<Int2D> onSelectGridSpace;
    private IEnumerator _selectRoutine;

    private Vector2 _expandedOffset;
    private Vector2 _expandedOffsetRounded;

    private void Awake() { instance = this; }

    private void Start ()
    {
        var layoutManager = FindObjectOfType<LayoutManager>();
        if (layoutManager == null)
        {
            Debug.LogWarning("Automap could not find layoutManager so its destroying itself.");
            DestroyImmediate(gameObject);
            return;
        }

        _layout = layoutManager.layout;
        float glitchWidth = 0;
        Rect glitchBounds;
        if(_layout.environmentLimits.TryGetValue(EnvironmentType.Glitch, out glitchBounds))
        {
            glitchWidth = glitchBounds.width;
        }
        _visibleLayoutWidth = _layout.width - glitchWidth;
        _rectTransform = GetComponent<RectTransform>();
        _originalPosition = _rectTransform.localPosition;
        _originalSize = _rectTransform.sizeDelta;

        var offset = new Vector2(_visibleLayoutWidth / 2f, _layout.height / 2f);

        var exterminator = _layout.gameMode == GameMode.Exterminator;
        Dictionary<RoomAbstract, int> exterminatorRoomIndices = null;
        if (exterminator)
        {
            exterminatorRoomIndices = new Dictionary<RoomAbstract, int>();
            _exterminatorSegmentColorIndex = new Dictionary<string, int>();

            int eci = -1;
            foreach (var roomAbstract in _layout.roomAbstracts)
            {
                foreach (var exit in roomAbstract.exits)
                {
                    var targetRoom = _layout.GetRoomAtPositon(exit.TargetPosition());

                    if (targetRoom.expectedCapabilitiesIndex < roomAbstract.expectedCapabilitiesIndex) continue;

                    if (!RoomTransitionTrigger.GetExterminatorIndex(exit, 0, out eci)) continue;

                    var index = eci - 1;
                    if (index < 0 || index >= exterminatorColors.Length) continue;

                    int existing;
                    if (!exterminatorRoomIndices.TryGetValue(targetRoom, out existing) || index > existing)
                    {
                        exterminatorRoomIndices[targetRoom] = index;
                    }
                }
            }
        }

        foreach (var roomAbstract in _layout.roomAbstracts)
        {
            if (roomAbstract.assignedRoomInfo.roomType == RoomType.Shop ||
                roomAbstract.assignedRoomInfo.roomType == RoomType.SaveRoom ||
                roomAbstract.assignedRoomInfo.roomType == RoomType.BossRoom ||
                roomAbstract.assignedRoomInfo.roomType == RoomType.MegaBeast ||
                roomAbstract.assignedRoomInfo.roomType == RoomType.Shrine ||
                roomAbstract.assignedRoomInfo.roomType == RoomType.Teleporter ||
                roomAbstract.assignedRoomInfo.roomType == RoomType.OtherSpecial)
            {
                var roomIcon = new GameObject().AddComponent<Image>();
                roomIcon.name = roomAbstract.roomID;
                roomIcon.transform.SetParent(mapContainer);
                roomIcon.transform.localScale = Vector3.one;
                switch (roomAbstract.assignedRoomInfo.roomType)
                {
                    case RoomType.MegaBeast:
                        roomIcon.sprite = megaBeast;
                        break;
                    case RoomType.BossRoom:
                        roomIcon.sprite = boss;
                        break;
                    case RoomType.Shop:
                        roomIcon.sprite = shop;
                        break;
                    case RoomType.SaveRoom:
                        roomIcon.sprite = save;                        
                        break;
                    case RoomType.Shrine:
                        roomIcon.sprite = shrine;
                        break;
                    case RoomType.Teleporter:
                        roomIcon.sprite = teleporter;
                        break;
                    case RoomType.OtherSpecial:
                        var sceneName = roomAbstract.assignedRoomInfo.sceneName;
                        if (sceneName.Contains("BeastRemnant"))
                        {
                            roomIcon.sprite = beastRemnant;
                        }
                        else if (sceneName.Contains("BossRushStart"))
                        {
                            roomIcon.sprite = bossRushStart;
                        }
                        else if (sceneName.Contains("GateLock"))
                        {
                            roomIcon.sprite = beastGutsGateLock;
                        }
                        else if (sceneName.Contains("BeastGuts2x1PreBoss"))
                        {
                            roomIcon.sprite = beastGutsPreBoss;
                        }
                        break;
                }

                var rectTransform = roomIcon.transform as RectTransform;
                rectTransform.pivot = new Vector2(0.5f, 0.5f);

                if (roomIcon && roomIcon.sprite)
                {
                    rectTransform.sizeDelta = roomIcon.sprite.rect.size;
                }
                else
                {
                    if (roomAbstract.assignedRoomInfo.roomType == RoomType.MegaBeast)
                    {
                        rectTransform.sizeDelta = new Vector2(roomUnit.x * 4, roomUnit.y * 2);
                    }
                    else
                    {
                        rectTransform.sizeDelta = roomUnit;
                    }
                }

                var position = new Vector2(roomAbstract.gridPosition.x + roomAbstract.width * 0.5f, roomAbstract.gridPosition.y - roomAbstract.height * 0.5f);
                rectTransform.anchoredPosition = new Vector2((position.x - offset.x) * roomUnit.x, (position.y - offset.y) * roomUnit.y);
                _roomIcons.Add(roomAbstract.roomID, roomIcon);
                roomIcon.enabled = false;
            }

            int eci = -1;
            if (exterminator && exterminatorRoomIndices.ContainsKey(roomAbstract))
            {
                eci = exterminatorRoomIndices[roomAbstract];
            }

            for (int x = 0; x < roomAbstract.width; x++)
            {
                for (int y = 0; y < roomAbstract.height; y++)
                {
                    var position = new Vector2(roomAbstract.gridPosition.x + x, roomAbstract.gridPosition.y - y);

                    var mapSegment = new GameObject().AddComponent<Image>();
                    mapSegment.name = position.ToString();
                    mapSegment.transform.SetParent(mapContainer);
                    mapSegment.transform.localScale = Vector3.one;

                    if(eci >= 0) { _exterminatorSegmentColorIndex[mapSegment.name] = eci; }

                    var spriteIndex = 0;
                    if(roomAbstract.height > 0)
                    {
                        if(y != 0)
                            spriteIndex += 1;

                        if(y != roomAbstract.height-1)                        
                            spriteIndex += 2;                        
                    }

                    if(roomAbstract.width > 0)
                    {
                        if(x != 0)
                            spriteIndex += 4;

                        if(x != roomAbstract.width-1)
                            spriteIndex += 8;
                    }

                    mapSegment.sprite = autoMapTiles[spriteIndex];

                    var rectTransform = mapSegment.transform as RectTransform;
                    rectTransform.pivot = new Vector2(0, 1);
                    rectTransform.sizeDelta = roomUnit;
                    rectTransform.anchoredPosition = new Vector2((position.x - offset.x) * roomUnit.x, (position.y - offset.y) * roomUnit.y);
                    var localGridPosition = new Int2D(x, (int)roomAbstract.height - 1 - y);

                    var minorItems = roomAbstract.minorItems.FindAll(i => i.spawnInfo.localGridPosition == localGridPosition);
                    foreach (var minorItem in minorItems)
                    {
                        var image = CreateSprite(itemUncollected, rectTransform);
                        image.name = minorItem.type.ToString() + " " + minorItem.spawnInfo.localGridPosition;
                        if (minorItem.globalID != -99)
                        {
                            _minorItems.Add(minorItem.globalID, image);
                        }
                    }

                    if (roomAbstract.majorItem != 0 && roomAbstract.assignedRoomInfo.majorItemLocalPosition == localGridPosition)
                    {
                        var image = CreateSprite(itemUncollected, rectTransform);
                        image.name = roomAbstract.majorItem.ToString() + " " + roomAbstract.assignedRoomInfo.majorItemLocalPosition;
                        _majorItems.Add(roomAbstract.majorItem, image);
                    }

                    _exits.Add(mapSegment.name, new Dictionary<int, Image>());
                    var exits = roomAbstract.GetExitsAtLocalPosition(localGridPosition);

                    foreach (var exit in exits)
                    {
                        var exitSprite = new GameObject().AddComponent<Image>();
                        var exitRect = exitSprite.GetComponent<RectTransform>();
                        exitRect.SetParent(rectTransform);
                        exitRect.localScale = Vector2.one;
                        exitSprite.name = exit.direction.ToString();
                        var smallSize = 1;
                        switch (exit.direction)
                        {
                            case Direction.Up:
                                exitRect.anchoredPosition = new Vector2(0, (roomUnit.y - smallSize) / 2);
                                exitRect.sizeDelta = new Vector2(roomUnit.y / 2, smallSize);
                                break;
                            case Direction.Down:
                                exitRect.anchoredPosition = new Vector2(0, -(roomUnit.y - smallSize) / 2);
                                exitRect.sizeDelta = new Vector2(roomUnit.y / 2, smallSize);
                                break;
                            case Direction.Left:
                                exitRect.anchoredPosition = new Vector2(-(roomUnit.x - smallSize) / 2, 0);
                                exitRect.sizeDelta = new Vector2(smallSize, roomUnit.y / 2);
                                break;
                            case Direction.Right:
                                exitRect.anchoredPosition = new Vector2((roomUnit.x - smallSize) / 2, 0);
                                exitRect.sizeDelta = new Vector2(smallSize, roomUnit.y / 2);
                                break;
                        }

                        if (exit.toExit.requiredDamageType.HasFlag(DamageType.Fire))
                        {
                            exitSprite.color = new Color32(255, 128, 0, 255);
                        }

                        if (exit.toExit.requiredDamageType.HasFlag(DamageType.Electric))
                        {
                            exitSprite.color = new Color(230, 230, 0, 255);
                        }

                        if (exit.toExit.requiredDamageType.HasFlag(DamageType.Explosive))
                        {
                            exitSprite.color = Color.red;
                        }

                        if (exit.toExit.requiredDamageType.HasFlag(DamageType.Velocity))
                        {
                            exitSprite.color = Color.green;
                        }

                        if (exit.toExit.requiredDamageType.HasFlag(DamageType.Mechanical))
                        {
                            exitSprite.color = new Color32(164, 164, 64, 255);
                        }

                        if (_exits[mapSegment.name].ContainsKey(exit.id))
                        {
                            Debug.LogWarning("exit.id " + exit.id + " already exists in Dictionary for mapSegment " + mapSegment.name);
                        }
                        else
                        {
                            _exits[mapSegment.name].Add(exit.id, exitSprite);
                        }
                    }

                    if (roomAbstract.environmentalEffect.HasFlag(EnvironmentalEffect.Underwater)) { _waterSegments.Add(mapSegment.name); }
                    if (roomAbstract.environmentalEffect.HasFlag(EnvironmentalEffect.Darkness)) { _darkSegments.Add(mapSegment.name); }
                    if (roomAbstract.environmentalEffect.HasFlag(EnvironmentalEffect.Heat)) { _heatSegments.Add(mapSegment.name); }
                    if (roomAbstract.environmentalEffect.HasFlag(EnvironmentalEffect.Confusion)) { _confuseSegments.Add(mapSegment.name); }

                    RefreshMapSegment(mapSegment);

                    if(_mapSegments.ContainsKey(mapSegment.name))
                    {
                        Debug.LogError("Map Segment Named " + mapSegment.name + " is being added twice for a layout with the following password " + _layout.password);
                    }

                    _mapSegments.Add(mapSegment.name, mapSegment);
                    _envTypes.Add(mapSegment.name, roomAbstract.assignedRoomInfo.environmentType);

                    if(roomAbstract.assignedRoomInfo.environmentType == EnvironmentType.BeastGuts && !SaveGameManager.beastGutsUnlocked)
                    {
                        _unrevealableSegments.Add(mapSegment.name);
                    }

                    if(roomAbstract.assignedRoomInfo.environmentType == EnvironmentType.Glitch)
                    {
                        _unrevealableSegments.Add(mapSegment.name);
                    }
                }
            }
        }

        RefreshItems();

        player.onCollectItem += RefreshItems;
        player.onGridPositionChanged += Refresh;
    }

    public void HighlightSquare(Int2D gridPosition, float time)
    {
        var highlight = GetAvailableHighlight();
        var offset = new Vector2(_visibleLayoutWidth / 2f, _layout.height / 2f);
        highlight.rectTransform.anchoredPosition = new Vector2((gridPosition.x - offset.x) * roomUnit.x, (gridPosition.y - offset.y) * roomUnit.y);
        StartCoroutine(HighlightSquare(highlight, time));
    }

    public Image HighlightSquare(Int2D gridPosition, Color color)
    {
        var highlight = GetAvailableHighlight();
        var offset = new Vector2(_visibleLayoutWidth / 2f, _layout.height / 2f);
        highlight.rectTransform.anchoredPosition = new Vector2((gridPosition.x - offset.x) * roomUnit.x, (gridPosition.y - offset.y) * roomUnit.y);
        highlight.gameObject.SetActive(true);
        highlight.color = color;
        return highlight;
    }

    public Image GetAvailableHighlight()
    {
        var highlight = highlightSquares.FirstOrDefault((i) => !i.gameObject.activeInHierarchy);
        if (!highlight)
        {
            highlight = CreateSprite(this.highlight, mapContainer);
            highlight.rectTransform.pivot = new Vector2(0, 1);
            highlight.rectTransform.sizeDelta = roomUnit;
            highlight.name = "highlight";
            highlightSquares.Add(highlight);
        }

        return highlight;
    }

    private void OnDestroy() { if (instance == this) { instance = null; } }

    public void RevealMap(Vector2 gridPosition, int range, bool perimeterOnly)
    {
        if (LayoutManager.instance && SaveGameManager.activeGame != null)
        {
            int minY = (int)Mathf.Clamp((gridPosition.y - range), 0, LayoutManager.instance.layout.height);
            int maxY = (int)Mathf.Clamp((gridPosition.y + range), 0, LayoutManager.instance.layout.height);
            int minX = (int)Mathf.Clamp((gridPosition.x - range), 0, LayoutManager.instance.layout.width);
            int maxX = (int)Mathf.Clamp((gridPosition.x + range), 0, LayoutManager.instance.layout.width);

            var layout = SaveGameManager.activeGame.layout;
            var currentRoom = layout.GetRoomAtPositon(gridPosition);
            var automapSegmentStates = SaveGameManager.activeGame.automapSegmentStates;
            var roomsDiscovered = SaveGameManager.activeGame.roomsDiscovered;

            if (currentRoom != null)
            {
                for (int y = minY; y <= maxY; y++)
                {
                    for (int x = minX; x <= maxX; x++)
                    {
                        if (perimeterOnly && (y != minY && y != maxY && x != minX && x != maxX))
                        {
                            continue;
                        }

                        var position = new Vector2(x, y);
                        HighlightSquare(new Int2D(x, y), 0.33f);
                        var pos = position.ToString();
                        var room = layout.GetRoomAtPositon(position);
                        if (room != null && !roomsDiscovered.Contains(room.roomID)) { roomsDiscovered.Add(room.roomID); }

                        if (!automapSegmentStates.ContainsKey(pos)) // Hidden spaces won't be shown by this
                        {
                            automapSegmentStates[pos] = AutomapSegmentState.Discovered;
                        }
                    }
                }
            }
        }
        Refresh();
    }

    private IEnumerator HighlightSquare(Image highlight, float time)
    {
        highlight.gameObject.SetActive(true);
        var timer = 0f;
        var halfTime = time * 0.5f;
        while(timer < halfTime)
        {
            timer += Time.unscaledDeltaTime;
            highlight.color = Color.Lerp(Color.clear, Constants.blasterGreen, timer / halfTime);
            yield return null;
        }

        timer = 0f;
        while (timer < halfTime)
        {
            timer += Time.unscaledDeltaTime;
            highlight.color = Color.Lerp(Constants.blasterGreen, Color.clear, timer / halfTime);
            yield return null;
        }

        highlight.gameObject.SetActive(false);
    }

    public Image CreateSprite(Sprite sprite, RectTransform parentRectTransform)
    {
        var image = new GameObject().AddComponent<Image>();
        image.sprite = sprite;
        var spriteRect = image.GetComponent<RectTransform>();
        spriteRect.SetParent(parentRectTransform);
        spriteRect.localScale = Vector2.one;
        spriteRect.anchoredPosition = Vector2.zero;
        spriteRect.sizeDelta = roomUnit;
        return image;
    }

    public AutomapSegmentState RefreshMapSegment(Image mapSegment)
    {
#if SHOWFULLMAP
        mapSegment.gameObject.SetActive(true);
        mapSegment.color = Constants.blasterGreen;
        return;
#else
        AutomapSegmentState mapSegmentState;

        var activeGame = SaveGameManager.activeGame;
        if (activeGame != null && activeGame.automapSegmentStates.TryGetValue(mapSegment.name, out mapSegmentState))
        {
            EnvironmentType environmentType;            
            if (LayoutManager.instance.currentEnvironment == EnvironmentType.Glitch && 
                _envTypes.TryGetValue(mapSegment.name, out environmentType) &&
                environmentType != EnvironmentType.Glitch)
            {
                mapSegment.gameObject.SetActive(false);
                return mapSegmentState;
            }

            mapSegment.gameObject.SetActive(mapSegmentState != AutomapSegmentState.Hidden);
            
            Color unvisitedColor = Constants.unvistedGreen;
            Color visitedColor = Constants.blasterGreen;

            if (_exterminatorSegmentColorIndex != null)
            {
                int exterminatorColorIndex;
                if (_exterminatorSegmentColorIndex.TryGetValue(mapSegment.name, out exterminatorColorIndex))
                {
                    visitedColor = exterminatorColors[exterminatorColorIndex];
                    unvisitedColor = visitedColor * 0.5f;
                    unvisitedColor.a = 1;
                }
            }
            else
            {
                var segName = mapSegment.name;
                if (_heatSegments.Contains(segName))
                {
                    visitedColor = new Color(1, 0.5f, 0, 1);
                    unvisitedColor = visitedColor * 0.5f;
                    unvisitedColor.a = 1;
                }
                else if (_darkSegments.Contains(segName))
                {
                    visitedColor = Color.gray;
                    unvisitedColor = visitedColor * 0.5f;
                    unvisitedColor.a = 1;
                }
                else if(_confuseSegments.Contains(segName))
                {
                    visitedColor = new Color(1, 0, 1, 1);
                    unvisitedColor = visitedColor * 0.5f;
                    unvisitedColor.a = 1;
                }
                else if(_waterSegments.Contains(segName))
                {
                    visitedColor = Color.blue;
                    unvisitedColor = visitedColor * 0.5f;
                    unvisitedColor.a = 1;
                }
            }

            switch (mapSegmentState)
            {
                case AutomapSegmentState.Discovered:
                    mapSegment.color = unvisitedColor;
                    break;
                case AutomapSegmentState.Visited:
                    mapSegment.color = visitedColor;
                    break;
            }

            foreach (var kvp in _exits[mapSegment.name])
            {
                if (activeGame.doorsOpened.Contains(kvp.Key)) { kvp.Value.color = Color.white; }
            }
            return mapSegmentState;
        }
        else
        {
            mapSegment.gameObject.SetActive(false);
            return AutomapSegmentState.Hidden;
        }
#endif
    }

    public void RefreshItems()
    {
        if (SaveGameManager.activeGame == null) { return; }

        foreach (var kvp in _minorItems)
        {
            kvp.Value.sprite = SaveGameManager.activeGame.minorItemIdsCollected.Contains(kvp.Key) ? itemCollected : itemUncollected;
        }

        foreach (var kvp in _majorItems)
        {
            kvp.Value.sprite = SaveGameManager.activeGame.itemsCollected.Contains(kvp.Key) ? itemCollected : itemUncollected;
        }
    }

    public void Refresh()
    {
        if (SaveGameManager.activeGame == null) { return; }

        bool allRevealed = true;
        foreach (var keyValuePair in _mapSegments)
        {
            var segment = RefreshMapSegment(keyValuePair.Value);
            if (allRevealed && !(segment == AutomapSegmentState.Visited || segment == AutomapSegmentState.Discovered) && !_unrevealableSegments.Contains(keyValuePair.Key))
            {
                allRevealed = false;
            }
        }

        if (SaveGameManager.activeGame != null)
        {
            if(allRevealed)
            {
                AchievementManager.instance.TryEarnAchievement(AchievementID.MasterMap);
            }

            Image image;

            foreach (var kvp in _roomIcons)
            {
                if (_mapSegments.TryGetValue(kvp.Key, out image) && !image.isActiveAndEnabled)
                {
                    kvp.Value.enabled = false;
                    continue;
                }

                kvp.Value.enabled = SaveGameManager.activeGame.roomsDiscovered.Contains(kvp.Key);

                if (kvp.Value.enabled && kvp.Value.sprite == shop && SaveGameManager.activeGame.shopsEmptied.Contains(kvp.Key))
                {
                    kvp.Value.sprite = itemCollected;
                }
            }
        }
    }

    public void Update()
    {
        bool glitchWorld = LayoutManager.instance.currentEnvironment == EnvironmentType.Glitch;
        bool blueKey = (player && player.blueKey);
        if (glitchWorld && !blueKey)
        {
            mapContainer.gameObject.SetActive(false);
            playerDot.gameObject.SetActive(false);
            questionMarks.gameObject.SetActive(true);
            if(_expanded)
            {
                Retract();
            }
            return;
        }
        else
        {
            mapContainer.gameObject.SetActive(true);
            playerDot.gameObject.SetActive(true);
            questionMarks.gameObject.SetActive(false);
        }

        Vector2 containerPosition;
        if (_expanded && !glitchWorld)
        {
            containerPosition = new Vector2(-_visibleLayoutWidth/2 * Constants.roomWidth, -_layout.height/2 * Constants.roomHeight);
            var dotPosition = Constants.WorldToLayoutPosition(player.mainCamera.transform.position) - new Vector2(_visibleLayoutWidth/2, _layout.height/2);
            dotPosition.x *= roomUnit.x;
            dotPosition.y *= roomUnit.y;
            playerDot.localPosition = dotPosition;
        }
        else
        {
            _expandedOffset = Vector2.zero;
            _expandedOffsetRounded = Vector2.zero;
            containerPosition = -player.mainCamera.transform.position;
            playerDot.localPosition = Vector2.zero;
        }

        containerPosition = TranslatePosition(containerPosition);
        containerPosition += _expandedOffsetRounded;
        mapContainer.localPosition = containerPosition;
        playerDot.localPosition += (Vector3)_expandedOffsetRounded;

        if (_expanded && !_gridSelectMode)
        {
            var x = player.controller.GetAxis("WeaponsHorizontal");
            var y = player.controller.GetAxis("WeaponsVertical");
            if (x != 0 || y != 0)
            {
                _expandedOffset.x -= x * Time.deltaTime * 48;
                _expandedOffset.y -= y * Time.deltaTime * 48;
                ProcessExpandedOffset(glitchWorld);
            }
        }

        if (_gridSelectMode)
        {
            TileSelectUpdate();
            return;
        }

        if (!_expanded && !PauseMenu.instance.visible && player.controller.GetButtonDown("ExpandMap")) { Expand(); }
        if (_expanded && (player.controller.GetButtonUp("ExpandMap") || PauseMenu.instance.visible)) { Retract(); }
    }

    public Vector3 TranslatePosition(Vector3 position)
    {
        position.x += (_visibleLayoutWidth / 2) * Constants.roomWidth;
        position.y += (_layout.height / 2) * Constants.roomHeight;
        position.x *= roomUnit.x / Constants.roomWidth;
        position.y *= roomUnit.y / Constants.roomHeight;
        position.x = Mathf.Round(position.x) - roomUnit.x / 2f;
        position.y = Mathf.Round(position.y) + roomUnit.y / 2f;
        return position;
    }

    public void ProcessExpandedOffset(bool glitchWorld, bool sound = true)
    {
        var maxX = (glitchWorld ? _layout.width + 3 : _visibleLayoutWidth + 3) * roomUnit.x;
        maxX -= _rectTransform.sizeDelta.x;
        maxX += roomUnit.x;
        maxX *= 0.5f;

        var maxY = (_layout.height + 3) * roomUnit.y;
        maxY -= _rectTransform.sizeDelta.y;
        maxY += roomUnit.y;
        maxY *= 0.5f;

        if (_expandedOffset.x > maxX) { _expandedOffset.x = maxX; }
        if (_expandedOffset.x < -maxX) { _expandedOffset.x = -maxX; }
        if (_expandedOffset.y > maxY) { _expandedOffset.y = maxY; }
        if (_expandedOffset.y < -maxY) { _expandedOffset.y = -maxY; }

        var factor = 8;
        var newRoundX = Mathf.Round(_expandedOffset.x / factor) * factor;
        var newRoundY = Mathf.Round(_expandedOffset.y / factor) * factor;
        if (_expandedOffsetRounded.x != newRoundX || _expandedOffsetRounded.y != newRoundY)
        {
            if (sound) { UISounds.instance.Blip(); }
            _expandedOffsetRounded.x = newRoundX;
            _expandedOffsetRounded.y = newRoundY;
        }
    }

    public void TileSelectUpdate()
    {
        if (SaveGameManager.activeGame != null)
        {
            if (_selectionDelay > 0)
            {
                _selectionDelay -= Time.unscaledDeltaTime;
            }
            else
            {
                var xAxis = player.controller.GetAxis("UIHorizontal");

                bool newSelection = false;
                int currentIndex = 0;

                if (Mathf.Abs(xAxis) > 0.33f)
                {
                    _selectableGridSpaces.Sort(delegate (Int2D one, Int2D two)
                    {
                        if (one.x == two.x) return 0;
                        else if (one.x > two.x) return 1;
                        else return -1;
                    });

                    currentIndex = (_selectableGridSpaces.IndexOf(_highlightedPos) + (xAxis > 0 ? 1 : -1)) % _selectableGridSpaces.Count;
                    if (currentIndex < 0) currentIndex = _selectableGridSpaces.Count + currentIndex;
                    newSelection = true;
                }

                var yAxis = player.controller.GetAxis("UIVertical");

                if (Mathf.Abs(yAxis) > 0.33f)
                {
                    Debug.Log("Map Menu Vertical");
                    _selectableGridSpaces.Sort(delegate (Int2D one, Int2D two)
                    {
                        if (one.y == two.y) return 0;
                        else if (one.y > two.y) return 1;
                        else return -1;
                    });

                    currentIndex = (_selectableGridSpaces.IndexOf(_highlightedPos) + (yAxis > 0 ? 1 : -1)) % _selectableGridSpaces.Count;
                    if (currentIndex < 0) currentIndex = _selectableGridSpaces.Count + currentIndex;
                    newSelection = true;
                }

                if(newSelection)
                {
                    if(_selectRoutine != null) { StopCoroutine(_selectRoutine); }
                    _selectRoutine = AnimateGridSelect(_selectableGridSpaces[currentIndex]);
                    StartCoroutine(_selectRoutine);
                    _selectionDelay = 0.5f;
                }
            }
        }

        if (_highlightedPos != player.gridPosition.Int2D() && _selectRoutine == null)
        {
            UIConfirmCancel.instance.confirm.SetActive(true);
            if (player.controller.GetButtonDown("UISubmit"))
            {
                UISounds.instance.Confirm();
                StartCoroutine(EndTeleporterMode());
                if (onSelectGridSpace != null) onSelectGridSpace(_highlightedPos);
            }
        }
        else
        {
            UIConfirmCancel.instance.confirm.SetActive(false);
        }

        if (player.controller.GetButtonDown("UICancel")) { StartCoroutine(EndTeleporterMode()); }
    }

    public IEnumerator AnimateGridSelect(Int2D newPos)
    {
        var oldPosition = _highlightedPos;
        _highlightedPos = newPos;
        _highlightedImage.gameObject.SetActive(false);
        var direction = (_highlightedPos - oldPosition).Vector2().normalized;
        Vector2 newPosition = oldPosition.Vector2();
        while (oldPosition != _highlightedPos)
        {
            newPosition = Vector2.MoveTowards(newPosition, _highlightedPos.Vector2(), direction.magnitude);
            var testPosition = newPosition.Int2D();
            if (testPosition != oldPosition)
            {
                CenterExpandedOn(Constants.LayoutToWorldPosition(testPosition));
                oldPosition = testPosition;
                HighlightSquare(testPosition, 0.2f);
            }
            yield return null;
        }
        CenterExpandedOn(Constants.LayoutToWorldPosition(_highlightedPos));
        _highlightedImage = HighlightSquare(_highlightedPos, Color.yellow);
        UISounds.instance.OptionChange();
        _selectRoutine = null;
    }

    public IEnumerator EndTeleporterMode()
    {
        if (_selectRoutine != null) { StopCoroutine(_selectRoutine); }
        foreach (var highlight in highlightSquares)
        {
            highlight.gameObject.SetActive(false);
        }
        UISounds.instance.Cancel();
        UIConfirmCancel.instance.Hide();
        Time.timeScale = 1;
        Retract();
        yield return null; //wait a frame to prevent retriggering teleporter bounds
        _gridSelectMode = false;
    }

    public void Expand()
    {
        _expanded = true;
        _rectTransform.localPosition = Vector3.zero;
        var size = _originalSize;
        size.x *= 4.5f;
        size.y *= 4.5f;
        _rectTransform.sizeDelta = size;

        CenterExpandedOn(player.mainCamera.transform.position);
    }

    public void CenterExpandedOn (Vector3 pos)
    {
        var newCenter = TranslatePosition(-pos);
        var trueCenter = TranslatePosition(new Vector2(-_visibleLayoutWidth / 2 * Constants.roomWidth, -_layout.height / 2 * Constants.roomHeight));
        _expandedOffset = newCenter - trueCenter;
        bool glitchWorld = LayoutManager.instance.currentEnvironment == EnvironmentType.Glitch;
        ProcessExpandedOffset(glitchWorld, false);
    }

    public void Retract()
    {
        _expanded = false;
        _rectTransform.localPosition = _originalPosition;
        _rectTransform.sizeDelta = _originalSize;
    }

    public void OpenGridSelect(List<Int2D> selectableGridSpaces, Action<Int2D> callback)
    {
        onSelectGridSpace = callback;
        _selectableGridSpaces = selectableGridSpaces != null ? new List<Int2D>(selectableGridSpaces) : null;

        Expand();
        UIConfirmCancel.instance.Show(false, true);
        _gridSelectMode = true;
        _highlightedPos = player.gridPosition.Int2D();
        _highlightedImage = HighlightSquare(_highlightedPos, Color.yellow);
        CenterExpandedOn(Constants.LayoutToWorldPosition(_highlightedPos));
        Time.timeScale = 0;
    }
}
