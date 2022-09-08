using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TemporaryStatMod : TemporaryPlayerEffect
{
    private float _bonus;
    public PlayerStatType statType { get; private set; }
    public float rank { get; private set; }
    public float timeLeft { get { return _duration / _maxDuration; } }
    public Color32 flashColor { get; private set; }
    
    private float _maxDuration;

    public override void Equip(Player player, float duration)
    {
        throw new System.Exception("TemporaryStatMod should be equipped using Equip(Player player , float duration, PlayerStatType statType, float amount)");
    }

    public void Equip(Player player , PlayerStatType statType, float rank)
    {
        this.statType = statType;
        _player = player;
        if (!_player.tempStatMods.Contains(this)) { _player.tempStatMods.Add(this); }
        _equipped = true;
        RemoveBonus(statType); //remove old bonus before it's recalculated

        this.rank += rank;
        if(this.rank > 1) { this.rank = 1; }
        SetToMaxDuration();

        _bonus = GetBonus();
        ApplyBonus(statType);

        //base.Equip(player, duration);

        Color32 color;
        if(Constants.statColors.TryGetValue(statType, out color))
        {
            flashColor = color;
        }
    }

    public void SetToMaxDuration()
    {
        var minTime = 90 * _player.blessingTimeMod;
        var maxTime = 600 * _player.blessingTimeMod;
        _maxDuration = Mathf.Lerp(minTime, maxTime, this.rank);
        _duration = _maxDuration;
    }

    public float GetBonus()
    {
        var sign = Mathf.Sign(rank);
        float min, max;
        switch(statType)
        {
            case PlayerStatType.Damage:
                min = 0;
                max = 10;
                break;
            case PlayerStatType.Attack:
                min = 0;
                max = 8;
                break;
            case PlayerStatType.ShotSize:
                min = 0;
                max = 2f;
                break;
            case PlayerStatType.Speed:
                min = 1;
                max = 4;
                break;
            default:
                min = 0;
                max = 5;
                break;
        }
        return Mathf.Lerp(min, max, Mathf.Abs(rank)) * sign;
    }

    public void ApplyBonus(PlayerStatType statType)
    {
        switch (statType)
        {
            case PlayerStatType.Damage:
                _player.bonusDamage += _bonus;
                break;
            case PlayerStatType.Attack:
                _player.bonusAttack += Mathf.CeilToInt(_bonus);
                break;
            case PlayerStatType.Speed:
                _player.bonusSpeed += _bonus;
                break;
            case PlayerStatType.ShotSize:
                _player.bonusShotSizeMod += _bonus;
                break;
            case PlayerStatType.ShotSpeed:
                _player.bonusShotSpeed += _bonus;
                break;
            default:
                Debug.LogError("TemporaryStatMod doesn't currently support PlayerStatType " + statType);
                break;
        }
    }

    public void RemoveBonus(PlayerStatType statType)
    {
        switch (statType)
        {
            case PlayerStatType.Damage:
                _player.bonusDamage -= _bonus;
                break;
            case PlayerStatType.Attack:
                _player.bonusAttack -= Mathf.CeilToInt(_bonus);
                break;
            case PlayerStatType.Speed:
                _player.bonusSpeed -= _bonus;
                break;
            case PlayerStatType.ShotSize:
                _player.bonusShotSizeMod -= _bonus;
                break;
            case PlayerStatType.ShotSpeed:
                _player.bonusShotSpeed -= _bonus;
                break;
            default:
                Debug.LogError("TemporaryStatMod doesn't currently support PlayerStatType " + statType);
                break;
        }
    }

    public override void Unequip()
    {
        if (!_player) return;
        if (UISounds.instance) { UISounds.instance.EffectEnd(); }
        _player.tempStatMods.Remove(this);
        RemoveBonus(statType);
        base.Unequip();
    }
}
