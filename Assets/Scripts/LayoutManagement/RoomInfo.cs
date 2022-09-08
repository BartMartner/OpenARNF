using UnityEngine;
using System.Collections;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using Newtonsoft.Json;

[Serializable]
public class RoomInfo
{
    [JsonProperty(PropertyName = "scnNm")]
    public string sceneName;

    [JsonProperty(PropertyName = "versionCreated")]
    public string versionCreated = "1.1.1.13";

    [JsonProperty(PropertyName = "envTyp")]
    public EnvironmentType environmentType;

    public Int2D size;

    [JsonProperty(PropertyName = "travLim")]
    public TraversalLimitations traversalLimitations;

    [Range(0.01f,2f)]
    [JsonProperty(PropertyName = "bsWght")]
    public float baseWeight = 1f;

    [JsonProperty(PropertyName = "reqEx")]
    public List<ExitLimitations> requiredExits = new List<ExitLimitations>();

    [JsonProperty(PropertyName = "posEx")]
    public List<ExitLimitations> possibleExits = new List<ExitLimitations>();

    [JsonProperty(PropertyName = "travPths")]
    public List<TraversalPath> traversalPaths = new List<TraversalPath>();

    [JsonProperty(PropertyName = "minItLoc")]
    public List<MinorItemSpawnInfo> minorItemLocations = new List<MinorItemSpawnInfo>();

    [JsonProperty(PropertyName = "prmObjCnt")]
    public int permanentStateObjectCount;

    [JsonProperty(PropertyName = "transTo")]
    public EnvironmentType transitionsTo;

    [Tooltip("If true, this room can not be the dead end item room at the end of a minor item path.")]
    [JsonProperty(PropertyName = "minItPthOn")]
    public bool minorItemAlongPathOnly;

    [JsonProperty(PropertyName = "rmTyp")]
    public RoomType roomType;

    [JsonProperty(PropertyName = "pStrt")]
    public Vector3 playerStartOffset;

    [JsonProperty(PropertyName = "pFc")]
    public Direction playerStartFacing = Direction.Right;

    [DataMember]
    public BossName boss;

    [JsonProperty(PropertyName = "shpTyp")]
    public ShopType shopType;

    [JsonProperty(PropertyName = "shrnTyp")]
    public ShrineType shrineType;

    [JsonProperty(PropertyName = "mjItmLPos")]
    public Int2D majorItemLocalPosition;

    [JsonProperty(PropertyName = "itmRmLim")]
    public List<MajorItem> itemRoomLimitations = new List<MajorItem>();

    [JsonProperty(PropertyName = "tsDir")]
    public Direction transitionStartDirection;

    [JsonProperty(PropertyName = "rnThr")]
    [Tooltip("Currently only works in DEBUG mode. Doesn't account for seed.")]
    public int runThreshhold;

    [JsonProperty(PropertyName = "rqAch")]
    public List<AchievementID> requiredAchievements;

    public int CountRequiredExitsInDirection(Direction direction)
    {
        int exitCount = 0;
        foreach (var exit in requiredExits)
        {
            if (exit.direction == direction)
            {
                exitCount++;
            }
        }

        return exitCount;
    }

    public bool HasPossibleExitAtLocalPosition(Int2D localPosition, Direction direction)
    {
        foreach (var exit in possibleExits)
        {
            if (exit.direction == direction && exit.localGridPosition == localPosition)
            {
                return true;
            }
        }

        return false;
    }

    /// <summary>
    ///Assumes that grid position describes the bottom left corner of the top left block of the room
    /// </summary>
    public Rect GetRectAtGridPosition(Int2D gridPosition)
    {
        var rect = new Rect();
        rect.xMin = gridPosition.x;
        rect.xMax = gridPosition.x + size.x;
        rect.yMax = gridPosition.y + 1;
        rect.yMin = gridPosition.y + 1 - size.y;
        return rect;
    }

    public bool SuitableBranchingRoom(TraversalCapabilities currentCapabilities, EnvironmentalEffect environmentalEffect = EnvironmentalEffect.None)
    {
        return requiredExits.Count == 0 && HasAllPossibleExitsForSize() && !traversalPaths.Any(p => !p.limitations.CapabilitesSufficient(currentCapabilities, environmentalEffect)) &&
            (traversalLimitations == null || traversalLimitations.CapabilitesSufficient(currentCapabilities, environmentalEffect));
    }

    public bool HasAllPossibleExitsForSize()
    {
        return possibleExits.Count == (size.x + size.y) * 2;
    }

    public bool HasPossibleVerticalExits()
    {
        foreach (var exit in possibleExits)
        {
            if (exit.direction == Direction.Up || exit.direction == Direction.Down)
            {
                return true;
            }
        }

        return false;
    }

    public bool HasPossibleHorizontalExits()
    {
        foreach (var exit in possibleExits)
        {
            if (exit.direction == Direction.Right || exit.direction == Direction.Left)
            {
                return true;
            }
        }

        return false;
    }

