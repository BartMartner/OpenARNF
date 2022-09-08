using UnityEngine;
using System.Collections;
using System;
using System.Linq;
using System.Runtime.Serialization;
using Newtonsoft.Json;
using System.Text;

/// <summary>
/// For what is in a scene
/// </summary>
[Serializable]
public class TraversalLimitations
{
    /// <summary>
    /// The object these limitations pertain to can be made to require any of the damage types specified
    /// </summary>
    [EnumFlags]
    [JsonProperty(PropertyName = "dmgTyps")]
    public DamageType supportedDamageTypes = DamageType.Generic;

    /// <summary>
    /// The player has to be able to jump this high to access the object the limitation pertains to
    /// </summary>
    [JsonProperty(PropertyName = "reqJmp")]
    public float requiredJumpHeight;

    [JsonProperty(PropertyName = "reqGap")]
    public bool requiresGroundedSmallGaps;
    [JsonProperty(PropertyName = "supGap")]
    public bool supportsGroundedSmallGaps;

    /// <summary>
    /// Currently only implemented in CapabilitesSufficient and used for room limitations
    /// </summary>
    [JsonProperty(PropertyName = "reqElvGap")]
    public bool requiresElevatedSmallGaps;

    [JsonProperty(PropertyName = "reqIgnT")]
    public bool requiresShotIgnoresTerrain;
    [JsonProperty(PropertyName = "supIgnT")]
    public bool supportsShotIgnoresTerrain;

    [JsonProperty(PropertyName = "reqPhsW")]
    public bool requiresPhaseThroughWalls;
    [JsonProperty(PropertyName = "supPhsW")]
    public bool supportsPhaseThroughWalls;
    [JsonProperty(PropertyName = "phsPrf")]
    public bool phaseProof;

    public bool CapabilitesSufficient(TraversalCapabilities capabilites, EnvironmentalEffect envEffect = EnvironmentalEffect.None)
    {
        if(envEffect == EnvironmentalEffect.Underwater)
        {
            if(requiredJumpHeight > capabilites.waterJumpHeight)
            {
                //if this were a door, the player with the provided capabilites couldn't jump high enough to reach it
                return false;
            }
        }
        else if (requiredJumpHeight > capabilites.effectiveJumpHeight)
        {
            //if this were a door, the player with the provided capabilites couldn't jump high enough to reach it
            return false;
        }

        if (requiresElevatedSmallGaps && !capabilites.canTraverseElevatedSmallGaps) return false;

        if (requiresGroundedSmallGaps && !capabilites.canTraverseGroundedSmallGaps) return false;

        if (requiresShotIgnoresTerrain && !capabilites.shotIgnoresTerrain) return false;

        if (requiresPhaseThroughWalls && !capabilites.canPhaseThroughWalls) return false;

        if (!supportedDamageTypes.HasFlag(DamageType.Generic) && supportedDamageTypes != 0) //if supported damage type is nothing, assume this is a traversal path that doesn't require any damage type to traverse
        {
            var damageTypes = capabilites.damageTypes.GetFlags();
            var foundMatch = false;
            foreach (var damageType in damageTypes)
            {
                if (supportedDamageTypes.HasFlag(damageType))
                {
                    foundMatch = true;
                    break;
                }
            }

            if (!foundMatch)
            {
                return false; //none of the damage type in capabilites match a damage type these limitations support. (If this is for a door, the player doesn't have the damage type to open it)
            }
        }

        return true;
    }

    public bool CanSatisfyRequirements(TraversalRequirements requirements)
    {
        if (requirements == null)
        {
            return true;
        }

        if(requirements.requiredEnvironmentalResistance == EnvironmentalEffect.Underwater)
        {
            if (requiredJumpHeight < requirements.minWaterJumpHeight || requiredJumpHeight > requirements.maxWaterJumpHeight)
            {
                return false; //the required water jump height for a door in this room is either too high or too low
            }
        }
        else if (requiredJumpHeight < requirements.minEffectiveJumpHeight || requiredJumpHeight > requirements.maxEffectiveJumpHeight)
        {
            return false; //the required jump height for a door in this room is either too high or too low
        }

        if ((requirements.requiresGroundedSmallGaps && !(supportsGroundedSmallGaps || requiresGroundedSmallGaps)) ||
            (!requirements.requiresGroundedSmallGaps && requiresGroundedSmallGaps))
        {
            return false;
        }

        if ((requirements.requiresShotIgnoresTerrain && !(supportsShotIgnoresTerrain || requiresShotIgnoresTerrain)) ||
            (!requirements.supportsShotIgnoresTerrain && requiresShotIgnoresTerrain))
        {
            return false;
        }

        if((requirements.requiresPhaseThroughWalls && !(supportsPhaseThroughWalls || requiresPhaseThroughWalls)) ||
            (!requirements.requiresPhaseThroughWalls && requiresPhaseThroughWalls))
        {
            return false;
        }

        if(requirements.requiresPhaseProof && !phaseProof && (requirements.requiresGroundedSmallGaps || requirements.requiresShotIgnoresTerrain))
        {
            return false;
        }

        bool damageMatch = requirements.requiredDamageType == 0 || supportedDamageTypes.HasFlag(requirements.requiredDamageType);
        //This should mean, if requireShotIgnoresTerrain is satisfied, damageMatch is unnecessary
        if(!damageMatch && !(requiresShotIgnoresTerrain && requirements.supportsShotIgnoresTerrain))
        {
            return false;
        }

        return true;
    }

    public override string ToString()
    {
        var stringBuilder = new StringBuilder();
        stringBuilder.AppendLine("-Traversal Limitations-");
        var damageTypes = supportedDamageTypes.GetFlags();
        if (supportedDamageTypes != 0)
        {
            stringBuilder.AppendLine("Supported Damage Types:");
            foreach (var flags in damageTypes)
            {
                stringBuilder.AppendLine(flags.ToString());
            }
        }
        if (requiredJumpHeight > 0) { stringBuilder.AppendLine("Required Jump Height: " + requiredJumpHeight); }
        if (supportsGroundedSmallGaps) { stringBuilder.AppendLine("Supports Grounded Small Gaps: " + supportsGroundedSmallGaps); }
        if (requiresGroundedSmallGaps) { stringBuilder.AppendLine("Requires Grounded Small Gaps: " + requiresGroundedSmallGaps); }
        if (supportsShotIgnoresTerrain) { stringBuilder.AppendLine("Supports Shot Ignores Terrain: " + supportsShotIgnoresTerrain); }
        if (requiresShotIgnoresTerrain) { stringBuilder.AppendLine("Requires Shot Ignores Terrain: " + requiresShotIgnoresTerrain); }

        return stringBuilder.ToString();
    }
}
