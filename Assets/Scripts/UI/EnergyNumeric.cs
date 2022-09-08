using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class EnergyNumeric : ValueBar
{
    public Text text;
    public Sprite noItem;
    public Image icon;
    public Player player;
    private bool _setup;

    public IEnumerator Start()
    {
        while (!player.started) { yield return null; }
        Setup();
    }

    private void Setup()
    {
        _setup = true;
        player.onSelectedWeaponChanged += RefreshBar;
        player.onCollectItem += RefreshBar;
        player.onRespawn += RefreshBar;
        _value = GetValue();
        _maxValue = GetMaxValue();
        MatchValues();
        RefreshBar();
    }

    private void OnEnable()
    {
        if (player && player.started && !_setup) { Setup(); }
    }

    protected override void Update()
    {
        base.Update();

        if (player)
        {
            if (!_flashing)
            {
                if (player.bonusEnergyRegenRate > 0 || player.itemEnergyRegenRate > 0)
                {
                    bar.material.SetColor("_FlashColor", gainFlash);
                    bar.material.SetFloat("_FlashAmount", Mathf.PingPong(Time.time * 0.5f, 0.5f));
                }
                else
                {
                    bar.material.SetFloat("_FlashAmount", 0);
                }
            }

            if (player.selectedEnergyWeapon != null)
            {
                icon.color = player.selectedEnergyWeapon.Usable() ? Color.white : Color.gray;
            }
        }
    }

    public void OnDestroy()
    {
        if (player != null)
        {
            player.onSelectedWeaponChanged -= RefreshBar;
            player.onCollectItem -= RefreshBar;
            player.onRespawn -= RefreshBar;
        }
    }

    protected override float GetValue()
    {
        return player ? player.energy : 0;        
    }

    protected override float GetMaxValue()
    {
        return player ? player.maxEnergy : 0;
    }

    public override void MatchValues()
    {
        text.text = Mathf.CeilToInt(Mathf.Clamp(GetValue(), 0, GetMaxValue())).ToString() + "/" + _maxValue;
        bar.rectTransform.sizeDelta = new Vector2(24f * _value / _maxValue, 5);
    }

    protected void RefreshBar()
    {
        _value = GetValue();
        _maxValue = GetMaxValue();
        MatchValues();

        gameObject.SetActive(true);

        if (player.selectedEnergyWeapon == null)
        {
            icon.sprite = noItem;            
            bar.color = new Color32(105, 101, 4, 255);
            gainFlash = new Color32(210, 203, 8, 255);
            lossFlash = gainFlash;
        }
        else
        {            
            icon.sprite = player.selectedEnergyWeapon.icon;
            bar.color = player.selectedEnergyWeapon.barColor * 0.5f;
            gainFlash = player.selectedEnergyWeapon.barColor;
            lossFlash = player.selectedEnergyWeapon.barColor;
        }
    }
}