    public bool HasRequiredExitInDirection(Direction direction)
    {
        foreach (var exit in requiredExits)
        {
            if (exit.direction == direction)
            {
                return true;
            }
        }

        return false;
    }

    public bool HasRequiredExitMatch(ExitLimitations e)
    {
        return requiredExits.Any((exit) => e.localGridPosition == exit.localGridPosition && e.direction == exit.direction);
    }


    public bool HasPossibleExitInDirection(Direction direction)
    {
        foreach (var exit in possibleExits)
        {
            if (exit.direction == direction) { return true; }
        }

        return false;
    }

    public bool HasPossibleExitInDirection(Direction direction, TraversalRequirements requirements)
    {
        foreach (var exit in possibleExits)
        {
            if (exit.direction == direction && exit.toExit.CanSatisfyRequirements(requirements))
            {
                return true;
            }
        }

        return false;
    }

    public RoomMatchResult MatchesAbstract(RoomAbstract roomAbstract)
    {
        //check size
        if (size.x != roomAbstract.width || size.y != roomAbstract.height)
        {
            return RoomMatchResult.WrongSize;
        }

        if (roomType == RoomType.StartingRoom && !(roomAbstract.isStartingRoom || roomAbstract.isEnvironmentStart))
        {
            return RoomMatchResult.MustBeStartRoom;
        }

        if(roomType == RoomType.ItemRoom && roomAbstract.majorItem == 0 && roomAbstract.minorItems.Count == 0)
        {
            return RoomMatchResult.MustBeItemRoom;
        }

        if(roomAbstract.minorItems.Count > minorItemLocations.Count)
        {
            return RoomMatchResult.TooManyMinorItems;
        }

        if(!traversalLimitations.CapabilitesSufficient(roomAbstract.expectedCapabilities))
        {
            return RoomMatchResult.RoomLimitationMismatch;
        }

        if (itemRoomLimitations.Count > 0 && !itemRoomLimitations.Contains(roomAbstract.majorItem))
        {
            return RoomMatchResult.ItemNotSupported;
        }

        if (possibleExits.Count < roomAbstract.exits.Count)
        {
            return RoomMatchResult.TooFewExits;
        }

        foreach (var exitAbstract in roomAbstract.exits)
        {
            var exit = possibleExits.Find(e => e.CanSupportExitAbstract(exitAbstract));
            if (exit == null)
            {
                return RoomMatchResult.PossibleExitMismatch;
            }
        }

        foreach (var exitLimitation in requiredExits)
        {
            var exit = roomAbstract.exits.Find(e => exitLimitation.CanSupportExitAbstract(e));
            if (exit == null)
            {
                return RoomMatchResult.RequiredExitMismatch;
            }
        }

        foreach (var minorItem in roomAbstract.minorItems)
        {
            if (!minorItemLocations.Any(l => l.localGridPosition == minorItem.spawnInfo.localGridPosition &&
                                             l.conflictingExits.Count == minorItem.spawnInfo.conflictingExits.Count &&
                                             l.localID == minorItem.spawnInfo.localID))
            {
                return RoomMatchResult.MinorItemMismatch;
            }
        }

        if(roomAbstract.traversalPathRequirements.Count > traversalPaths.Count) //Should this be !=???
        {
            return RoomMatchResult.TraversalPathCountMismatch;
        }

        foreach (var path in traversalPaths)
        {
            if (!roomAbstract.traversalPathRequirements.Any(p => path.limitations.CanSatisfyRequirements(p)))
            {
                return RoomMatchResult.TraversalPathRequirementMismatch;
            }
        }

        return RoomMatchResult.Success;
    }
}

[Serializable]
public class TraversalPath
{
    public TraversalPathPoint from;
    public TraversalPathPoint to;
    [JsonProperty(PropertyName = "lim")]
    public TraversalLimitations limitations;
    [JsonProperty(PropertyName = "recip")]
    public bool reciprocal;

    public bool FitsDirection(Vector2 direction, bool tightFit)
    {
        var toMid = to.GetMidPoint();
        var fromMid = from.GetMidPoint();
        var pathDirection = new Vector2(Extensions.SignOrZero(toMid.x - fromMid.x), Extensions.SignOrZero(toMid.y - fromMid.y));

        if (((!tightFit && pathDirection.x == 0) || pathDirection.x == Extensions.SignOrZero(direction.x)) && ((!tightFit && pathDirection.y == 0) || pathDirection.y == Extensions.SignOrZero(direction.y)))
        {
            return true;
        }
        else if (reciprocal)
        {
            pathDirection = new Vector2(Extensions.SignOrZero(fromMid.x - toMid.x), Extensions.SignOrZero(fromMid.y - toMid.y));
            return ((!tightFit && pathDirection.x == 0) || pathDirection.x == Extensions.SignOrZero(direction.x)) && ((!tightFit && pathDirection.y == 0) || pathDirection.y == Extensions.SignOrZero(direction.y));
        }
        else
        {
            return false;
        }
    }
}
