using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Rewired.Data.Mapping;
using Rewired;

public class ControllerGlyphs : MonoBehaviour
{
    [SerializeField]
    private ControllerEntry[] _controllers;

    private static ControllerGlyphs _instance;

    void Awake()
    {
        if (!_instance)
        {
            _instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }
    }

    public static Sprite GetGlyph(System.Guid joystickGuid, int elementIdentifierId, AxisRange axisRange)
    {
        if (_instance == null) return null;
        if (_instance._controllers == null) return null;

        // Try to find the glyph
        for (int i = 0; i < _instance._controllers.Length; i++)
        {
            if (_instance._controllers[i] == null) continue;
            if (_instance._controllers[i].joystick == null) continue; // no joystick assigned
            if (_instance._controllers[i].joystick.Guid != joystickGuid) continue; // guid does not match
            return _instance._controllers[i].GetGlyph(elementIdentifierId, axisRange);
        }

        return null;
    }

    [System.Serializable]
    private class ControllerEntry
    {
#pragma warning disable
        public string name;
        public HardwareJoystickMap joystick;
        public GlyphEntry[] glyphs;
#pragma warning restore

        public Sprite GetGlyph(int elementIdentifierId, AxisRange axisRange)
        {
            if (glyphs == null) return null;
            for (int i = 0; i < glyphs.Length; i++)
            {
                if (glyphs[i] == null) continue;
                if (glyphs[i].elementIdentifierId != elementIdentifierId) continue;
                return glyphs[i].GetGlyph(axisRange);
            }
            return null;
        }
    }

    [System.Serializable]
    private class GlyphEntry
    {
#pragma warning disable
        public int elementIdentifierId;
        public Sprite glyph;
        public Sprite glyphPos;
        public Sprite glyphNeg;
#pragma warning restore

        public Sprite GetGlyph(AxisRange axisRange)
        {
            switch (axisRange)
            {
                case AxisRange.Full: return glyph;
                case AxisRange.Positive: return glyphPos != null ? glyphPos : glyph;
                case AxisRange.Negative: return glyphNeg != null ? glyphNeg : glyph;
            }
            return null;
        }
    }
}

