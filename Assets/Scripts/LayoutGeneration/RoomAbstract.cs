using System;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System.Runtime.Serialization;
using Newtonsoft.Json;

[Serializable]
public class RoomAbstract
{
    public int index;

    /// <summary>
    /// the grid index of the rooms top-most, left-most grid space. DOES NOT NECESSARILY CORRESPOND TO LOCAL 0,0
    /// </summary>
    public Int2D gridPosition;

    [JsonProperty(PropertyName = "rmnfo")]
    public RoomInfo assignedRoomInfo;

    [JsonProperty(PropertyName = "envFX")]
    public EnvironmentalEffect environmentalEffect = 0;

    [JsonProperty(PropertyName = "strtRm")]
    public bool isStartingRoom;

    [JsonProperty(PropertyName = "envStrt")]
    public bool isEnvironmentStart;

    [JsonProperty(PropertyName = "altSet")]
    public bool useAltTileset;

    [JsonProperty(PropertyName = "altPal")]
    public int altPalette;

    [JsonProperty(PropertyName = "brnchRm")]
    public bool branchRoom;

    [JsonProperty(PropertyName = "preBsRm")]
    public BossName preBossRoom;

    [JsonProperty(PropertyName = "svID")]
    public int saveID;

    [JsonProperty(PropertyName = "majItm")]
    public MajorItem majorItem = MajorItem.None;

    [JsonProperty(PropertyName = "minItm")]
    public List<MinorItemData> minorItems = new List<MinorItemData>();

    [JsonProperty(PropertyName = "permanentStateObjects")]
    public List<int> permanentStateObjectGlobalIds = new List<int>();

    /// <summary>
    /// When CreatePathBetweenRooms is called, a LayoutPathInfo is added to the layout. This is the index of that path.
    /// </summary>
    [JsonProperty(PropertyName = "pthid")]
    public int parentPath = -1;

    public int width = 1;
    public int height = 1;
    public List<ExitAbstract> exits = new List<ExitAbstract>();

    [JsonProperty(PropertyName = "trvRqs")]
    public List<TraversalRequirements> traversalPathRequirements = new List<TraversalRequirements>();

    public int seed;

    [JsonProperty(PropertyName = "shop")]
    public List<MajorItem> shopOfferings = new List<MajorItem>();

    public float light = 1;

    [JsonProperty(PropertyName = "exCaI")]
    //The anticipated number of traversal items the player will have collected when they first reach this room
    public int expectedCapabilitiesIndex;

    [NonSerialized]
    public bool wasReplaced;

    [NonSerialized]
    public RoomLayout layout;

    [NonSerialized]
    public Dictionary<Int2D, int> specialExpectedCapabilities;

    public TraversalCapabilities expectedCapabilities
    {
        get
        {
            if(layout == null && LayoutManager.instance && LayoutManager.instance.layout != null)
            {
                layout = LayoutManager.instance.layout;
            }

            if(layout != null && layout.traversalCapabilities != null && layout.traversalCapabilities.Count > expectedCapabilitiesIndex)
            {
                return layout.traversalCapabilities[expectedCapabilitiesIndex];
            }
            else
            {
                return null;
            }
        }
    }
    
    public string roomID
    {
        get { return gridPosition.ToString() + " " + seed.ToString(); }
    }

    public Vector3 worldPosition
    {
        get
        {
            //TODO: decide if this is center or top-left (currently its the position of top-most left-most grid space's center)
            return new Vector3(gridPosition.x * Constants.roomWidth, gridPosition.y * Constants.roomHeight, 0);
        }
    }

    public Int2D gridCenter
    {
        get
        {
            return new Int2D(Mathf.FloorToInt(gridPosition.x + width * 0.5f), Mathf.CeilToInt(gridPosition.y - height * 0.5f));
        }
    }

    //Assumes that grid position describes the bottom left corner of the top left block of the room
    public Vector2 nonDiscreteCenter
    {
        get
        {
            return new Vector2(gridPosition.x + width * 0.5f, gridPosition.y + 1f - height * 0.5f);
        }
    }

