using Newtonsoft.Json;
using System;

[Serializable]
public class ExitAbstract
{
    public int id;
    [JsonProperty(PropertyName = "gPos")]
    public Int2D globalGridPosition;

    /// <summary>
    /// the local grid index of the exit where 0,0 is the bottom most, left most space
    /// </summary>
    [JsonProperty(PropertyName = "locPos")]
    public Int2D localGridPosition;
    [JsonProperty(PropertyName = "dir")]
    public Direction direction;
    [JsonProperty(PropertyName = "2htRm")]
    public bool toHeatRoom;
    [JsonProperty(PropertyName = "2cRm")]
    public bool toConfusionRoom;
    [JsonProperty(PropertyName = "2wtRm")]
    public bool toWaterRoom;
    [JsonProperty(PropertyName = "2bsRm")]
    public bool toBossRoom;

    //In the abstract, these limitations act more as requirements
    public TraversalRequirements toExit = new TraversalRequirements();

    public ExitAbstract() { }

    public ExitAbstract(ExitLimitations limitations, RoomAbstract parent, TraversalRequirements traversalRequirements)
    {
        globalGridPosition = parent.GetGridPosition(limitations.localGridPosition);
        if (!parent.ContainsGridPosition(globalGridPosition))
        {
            var password = parent.layout != null ? parent.layout.password : string.Empty;
            throw new Exception("Exit Abstact in room " + parent.assignedRoomInfo.sceneName + " added at invalid space! password: " + password);
        }

        localGridPosition = limitations.localGridPosition;
        direction = limitations.direction;
        if (traversalRequirements != null)
        {
            toExit = traversalRequirements;
        }
        else
        {
            var password = parent.layout != null ? parent.layout.password : string.Empty;
            throw new Exception("Exit Abstact in room " + parent.assignedRoomInfo.sceneName + " passed null traversalRequirements! password: " + password);
        }
    }

    public ExitAbstract(Int2D globalGridPosition, Int2D localGridPosition, Direction direction, TraversalRequirements traversalRequirements = null)
    {
        this.globalGridPosition = globalGridPosition;
        this.localGridPosition = localGridPosition;
        this.direction = direction;
        if (traversalRequirements != null)
        {
            toExit = traversalRequirements;
        }
        else
        {
            toExit = new TraversalRequirements() { requiredDamageType = DamageType.Generic };
        }
    }

    public Int2D TargetPosition()
    {
        switch (direction)
        {
            case Direction.Up:
                return new Int2D(globalGridPosition.x, globalGridPosition.y + 1);
            case Direction.Down:
                return new Int2D(globalGridPosition.x, globalGridPosition.y - 1);
            case Direction.Left:
                return new Int2D(globalGridPosition.x - 1, globalGridPosition.y);
            case Direction.Right:
                return new Int2D(globalGridPosition.x + 1, globalGridPosition.y);
            default:
                return new Int2D((int)globalGridPosition.x, (int)globalGridPosition.y);
        }
    }

    public bool MatchesInt2DDirecitonLocal(Int2DDirection int2DDirection)
    {
        return int2DDirection.direction == direction && localGridPosition == int2DDirection.position;
    }

    public bool MatchesInt2DDirecitonGlobal(Int2DDirection int2DDirection)
    {
        return int2DDirection.direction == direction && globalGridPosition == int2DDirection.position;
    }
}