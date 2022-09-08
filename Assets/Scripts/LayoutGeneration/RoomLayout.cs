using System.Collections.Generic;
using System;
using UnityEngine;
using System.Linq;
using Newtonsoft.Json;

[Serializable]
public class RoomLayout
{
    public const int standardWidth = 52;
    public const int standardHeight = 40;
    public const int standardTraversalItemCount = 7;
    public const int standardMinorItemCount = 32;

    [JsonProperty(PropertyName = "strtPos")]
    public Int2D startingPosition;
    public int seed;
    public int width;
    public int height;
    [JsonProperty(PropertyName = "travItm")]
    public int traversalItemCount;
    [JsonProperty(PropertyName = "minItmCnt")]
    public int minorItemCount;
    [JsonProperty(PropertyName = "minItm")]
    public List<MinorItemType> minorItemsAdded = new List<MinorItemType>();
    [JsonProperty(PropertyName = "rmAbs")]
    public List<RoomAbstract> roomAbstracts = new List<RoomAbstract>();
    [JsonProperty(PropertyName = "itmOrd")]
    public List<MajorItem> itemOrder;
    [JsonProperty(PropertyName = "nonTravs")]
    public List<MajorItem> allNonTraversalItemsAdded = new List<MajorItem>();
    [JsonProperty(PropertyName = "bonusItms")]
    public List<MajorItem> bonusItemsAdded = new List<MajorItem>();
    [JsonProperty(PropertyName = "shopItms")]
    public List<MajorItem> shopItemsAdded = new List<MajorItem>();
    [JsonProperty(PropertyName = "envLims")]
    public Dictionary<EnvironmentType, Rect> environmentLimits = new Dictionary<EnvironmentType, Rect>();
    [JsonProperty(PropertyName = "travCaps")]
    public List<TraversalCapabilities> traversalCapabilities;
    [JsonProperty(PropertyName = "rmCnt")]
    public Dictionary<string, int> roomCounts = new Dictionary<string, int>();
    [JsonProperty(PropertyName = "bosses")]
    public List<BossName> bossesAdded = new List<BossName>();
    [JsonProperty(PropertyName = "itmCnt")]
    public int traversalItemsAdded;
    [JsonProperty(PropertyName = "ptrnid")]
    public int patternID;
    [JsonProperty(PropertyName = "glch")]
    public bool hasGlitchWorld;
    [JsonProperty(PropertyName = "pswrd")]
    public string password;
    [JsonProperty(PropertyName = "gameMode")]
    public GameMode gameMode = GameMode.Normal;
    [JsonProperty(PropertyName = "envOrdr")]
    public EnvironmentType[] environmentOrder;

    [JsonIgnore]
    //Meant to help with certain layout stuff. First intended for linking up glitch world
    public List<LayoutPathInfo> pathInfos = new List<LayoutPathInfo>();

    [JsonIgnore]
    //to place teleporters
    public List<int> connectingPaths = new List<int>();

    [JsonIgnore]
    public LayoutPattern pattern;

    /// <summary>
    /// Branches saved for later connection
    /// </summary>
    [JsonProperty(PropertyName = "conBrnch")]
    public Dictionary<string, int> connectorBranches = new Dictionary<string, int>();

    public int totalStartingMajorItems;
    [JsonProperty(PropertyName = "StrtPsv")]
    public MajorItem startingPassiveItem;
    [JsonProperty(PropertyName = "StrtAct")]
    public MajorItem startingActivatedItem;
    [JsonProperty(PropertyName = "StrtMin")]
    public MinorItemType startingMinorItem;
    [JsonProperty(PropertyName = "StrtWep")]
    public MajorItem startingWeapon;
    [JsonProperty(PropertyName = "StrtOrb")]
    public MajorItem startingFollower;

    //grid with positional indices to rooms corresponding to those positions
    [JsonPropertyAttribute]
    private int[,] _grid;

    private Dictionary<string, int> _backUpConnectorBranches;
    private List<BossName> _backUpBossesAdded;
    private List<RoomAbstract> _backUpRoomAbstracts;    
    private Dictionary<string, int> _backUpRoomCounts;
    private List<MinorItemType> _backUpMinorItemsAdded;    
    private List<MajorItem> _backupAllNonTraversalItemsAdded;    
    private List<MajorItem> _backupBonusItemsAdded;    
    private List<MajorItem> _backupShopItemsAdded;
    private List<LayoutPathInfo> _backupPathInfos;
    private int[,] _backUpGrid;