    /// <summary>
    /// A weird quirk of the grid is that it presumes each space is discrete. GridPosition describes the top left discrete space.
    /// So think of grid position as a dot in the center of a block on graph paper.
    /// To describe non-discrete rectangles accurately, such that they play nice with environment bounds we need to assume 
    /// that the grid position of a room describes the bottom left corner of the block that has the dot in the center
    /// So yMax is actually gridPosition.y + 1, while  yMin is gridPosition.y + 1 - height
    /// </summary>
    public Rect gridBounds
    {
        get
        {
            return new Rect() { xMin = gridPosition.x, xMax = gridPosition.x + width, yMin = gridPosition.y + 1 - height, yMax = gridPosition.y + 1 };
        }
    }

    public RoomAbstract() { }
    public RoomAbstract(RoomInfo roomInfo, Int2D gridPosition)
    {
        width = roomInfo.size.x;
        height = roomInfo.size.y;
        assignedRoomInfo = roomInfo;
        this.gridPosition = gridPosition;
    }

    public RoomAbstract(RoomAbstract original)
    {
        index = original.index;
        gridPosition = original.gridPosition;
        assignedRoomInfo = original.assignedRoomInfo;
        environmentalEffect = original.environmentalEffect;
        isStartingRoom = original.isStartingRoom;
        isEnvironmentStart = original.isEnvironmentStart;
        altPalette = original.altPalette;
        useAltTileset = original.useAltTileset;
        branchRoom = original.branchRoom;
        preBossRoom = original.preBossRoom;
        saveID = original.saveID;
        majorItem = original.majorItem;
        minorItems = new List<MinorItemData>(original.minorItems);
        permanentStateObjectGlobalIds = new List<int>(original.permanentStateObjectGlobalIds);
        parentPath = original.parentPath;
        width = original.width;
        height = original.height;
        exits = new List<ExitAbstract>(original.exits);
        traversalPathRequirements = new List<TraversalRequirements>(original.traversalPathRequirements);
        seed = original.seed;
        shopOfferings = new List<MajorItem>(original.shopOfferings);
        light = original.light;
        layout = original.layout;
    }

    /// <summary>
    /// This is need to keep references after undo
    /// </summary>
    /// <param name="original"></param>
    public void CopyFrom(RoomAbstract original)
    {
        index = original.index;
        gridPosition = original.gridPosition;
        assignedRoomInfo = original.assignedRoomInfo;
        environmentalEffect = original.environmentalEffect;
        isStartingRoom = original.isStartingRoom;
        isEnvironmentStart = original.isEnvironmentStart;
        altPalette = original.altPalette;
        useAltTileset = original.useAltTileset;
        branchRoom = original.branchRoom;
        preBossRoom = original.preBossRoom;
        saveID = original.saveID;
        majorItem = original.majorItem;
        minorItems = new List<MinorItemData>(original.minorItems);
        permanentStateObjectGlobalIds = new List<int>(original.permanentStateObjectGlobalIds);
        parentPath = original.parentPath;
        width = original.width;
        height = original.height;
        exits = new List<ExitAbstract>(original.exits);
        traversalPathRequirements = new List<TraversalRequirements>(original.traversalPathRequirements);
        seed = original.seed;
        shopOfferings = new List<MajorItem>(original.shopOfferings);
        light = original.light;
        layout = original.layout;
    }

    public void Expand(Direction direction)
    {
        switch (direction)
        {
            case Direction.Up:
                gridPosition.y++;
                height++;
                break;
            case Direction.Down:
                height++;
                foreach (var exit in exits)
                {
                    exit.localGridPosition.y += 1;
                }
                break;
            case Direction.Left:
                gridPosition.x--;
                width++;
                foreach (var exit in exits)
                {
                    exit.localGridPosition.x += 1;
                }
                break;
            case Direction.Right:
                width++;
                break;
        }
    }

