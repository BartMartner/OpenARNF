using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class BossHealthBar : ValueBar
{
    private BossFight _bossFight;

    public void Show(BossFight bossFight)
    {
        _bossFight = bossFight;
        gameObject.SetActive(true);
    }

    public void Hide()
    {
        _bossFight = null;
        gameObject.SetActive(false);
    }

    protected override float GetValue()
    {
        return _bossFight ? _bossFight.currentHealth : 0;
    }

    protected override float GetMaxValue()
    {
        return _bossFight ? _bossFight.maxHealth : 0;
    }
}
