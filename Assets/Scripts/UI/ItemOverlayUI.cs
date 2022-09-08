using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class ItemOverlayUI : MonoBehaviour
{
    public GameObject overlay;
    public Image[] itemIcons;
    public Text health;
    public Text energy;
    public Text damage;
    public Text attack;
    public Text speed;
    public Text shotSpeed;
    public BonusBar[] bonusBars;    
    public Text collectionRate;
    public CanvasGroup minorItems;
    public RectTransform healthBar;
    public RectTransform energyBar;

    private bool _expanded;
    private Player _player;
    private RectTransform _minorItemsRect;
    private float _defaultAlpha = 0.33f;
    private IEnumerator _statFlash;

    private void OnEnable()
    {
        overlay.gameObject.SetActive(false);
    }

    public IEnumerator Start()
    {
        while(!PlayerManager.instance)
        {
            yield return null;
        }

        _player = PlayerManager.instance.player1;
        _player.onSetStats += SetItems;
        _minorItemsRect = minorItems.GetComponent<RectTransform>();
        if (SaveGameManager.activeSlot != null) { minorItems.alpha = SaveGameManager.activeSlot.statsAlwaysVisible ? _defaultAlpha : 0; }
    }

    public void SetItems()
    {
        if (SaveGameManager.activeSlot != null && !_expanded) { minorItems.alpha = SaveGameManager.activeSlot.statsAlwaysVisible ? _defaultAlpha : 0; }

        if (_player)
        {
            var items = _player.itemsPossessed;
            for (int i = 0; i < itemIcons.Length; i++)
            {
                if (i < items.Count)
                {
                    itemIcons[i].sprite = ItemManager.items[items[i]].icon;
                    itemIcons[i].enabled = true;
                }
                else
                {
                    itemIcons[i].enabled = false;
                }
            }

            health.text = _player.healthUps.ToString() + " (" + _player.maxHealth.ToString("0") + ")";
            energy.text = _player.energyUps.ToString() + " (" + _player.maxEnergy.ToString("0") + ")";

            damage.text = _player.damageUps.ToString() + " (" + _player.projectileStats.damage.ToString("0.0") + ")";
            damage.color = _player.bonusDamage == 0 ? Color.white : _player.bonusDamage > 0 ? Color.green : Color.red;

            if (_player.attackDelay < 1)
            {
                attack.text = _player.attackUps.ToString() + " (" + _player.attackDelay.ToString(".00") + ")";
            }
            else
            {
                attack.text = _player.attackUps.ToString() + " (" + _player.attackDelay.ToString("0.0") + ")";
            }
            attack.color = _player.bonusAttack == 0 ? Color.white : _player.bonusAttack > 0 ? Color.green : Color.red;

            speed.text = _player.speedUps.ToString() + " (" + _player.maxSpeed.ToString("0.0") + ")";
            speed.color = _player.bonusSpeed == 0 ? Color.white : _player.bonusSpeed > 0 ? Color.green : Color.red;

            shotSpeed.text = _player.shotSpeedUps.ToString() + " (" + _player.projectileStats.speed.ToString("0.0") + ")";
            shotSpeed.color = _player.bonusShotSpeed == 0 ? Color.white : _player.bonusShotSpeed > 0 ? Color.green : Color.red;

            if (SaveGameManager.activeGame != null)
            {
                var collectRate = (int)(SaveGameManager.activeGame.collectRate * 100);
                collectionRate.text = collectRate + "%";
            }
        }
        else
        {
            foreach (var bar in bonusBars) { bar.gameObject.SetActive(false); }
            foreach (var item in itemIcons) { item.enabled = false; }

            health.text = "0";
            energy.text = "0";
            damage.text = "0";
            attack.text = "0";
            speed.text = "0";
            shotSpeed.text = "0";
        }
    }

    public void Update()
    {
        bool numeric = false;
        _defaultAlpha = 0.33f;

        if (SaveGameManager.activeSlot != null)
        {
            numeric = SaveGameManager.activeSlot.numericHealthAndEnergy;
            if(!SaveGameManager.activeSlot.statsAlwaysVisible) { _defaultAlpha = 0; }
        }

        if (numeric)
        {
            _minorItemsRect.SetInsetAndSizeFromParentEdge(RectTransform.Edge.Top, 36, 72);
        }
        else if (healthBar)
        {
            float barBottom;
            barBottom = healthBar.rect.yMin;
            if (energyBar && energyBar.rect.yMin < barBottom)
            {                
                barBottom = energyBar.rect.yMin;
            }
            _minorItemsRect.SetInsetAndSizeFromParentEdge(RectTransform.Edge.Top, -(barBottom - 4), 72);
        }

        if (!_expanded && !PauseMenu.instance.visible && _player.controller.GetButtonDown("ExpandMap"))
        {
            SetItems();
            _expanded = true;
            overlay.gameObject.SetActive(true);
            minorItems.alpha = 1f;
        }

        if (_expanded && (_player.controller.GetButtonUp("ExpandMap") || PauseMenu.instance.visible))
        {
            _expanded = false;
            overlay.gameObject.SetActive(false);
            minorItems.alpha = _defaultAlpha;
        }

        foreach (var bar in bonusBars)
        {
            var barWasActive = bar.gameObject.activeInHierarchy;
            bar.gameObject.SetActive(false);
            bar.negBar.fillAmount = 0;
            bar.posBar.fillAmount = 0;

            var bonuses = _player.tempStatMods.Where(t => t.statType == bar.statType);
            foreach (var bonus in bonuses)
            {
                bar.gameObject.SetActive(true);
                if (!barWasActive || bonus.timeLeft > 0.99)
                {
                    if (_statFlash != null) { StopCoroutine(_statFlash); }
                    _statFlash = StatsFlash();
                    StartCoroutine(_statFlash);
                }
                if (bonus.rank > 0) { bar.posBar.fillAmount = bonus.timeLeft; }
                else { bar.negBar.fillAmount = bonus.timeLeft; }
            }
        }
    }    

    private IEnumerator StatsFlash()
    {
        minorItems.alpha = 1;

        while(!_expanded && minorItems.alpha > _defaultAlpha)
        {
            minorItems.alpha -= Time.deltaTime;
            yield return null;
        }

        minorItems.alpha = _expanded && !PauseMenu.instance.visible ? 1 : _defaultAlpha;
        _statFlash = null;
    }

    private void OnDestroy()
    {
        if(_player)
        {
            _player.onSetStats -= SetItems;
        }
    }
}
