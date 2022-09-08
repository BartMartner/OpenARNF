using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ShopOption : MonoBehaviour
{
    public Image selectionArrow;
    public Image selectionBG;

    public Sprite activeArrow;
    public Sprite inactiveArrow;

    private CanvasGroup _canvasGroup;
    protected Color32 _selectedColor = new Color32(18, 28, 28, 255);

    private void Awake()
    {
        _canvasGroup = GetComponent<CanvasGroup>();
    }

    public void ToggleSelected(bool selected)
    {
        if (selected)
        {
            selectionBG.color = _selectedColor;
            selectionArrow.sprite = activeArrow;
            _canvasGroup.alpha = 1;
        }
        else
        {
            selectionBG.color = Color.black;
            selectionArrow.sprite = inactiveArrow;
            _canvasGroup.alpha = 0.5f;
        }
    }
}
