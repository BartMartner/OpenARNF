using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnvironmentalDamageEffect : MonoBehaviour
{
    public EnvironmentalEffect type;
    public float frequency = 1;
    public float preDelay = 1.5f;
    public float damage = 1;
    private float _timer;

    private float _preDelayTimer = 0;

    public void Update()
    {
        if(_preDelayTimer < preDelay)
        {
            _preDelayTimer += Time.deltaTime;
            foreach (var player in PlayerManager.instance.players)
            {
                if (player.state == DamageableState.Alive && !player.flashing && !player.environmentalResistances.HasFlag(type))
                {
                    player.StartFlash(1, 0.4f, GetFlashColor(), (_preDelayTimer / preDelay * 0.25f), false);
                }
            }
            return;            
        }

        if(_timer < frequency)
        {
            _timer += Time.deltaTime;

            foreach (var player in PlayerManager.instance.players)
            {
                if (player.state == DamageableState.Alive && !player.flashing && !player.environmentalResistances.HasFlag(type))
                {
                    player.StartFlash(1, 0.4f, GetFlashColor(), 0.25f + (_timer / frequency * 0.25f), false);
                }
            }
        }
        else
        {
            foreach (var player in PlayerManager.instance.players)
            {
                if (!player.environmentalResistances.HasFlag(type))
                {
                    player.Hurt(damage, gameObject, GetDamageType(), true);
                }
                else
                {
                    player.StartFlash(1, 0.4f, GetFlashColor(), 0.1f, false);
                }
            }
            _timer = 0;
        }
    }

    private void OnDestroy()
    {
        foreach (var player in PlayerManager.instance.players)
        {
            if (player && player.flashing)
            {
                player.StopFlash();
            }
        }        
    }

    public Color GetFlashColor()
    {
        switch(type)
        {
            case EnvironmentalEffect.Heat:
                return Constants.damageFlashColor;
            default:
                return Constants.damageFlashColor;
        }
    }

    public DamageType GetDamageType()
    {
        switch(type)
        {
            case EnvironmentalEffect.Heat:
                return DamageType.Fire;
            default:
                return DamageType.Generic;
        }
    }
}
