using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIConfirmCancel : MonoBehaviour
{
    public static UIConfirmCancel instance;
    public Text cancelText;
    public Text confirmText;
    public GameObject confirm;
    public GameObject cancel;

    public void Awake()
    {
        instance = this;
        gameObject.SetActive(false);
    }

    private void OnDestroy()
    {
        if (instance == this) { instance = null; }
    }

    public void Show(bool confirm = true, bool cancel = true, string confirmText = "Confirm", string cancelText = "Cancel")
    {
        this.confirmText.text = confirmText;
        this.cancelText.text = cancelText;
        this.confirm.SetActive(confirm);
        this.cancel.SetActive(cancel);
        gameObject.SetActive(true);
    }

    public void Hide()
    {
        gameObject.SetActive(false);
    }
}
