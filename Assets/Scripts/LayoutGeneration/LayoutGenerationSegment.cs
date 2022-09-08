using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

/// <summary>
/// A set of parameters that get used by the layout generator
/// A segment consists of a long hub path consisting of one or more rooms traveling in a certain direction
/// and some number of spoke paths coming off of branch rooms along the hub
/// </summary>
[Serializable]
public class LayoutGenerationSegment
{
    public EnvironmentType environmentType;
    public int id;

    /// <summary>
    /// A pre-existing room this segment is being created from. This will typically be the finalBranch of the last segment (until I change environment stuff)
    /// </summary>
    [NonSerialized]
    public RoomAbstract sourceRoom;

    /// <summary>
    /// A branch room left over for the next segment to connect to
    /// </summary>
    [NonSerialized]
    public RoomAbstract finalBranch;

    /// <summary>
    /// Should a SaveRoom Generate along this path.
    /// </summary>
    [NonSerialized]
    public bool hasSaveRoom;

    /// <summary>
    /// Should paths with similar direction have linking cross paths?
    /// </summary>
    public bool hasLinkingPaths;

    /// <summary>
    /// How many dead end branches should the path generate?
    /// </summary>
    public int deadEnds;

    /// <summary>
    /// the index of the max item in this segment
    /// </summary>
    [NonSerialized]
    public int minCapabilitiesIndex;

    [Header("Hub Data")]
    public int hubPathLength;

    [Tooltip("The main direciton of the segment's hub")]
    public Vector2 hubDirection;

    [Tooltip("Setting this will rotate the direction of the hub path + or - the specified angle. This occurs regardless of whether or not setHubDirectionPerpindicular is set.")]
    public float hubDirectionDeviation = 0f;

    [Tooltip("If this is false, the main path generated for this segment will end in a normal room instead of an item room")]
    public bool itemAtEndOfHubPath = true;

    [Header("Source Room")]

    [Tooltip("Where to spawn the source room for this segment")]
    public Int2D sourcePosition = Int2D.negOne;

    [Tooltip("Can spokes come off of the starting room?")]
    public bool sourceIsBranchRoom = true;

    [Tooltip("should the created source room be marked as an environment start")]
    public bool sourceIsEnvironmentStart;

    [Tooltip("should the created source room be marked as the starting room for the layout")]
    public bool sourceIsStartingRoom;

    [Tooltip("the direction the starting room needs to face")]
    public Direction startingRoomDirection;

    [Header("Other")]
    public int minSpokeLength = 2;
    public int maxSpokeLength = 7;
    public int numberOfItems;

    [Tooltip ("Specific items this segment should add")]
    public MajorItem[] specificItems;

    [Tooltip("The number of branch rooms needed along this segment's hub. -1 = auto-calculate.")]
    public int branchRoomsNeeded = -1;

    [Tooltip("For handling the special connection between the first area and the second.")]
    public bool finalTraversalStart;

    [Tooltip("Set true if generator shouldn't generate items after boss rooms. noItemsAfterBosses override itemAtEndOfPath if bossPaths contains(0)")]
    public bool noItemsAfterBosses;

    [Tooltip("don't count items in segment as traversal items.")]
    public bool noTraversalItems;
    
    [Tooltip("this segment needs a connector branch to be used later")]
    public List<EnvironmentType> neededConnectorBranches;

    [Tooltip("if this is true the last branch along the path will always be start of the next segment")]
    public bool saveLastBranchForNextSegment;

    /// <summary>
    /// describes which paths should have bosses at the end of them. 0 is the hub path and 1 to N are the spokes
    /// </summary>
    public List<int> bossPaths = new List<int>();

    [Tooltip("if this is true calls to CreatePathBetweenRooms will use size bonus")]
    public bool sizeBonus;

    /// <summary>
    /// A list containing pools of specific rooms that at least one branch must end with.
    /// </summary>
    public List<RoomTypeSceneList> specificBranchEnds;

    /// <summary>
    /// A specific room to place before the boss room
    /// </summary>
    public string preBossRoom;

    public void CalculateBranchesNeeded()
    {
        //unless otherwise specified, the layout needs a spoke for each item                    
        branchRoomsNeeded = numberOfItems + deadEnds;
        if (specificBranchEnds != null) { branchRoomsNeeded += specificBranchEnds.Count; }

        if (itemAtEndOfHubPath) // noItemsAfterBosses override itemAtEndOfPath if bossPaths contains(0)
        {
            if (bossPaths.Contains(0) && noItemsAfterBosses)
            {
                Debug.LogWarning("Segment " + id + " has both an item and a boss at the end of the hub path, but noItemsAfterBosses is set. segment.itemAtEndOfHubPath set to false by generator.");
                itemAtEndOfHubPath = false;
            }
            else if (numberOfItems <= 0)
            {
                Debug.LogWarning("Segment " + id + " has itemAtEndOfPath set true but numberOfItems set to 0. segment.itemAtEndOfHubPath set to false by generator.");
                itemAtEndOfHubPath = false;
            }
            else //if (itemAtEndOfHubPath)
            {
                branchRoomsNeeded -= 1;
            }
        }

        if (neededConnectorBranches != null)
        {
            branchRoomsNeeded += neededConnectorBranches.Count;
        }
    }

    public int GetRandomBranchPathLength(MicrosoftRandom random)
    {
        return random.Range(minSpokeLength, maxSpokeLength+1);
    }
}

[Serializable]
public class RoomTypeSceneList
{
    public RoomType type;
    public List<string> sceneNames;
}
