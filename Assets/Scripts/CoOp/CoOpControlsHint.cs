using Rewired;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CoOpControlsHint : MonoBehaviour
{
    public GameObject joystickMove;
    public GameObject joystickShoot;
    public GameObject keyboardMove;
    public GameObject keyboardShoot;
    private TextMesh[] _texts;
    private SpriteRenderer[] _renderers;
    private Transform _followTransform;

    public void Update()
    {
        if(_followTransform)
        {
            transform.position = _followTransform.transform.position + Vector3.up * 2;
        }
    }

    public void Show(Transform followTransform, int id)
    {
        _followTransform = followTransform;

        var p = ReInput.players.GetPlayer(id);

        Controller activeController = p.controllers.GetLastActiveController();
        if (activeController == null)
        {
            if (p.controllers.joystickCount > 0)
            { 
                activeController = p.controllers.Joysticks[0];
            }
            else
            { 
                activeController = p.controllers.Keyboard;
            }
        }

        var glyphs = GetComponentsInChildren<SetSpriteForControl>();
        foreach (var g in glyphs)
        {
            g.playerId = id;            
            g.SetSprite();
        }

        keyboardMove.SetActive(activeController.type == ControllerType.Keyboard);
        keyboardShoot.SetActive(activeController.type == ControllerType.Keyboard);
        joystickMove.SetActive(activeController.type == ControllerType.Joystick);
        joystickShoot.SetActive(activeController.type == ControllerType.Joystick);

        gameObject.SetActive(true);
        StartCoroutine(Fade());
    }

    public IEnumerator Fade()
    {
        yield return null;

        _renderers = GetComponentsInChildren<SpriteRenderer>();
        _texts = GetComponentsInChildren<TextMesh>();

        foreach (var r in _renderers)
        {
            r.color = Color.white;
        }

        foreach (var t in _texts)
        {
            var c = t.color;
            c.a = 1;
            t.color = c;
        }

        yield return new WaitForSeconds(1.5f);

        float timer = 0f;
        while (timer < 1)
        {
            timer += Time.deltaTime;
            foreach (var r in _renderers)
            {
                r.color = Color.Lerp(Color.white, Color.clear, timer);
            }

            foreach (var t in _texts)
            {
                var c = t.color;
                c.a = 1 - timer;
                t.color = c;
            }

            yield return null;
        }

        gameObject.SetActive(false);
    }
}