    public int GetExpectedCapabilitiesIndex(Int2D localPosition)
    {
        int cap = expectedCapabilitiesIndex;
        if (specialExpectedCapabilities != null && specialExpectedCapabilities.ContainsKey(localPosition))
        {
            cap = specialExpectedCapabilities[localPosition]; 
        }
        return cap;
    }

    /// <summary>
    /// Finds the grid position in this room closet to the specified grid position
    /// </summary>
    /// <param name="gridPosition">The grid space to compare to</param>
    /// <returns>the global grid position of the space in this room closest to the grid position passed in</returns>
    public Int2D GetClosestGlobalGridPosition(Int2D gridPosition)
    {
        float closestDistance = Int2D.Distance(this.gridPosition, gridPosition);
        Int2D closestPositon = this.gridPosition;
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                var position = this.gridPosition + new Int2D(x, -y);
                var distance = Int2D.Distance(position, gridPosition);
                if(distance < closestDistance)
                {
                    closestDistance = distance;
                    closestPositon = position;
                }
            }
        }

        return closestPositon;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="gridPosition">Find the closest position to this position</param>
    /// <param name="distanceThreshold">The space can't be closer than this threshhold</param>
    /// <returns></returns>
    public Int2D GetClosestGlobalPositionWithExit(Int2D gridPosition, uint distanceThreshold = 0)
    {
        if (assignedRoomInfo == null)
        {
            Debug.LogError("GetClosestGlobalPositionWithExit can't be called on an abstract with no room info.");
            return gridPosition;
        }

        float closestDistance = int.MaxValue;
        Int2D closestPositon = this.gridPosition;

        foreach (var exit in assignedRoomInfo.possibleExits)
        {
            var position = GetGridPosition(exit.localGridPosition);
            var distance = Int2D.Distance(position, gridPosition);
            if (distance < closestDistance && distance >= distanceThreshold)
            {
                closestDistance = distance;
                closestPositon = position;
            }
        }

        return closestPositon;
    }

    public bool GetClosestGlobalPositionWithExit(Int2D gridPosition, out Int2D closestPositon, Func<RoomAbstract, Int2D, bool> validate)
    {
        closestPositon = gridPosition;

        if (assignedRoomInfo == null)
        {
            Debug.LogError("GetClosestGlobalPositionWithExit can't be called on an abstract with no room info.");
            return false;
        }

        bool success = false;
        float closestDistance = int.MaxValue;
        closestPositon = this.gridPosition;

        foreach (var exit in assignedRoomInfo.possibleExits)
        {
            if (!validate(this, GetGridPosition(exit.localGridPosition))) { continue; }

            var target = GetExitTarget(exit);
            var roomAtPosition = layout.GetRoomAtPositon(target);

            if (roomAtPosition == null || validate(roomAtPosition, target))
            {
                var position = GetGridPosition(exit.localGridPosition);
                var distance = Int2D.Distance(position, gridPosition);
                if (distance < closestDistance)
                {
                    success = true;
                    closestDistance = distance;
                    closestPositon = position;
                }
            }
        }

        return success;
    }

    public Int2D GetClosestGlobalPositionWithUsableExitTorwardsTarget(Int2D gridPosition, RoomAbstract permittedRoom)
    {
        if(assignedRoomInfo == null)
        {
            Debug.LogError("FindClosestGlobalPositionWithUsableExitTorwardsTarget can't be called on an abstract with no room info.");
            return gridPosition;
        }

        var exits = GetUsableUnusedExits(permittedRoom);

        float closestDistance = int.MaxValue;
        Int2D closestPositon = this.gridPosition;

        foreach (var exit in exits)
        {
            var position = GetGridPosition(exit.localGridPosition);
            if (exit.FitsDirection((gridPosition - position).Vector2().normalized))
            {
                var distance = Int2D.Distance(position, gridPosition);
                if (distance < closestDistance)
                {
                    closestDistance = distance;
                    closestPositon = position;
                }
            }
        }

        return closestPositon;
    }

    public bool HasAnyMatchingExits(List<Int2DDirection> checkForExits)
    {
        if(checkForExits == null)
        {
            Debug.Log("Null check for exits!");
            return false;
        }

        foreach (var exit in checkForExits)
        {
            if(HasAnyMatchingExits(exit))
            {
                return true;
            }
        }

        return false;
    }

    public bool HasAnyMatchingExits(Int2DDirection checkFor)
    {
        return exits.Any((e) => e.MatchesInt2DDirecitonLocal(checkFor));
    }

    public bool CanSupportMinorItemLocation(MinorItemSpawnInfo spawnInfo)
    {
        return !minorItems.Any((m) => m.spawnInfo == spawnInfo) && !HasAnyMatchingExits(spawnInfo.conflictingExits);
    }

    public MinorItemSpawnInfo GetBestMinorItemSpawn()
    {
        var usableSpots = assignedRoomInfo.minorItemLocations.FindAll((i) => CanSupportMinorItemLocation(i));
        var bestSpot = usableSpots.FirstOrDefault(i => !exits.Any(e => e.localGridPosition == i.localGridPosition));
        return bestSpot != null ? bestSpot : usableSpots.FirstOrDefault();
    }

    public MinorItemSpawnInfo GetFirstUsableMinorItemSpawn()
    {
        return assignedRoomInfo.minorItemLocations.FirstOrDefault((i) => CanSupportMinorItemLocation(i));
    }

    public bool ValidMinorItemHidingSpot()
    {
        if(assignedRoomInfo != null && minorItems.Count < assignedRoomInfo.minorItemLocations.Count)
        {
            foreach (var itemSpawn in assignedRoomInfo.minorItemLocations)
            {
                if(CanSupportMinorItemLocation(itemSpawn))
                {
                    return true;
                }
            }
        }
        
        return false;        
    }

    public Int2D GetLocalPosition(Int2D gridPosition)
    {
        return new Int2D(gridPosition.x - this.gridPosition.x, (int)height - 1 - this.gridPosition.y + gridPosition.y);
    }

    public Int2D GetGridPosition(Int2D localPosition)
    {
        return new Int2D(gridPosition.x + localPosition.x, gridPosition.y - ((int)height - 1 - localPosition.y));
    }

    public Int2D GetExitTarget(ExitLimitations exit)
    {
        return GetGridPosition(exit.localGridPosition) + exit.direction.ToInt2D();
    }

    public bool CanConnectToExit(Int2D exitTarget, Direction exitDirection)
    {
        if (assignedRoomInfo == null || string.IsNullOrEmpty(assignedRoomInfo.sceneName))
        {
            Debug.LogWarning("CanConnectToExit returned false because assignedRoomInfo or assignedRoomInfo.sceneName is null or empty");
            return false;
        }

        return assignedRoomInfo.HasPossibleExitAtLocalPosition(GetLocalPosition(exitTarget), exitDirection.Opposite());
    }

    public List<ExitAbstract> GetExitsAtLocalPosition(Int2D localPosition)
    {
        return exits.Where(e => e.localGridPosition == localPosition).ToList();
    }

    public List<ExitLimitations> GetAllUnusedExits()
    {
        if(assignedRoomInfo == null || string.IsNullOrEmpty(assignedRoomInfo.sceneName))
        {
            Debug.LogWarning("GetUnusedExits failed because assignedRoomInfo or assignedRoomInfo.sceneName is null or empty");
            return null;
        }

        var unusedExits = new List<ExitLimitations>();
        foreach (var exit in assignedRoomInfo.possibleExits)
        {
            if(!exits.Any(e => e.direction == exit.direction && e.localGridPosition == exit.localGridPosition))
            {
                unusedExits.Add(exit);
            }
        }
        return unusedExits;
    }

    public bool HasMinorItemSpotInUnusedSpace()
    {
        if (assignedRoomInfo == null || string.IsNullOrEmpty(assignedRoomInfo.sceneName))
        {
            Debug.LogWarning("HasUnusedExitsInUnusedSpaces failed because assignedRoomInfo or assignedRoomInfo.sceneName is null or empty");
            return false;
        }

        if (width == 1 && height == 1)
        {
            return false;
        }

        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                var position = new Int2D(x, y);
                if (!exits.Any(e => e.localGridPosition == position))
                {
                    var minorItemSpots = assignedRoomInfo.minorItemLocations.FindAll(i => i.localGridPosition == position);
                    foreach (var spot in minorItemSpots)
                    {
                        if (!minorItems.Any(m => m.spawnInfo == spot))
                        {
                            return true;
                        }
                    }
                }
            }
        }
        return false;
    }

    public bool HasUsableUnusedExitsInUnusedSpaces()
    {
        if (assignedRoomInfo == null || string.IsNullOrEmpty(assignedRoomInfo.sceneName))
        {
            Debug.LogWarning("HasUnusedExitsInUnusedSpaces failed because assignedRoomInfo or assignedRoomInfo.sceneName is null or empty");
            return false;
        }

        if (width == 1 && height == 1)
        {
            return false;
        }

        var usableUnused = GetUsableUnusedExits();

        foreach (var unusedExit in usableUnused)
        {
            if (!exits.Any(e => e.localGridPosition.x == unusedExit.localGridPosition.x && e.localGridPosition.y == unusedExit.localGridPosition.y))
            {
                return true;
            }
        }

        return false;
    }

    public bool HasExistingExitToGridPosition(Int2D gridPosition)
    {
        return exits.Any((e) => e.TargetPosition() == gridPosition);
    }

    public bool HasAnyExitToGridPosition(Int2D gridPosition)
    {
        if (assignedRoomInfo == null || string.IsNullOrEmpty(assignedRoomInfo.sceneName))
        {
            Debug.LogWarning("HasAnyExitToGridPosition failed because assignedRoomInfo or assignedRoomInfo.sceneName is null or empty");
            return false;
        }

        return assignedRoomInfo.possibleExits.Any((e) => GetExitTarget(e) == gridPosition);
    }

    public bool HasAnyPathExitToGridPosition(Int2D gridPosition)
    {
        if (assignedRoomInfo == null || string.IsNullOrEmpty(assignedRoomInfo.sceneName))
        {
            Debug.LogWarning("HasAnyExitToGridPosition failed because assignedRoomInfo or assignedRoomInfo.sceneName is null or empty");
            return false;
        }

        var exit = assignedRoomInfo.possibleExits.FirstOrDefault((e) => GetExitTarget(e) == gridPosition);
        if (exit == null) return false;
        if (minorItems.Count == 0) return true;

        bool minorItemConflict = false;
        foreach (var item in minorItems)
        {
            if (item.spawnInfo.conflictingExits.Any((e) => e.direction == exit.direction && e.position == exit.localGridPosition))
            {
                minorItemConflict = true;
                break;
            }
        }

        return !minorItemConflict;
    }

    public bool HasExitInLocalGridPosition(Int2D localGridPosition)
    {
        return exits.Any((e) => e.localGridPosition == localGridPosition);
    }

    public bool IsExitlessSpace(Int2D localGridPosition)
    {
        return !exits.Any(e => e.localGridPosition == localGridPosition);
    }

    public bool HasUnusedSpaces()
    {
        if (assignedRoomInfo == null || string.IsNullOrEmpty(assignedRoomInfo.sceneName))
        {
            Debug.LogWarning("HasUnusedSpaces failed because assignedRoomInfo or assignedRoomInfo.sceneName is null or empty");
            return false;
        }

        if (width == 1 && height == 1)
        {
            return false;
        }

        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                var position = new Int2D(x, y);
                if (!exits.Any(e => e.localGridPosition == position))
                {
                    var minorItemSpots = assignedRoomInfo.minorItemLocations.FindAll(i => i.localGridPosition == position);

                    if (minorItemSpots.Count == 0) return true;

                    foreach (var spot in minorItemSpots)
                    {
                        if (!minorItems.Any(m => m.spawnInfo == spot)) { return true; }
                    }
                }
            }
        }

        return false;
    }

    public bool HasUnusedExitsInUnusedSpaces()
    {
        if (assignedRoomInfo == null || string.IsNullOrEmpty(assignedRoomInfo.sceneName))
        {
            Debug.LogWarning("HasUnusedExitsInUnusedSpaces failed because assignedRoomInfo or assignedRoomInfo.sceneName is null or empty");
            return false;
        }
        
        if(width == 1 && height == 1)
        {
            return false;
        }

        var unused = GetAllUnusedExits();

        foreach (var unusedExit in unused)
        {
            if (!exits.Any(e => e.localGridPosition.x == unusedExit.localGridPosition.x && e.localGridPosition.y == unusedExit.localGridPosition.y))
            {
                return true;
            }
        }

        return false;
    }

    public bool HasUsableUnusedExits(RoomLayout layout)
    {
        if (assignedRoomInfo == null || string.IsNullOrEmpty(assignedRoomInfo.sceneName))
        {
            Debug.LogWarning("GetUnusedExits failed because assignedRoomInfo or assignedRoomInfo.sceneName is null or empty");
            return false;
        }

        foreach (var exit in assignedRoomInfo.possibleExits)
        {
            if (!exits.Any(e => e.direction == exit.direction && e.localGridPosition == exit.localGridPosition))
            {
                var target = GetExitTarget(exit);
                if (layout.ContainsPosition(target) && layout.GetRoomAtPositon(target) == null)
                {
                    bool minorItemConflict = false;

                    if (minorItems.Count > 0)
                    {
                        foreach (var item in minorItems)
                        {
                            if (item.spawnInfo.conflictingExits.Any((e) => e.direction == exit.direction && e.position == exit.localGridPosition))
                            {
                                minorItemConflict = true;
                                break;
                            }
                        }
                    }

                    if (!minorItemConflict)
                    {
                        return true;
                    }
                }
            }
        }

        return false;
    }


    public List<ExitLimitations> GetUsableUnusedExits()
    {
        return _GetUsableUnusedExits(this.layout, null, null);
    }

    public List<ExitLimitations> GetUsableUnusedExits(Func<ExitLimitations, bool> comparison)
    {
        return _GetUsableUnusedExits(this.layout, null, comparison);
    }

    public List<ExitLimitations> GetUsableUnusedExits(RoomAbstract permittedRoom, Func<ExitLimitations, bool> comparison = null)
    {
        return _GetUsableUnusedExits(this.layout, new List<RoomAbstract> { permittedRoom }, comparison);
    }

    public List<ExitLimitations> GetUsableUnusedExits(List<RoomAbstract> permittedRooms, Func<ExitLimitations, bool> comparison = null)
    {
        return _GetUsableUnusedExits(this.layout, permittedRooms, comparison);
    }

    public List<ExitLimitations> GetUsableUnusedExits(RoomLayout layout, RoomAbstract permittedRoom)
    {
        return _GetUsableUnusedExits(layout, new List<RoomAbstract> { permittedRoom });
    }

    public List<ExitLimitations> GetUsableUnusedExits(RoomLayout layout, RoomAbstract permittedRoom, Func<ExitLimitations, bool> comparison)
    {
        return _GetUsableUnusedExits(layout, new List<RoomAbstract> { permittedRoom }, comparison);
    }

    private List<ExitLimitations> _GetUsableUnusedExits(RoomLayout layout, List<RoomAbstract> permittedRooms = null, Func<ExitLimitations, bool> comparison = null)
    {
        if (assignedRoomInfo == null || string.IsNullOrEmpty(assignedRoomInfo.sceneName))
        {
            Debug.LogWarning("GetUnusedExits failed because assignedRoomInfo or assignedRoomInfo.sceneName is null or empty");
            return null;
        }

        if (exits.Count == assignedRoomInfo.possibleExits.Count) { return new List<ExitLimitations>(); }

        var unusedExits = new List<ExitLimitations>();
        foreach (var exit in assignedRoomInfo.possibleExits)
        {
            if (comparison != null && !comparison(exit)) continue;

            if (!exits.Any(e => e.direction == exit.direction && e.localGridPosition == exit.localGridPosition))
            {
                var target = GetExitTarget(exit);
                if (layout.ContainsPosition(target))
                {
                    var room = layout.GetRoomAtPositon(target);
                    if (room == null || (permittedRooms != null && permittedRooms.Contains(room)))
                    {
                        bool minorItemConflict = false;
                        if (minorItems.Count > 0)
                        {
                            foreach (var item in minorItems)
                            {
                                if (item.spawnInfo.conflictingExits.Any((e) => e.direction == exit.direction && e.position == exit.localGridPosition))
                                {
                                    minorItemConflict = true;
                                    break;
                                }
                            }
                        }

                        if (!minorItemConflict) { unusedExits.Add(exit); }
                    }
                }
            }
        }
        return unusedExits;
    }

    public List<RoomAbstract> GetConnectedRooms(Func<RoomAbstract, bool> comparison = null)
    {
        var connectedRooms = new HashSet<RoomAbstract>();
        foreach (var exit in exits)
        {
            var target = exit.TargetPosition();
            var room = layout.GetRoomAtPositon(target);
            if(room != null && (comparison == null || comparison(room)))
            {
                connectedRooms.Add(room);
            }
        }
        return connectedRooms.ToList();
    }

    public bool IsConnectedTo(RoomAbstract room)
    {
        foreach (var exit in exits)
        {
            var target = exit.TargetPosition();
            var r = layout.GetRoomAtPositon(target);
            if (r == room) { return true; }
        }

        return false;
    }

    public bool IsConnectedToAny(List<RoomAbstract> rooms)
    {
        foreach (var exit in exits)
        {
            var target = exit.TargetPosition();
            var r = layout.GetRoomAtPositon(target);
            if (rooms.Contains(r)) { return true; }
        }

        return false;
    }

    public List<RoomAbstract> GetSurroundingRooms(Func<RoomAbstract, bool> comparison = null)
    {
        var surroundingRooms = new HashSet<RoomAbstract>();
        var xMin = gridPosition.x - 1;
        var xMax = gridPosition.x + width;
        var yMin = gridPosition.y - height;
        var yMax = gridPosition.y + 1;

        for (int x = xMin; x <= xMax ; x++)
        {
            var room = layout.GetRoomAtPositon(new Int2D(x, yMax));
            if (room != null && (comparison == null || comparison(room)))
            {
                surroundingRooms.Add(room);
            }

            room = layout.GetRoomAtPositon(new Int2D(x, yMin));
            if (room != null && (comparison == null || comparison(room)))
            {
                surroundingRooms.Add(room);
            }
        }

        for (int y = yMin; y <= yMax; y++)
        {
            var room = layout.GetRoomAtPositon(new Int2D(xMin, y));
            if (room != null && (comparison == null || comparison(room)))
            {
                surroundingRooms.Add(room);
            }

            room = layout.GetRoomAtPositon(new Int2D(xMax, y));
            if (room != null && (comparison == null || comparison(room)))
            {
                surroundingRooms.Add(room);
            }
        }

        return surroundingRooms.ToList();
    }

    public ExitLimitations GetReciprocalExit(ExitAbstract exit)
    {
        return assignedRoomInfo.possibleExits.FirstOrDefault((e) =>
        {
            var gridPosition = GetGridPosition(e.localGridPosition);
            return gridPosition == exit.TargetPosition() && ContainsGridPosition(gridPosition) && e.direction == exit.direction.Opposite();
        });
    }

    public List<Int2D> GetPositionsInRoom()
    {
        var positions = new List<Int2D>();
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                positions.Add(new Int2D(gridPosition.x + x, gridPosition.y - y));
            }
        }
        return positions;
    }

    public bool ContainsGridPosition(Int2D position)
    {
        return position.x >= gridPosition.x && position.x < gridPosition.x + width && position.y <= gridPosition.y && position.y > gridPosition.y - height;
    }
}
