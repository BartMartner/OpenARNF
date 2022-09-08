using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class WeaponWheel : MonoBehaviour
{
    public Player player;
    private CanvasGroup _wheel;
    public List<Image> icons;
    public Sprite blasterIcon;
    public Image highlight;

    private int lastIndex;
    private float _linger;

    private void Start()
    {
        if (!player) { player = PlayerManager.instance.player1; }
        _wheel = GetComponentInChildren<CanvasGroup>();
    }
    
    void Update()
    {
        if (!player) { return; }
        if (player.energyWeapons.Count == 0 || player.state != DamageableState.Alive ||
            (AchievementScreen.instance && AchievementScreen.instance.visible) ||
            (PauseMenu.instance && PauseMenu.instance.visible) ||
            (Automap.instance && Automap.instance.expanded) ||
            (NPCDialogueManager.instance && NPCDialogueManager.instance.dialogueActive))
        {
            _wheel.alpha = 0;
            return;
        }

        var weaponWheelIndex = player.GetWeaponWheelIndex() + 1;
        if (weaponWheelIndex >= 0)
        {
            if(lastIndex != weaponWheelIndex)
            {
                UISounds.instance.OptionChange();
            }

            ShowWheel(weaponWheelIndex);

            lastIndex = weaponWheelIndex;
        }
        else if (_wheel.alpha > 0)
        {
            if (_linger > 0)
            {
                _linger -= Time.deltaTime;
            }
            else
            {
                _wheel.alpha -= Time.unscaledDeltaTime * 2;
            }
        }
    }

    public void ShowWheel(int weaponWheelIndex, float linger = 0)
    {
        _wheel.alpha = 1;
        _linger = linger;

        var eCount = player.energyWeapons.Count + 1;
        var sliceSize = 360 / eCount;
        var count = Mathf.Max(eCount, icons.Count);

        for (int i = 0; i < count; i++)
        {
            if (i < eCount)
            {
                if (i >= icons.Count) { icons.Add(Instantiate(icons[0], icons[0].transform.parent)); }
                icons[i].sprite = i == 0 ? blasterIcon : player.energyWeapons[i - 1].icon;
                var color = i == weaponWheelIndex ? Color.white : Color.gray;
                icons[i].color = color;
                icons[i].gameObject.SetActive(true);
                var posAngle = 360f * (float)i / eCount;
                var distance = eCount < 6 ? 20 : 22;
                icons[i].rectTransform.localPosition = Quaternion.Euler(0, 0, -posAngle) * Vector2.up * distance;
            }
            else if (i < icons.Count)
            {
                icons[i].gameObject.SetActive(false);
            }
        }

        highlight.fillAmount = 1f / eCount;
        highlight.rectTransform.rotation = Quaternion.Euler(0, 0, (-weaponWheelIndex * sliceSize) + (sliceSize * 0.5f));
        var c = weaponWheelIndex == 0 || (player.selectedEnergyWeapon && player.selectedEnergyWeapon.Usable()) ? (Color)Constants.blasterGreen : Constants.damageFlashColor;
        c.a = 0.5f;
        highlight.color = c;
    }
}
