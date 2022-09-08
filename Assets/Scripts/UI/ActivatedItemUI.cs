using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ActivatedItemUI : MonoBehaviour
{
    public Image icon;
    public Image xButton;
    private Image _background;
    public Player player;

    public void Start()
    {
        _background = GetComponent<Image>();
    }

    private void Update ()
    {
        if (player)
        {
            if (player.activatedItem)
            {
                _background.enabled = true;
                icon.gameObject.SetActive(true);
                icon.sprite = player.activatedItem.icon;
                xButton.gameObject.SetActive(player.activatedItem.Usable());
            }
            else
            {
                xButton.gameObject.SetActive(false);
                _background.enabled = false;
                icon.gameObject.SetActive(false);
            }
        }
	}
}