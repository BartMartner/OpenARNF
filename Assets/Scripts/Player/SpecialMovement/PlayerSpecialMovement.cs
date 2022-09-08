using UnityEngine;
using System.Collections;

public abstract class PlayerSpecialMovement : ScriptableObject
{
    protected Player _player;
    protected int _priority;
    public int priorty { get { return _priority; } }

    protected bool _allowAttack;
    public bool allowAttack { get { return _allowAttack; } }

    protected bool _allowJumping;
    public bool allowJumping { get { return _allowJumping; } }

    protected bool _allowSpinJumping = true;
    public bool allowSpinJumping { get { return _allowSpinJumping; } }

    protected bool _allowKnockback;
    public bool allowKnockback { get { return _allowKnockback; } }

    protected bool _allowCrouching;
    public bool allowCrouching { get { return _allowCrouching; } }

    protected bool _allowMovement;
    public bool allowMovement { get { return _allowMovement; } }

    protected bool _allowTurning = true;
    public bool allowTurning { get { return _allowTurning; } }

    protected bool _allowDeceleration = true;
    public bool allowDeceleration { get { return _allowDeceleration; } }

    protected bool _allowGravity = true;
    public bool allowGravity { get { return _allowGravity; } }

    protected bool _supressMorph = false;
    public bool supressMorph { get { return _supressMorph; } }

    protected bool _complete;
    public bool complete { get { return _complete; } }

    public virtual void Initialize(Player player)
    {
        _player = player;
    }

    public abstract bool TryToActivate();

    public abstract void DeathStop();

    public abstract void OnPause();

    public abstract void OnUnpause();
}
