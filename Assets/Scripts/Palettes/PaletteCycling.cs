using UnityEngine;
using System.Collections;

public class PaletteCycling : MonoBehaviour
{
    public Texture2D defaultPalette;
    public PaletteCycle paletteCycle;
    public Renderer[] renderers;
    public float cycleFrequency = 1 / 12f;
    private int _currentIndex;
    public int currentIndex
    {
        get { return _currentIndex; }
    }
    private float _cycleTimer;

    private void Awake()
    {
        if (renderers == null || renderers.Length <= 0)
        {
            renderers = GetComponentsInChildren<Renderer>();
        }
    }

    private void Update()
    {
        if (paletteCycle)
        {
            if (_cycleTimer < cycleFrequency)
            {
                _cycleTimer += Time.deltaTime;
            }
            else
            {
                _currentIndex = (_currentIndex + 1) % paletteCycle.palettes.Length;
                SetPalettes(paletteCycle.palettes[_currentIndex]);
                _cycleTimer = 0;
            }
        }
    }

    public void SetPalettes(Texture2D palette)
    {
        foreach (var sprite in renderers)
        {
            if(sprite.material.HasProperty("_Palette"))
            {
                sprite.material.SetTexture("_Palette", palette);
            }
        }
    }

    public void OnEnable()
    {
        if (paletteCycle)
        {
            SetPalettes(paletteCycle.palettes[_currentIndex]);
        }
    }

    public void OnDisable()
    {
        if (defaultPalette)
        {
            SetPalettes(defaultPalette);
        }
    }
}
