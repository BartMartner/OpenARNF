using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

[RequireComponent(typeof(Text), typeof(AudioSource))]
public class AnimateText : MonoBehaviour
{
    public string textToType;
    public float time;
    public float commaPause = 0.5f;
    public float periodPause = 1f;
    public float colonPause = 1f;
    public bool triggerText;
    public AudioClip[] characterAudio;

    private Text _text;
    private AudioSource _audioSource;
    private bool _typing;

    private void Awake()
    {
        _text = GetComponent<Text>();

        if (textToType == string.Empty)
        {
            textToType = _text.text;
            _text.text = string.Empty;
        }

        _audioSource = GetComponent<AudioSource>();
    }

    public void Update()
    {
        if(triggerText && !_typing)
        {
            triggerText = false;
            StartTypeText();
        }
    }

    public void ClearText()
    {
        _text.text = string.Empty;
    }

    public void SetText(string newText, float newTime)
    {
        StopAllCoroutines();
        textToType = newText;
        time = newTime;
        StartTypeText();
    }

    public void StartTypeText()
    {
        StartCoroutine(TypeText());
    }

    public IEnumerator TypeText()
    {
        _typing = true;
        if(time <= 0)
        {
            _text.text = textToType;
        }
        else
        {
            var chars = textToType.ToCharArray();
            float charsPerSecond = chars.Length / time;
            float delay = 1 / charsPerSecond;
            string currentText = string.Empty;

            for (int i = 0; i < chars.Length; i++)
            {
                currentText = textToType.Insert(i, "<color=#00000000>");
                currentText = currentText.Insert(currentText.Length, "</color>");
                _text.text = currentText;

                if (i % 2 == 0 && characterAudio.Length > 0)
                {
                    _audioSource.PlayOneShot(characterAudio[Random.Range(0, characterAudio.Length)]);
                }

                if (i > 0 && chars[i - 1] == ':')
                {
                    yield return new WaitForSeconds(colonPause);
                }
                else if (i > 0 && chars[i-1] == ',')
                {
                    yield return new WaitForSeconds(commaPause);
                }
                else if (i > 0 && chars[i-1] == '.')
                {
                    yield return new WaitForSeconds(periodPause);
                }
                else
                {
                    yield return new WaitForSeconds(delay);
                }
            }
        }

        _text.text = textToType;
        _typing = false;
    }

    private void OnDisable()
    {
        _typing = false;
    }
}