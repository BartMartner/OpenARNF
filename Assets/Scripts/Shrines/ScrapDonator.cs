using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ScrapDonator : MonoBehaviour
{
    private CanvasGroup _canvasGroup;
    public Image leftArrow;
    public Text amount;
    public Image rightArrow;
    public CurrencyType scrapType;

    private void Awake()
    {
        _canvasGroup = GetComponent<CanvasGroup>();
    }

    public void Refresh(bool selected, ItemPrice price)
    {
        var amount = price[scrapType];

        leftArrow.gameObject.SetActive(amount > 0);
        rightArrow.gameObject.SetActive(amount < Player.instance.GetScrap(scrapType));
        this.amount.text = amount.ToString();

        if (selected)
        {
            _canvasGroup.alpha = 1;
        }
        else
        {
            _canvasGroup.alpha = 0.5f;
        }
    }
}
