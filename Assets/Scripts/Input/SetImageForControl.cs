using Rewired;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Image))]
public class SetImageForControl : MonoBehaviour
{
    public string control;
    private Image _image;
    public bool useSystemPlayer;
    public int playerID = 0;
    private Vector3 _defaultSize;

    public void Start()
    {
        SetImage();
        if (PauseMenu.instance) { PauseMenu.instance.onHide += SetImage; }
    }

    public void SetImage()
    {
        var p = useSystemPlayer ? ReInput.players.SystemPlayer : ReInput.players.GetPlayer(playerID); // just use Player 0 in this example

        // Get the last active controller the Player was using
#if UNITY_SWITCH //only look for Joysticks
        Controller activeController = p.controllers.GetLastActiveController(ControllerType.Joystick);
        if (activeController == null)
        {
            if (p.controllers.joystickCount > 0)
            { // try to get the first joystick in player
                activeController = p.controllers.Joysticks[0];
            }
            else
            {
                activeController = ReInput.players.SystemPlayer.controllers.GetLastActiveController(ControllerType.Joystick);
            }
        }
#else
        Controller activeController = p.controllers.GetLastActiveController();
#endif

        if (activeController == null)
        { // player hasn't used any controllers yet
          // No active controller, set a default
            if (p.controllers.joystickCount > 0)
            { // try to get the first joystick in player
                activeController = p.controllers.Joysticks[0];
            }
            else
            { // no joysticks assigned, just get keyboard
                activeController = p.controllers.Keyboard;
            }
        }

        if (!string.IsNullOrEmpty(control))
        {
            InputAction action;
            switch(control)
            {
                case "Up":
                case "Down":
                    action = ReInput.mapping.GetAction("Vertical");
                    break;
                case "Left":
                case "Right":
                    action = ReInput.mapping.GetAction("Horizontal");
                    break;
                default:
                    action = ReInput.mapping.GetAction(control);
                    break;
            }

            if (action != null)
            {
                ActionElementMap aem = null;
                if (control == "Left" || control == "Down" || control == "Up" || control == "Right")
                {
                    var contribution = control == "Left" || control == "Down" ? Pole.Negative : Pole.Positive;
                    foreach (var m in p.controllers.maps.ButtonMapsWithAction(action.id, false))
                    {
                        if (m.controllerMap.controllerId == activeController.id && m.axisContribution == contribution)
                        {
                            aem = m;
                            break;
                        }
                    }
                }
                else
                {
                    aem = p.controllers.maps.GetFirstElementMapWithAction(activeController, action.id, true);
                }
                
                if (aem == null) return; // nothing was mapped on this controller for this Action

                if (activeController.type == ControllerType.Joystick)
                {
                    var text = GetComponentInChildren<Text>();
                    if (text)
                    {
                        Destroy(text.gameObject);
                    }

                    // Find the glyph for the element on the controller
                    Sprite glyph = ControllerGlyphs.GetGlyph((activeController as Joystick).hardwareTypeGuid, aem.elementIdentifierId, aem.axisRange);
                    if (!glyph)
                    {
                        Debug.LogWarning("No glyph found for action " + control);
                        return; // no glyph found
                    }

                    if (!_image)
                    {
                        _image = GetComponent<Image>();
                        _defaultSize = _image.rectTransform.sizeDelta;
                    }
                    else if (_defaultSize == Vector3.zero)
                    {
                        _defaultSize = _image.rectTransform.sizeDelta;
                    }
                    else
                    {
                        _image.rectTransform.sizeDelta = _defaultSize;
                    }

                    _image.sprite = glyph;
                    transform.rotation = Quaternion.identity;
                }
                else if (activeController.type == ControllerType.Keyboard)
                {
                    SetKeyboardText(aem.elementIdentifierName);
                }
            }
        }
    }

    public void SetKeyboardText(string name)
    {
        if (!_image)
        {
            _image = GetComponent<Image>();
            _defaultSize = _image.rectTransform.sizeDelta;
        }
        else if (_defaultSize == Vector3.zero)
        {
            _defaultSize = _image.rectTransform.sizeDelta;
        }

        var sprites = Resources.LoadAll<Sprite>("Sprites/UI/Buttons");
        var text = GetComponentInChildren<Text>();
        Vector2 imageSize = _defaultSize;

        if (name == "Up Arrow")
        {
            _image.sprite = sprites.FirstOrDefault((s) => s.name == "KeyUp");
            if (text) Destroy(text.gameObject);
        }
        else if (name == "Down Arrow")
        {
            _image.sprite = sprites.FirstOrDefault((s) => s.name == "KeyDown");
            if (text) Destroy(text.gameObject);
        }
        else if (name == "Left Arrow")
        {
            _image.sprite = sprites.FirstOrDefault((s) => s.name == "KeyLeft");
            if (text) Destroy(text.gameObject);
        }
        else if (name == "Right Arrow")
        {
            _image.sprite = sprites.FirstOrDefault((s) => s.name == "KeyRight");
            if (text) Destroy(text.gameObject);
        }
        else
        {
            if (name.Length == 1)
            {
                _image.sprite = sprites.FirstOrDefault((s) => s.name == "Key");
            }
            else
            {
                Sprite sprite = sprites.FirstOrDefault((s) => name.Length <= 3 ? s.name == "KeyLarge" : s.name == "KeyVeryLarge"); ;
                _image.sprite = sprite;
                if (_defaultSize == Vector3.zero)
                {
                    imageSize = sprite.bounds.size * sprite.pixelsPerUnit;
                }
                else
                {
                    imageSize.x = _defaultSize.x * sprite.bounds.extents.x;
                }
                _image.rectTransform.sizeDelta = imageSize;
            }

            if (!text)
            {
                var textObject = new GameObject();
                textObject.name = "Text";
                textObject.transform.SetParent(transform);
                text = textObject.AddComponent<Text>();
                text.alignByGeometry = true;
            }

            text.alignment = TextAnchor.MiddleCenter;
            text.transform.localRotation = Quaternion.identity;
            
            if (name.Length == 1)
            {
                text.transform.localScale = Vector3.one;
                text.transform.localPosition = new Vector2(1f, 1.5f);
            }
            else
            {
                var rectTransform = text.GetComponent<RectTransform>();
                rectTransform.anchoredPosition = Vector2.zero;
                rectTransform.sizeDelta = imageSize;
                rectTransform.localScale = Vector3.one;
                text.resizeTextMinSize = 0;
                text.resizeTextForBestFit = true;
            }

            text.text = name;            
            text.font = Resources.Load<Font>("Fonts/uni0553-webfont"); ;
            text.color = Color.black;
        }
    }


    private void OnDestroy()
    {
        if(PauseMenu.instance)
        {
            PauseMenu.instance.onHide -= SetImage;
        }
    }
}
