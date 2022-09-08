using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class HealthBar : ValueBar
{
    public Player player;
    private Damageable _damageable;
    private bool _playerSetup;
    private bool _started;
    public bool started { get { return _started; } }

    public void Start()
    {
        SetupPlayer();
        _started = true;
    }

    public bool SetupPlayer()
    {
        if (!_playerSetup && player)
        {             
            _playerSetup = true;
            player.onCollectItem += OnCollectItem;
            _damageable = player;
            _value = GetValue();
            _maxValue = GetMaxValue();
            MatchValues();
            return true;
        }

        return false;
    }

    public void UnassignPlayer()
    {
        player = null;
        _playerSetup = false;
    }

    public void OnCollectItem()
    {
        gameObject.SetActive(!player.artificeMode);
    }

    protected override void Update()
    {
        if (player.artificeMode)
        {
            gameObject.SetActive(false);
        }

        base.Update();

        if (!_flashing)
        {
            if (player.health <= 3)
            {
                bar.material.SetColor("_FlashColor", Constants.damageFlashColor);
                bar.material.SetFloat("_FlashAmount", Mathf.PingPong(Time.time, 0.5f));
            }
            else if (player.bonusRegenerationRate > 0 || player.regenerationRate > 0)
            {
                bar.material.SetColor("_FlashColor", Constants.blasterGreen);
                bar.material.SetFloat("_FlashAmount", Mathf.PingPong(Time.time * 0.5f, 0.5f));
            }
            else
            {
                bar.material.SetFloat("_FlashAmount", 0);
            }
        }
    }

    protected override float GetValue()
    {
        if (_damageable)
        {
            return _damageable.health;
        }
        else
        {
            return 0;
        }
    }

    protected override float GetMaxValue()
    {
        if (_damageable)
        {
            return _damageable.maxHealth;
        }
        else
        {
            return 0;
        }
    }

    private void OnDestroy()
    {
        if (player)
        {
            player.onCollectItem -= OnCollectItem;
        }
    }
}
