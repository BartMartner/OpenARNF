using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MinorItemChanger : PermanentStateObject, IAbstractDependantObject
{
    public MinorItemType fromMinorItem;
    public MinorItemType toMinorItem;
    public SpriteRenderer fromItemSprite;
    public SpriteRenderer toItemSprite;
    public Sprite questionSprite;

    private ButtonTriggerBounds _buttonTriggerBounds;
    private Animator _animator;

    private void Awake()
    {
        _animator = GetComponentInChildren<Animator>();
        _buttonTriggerBounds = GetComponent<ButtonTriggerBounds>();
    }

    public void Start()
    {
        _buttonTriggerBounds.onSubmit.AddListener(OnSubmit);
    }

    public override void SetStateFromSave(int state)
    {
        if (!_animator) { _animator = GetComponentInChildren<Animator>(); }
        if (!_buttonTriggerBounds) { _buttonTriggerBounds = GetComponent<ButtonTriggerBounds>(); }

        base.SetStateFromSave(state);
        _buttonTriggerBounds.enabled = state == 0;
        if (state == 1) { _animator.Play("Deactivated"); }
    }

    public override void CompareWithAbstract(RoomAbstract roomAbstract)
    {
        base.CompareWithAbstract(roomAbstract);

        if(softFail)
        {
            Destroy(gameObject);
            return;
        }

        var seed = roomAbstract.seed;
        seed -= (int)(transform.position.x + transform.position.y);
        XorShift random = new XorShift(seed);        
        var list = new List<MinorItemType>() //no damage for from... no one wants to trade damage
        {
            MinorItemType.AttackModule, MinorItemType.BlueScrap, MinorItemType.EnergyModule,
            MinorItemType.GreenScrap, MinorItemType.HealthTank, MinorItemType.RedScrap, MinorItemType.ShotSpeedModule,
            MinorItemType.SpeedModule,
        };
        fromMinorItem = list[random.Range(0, list.Count)];
        var toRoll = random.Value();
        list.Remove(fromMinorItem);
        //increase chance of scrap
        if(fromMinorItem != MinorItemType.RedScrap) { list.Add(MinorItemType.RedScrap); }
        if (fromMinorItem != MinorItemType.GreenScrap) { list.Add(MinorItemType.GreenScrap); }
        if (fromMinorItem != MinorItemType.BlueScrap) { list.Add(MinorItemType.BlueScrap); }
        list.Add(MinorItemType.DamageModule); //add damage
        list.Remove(MinorItemType.ShotSpeedModule); //remove shot speed. No one wants shot speed.
        toMinorItem = list[random.Range(0, list.Count)];
        
        fromItemSprite.sprite = PickUpManager.instance.GetMinorItemSprite(fromMinorItem);
        var roll = random.Value();
        if (roll < 0.1)
        {
            toItemSprite.sprite = questionSprite;
        }
        else
        {
            toItemSprite.sprite = PickUpManager.instance.GetMinorItemSprite(toMinorItem);
        }
    }

    public void OnSubmit()
    {
        var player = PlayerManager.instance.player1;

        bool fromClear;
        switch(fromMinorItem)
        {
            case MinorItemType.EnergyModule:
                fromClear = player.maxEnergy > Constants.energyModuleAmount;
                break;
            case MinorItemType.HealthTank:
                fromClear = player.maxHealth > Constants.healthTankAmount;
                break;
            case MinorItemType.DamageModule:
                fromClear = player.projectileStats.damage > Constants.damageModuleBonus;
                break;
            case MinorItemType.SpeedModule:
                fromClear = player.maxSpeed > Constants.speedModuleAmount;
                break;
            case MinorItemType.RedScrap:
                fromClear = player.redScrap > 0;
                break;
            case MinorItemType.GreenScrap:
                fromClear = player.greenScrap > 0;
                break;
            case MinorItemType.BlueScrap:
                fromClear = player.blueScrap > 0;
                break;
            default:
                fromClear = true;
                break;
        }

        bool toClear = true;
        switch(toMinorItem)
        {
            case MinorItemType.RedScrap:
                toClear = player.redScrap < 99;
                break;
            case MinorItemType.GreenScrap:
                toClear = player.greenScrap < 99;
                break;
            case MinorItemType.BlueScrap:
                toClear = player.blueScrap < 99;
                break;
        }

        if (fromClear && toClear)
        {
            StartCoroutine(ChangeRoutine());
        }
        else
        {
            UISounds.instance.UIFail();
        }
    }

    private IEnumerator ChangeRoutine()
    {
        UISounds.instance.Confirm();
        toItemSprite.sprite = PickUpManager.instance.GetMinorItemSprite(toMinorItem);
        _animator.Play("Activate");
        _buttonTriggerBounds.enabled = false;
        _buttonTriggerBounds.FadeOut();
        yield return new WaitForSeconds(1f);
        var player = PlayerManager.instance.player1;
        player.ModifyMinorItemCollection(fromMinorItem, -1, false);
        player.ModifyMinorItemCollection(toMinorItem, 1, false);
        if (player.onCollectItem != null) { player.onCollectItem(); }
        SaveState(1);
    }
}
