using Rewired;
using Rewired.UI.ControlMapper;
using System.Collections;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Player = Rewired.Player;

[RequireComponent(typeof(SpriteRenderer))]
public class SetSpriteForControl : MonoBehaviour
{
    public string control;
    public int playerId = 0;
    private SpriteRenderer _spriteRenderer;
    private ControlMapper _controlMapper;

    public IEnumerator Start()
    {
        SetSprite();

        while(!InputHelper.instance.controlMapper)
        {
            yield return null;
        }

        _controlMapper = InputHelper.instance.controlMapper;
        if(_controlMapper)
        {
            _controlMapper.ScreenClosedEvent += SetSprite;
        }
    }

    public void SetSprite()
    {
        var p = ReInput.players.GetPlayer(playerId); // just use Player 0 in this example

        // Get the last active controller the Player was using
#if UNITY_SWITCH //only look for Joysticks
        Controller activeController = p.controllers.GetLastActiveController(ControllerType.Joystick);
        if (activeController == null)
        {
            if (p.controllers.joystickCount > 0)
            {   
                activeController = p.controllers.Joysticks[0];
            }
            else
            {
                activeController = ReInput.players.SystemPlayer.controllers.GetLastActiveController(ControllerType.Joystick);
            }
        }
#else
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
#endif

        if (!string.IsNullOrEmpty(control))
        {
            InputAction action;
            switch (control)
            {
                case "Up":
                case "Down":
                    action = ReInput.mapping.GetAction("Vertical");
                    break;
                case "Left":
                case "Right":
                    action = ReInput.mapping.GetAction("Horizontal");
                    break;
                case "CoOpMoveUp":
                case "CoOpMoveDown":
                    action = ReInput.mapping.GetAction("CoOpMoveVertical");
                    break;
                case "CoOpMoveLeft":
                case "CoOpMoveRight":
                    action = ReInput.mapping.GetAction("CoOpMoveHorizontal");
                    break;
                case "CoOpShootUp":
                case "CoOpShootDown":
                    action = ReInput.mapping.GetAction("CoOpShootVertical");
                    break;
                case "CoOpShootLeft":
                case "CoOpShootRight":
                    action = ReInput.mapping.GetAction("CoOpShootHorizontal");
                    break;
                default:
                    action = ReInput.mapping.GetAction(control);
                    break;
            }

            if (action != null)
            {
                ActionElementMap aem = null;
                switch (control)
                {
                    case "Up":
                    case "Down":
                    case "Left":
                    case "Right":
                    case "CoOpMoveUp":
                    case "CoOpMoveDown":
                    case "CoOpMoveLeft":
                    case "CoOpMoveRight":
                    case "CoOpShootUp":
                    case "CoOpShootDown":
                    case "CoOpShootLeft":
                    case "CoOpShootRight":
                        var negs = new HashSet<string> { "Left", "Down", "CoOpMoveDown", "CoOpMoveLeft", "CoOpShootDown", "CoOpShootLeft" };
                        var contribution = negs.Contains(control) ? Pole.Negative : Pole.Positive;
                        foreach (var m in p.controllers.maps.ButtonMapsWithAction(action.id, false))
                        {
                            if (m.controllerMap.controllerId == activeController.id && m.axisContribution == contribution)
                            {
                                aem = m;
                                break;
                            }
                        }

                        if(aem == null)
                        {
                            aem = p.controllers.maps.GetFirstElementMapWithAction(activeController, action.id, false);
                        }
                        break;
                    case "CoOpMoveVertical":
                    case "CoOpMoveHorizontal":
                    case "CoOpShootVertical":
                    case "CoOpShootHorizontal":
                        foreach (var m in p.controllers.maps.ElementMapsWithAction(action.id, false))
                        {
                            if (m.controllerMap.controllerId == activeController.id && 
                                m.axisType != AxisType.None && m.axisRange == AxisRange.Full)
                            {
                                aem = m;
                                break;
                            }
                        }
                        break;
                    default:
                        aem = p.controllers.maps.GetFirstElementMapWithAction(activeController, action.id, false);                        
                        break;
                }
                
                if (aem == null) return; // nothing was mapped on this controller for this Action                

                if (activeController.type == ControllerType.Joystick)
                {
                    var text = GetComponentInChildren<TextMesh>();
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

                    if (!_spriteRenderer)
                    {
                        _spriteRenderer = GetComponent<SpriteRenderer>();
                    }

                    _spriteRenderer.sprite = glyph;
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
        if (!_spriteRenderer)
        {
            _spriteRenderer = GetComponent<SpriteRenderer>();
        }

        var sprites = Resources.LoadAll<Sprite>("Sprites/UI/Buttons");
        var text = GetComponentInChildren<TextMesh>();

        if (name == "Up Arrow")
        {
            _spriteRenderer.sprite = sprites.FirstOrDefault((s) => s.name == "KeyUp");
            if(text) Destroy(text.gameObject);
        }
        else if (name == "Down Arrow")
        {
            _spriteRenderer.sprite = sprites.FirstOrDefault((s) => s.name == "KeyDown");
            if (text) Destroy(text.gameObject);
        }
        else if (name == "Left Arrow")
        {
            _spriteRenderer.sprite = sprites.FirstOrDefault((s) => s.name == "KeyLeft");
            if (text) Destroy(text.gameObject);
        }
        else if (name == "Right Arrow")
        {
            _spriteRenderer.sprite = sprites.FirstOrDefault((s) => s.name == "KeyRight");
            if (text) Destroy(text.gameObject);
        }
        else
        {
            if (name.Length == 1)
            {
                _spriteRenderer.sprite = sprites.FirstOrDefault((s) => s.name == "Key");
            }
            else if (name.Length <= 3)
            {
                _spriteRenderer.sprite = sprites.FirstOrDefault((s) => s.name == "KeyLarge");
            }
            else
            {
                _spriteRenderer.sprite = sprites.FirstOrDefault((s) => s.name == "KeyVeryLarge");
            }

            if (!text)
            {
                var textObject = new GameObject();
                textObject.name = "Text";
                textObject.transform.SetParent(transform);
                textObject.transform.localPosition = new Vector2(0.05f, 0.1f);
                text = textObject.AddComponent<TextMesh>();
            }

            var renderer = text.GetComponent<Renderer>();
            renderer.sortingLayerID = _spriteRenderer.sortingLayerID;
            renderer.sortingOrder = _spriteRenderer.sortingOrder + 1;
            text.alignment = TextAlignment.Center;
            text.anchor = TextAnchor.MiddleCenter;
            text.text = name;
            var font = Resources.Load<Font>("Fonts/uni0553-webfont");
            text.font = font;
            text.color = new Color(0,0,0, _spriteRenderer.color.a);
            renderer.material = font.material;
            text.transform.localRotation = Quaternion.identity;
            text.transform.localScale = new Vector3(0.075f, 0.075f, 1);

            text.fontSize = 100;
        }
    }

    public void OnDestroy()
    {
        if (_controlMapper)
        {
            _controlMapper.ScreenClosedEvent -= SetSprite;
        }        
    }
}
