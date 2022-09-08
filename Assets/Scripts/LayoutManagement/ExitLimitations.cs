using System.Collections.Generic;
using System;
using UnityEngine;
using Newtonsoft.Json;

[Serializable]
public class ExitLimitations
{
    [JsonProperty(PropertyName ="locGrd")]
    public Int2D localGridPosition;

    [JsonProperty(PropertyName = "dir")]
    public Direction direction;

    public Int2DDirection posDir { get { return new Int2DDirection() { direction = direction, position = localGridPosition }; } }

    /// <summary>
    /// toExit TraversalInfo describes the abilities need to leave the room through this exit.
    /// It supports every damage type by default (door colors can be set to support any damage type).
    /// </summary>
    public TraversalLimitations toExit = new TraversalLimitations() { supportedDamageTypes = ~(DamageType)0 };

    public bool MatchExists(RoomAbstract roomAbstract)
    {
        //TODO: Compare to code in Room.AssignAbstract that matches transitions to exits and improve this
        var validExit = roomAbstract.exits.Find(e => CanSupportExitAbstract(e));
        return validExit != null;
    }

    public bool CanSupportExitAbstract(ExitAbstract exitAbstract)
    {
        if(exitAbstract.direction == direction && exitAbstract.localGridPosition == localGridPosition)
        {
            return toExit.CanSatisfyRequirements(exitAbstract.toExit);
        }
        else
        {
            return false;
        }
    }

    public bool MatchesPosAndDir(ExitLimitations other)
    {
        return localGridPosition == other.localGridPosition && direction == other.direction;
    }

    /// <summary>
    /// Does this exit's cardinal direction match the y or x component of a given Vector2 direction
    /// </summary>    
    public bool FitsDirection(Vector2 direction, bool tightFit = true)
    {
        if (tightFit)
        {
            return (direction.y > 0 && this.direction == Direction.Up) ||
                    (direction.y < 0 && this.direction == Direction.Down) ||
                    (direction.x > 0 && this.direction == Direction.Right) ||
                    (direction.x < 0 && this.direction == Direction.Left);
        }
        else
        {
            return (direction.y >= 0 && this.direction == Direction.Up) ||
                    (direction.y <= 0 && this.direction == Direction.Down) ||
                    (direction.x >= 0 && this.direction == Direction.Right) ||
                    (direction.x <= 0 && this.direction == Direction.Left);
        }
    }

    /// <summary>
    /// Does this exit's cardinal direction match the y or x component of a given Int2D direction
    /// </summary>    
    public bool FitsDirection(Int2D direction, bool tightFit = true)
    {
        if (tightFit)
        {
            return (direction.y > 0 && this.direction == Direction.Up) ||
                    (direction.y < 0 && this.direction == Direction.Down) ||
                    (direction.x > 0 && this.direction == Direction.Right) ||
                    (direction.x < 0 && this.direction == Direction.Left);
        }
        else
        {
            return (direction.y >= 0 && this.direction == Direction.Up) ||
                    (direction.y <= 0 && this.direction == Direction.Down) ||
                    (direction.x >= 0 && this.direction == Direction.Right) ||
                    (direction.x <= 0 && this.direction == Direction.Left);
        }
    }
}
