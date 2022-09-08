using UnityEngine;
using System.Collections;

public abstract class PlayerEnergyWeapon : ScriptableObject
{
    public Sprite icon;
    public Color barColor;
    public MajorItem item;

    protected Player _player;   
    public bool allowSpinJumping;
    protected bool _allowTurning;
    public bool allowTurning { get { return _allowTurning; } }
    public float minEnergy = 0;

    // Update is called once per frame

    public virtual void Initialize(Player player)
    {
        _player = player;
    }

    public virtual void Update() { }
    public virtual void OnAttackDown() { }
    public virtual void OnAttackUp() { }
    public virtual void OnSelect() { }
    public virtual void OnDeselect() { }
    public virtual void Stop() { }
    public virtual void ImmediateStop() { Stop(); }
    public virtual bool Usable() { return _player && _player.energy >= minEnergy; }
}
