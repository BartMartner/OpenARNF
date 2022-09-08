using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class ContinueScreen : MonoBehaviour
{
    public static ContinueScreen instance;
    public Text continueText;
    public Text coinsText;
    public bool visible { get { return _visible; } }
    private bool _visible;
    public float timer = 9;

    public void Awake()
    {
#if !ARCADE
        Destroy(gameObject);
        return;
#endif
        instance = this;
        gameObject.SetActive(false);
    }

    public void Show()
    {
        _visible = true;
        Time.timeScale = 0;
        gameObject.SetActive(true);
        timer = 9;
    }

    public void Hide()
    {
        _visible = true;
        Time.timeScale = 1;
        gameObject.SetActive(false);
        timer = 9;
    }

    private void Update()
    {
        if (_visible)
        {
            if (Player.instance)
            {
                coinsText.text = "Coins: " + Player.instance.extraLives;
            }
            else
            {
                coinsText.text = "Coins: Error";
            }

            timer -= Time.unscaledDeltaTime;
            if (timer < 0) { BackToStartScreen(); }
            else { continueText.text = "Continue? " + Mathf.RoundToInt(timer); }
        }
    }

    public void Continue()
    {
        if (Player.instance.extraLives > 0)
        {
            Hide();
            Player.instance.UseExtraLife();
        }
        else
        {
            UISounds.instance.UIFail();
        }
    }

    public void BackToStartScreen()
    {
        _visible = false;
        Time.timeScale = 1;
        SaveGameManager.instance.ClearActiveGame();
        SceneManager.LoadScene("StartScreen");
    }

    public void OnDestroy()
    {
        if (instance = this)
        {
            instance = null;
        }
    }
}
