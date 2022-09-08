using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class EnergyBar : ValueBar
{
    public Image icon;
    public Image bottom;
    public Sprite noItem;
    public Sprite item;
    public Player player;
    private bool _playerSetup;
    private bool _started;
    public bool started { get { return _started; } }

    protected Color _defaultColor = new Color32(210, 203, 8, 255);

    public IEnumerator Start()
    {
        yield return (StartCoroutine(SetupPlayer()));
        _started = true;
    }

    public IEnumerator SetupPlayer()
    {
        if (!player || _playerSetup) { yield break; }

        while (!player.started) { yield return null; }

        _playerSetup = true;
        player.onSelectedWeaponChanged += RefreshBar;
        player.onCollectItem += RefreshBar;
        player.onRespawn += RefreshBar;
        _value = GetValue();
        _maxValue = GetMaxValue();
        MatchValues();
        RefreshBar();
    }

    public void UnassignPlayer()
    {
        player = null;
        _playerSetup = false;
    }

    protected override void Update()
    {
        base.Update();

        if (player && player.started)
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

    protected virtual void RefreshBar()
    {
        _value = GetValue();
        _maxValue = GetMaxValue();
        MatchValues();

        gameObject.SetActive(true);

        if (player.selectedEnergyWeapon == null)
        {
            icon.sprite = null;
            icon.color = Color.clear;
            bar.color = _defaultColor;
            gainFlash = _defaultColor;
            lossFlash = _defaultColor;
            bottom.sprite = noItem;
        }
        else
        {
            icon.color = Color.white;
            icon.sprite = player.selectedEnergyWeapon.icon;
            bar.color = player.selectedEnergyWeapon.barColor;
            gainFlash = player.selectedEnergyWeapon.barColor;
            lossFlash = player.selectedEnergyWeapon.barColor;
            bottom.sprite = item;
        }
    }
}
