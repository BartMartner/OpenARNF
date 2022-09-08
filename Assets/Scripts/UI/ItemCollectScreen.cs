using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using System.Collections.Generic;

public class ItemCollectScreen : MonoBehaviour
{
    public static ItemCollectScreen instance;
    public bool visible { get { return _visible; } }
    public AnimationCurve easing;
    private bool _visible;
    private RectTransform _rectTransform;
    private CanvasGroup _canvasGroup;

    public Text youGotText;
    public Text descriptionText;
    public RectTransform advancedDescription;

    public void Awake()
    {
        instance = this;
        _rectTransform = GetComponent<RectTransform>();
        _canvasGroup = GetComponent<CanvasGroup>();
        easing.Evaluate(0);
        gameObject.SetActive(false);
    }

    public void Show(ItemInfo itemInfo, bool shortJingle = false)
    {
        _visible = true;
        youGotText.text = "You Got " + itemInfo.fullName;

        var toDestroy = new List<GameObject>();
        foreach (Transform child in advancedDescription)
        {
            toDestroy.Add(child.gameObject);
        }

        for (int i = 0; i < toDestroy.Count; i++)
        {
            Destroy(toDestroy[i]);
        }

        toDestroy = null;

        if (itemInfo.advancedDescription)
        {
            advancedDescription.gameObject.SetActive(true);
            var advDesc = Instantiate(itemInfo.advancedDescription, advancedDescription, false);
            advDesc.transform.localScale = Vector3.one;
            advDesc.transform.localPosition = Vector3.zero;
            descriptionText.gameObject.SetActive(false);
        }
        else
        {
            advancedDescription.gameObject.SetActive(false);
            descriptionText.gameObject.SetActive(true);
            descriptionText.text = itemInfo.description;
        }
        gameObject.SetActive(true);
        StartCoroutine(WaitForJingle(shortJingle));
    }

    public IEnumerator WaitForJingle(bool shortJingle)
    {
        var zeroY = new Vector3(1, 0, 1);
        _rectTransform.localScale = zeroY;
        _canvasGroup.alpha = 0;

        Time.timeScale = 0;

        if (MusicController.instance.maxMusicVolume < 0.1f || shortJingle)
        {
            UISounds.instance.ItemCollect();
        }
        else
        {
            MusicController.instance.PlayItemJingle();
        }

        //give 3 frames for item collect code to run
        for (int i = 0; i < 3; i++) { yield return null; }

        var buttonsPressed = 0;

        var timer = 0f;
        var appearTime = 0.2f;
        var player1 = PlayerManager.instance.player1;
        while (timer < appearTime)
        {
            timer += Time.unscaledDeltaTime;
            var t = timer / appearTime;
            _rectTransform.localScale = Vector3.Lerp(zeroY, Vector3.one, easing.Evaluate(t));
            _canvasGroup.alpha = Mathf.Lerp(0, 1, t);
            if (player1.controller.GetAnyButtonDown()) { buttonsPressed++; }
            yield return null;
        }

        _canvasGroup.alpha = 1;
        _rectTransform.localScale = Vector3.one;

        var waitTime = shortJingle ? 0.5f : 2.25f;
        waitTime -= timer;
        timer = 0f;
        
        while (timer < waitTime)
        {
            timer += Time.unscaledDeltaTime;
            if (player1.controller.GetAnyButtonDown()) { buttonsPressed++; }
            yield return null;
        }

        while (!player1.controller.GetAnyButtonDown() && buttonsPressed < 5) { yield return null; }

        timer = 0f;
        while (timer < appearTime)
        {
            timer += Time.unscaledDeltaTime;
            var t = timer / appearTime;
            _rectTransform.localScale = Vector3.Lerp(zeroY, Vector3.one, easing.Evaluate(1-t));
            _canvasGroup.alpha = Mathf.Lerp(1, 0, t);            
            yield return null;
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