    public RoomLayout() { }

    public void Initialize(LayoutPattern pattern)
    {
        this.pattern = pattern;
        Initialize(pattern.width, pattern.height, pattern.minorItemCount, pattern.traversalItemCount);
    }

    public void Initialize(int width, int height, int minorItemCount, int traversalItemCount)
    {
        this.traversalItemCount = traversalItemCount;
        this.minorItemCount = minorItemCount;
        this.width = width;
        this.height = height;

        _grid = new int[width, height];
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                _grid[x, y] = -1;
            }
        }

        traversalItemsAdded = 0;
        totalStartingMajorItems = 0;

        startingActivatedItem = MajorItem.None;
        startingFollower = MajorItem.None;
        startingPassiveItem = MajorItem.None;
        startingMinorItem = MinorItemType.None;
        startingWeapon = MajorItem.None;

        roomAbstracts.Clear();
        roomCounts.Clear();
        bossesAdded.Clear();
        minorItemsAdded.Clear();
        allNonTraversalItemsAdded.Clear();
        bonusItemsAdded.Clear();
        shopItemsAdded.Clear();
        connectorBranches.Clear();
    }

    public void SetEnvironmentOrder(LayoutPattern pattern)
    {
        var order = new HashSet<EnvironmentType>();
        foreach (var segment in pattern.segments)
        {
            if (segment.environmentType == EnvironmentType.Glitch || segment.environmentType == EnvironmentType.BeastGuts) continue;
            order.Add(segment.environmentType);
        }
        order.Add(EnvironmentType.BeastGuts);
        order.Add(EnvironmentType.Glitch);

        environmentOrder = order.ToArray();
    }

    public void CreateUndo()
    {
        _backUpGrid = _grid.Clone() as int[,];
        _backUpRoomCounts = new Dictionary<string, int>(roomCounts);
        _backUpBossesAdded = new List<BossName>(bossesAdded);
        _backUpConnectorBranches = new Dictionary<string, int>(connectorBranches);
        _backUpMinorItemsAdded = new List<MinorItemType>(minorItemsAdded);
        _backupAllNonTraversalItemsAdded = new List<MajorItem>(allNonTraversalItemsAdded);
        _backupBonusItemsAdded = new List<MajorItem>(bonusItemsAdded);
        _backupShopItemsAdded = new List<MajorItem>(shopItemsAdded);

        _backUpRoomAbstracts = new List<RoomAbstract>();
        foreach (var room in roomAbstracts)
        {
            _backUpRoomAbstracts.Add(new RoomAbstract(room));
        }

        _backupPathInfos = new List<LayoutPathInfo>();
        foreach (var info in pathInfos)
        {
            _backupPathInfos.Add(new LayoutPathInfo(info));
        }
    }

    public void Undo()
    {
        Debug.Log("Layout Undo Called");
        if (_backUpGrid == null || _backUpRoomAbstracts == null)
        {
            Debug.LogError("Call CreateUndo before calling Undo!");
            return;
        }

        _grid = _backUpGrid.Clone() as int[,];
        var maxCount = roomAbstracts.Count > _backUpRoomAbstracts.Count ? roomAbstracts.Count : _backUpRoomAbstracts.Count;

        for (int i = 0; i < maxCount; i++)
        {
            var backUp = i < _backUpRoomAbstracts.Count ? _backUpRoomAbstracts[i] : null;
            if (i < roomAbstracts.Count)
            {
                if (backUp != null)
                {
                    roomAbstracts[i].CopyFrom(backUp);
                }
                else
                {
                    roomAbstracts[i] = null;
                }
            }
            else if (backUp != null)
            {
                roomAbstracts.Add(backUp);
            }
        }

        roomAbstracts.RemoveAll(r => r == null);

        pathInfos = new List<LayoutPathInfo>(_backupPathInfos);
        roomCounts = new Dictionary<string, int>(_backUpRoomCounts);
        connectorBranches = new Dictionary<string, int>(_backUpConnectorBranches);
        bossesAdded = new List<BossName>(_backUpBossesAdded);
        minorItemsAdded = new List<MinorItemType>(_backUpMinorItemsAdded);
        allNonTraversalItemsAdded = new List<MajorItem>(_backupAllNonTraversalItemsAdded);
        bonusItemsAdded = new List<MajorItem>(_backupBonusItemsAdded);
        shopItemsAdded = new List<MajorItem>(_backupShopItemsAdded);

        _backUpGrid = null;
        _backUpRoomAbstracts = null;
        _backupPathInfos = null;
        _backUpRoomCounts = null;
        _backUpBossesAdded = null;
        _backUpMinorItemsAdded = null;
        _backupAllNonTraversalItemsAdded = null;
        _backupBonusItemsAdded = null;
        _backupShopItemsAdded = null;
    }

    public bool ValidRoomPlacement(Int2D position, Int2D size, int buffer = 0)
    {
        //A 2x3 room with a buffer of 1 will be searched in this order
        //1, 6 ,11
        //2,(7),12
        //3,(8),13
        //4,(9),14
        //5, 10,15

        for (int x = -buffer; x < size.x+ buffer; x++)
        {
            for (int y = -buffer; y < size.y+ buffer; y++)
            {
                var p = new Int2D(position.x + x, position.y - y); //y is subtracted here because rooms are placed by their top left corner (not the room's local 0,0)
                if (p.x >= width || p.y >= height || p.x < 0 || p.y < 0) { return false; }
                if (_grid[p.x, p.y] != -1) { return false; }
            }
        }

        return true;
    }

    public bool ContainsPosition(Int2D position)
    {
        return position.x < width && position.y < height && position.x >= 0 && position.y >= 0;
    }

    public bool ContainsPosition(Int2D position, EnvironmentType environmentType)
    {
        if(!environmentLimits.ContainsKey(environmentType))
        {
            Debug.LogError("ContainsPosition called for " + environmentType + " when layout does not contain " + environmentType);
            return false;
        }
        var limit = environmentLimits[environmentType];
        return position.x < limit.max.x && position.y < limit.max.y && position.x >= limit.min.x && position.y >= limit.min.y;
    }

    //TODO: Create a bool TryAddRoom(RoomAbstact roomAbstract) method that's more secure?
    public RoomAbstract Add(RoomAbstract roomAbstract, int seed)
    {
        int roomIndex = -1;

        if (roomAbstracts.Contains(roomAbstract))
        {
            roomIndex = roomAbstracts.IndexOf(roomAbstract);
        }
        else
        {
            roomAbstracts.Add(roomAbstract);
            roomIndex = roomAbstracts.Count - 1;
        }

        roomAbstract.index = roomIndex;
        var positionX = roomAbstract.gridPosition.x;
        var positionY = roomAbstract.gridPosition.y;

        for (int x = 0; x < roomAbstract.width; x++)
        {
            for (int y = 0; y < roomAbstract.height; y++)
            {
                var p = new Int2D(positionX + x, positionY - y);
                if(_grid[p.x, p.y] == roomIndex)
                {
                    continue;
                }
                else if (_grid[p.x, p.y] == -1)
                {
                    _grid[p.x, p.y] = roomIndex;
                }
                else
                {
                    Debug.LogError("Attempting to add room (" + roomAbstract.assignedRoomInfo.sceneName + ") " +
                        "over top of an existing room(" + GetRoomAtPositon(p).assignedRoomInfo.sceneName + ") at position " + p.ToString());
                }
            }
        }

        roomAbstract.layout = this;
        roomAbstract.seed = seed;

        if (roomAbstract.isStartingRoom)
        {
            startingPosition = roomAbstract.gridPosition;
        }

        if (!roomCounts.ContainsKey(roomAbstract.assignedRoomInfo.sceneName))
        {
            roomCounts.Add(roomAbstract.assignedRoomInfo.sceneName, 1);
        }
        else
        {
            roomCounts[roomAbstract.assignedRoomInfo.sceneName]++;
        }

        return roomAbstract;
    }

    public RoomAbstract WeightedRoomPick(List<RoomAbstract> roomAbstracts, MicrosoftRandom random, bool sizeBonus = false)
    {
        var list = new List<RoomInfo>();
        foreach (var a in roomAbstracts)
        {
            list.Add(a.assignedRoomInfo);
        }

        var pick = WeightedRoomPick(list, random, sizeBonus);
        var i = list.IndexOf(pick);
        return roomAbstracts[i];
    }

    public RoomInfo WeightedRoomPick(List<RoomInfo> roomInfos, MicrosoftRandom random, bool sizeBonus = false)
    {
        var weights = new Dictionary<String, float>();
        int roomCount;
        var roomWeight = 0f;
        float totalWeight = 0f;

        foreach (var room in roomInfos)
        {
            if (roomCounts.TryGetValue(room.sceneName, out roomCount))
            {
                var divisor = (room.size.x == 1 && room.size.y == 1) ? ((roomCount + 2) * (roomCount + 1)) : ((roomCount + 1) * (roomCount + 1));
                roomWeight = room.baseWeight / divisor;
            }
            else
            {
                roomWeight = room.baseWeight;
            }

            if (sizeBonus)
            {
                if (room.size.x > 1)
                {
                    roomWeight *= 4;
                }

                if (room.size.y > 1)
                {
                    roomWeight *= 4;
                }
            }

            weights.Add(room.sceneName, roomWeight);
            totalWeight += roomWeight;
        }

        var rand = random.value;
        foreach (var room in roomInfos)
        {
            roomWeight = weights[room.sceneName];
            float probability = roomWeight / totalWeight;

            if (rand < probability)
            {
                return room;
            }

            rand -= probability;
        }

        return roomInfos.Last();
    }

    public RoomAbstract GetItemRoom(int itemIndex)
    {
        if (itemIndex < itemOrder.Count)
        {
            return roomAbstracts.FirstOrDefault((r) => r.majorItem == itemOrder[itemIndex]);
        }
        else
        {
            return null;
        }
    }

    public int GetIndexOfFirstSuitableCapabilities(TraversalLimitations limitations, EnvironmentalEffect environmentalEffect = EnvironmentalEffect.None)
    {
        if (traversalCapabilities == null) { return -1; }

        for (int i = 0; i < traversalCapabilities.Count; i++)
        {
            if (limitations.CapabilitesSufficient(traversalCapabilities[i], environmentalEffect)) { return i; }
        }

        return -1;
    }

    public int GetIndexOfFirstSuitableCapabilities(TraversalRequirements requirements)
    {
        if(traversalCapabilities == null) { return -1; }

        for (int i = 0; i < traversalCapabilities.Count; i++)
        {
            if (requirements.CapabilitesSufficient(traversalCapabilities[i])) { return i; }
        }

        return -1;
    }

    public RoomAbstract GetSaveRoom(EnvironmentType env)
    {
        return roomAbstracts.FirstOrDefault((r) => r.assignedRoomInfo.roomType == RoomType.SaveRoom && r.assignedRoomInfo.environmentType == env);
    }

    public RoomAbstract GetTeleporterRoom(EnvironmentType env)
    {
        return roomAbstracts.FirstOrDefault((r) => r.assignedRoomInfo.roomType == RoomType.Teleporter && r.assignedRoomInfo.environmentType == env);
    }

    public RoomAbstract GetShopRoom(ShopType shopType)
    {
        return roomAbstracts.FirstOrDefault((r) => r.assignedRoomInfo.shopType == shopType);
    }

    public RoomAbstract GetShrineRoom(ShrineType shrineType)
    {
        return roomAbstracts.FirstOrDefault((r) => r.assignedRoomInfo.shrineType == shrineType);
    }

    public RoomAbstract GetBossRoom(int bossIndex)
    {
        if(bossIndex < bossesAdded.Count)
        {
            return roomAbstracts.FirstOrDefault((r) => r.assignedRoomInfo.boss == bossesAdded[bossIndex]);
        }
        else
        {
            return null;
        }
    }

    public RoomAbstract GetEnvironmentStart(EnvironmentType env)
    {
        return roomAbstracts.FirstOrDefault((r) => (r.assignedRoomInfo.roomType == RoomType.TransitionRoom && r.assignedRoomInfo.transitionsTo == env) || 
        ((r.isStartingRoom || r.isEnvironmentStart) && r.assignedRoomInfo.environmentType == env));
    }

    public RoomAbstract GetRoomAtPositon(Vector2 position)
    {
        var i = new Int2D((int)position.x, (int)position.y);
        return GetRoomAtPositon(i);
    }

    public RoomAbstract GetRoomAtPositon(Int2D position)
    {
        RoomAbstract roomAbstract = null;
        if (ContainsPosition(position))
        {
            var index = _grid[position.x, position.y];
            if (index > -1)
            {
                roomAbstract = roomAbstracts[index];
            }
        }
        return roomAbstract;
    }

    /*
    public bool MovePositionAwayFromRooms(ref Int2D position, int bufferDistance, Vector2 direction)
    {
        if(PositionHasBuffer(position, bufferDistance))
        {
            return true;
        }

        var checkedPositions = new List<Int2D>() { position };
        var validPositions = new List<Int2D>();
        var preferedValidPositions = new List<Int2D>();
        var preferedValidPositionFound = false;
        var maxAttempts = 20;
        var currentAttempts = 0;

        var xSign = Mathf.Sign(direction.x);
        var ySign = Mathf.Sign(direction.y);

        while (!preferedValidPositionFound && currentAttempts < maxAttempts)
        {
            currentAttempts++;

            var minX = position.x - currentAttempts;
            var maxX = position.x + currentAttempts;
            var minY = position.y - currentAttempts;
            var maxY = position.y + currentAttempts;

            minX = Mathf.Clamp(minX, bufferDistance, width - bufferDistance);
            maxX = Mathf.Clamp(maxX, bufferDistance, width - bufferDistance);
            minY = Mathf.Clamp(minY, bufferDistance, height - bufferDistance);
            minY = Mathf.Clamp(maxY, bufferDistance, height - bufferDistance);

            for (int x = minX; x < maxX; x++)
            {
                for (int y = minY; y < maxY; y++)
                {
                    var posToCheck = new Int2D(x, y);
                    if (!checkedPositions.Contains(posToCheck) && PositionHasBuffer(posToCheck, bufferDistance))
                    {
                        validPositions.Add(posToCheck);
                        var dir = Int2D.Direction(position, posToCheck);
                        if((dir.x == 0 || Math.Sign(dir.x) == xSign) && (dir.y == 0 || Mathf.Sign(dir.y) == ySign))
                        {
                            preferedValidPositionFound = true;
                            preferedValidPositions.Add(posToCheck);
                        }
                    }
                }
            }
        }

        if (validPositions.Count <= 0)
        {
            Debug.LogWarning("MovePositionAwayFromRooms could not move " + position + " to have a buffer of " + bufferDistance + " units");
            return false;
        }
        else
        {
            var shortestDistance = 1000f;
            var list = preferedValidPositions.Count > 0 ? preferedValidPositions : validPositions;
            foreach (var validPos in list)
            {
                var distance = Int2D.Distance(position, validPos);
                if (distance < shortestDistance)
                {
                    shortestDistance = distance;
                    position = validPos;
                }
            }

            return true;
        }
    }

    public bool PositionHasBuffer(Int2D position, int bufferDistance)
    {
        var result = false;

        for (int x = -bufferDistance; x < bufferDistance + 1; x++)
        {
            for (int y = -bufferDistance; y < bufferDistance + 1; y++)
            {
                var testPosition = new Int2D(position.x + x, position.y - y);
                
                if (!ContainsPosition(testPosition) || GetRoomAtPositon(testPosition) != null)
                {
                    return false;
                }
                else
                {
                    result = true;
                }
            }
        }

        return result;
    }
    */

    public bool ValidRect(Rect rect, Rect? boundry)
    {
        //Assume a rect with a yMin of 0 and an xMin of 0 and a width of 3 
        //this will check 0,0 ; 1,0 ; 2,0
        //                0,1 ; 1,1 ; 2,1
        //                0,2 ; 1,2 ; 2,2

        for (int y = (int)rect.yMin; y < rect.yMax; y++)
        {
            for (int x = (int)rect.xMin; x < rect.xMax; x++)
            {
                var position = new Int2D(x, y);
                Rect limit = boundry.HasValue ? boundry.Value : new Rect() { xMin = 0, yMin = 0, xMax = width, yMax = height };
                var containsPos = x < limit.max.x && y < limit.max.y && x >= limit.min.x && y >= limit.min.y;
                if (!containsPos || GetRoomAtPositon(position) != null) { return false; }
            }
        }

        return true;
    }

    public bool ValidRect(Rect rect, Rect limit, out HashSet<Rect> invalidRects)
    {
        invalidRects = new HashSet<Rect>();
        HashSet<RoomAbstract> roomsAdded = new HashSet<RoomAbstract>();

        var xNegBuffer = new Rect() { xMin = -10, xMax = limit.xMin, yMin = 0, yMax = height };
        var xPosBuffer = new Rect() { xMin = limit.xMax, xMax = width + 10, yMin = 0, yMax = height };
        var yNegBuffer = new Rect() { xMin = 0, xMax = width, yMin = -10, yMax = limit.yMin };
        var yPosBuffer = new Rect() { xMin = 0, xMax = width, yMin = limit.yMax, yMax = height + 10 };

        if (rect.xMax > limit.max.x) { invalidRects.Add(xPosBuffer); }
        if (rect.xMin < limit.min.x) { invalidRects.Add(xNegBuffer); }
        if (rect.yMax > limit.max.y) { invalidRects.Add(yPosBuffer); }
        if (rect.yMin < limit.min.y) { invalidRects.Add(yNegBuffer); }

        for (int y = (int)rect.yMin; y < rect.yMax; y++)
        {
            for (int x = (int)rect.xMin; x < rect.xMax; x++)
            {
                //if this point is outside the limits don't concern ourselves with it
                if (x < limit.xMin || x > limit.xMax || y < limit.yMin || y > limit.yMax) { continue; }

                var roomAtPos = GetRoomAtPositon(new Int2D(x, y));
                if (roomAtPos != null && !roomsAdded.Contains(roomAtPos))
                {
                    Rect roomRect = roomAtPos.gridBounds;
                    invalidRects.Add(roomRect);
                    roomsAdded.Add(roomAtPos);
                }
            }
        }

        return invalidRects.Count <= 0;
    }

    public bool MovePositionAwayFromRooms(ref Int2D position, Buffer2D buffer, Vector2 direction, Rect? boundry = null)
    {
        var rect = new Rect() { xMin = position.x, yMin = position.y, xMax = position.x + 1, yMax = position.y+1 };
        var result = MovePositionAwayFromRooms(ref rect, buffer, direction, boundry);
        position = Int2D.GetRoomPosFromRect(rect);
        return result;
    }

    public bool MovePositionAwayFromRooms(ref Rect rect, Buffer2D buffer, Vector2 direction, Rect? boundry = null)
    {
        bool validPosition = false;
        var originalRect = rect;
        int attempts = 0;
        int maxAttempts = 20;
        var invalidBounds = new HashSet<Rect>();
        var verticalD = Mathf.Abs(direction.y) > Mathf.Abs(direction.x);
        Rect limit = boundry.HasValue ? boundry.Value : new Rect() { xMin = 0, yMin = 0, xMax = width, yMax = height };
        var lastSeparation = Vector2.zero;

        while (!validPosition && attempts < maxAttempts)
        {
            attempts++;
            var bufferedRect = new Rect() { xMin = rect.xMin - buffer.left, xMax = rect.xMax + buffer.right, yMin = rect.yMin - buffer.bottom, yMax = rect.yMax + buffer.top };
            validPosition = ValidRect(bufferedRect, limit, out invalidBounds);
            bool lastMoveX = false;

            if (!validPosition)
            {
                foreach (var problemRect in invalidBounds)
                {
                    var minXY = problemRect.min - bufferedRect.max;
                    //can be compared with origin to find separation axis
                    var size = problemRect.size + bufferedRect.size;
                    var minkowskiDiff = new Bounds(minXY + size * 0.5f, size);
                    Vector2 separationVector = Vector2.zero;
                    float minDist = 1000;
                    float d;

                    d = Math.Abs(minkowskiDiff.min.x);
                    if (d < minDist)
                    {
                        var testSep = new Vector2(minkowskiDiff.min.x, 0);
                        if (testSep + lastSeparation != Vector2.zero)
                        {
                            var testRect = bufferedRect;
                            testRect.center += testSep;
                            if (RectContainRect(limit, testRect))
                            {
                                separationVector = testSep;
                                minDist = d;
                            }
                        }
                    }

                    d = Mathf.Abs(minkowskiDiff.max.x);
                    if (d < minDist || (d == minDist && direction.x > 0))
                    {
                        var testSep = new Vector2(minkowskiDiff.max.x, 0);
                        if (testSep + lastSeparation != Vector2.zero)
                        {
                            var testRect = bufferedRect;
                            testRect.center += testSep;
                            if (RectContainRect(limit, testRect))
                            {
                                separationVector = testSep;
                                minDist = d;
                            }
                        }
                    }

                    d = Mathf.Abs(minkowskiDiff.min.y);
                    if (d < minDist || (d == minDist && (direction.y < 0 && verticalD) || (direction.y <= 0 && lastMoveX)))
                    {
                        var testSep = new Vector2(0, minkowskiDiff.min.y);
                        if (testSep + lastSeparation != Vector2.zero)
                        {
                            var testRect = bufferedRect;
                            testRect.center += testSep;
                            if (RectContainRect(limit, testRect))
                            {
                                separationVector = testSep;
                                minDist = d;
                            }
                        }
                    }

                    d = Mathf.Abs(minkowskiDiff.max.y);
                    if (d < minDist || (d == minDist && (direction.y > 0 && verticalD) || (direction.y >= 0 && lastMoveX)))
                    {
                        var testSep = new Vector2(0, minkowskiDiff.max.y);
                        if (testSep + lastSeparation != Vector2.zero)
                        {
                            var testRect = bufferedRect;
                            testRect.center += testSep;
                            if (RectContainRect(limit, testRect))
                            {
                                separationVector = testSep;
                                minDist = d;
                            }
                        }
                    }

                    lastMoveX = separationVector.x != 0;
                    var position = rect.position;
                    position = position + separationVector;
                    lastSeparation = separationVector;
                    rect.position = position;
                    bufferedRect = new Rect() { xMin = rect.xMin - buffer.left, xMax = rect.xMax + buffer.right, yMin = rect.yMin - buffer.bottom, yMax = rect.yMax + buffer.top };
                }
            }
        }

        if (attempts >= maxAttempts)
        {
            rect = originalRect;
            Debug.LogWarning("MovePositionAwayFromRooms got stuck trying to move " + originalRect + "(made more than " + maxAttempts + " attempts. Password: " + password);
            return false;
        }

        if (validPosition)
        {
            return true;
        }
        else
        {
            Debug.LogWarning("Unknown issue in MovePositionAwayFromRooms");
            return false;
        }
    }

    public bool RectContainRect(Rect rect1, Rect rect2)
    {
        if (rect2.xMax > rect1.xMax) return false;
        if (rect2.xMin < rect1.xMin) return false;
        if (rect2.yMax > rect1.yMax) return false;
        if (rect2.yMin < rect1.yMin) return false;
        return true;
    }

    public bool IsRoomLandLocked(RoomAbstract room)
    {
        var aboveY = room.gridPosition.y + 1;
        var belowY = room.gridPosition.y - room.height;
        var leftX = room.gridPosition.x - 1;
        var rightX = room.gridPosition.x + room.width;

        for (int i = 0; i < room.height; i++)
        {
            var adjacentRoom = GetRoomAtPositon(new Vector2(rightX, room.gridPosition.y + i));

            if (adjacentRoom == null)
            {
                return false;
            }

            adjacentRoom = GetRoomAtPositon(new Vector2(leftX, room.gridPosition.y + i));

            if (adjacentRoom == null)
            {
                return false;
            }
        }

        for (int i = 0; i < room.width; i++)
        {
            var adjacentRoom = GetRoomAtPositon(new Vector2(room.gridPosition.x + i, aboveY));

            if (adjacentRoom == null)
            {
                return false;
            }

            adjacentRoom = GetRoomAtPositon(new Vector2(room.gridPosition.x + i, belowY));

            if (adjacentRoom == null)
            {
                return false;
            }
        }

        return true;
    }

    public ExitAbstract GetConnectedExit(ExitAbstract exit)
    {
        var target = exit.TargetPosition();
        var destination = GetRoomAtPositon(target);
        return destination == null ? null : destination.exits.FirstOrDefault(e => (e.direction == exit.direction.Opposite() && e.globalGridPosition == target));
    }
}
