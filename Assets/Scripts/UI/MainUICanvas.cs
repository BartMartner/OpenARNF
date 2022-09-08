using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MainUICanvas : MonoBehaviour
{
    public GameObject healthAndEnergyBars;
    public GameObject healthAndEnergyNumeric;
    public ItemOverlayUI itemOverlayUI;

    public void Awake()
    {
        OnPauseHide();
    }

    public void Start()
    {
        if (PauseMenu.instance)
        {
            PauseMenu.instance.onHide += OnPauseHide;
        }
    }

    public void OnPauseHide()
    {
        if (SaveGameManager.activeSlot != null)
        {
            var numeric = SaveGameManager.activeSlot.numericHealthAndEnergy;
            healthAndEnergyBars.SetActive(!numeric);
            healthAndEnergyNumeric.SetActive(numeric);
            if (itemOverlayUI) { itemOverlayUI.SetItems(); }
        }
    }

    private void OnDestroy()
    {
        if (PauseMenu.instance)
        {
            PauseMenu.instance.onHide -= OnPauseHide;
        }
    }
}
