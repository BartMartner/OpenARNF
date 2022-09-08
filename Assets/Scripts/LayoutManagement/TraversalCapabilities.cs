using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

[Serializable]
public class TraversalCapabilities
{
    [JsonProperty(PropertyName = "dmgTyp")]
    public DamageType damageTypes;
    [JsonProperty(PropertyName = "baseJmp")]
    public float baseJumpHeight;
    [JsonProperty(PropertyName = "hovJmp")]
    public float hoverJumpHeight;
    [JsonProperty(PropertyName = "jmps")]
    public float jumps;
    [JsonProperty(PropertyName = "sbJmpMd")]
    public float waterJumpMod;

    public float effectiveJumpHeight
    {
        get
        {
            if (canReverseGravity)
            {
                return float.MaxValue;
            }
            else
            {
                return baseJumpHeight * jumps + hoverJumpHeight;
            }
        }
    }

    public float waterJumpHeight
    {
        get
        {
            if (canReverseGravity)
            {
                return float.MaxValue;
            }
            else
            {
                return effectiveJumpHeight * waterJumpMod;
            }
        }
    }

    [JsonProperty(PropertyName = "revGrav")]
    public bool canReverseGravity;
    [JsonProperty(PropertyName = "gaps")]
    public bool canTraverseGroundedSmallGaps;

    /// <summary>
    /// Doesn't currently effect lastGainedAffordance
    /// </summary>
    [JsonProperty(PropertyName = "elvGaps")]
    public bool canTraverseElevatedSmallGaps;

    [JsonProperty(PropertyName = "ignT")]
    public bool shotIgnoresTerrain;

    [JsonProperty(PropertyName = "lastAff")]
    public TraversalRequirements lastGainedAffordance;

    [JsonProperty(PropertyName = "envRes")]
    public EnvironmentalEffect environmentalResistance;

    [JsonProperty(PropertyName = "phsWalls")]
    public bool canPhaseThroughWalls;

    public TraversalCapabilities() { }

    public TraversalCapabilities(TraversalCapabilities capabilities)
    {
        damageTypes = capabilities.damageTypes;
        baseJumpHeight = capabilities.baseJumpHeight;
        jumps = capabilities.jumps;
        hoverJumpHeight = capabilities.hoverJumpHeight;
        waterJumpMod = capabilities.waterJumpMod;

        canTraverseElevatedSmallGaps = capabilities.canTraverseElevatedSmallGaps;
        canReverseGravity = capabilities.canReverseGravity;
        canTraverseGroundedSmallGaps = capabilities.canTraverseGroundedSmallGaps;
        canPhaseThroughWalls = capabilities.canPhaseThroughWalls;
        shotIgnoresTerrain = capabilities.shotIgnoresTerrain;

        environmentalResistance = capabilities.environmentalResistance;
    }

    public override string ToString()
    {
        var stringBuilder = new StringBuilder();
        stringBuilder.AppendLine("-Traversal Capabilites-");
        if (damageTypes != 0)
        {
            stringBuilder.AppendLine("Damage Types:");
            var dTypes = damageTypes.GetFlags();
            foreach (var flags in dTypes)
            {
                stringBuilder.AppendLine("-" + flags.ToString());
            }
        }
        if (jumps > 0) { stringBuilder.AppendLine("Jumps : " + jumps); }
        if (baseJumpHeight > 0) { stringBuilder.AppendLine("Base Jump Height: " + baseJumpHeight); }
        if (hoverJumpHeight > 0) { stringBuilder.AppendLine("Hover Jump Height: " + hoverJumpHeight); }
        if (effectiveJumpHeight > 0) { stringBuilder.AppendLine("Effective Jump Height: " + effectiveJumpHeight); }
        if (waterJumpMod > 0) { stringBuilder.AppendLine("Water Jump Mod: " + waterJumpMod); }
        if (waterJumpHeight > 0) { stringBuilder.AppendLine("Water Jump Height: " + waterJumpHeight); }
        if (canReverseGravity) { stringBuilder.AppendLine("Can Reverse Gravity: " + canReverseGravity); }
        if (canTraverseGroundedSmallGaps) { stringBuilder.AppendLine("Can Traverse Grounded Small Gaps: " + canTraverseGroundedSmallGaps); }
        if (canTraverseElevatedSmallGaps) { stringBuilder.AppendLine("Can Traverse Elevated Small Gaps: " + canTraverseElevatedSmallGaps); }
        if (canPhaseThroughWalls) { stringBuilder.AppendLine("Can Phase Through Walls: " + canPhaseThroughWalls); }
        if (shotIgnoresTerrain) { stringBuilder.AppendLine("Shot Ignores Terrain: " + shotIgnoresTerrain); }
        if (environmentalResistance > 0)
        {
            stringBuilder.AppendLine("Evironmental Resistance:");
            var eTypes = environmentalResistance.GetFlags();
            foreach (var flags in eTypes)
            {
                stringBuilder.AppendLine("-" + flags.ToString());
            }
        }
        return stringBuilder.ToString();
    }
}