using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

/// <summary>
/// For what is in a layout. Matched to TraversalLimitations.
/// </summary>
[Serializable]
public class TraversalRequirements
{
    [JsonProperty(PropertyName = "reqDmg")]
    public DamageType requiredDamageType = 0;
    [JsonProperty(PropertyName = "minJmp")]
    public float minEffectiveJumpHeight;
    [JsonProperty(PropertyName = "maxJmp")]
    public float maxEffectiveJumpHeight = Constants.startingMaxJumpHeight;

    [JsonProperty(PropertyName = "minWtrJmp")]
    public float minWaterJumpHeight;
    [JsonProperty(PropertyName = "maxWtrJmp")]
    public float maxWaterJumpHeight = Constants.startingMaxJumpHeight * 0.5f;

    [JsonProperty(PropertyName = "reqGap")]
    public bool requiresGroundedSmallGaps;

    /// <summary>
    /// used by TraversalCapabilities.lastGainedAffordance
    /// </summary>
    [JsonProperty(PropertyName = "supIgnT")]
    public bool supportsShotIgnoresTerrain;

    [JsonProperty(PropertyName = "reqIgnT")]
    public bool requiresShotIgnoresTerrain;

    [JsonProperty(PropertyName = "envRes")]
    public EnvironmentalEffect requiredEnvironmentalResistance;

    [JsonProperty(PropertyName = "reqPhsW")]
    public bool requiresPhaseThroughWalls;

    [JsonProperty(PropertyName = "reqPhsSf")]
    public bool requiresPhaseProof;

    public TraversalRequirements() { }

    public TraversalRequirements(TraversalRequirements requirements)
    {
        requiredDamageType = requirements.requiredDamageType;
        minEffectiveJumpHeight = requirements.minEffectiveJumpHeight;
        maxEffectiveJumpHeight = requirements.maxEffectiveJumpHeight;
        minWaterJumpHeight = requirements.minWaterJumpHeight;
        maxWaterJumpHeight = requirements.maxWaterJumpHeight;
        requiresGroundedSmallGaps = requirements.requiresGroundedSmallGaps;
        supportsShotIgnoresTerrain = requirements.supportsShotIgnoresTerrain;
        requiresShotIgnoresTerrain = requirements.requiresShotIgnoresTerrain;
        requiresPhaseThroughWalls = requirements.requiresPhaseThroughWalls;
        requiredEnvironmentalResistance = requirements.requiredEnvironmentalResistance;
        requiresPhaseProof = requirements.requiresPhaseProof;
    }

    public bool CapabilitesSufficient(TraversalCapabilities capabilites)
    {
        //if this were a door, the player with the provided capabilites couldn't jump high enough to reach it
        if (minEffectiveJumpHeight > capabilites.effectiveJumpHeight) return false;

        if (minWaterJumpHeight > capabilites.effectiveJumpHeight) return false;

        if (requiresGroundedSmallGaps && !capabilites.canTraverseGroundedSmallGaps) return false;

        if (requiresShotIgnoresTerrain && !capabilites.shotIgnoresTerrain) return false;

        if (requiresPhaseThroughWalls && !capabilites.canPhaseThroughWalls) return false;

        if (requiredDamageType != 0 && requiredDamageType != DamageType.Generic && !capabilites.damageTypes.HasFlag(requiredDamageType)) return false;

        if (requiredEnvironmentalResistance != 0 && !capabilites.environmentalResistance.HasFlag(requiredEnvironmentalResistance)) return false;

        return true;
    }

    /// <summary>
    /// Exclusively used for checking if lastGainedAffordance is only environmental
    /// </summary>
    /// <returns></returns>
    public bool JustEnvironmental()
    {
        if (requiredEnvironmentalResistance == EnvironmentalEffect.None)
        {
            return false;
        }
        else
        {
            return (requiredDamageType == 0 || requiredDamageType == DamageType.Generic) &&
                    minEffectiveJumpHeight == 0 && maxEffectiveJumpHeight == Constants.startingMaxJumpHeight &&
                    requiredEnvironmentalResistance != EnvironmentalEffect.Underwater && //There might be a way to check min/max WaterJumpHeight but this seems safer
                    !requiresGroundedSmallGaps && !requiresShotIgnoresTerrain && !requiresPhaseThroughWalls;
        }
    }

    public override string ToString()
    {
        var stringBuilder = new StringBuilder();
        if (requiredDamageType != DamageType.Generic) { stringBuilder.AppendLine("requiredDamageType: " + requiredDamageType); }
        stringBuilder.AppendLine("minEffectiveJumpHeight: " + minEffectiveJumpHeight);
        stringBuilder.AppendLine("maxEffectiveJumpHeight: " + maxEffectiveJumpHeight);
        if (requiresGroundedSmallGaps) { stringBuilder.AppendLine("requiresGroundedSmallGaps: " + requiresGroundedSmallGaps); }
        if (supportsShotIgnoresTerrain) { stringBuilder.AppendLine("supportsShotIgnoresTerrain: " + supportsShotIgnoresTerrain); }
        if (requiresShotIgnoresTerrain) { stringBuilder.AppendLine("requiresShotIgnoresTerrain: " + requiresShotIgnoresTerrain); }        
        if (requiresPhaseThroughWalls) { stringBuilder.AppendLine("requiresPhaseThroughWalls: " + requiresPhaseThroughWalls); }
        if (requiredEnvironmentalResistance != EnvironmentalEffect.None) { stringBuilder.AppendLine("requiredEnvironmentalResistance: " + requiredEnvironmentalResistance); }
        return stringBuilder.ToString();
    }
}
