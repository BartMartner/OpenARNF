using System;
using UnityEngine;

public enum RoomMatchResult
{
    Success = 0,
    WrongSize = 1,
    MustBeStartRoom = 2,
    MustBeItemRoom = 4,
    TooManyMinorItems = 5,
    ItemNotSupported = 6,
    TooFewExits = 7,
    TooManyExits = 8,
    PossibleExitMismatch = 9,
    RequiredExitMismatch = 10,
    MinorItemMismatch = 11,    
    TraversalPathRequirementMismatch = 12,    
    TraversalPathCountMismatch = 13,
    RoomLimitationMismatch = 14,
}

public class SetSceneErrorReporter
{
    private RoomMatchResult _errorCode;

    public void TrySetErrorCode(RoomMatchResult errorCode)
    {
        if(_errorCode < errorCode)
        {
            _errorCode = errorCode;
        }
    }

    public void LogResult()
    {
        switch (_errorCode)
        {
            case RoomMatchResult.WrongSize:
                Debug.LogWarning("Size of RoomInfo and Abstract did not match");
                break;
            case RoomMatchResult.MustBeStartRoom:
                Debug.LogWarning("RoomInfo is a start or transition room. Abstract is not.");
                break;
            case RoomMatchResult.MustBeItemRoom:
                Debug.LogWarning("match must be an item room.");
                break;
            case RoomMatchResult.TooManyMinorItems:
                Debug.LogWarning("match did not support as many minor items as abstract had.");
                break;
            case RoomMatchResult.ItemNotSupported:
                Debug.LogWarning("match did not support the desired item.");
                break;
            case RoomMatchResult.TooFewExits:
                Debug.LogWarning("match had too few exits.");
                break;
            case RoomMatchResult.TooManyExits:
                Debug.LogWarning("match requires more exits than abstract needs.");
                break;
            case RoomMatchResult.PossibleExitMismatch:
                Debug.LogWarning("RoomInfo doesn't contain a possible exit required by the RoomAbstract.");
                break;
            case RoomMatchResult.RequiredExitMismatch:
                Debug.LogWarning("RoomInfo has required exits that do not match the RoomAbstact.");
                break;
            case RoomMatchResult.MinorItemMismatch:
                Debug.LogWarning("RoomInfo does not support minor items presnt in the RoomAbstract");
                break;
            case RoomMatchResult.TraversalPathCountMismatch:
                Debug.LogWarning("RoomAbstract has more TraversalPathRequirements than RoomInfo.");
                break;
            case RoomMatchResult.TraversalPathRequirementMismatch:
                Debug.LogWarning("RoomInfo has traversal paths that don't match those found in RoomAbstract");
                break;
            case RoomMatchResult.RoomLimitationMismatch:
                Debug.LogWarning("RoomAbstract's expected capabilities aren't sufficient for RoomInfo's limitations");
                break;
            default:
                Debug.LogWarning("Room Match Successful");
                break;
        }
    }
}

