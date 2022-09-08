using Rewired;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class AchievementScreen : MonoBehaviour
{
    public static AchievementScreen instance;
    public AnimationCurve easing;
    public GameObject wallJump;
    public bool visible { get { return _visible; } }
    private bool _visible;
    private RectTransform _rectTransform;
    private CanvasGroup _canvasGroup;

    public Text achievementName;
    public Text achievementDescription;
    public Text achievementCollectMeans;
    public Image achievementIcon;

    public bool isTitleScreen;

    public void Awake()
    {
        instance = this;
        _rectTransform = GetComponent<RectTransform>();
        _canvasGroup = GetComponent<CanvasGroup>();
        gameObject.SetActive(false);
    }

    public void Show(AchievementInfo achievementInfo)
    {
        _visible = true;
        achievementName.text = achievementInfo.name;

        if(achievementInfo.associatedItem != MajorItem.None)
        {
            achievementIcon.sprite = Resources.Load<Sprite>("Sprites/Items/" + achievementInfo.associatedItem.ToString());
        }
        else
        {
            achievementIcon.sprite = Resources.Load<Sprite>("Sprites/AchievementIcons/" + achievementInfo.achievementID.ToString());
        }

        if(string.IsNullOrEmpty(achievementInfo.description))
        {
            achievementDescription.text = "has been unearthed and will appear in future playthroughs.";
        }
        else
        {
            achievementDescription.text = achievementInfo.description;
        }

        if (string.IsNullOrEmpty(achievementInfo.collectMeans))
        {
            achievementCollectMeans.text = "You earned an achievement";
        }
        else
        {
            achievementCollectMeans.text = achievementInfo.collectMeans;
        }

        gameObject.SetActive(true);
        StartCoroutine(WaitForJingle(achievementInfo));
    }

    public IEnumerator WaitForJingle(AchievementInfo achievementInfo)
    {
        var zeroY = new Vector3(1, 0, 1);
        _rectTransform.localScale = zeroY;
        _canvasGroup.alpha = 0;

        Time.timeScale = 0;
        var controller = isTitleScreen ? ReInput.players.SystemPlayer : PlayerManager.instance.player1.controller;

        float waitTime;
        if (MusicController.instance.maxMusicVolume <= 0.1f)
        {
            waitTime = 0.5f;
            UISounds.instance.ItemCollect();
        }
        else
        {
            waitTime = 2.25f;
            MusicController.instance.PlayAchievementJingle();
        }

        //give 3 frames for achievement collect code to run
        for (int i = 0; i < 3; i++) { yield return null; }

        var buttonsPressed = 0;

        var timer = 0f;
        var appearTime = 0.2f;
        while (timer < appearTime)
        {
            timer += Time.unscaledDeltaTime;
            var t = timer / appearTime;
            _rectTransform.localScale = Vector3.Lerp(zeroY, Vector3.one, easing.Evaluate(t));
            _canvasGroup.alpha = Mathf.Lerp(0, 1, t);
            if (controller.GetAnyButtonDown()) { buttonsPressed++; }
            yield return null;
        }

        _canvasGroup.alpha = 1;
        _rectTransform.localScale = Vector3.one;

        waitTime -= timer;
        timer = 0f;

        while (timer < waitTime)
        {
            timer += Time.unscaledDeltaTime;
            if (controller.GetAnyButtonDown()) { buttonsPressed++; }
            yield return null;
        }

        while (!controller.GetAnyButtonDown() && buttonsPressed < 5) { yield return null; }


        if (achievementInfo.achievementID == AchievementID.WallJump)
        {
            timer = 0f;
            while (timer < appearTime)
            {
                timer += Time.unscaledDeltaTime;
                var t = timer / appearTime;
                _rectTransform.localScale = Vector3.Lerp(zeroY, Vector3.one, easing.Evaluate(1 - t));
                yield return null;
            }

            var w = Instantiate(wallJump);
            var p = PlayerManager.instance.cameraPosition;
            p.z = 0;
            w.transform.position = p;                        
            yield return new WaitForSecondsRealtime(273f / 48f);
                
            Destroy(w);

            timer = 0f;
            while (timer < appearTime)
            {
                timer += Time.unscaledDeltaTime;
                var t = timer / appearTime;
                _canvasGroup.alpha = Mathf.Lerp(1, 0, t);
                yield return null;
            }
        }
        else
        {
            timer = 0f;
            while (timer < appearTime)
            {
                timer += Time.unscaledDeltaTime;
                var t = timer / appearTime;
                _rectTransform.localScale = Vector3.Lerp(zeroY, Vector3.one, easing.Evaluate(1 - t));
                _canvasGroup.alpha = Mathf.Lerp(1, 0, t);
                yield return null;
            }
        }

        Time.timeScale = 1;

        _visible = false;
        gameObject.SetActive(false);
    }

    public void OnDestroy()
    {
        if (instance = this)
        {
            instance = null;
        }
    }
}
