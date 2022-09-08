using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[CreateAssetMenu(fileName = "LayoutPattern", menuName = "Layout/Create Layout Pattern", order = 1)]
public class LayoutPattern : ScriptableObject
{
    public int width = RoomLayout.standardWidth;
    public int height = RoomLayout.standardHeight;
    public int traversalItemCount = RoomLayout.standardTraversalItemCount;
    public int minorItemCount = RoomLayout.standardMinorItemCount;

    public int id;
    [EnumFlags]
    public GameMode allowedGameModes;
    public float chanceToConnectNeighborRooms = 0;
    public List<EnvironmentBounds> environmentLimits = new List<EnvironmentBounds>();
    public List<LayoutGenerationSegment> segments;
    public List<SegmentConnection> segmentsToConnect;
    public Rect finalBossZone;
    public bool hasMegaBeast = true;
    public bool hasBeastRemnants = true;

    [NonSerialized]
    private RoomLayout _layout;

    public LayoutGenerationSegment GenerateGlitchWorld(MicrosoftRandom random)
    {
        var glitchLimits = environmentLimits.First((b) => b.type == EnvironmentType.Glitch).bounds;

        var up = random.value > 0.5f;

        var glitch = new LayoutGenerationSegment()
        {
            id = 6,
            environmentType = EnvironmentType.Glitch,
            hubPathLength = 12,
            sourceIsEnvironmentStart = true,
            sourcePosition = new Int2D((int)(glitchLimits.min.x + glitchLimits.width / 2), (int)(glitchLimits.yMax - glitchLimits.height * (up ? 0.9 : 0.1))),
            hubDirection = new Vector2(0, up ? 1 : -1),
            hubDirectionDeviation = 15,
            numberOfItems = 4,
            specificItems = new MajorItem[] { MajorItem.TheRedKey, MajorItem.TheBlueKey, MajorItem.TheGreenKey, MajorItem.TheBlackKey },
            noItemsAfterBosses = true,
            noTraversalItems = true,
            minSpokeLength = 4,
            maxSpokeLength = 7,
            itemAtEndOfHubPath = random.value > 0.5f,
            hasLinkingPaths = true,
        };

        glitch.bossPaths = new List<int> { random.Range(0, glitch.numberOfItems) };

        segments.Add(glitch);

        return glitch;
    }

    public void Initialize(RoomLayout layout, MicrosoftRandom random, bool hasSaves)
    {
        _layout = layout;

        _layout.environmentLimits = new Dictionary<EnvironmentType, Rect>();
        foreach (var limit in environmentLimits)
        {
            _layout.environmentLimits.Add(limit.type, limit.bounds);
        }

        List<EnvironmentType> saveRoomEnvironments = new List<EnvironmentType>();

        if (hasSaves)
        {
            var forbiddenEnvironments = new EnvironmentType[] { EnvironmentType.Surface, EnvironmentType.ForestSlums, EnvironmentType.BeastGuts, EnvironmentType.Glitch };
            saveRoomEnvironments = layout.environmentOrder.Where((e) => !forbiddenEnvironments.Contains(e)).ToList();
            var removeEnv = saveRoomEnvironments[random.Range(0, saveRoomEnvironments.Count)];
            saveRoomEnvironments.Remove(removeEnv);
            //Debug.Log("No Save Room in " + removeEnv);
        }

        foreach (var s in segments)
        {
            s.hasSaveRoom = saveRoomEnvironments.Contains(s.environmentType);
        }

        GenerateGlitchWorld(random);
    }

    public void SwapEnvironment(EnvironmentType env1, EnvironmentType env2)
    {
        foreach (var limits in environmentLimits)
        {
            if (limits.type == env1) { limits.type = env2; }
            else if (limits.type == env2) { limits.type = env1; }
        }

        foreach (var segment in segments)
        {
            if (segment.environmentType == env1) { segment.environmentType = env2; }
            else if (segment.environmentType == env2) { segment.environmentType = env1; }

            for (int i = 0; i < segment.neededConnectorBranches.Count; i++)
            {
                if (segment.neededConnectorBranches[i] == env1)
                {
                    segment.neededConnectorBranches[i] = env2;
                }
                else if(segment.neededConnectorBranches[i] == env2)
                {
                    segment.neededConnectorBranches[i] = env1;
                }
            }
        }
    }

    public EnvironmentType EnvironmentFromItemOrder(int itemOrder)
    {
        var items = 0;
        foreach (var segment in segments)
        {
            if (segment.environmentType == EnvironmentType.BeastGuts) { continue; }
            if (segment.environmentType == EnvironmentType.Glitch) { continue; }
            items += segment.numberOfItems;
            if (itemOrder < items) return segment.environmentType;
        }

        return EnvironmentType.BuriedCity;
    }
}

[Serializable]
public class EnvironmentBounds
{
    public EnvironmentType type;
    public Rect bounds;
}

[Serializable]
public class SegmentConnection
{
    public int from;
    public int to;
    public Direction direction;
}
