using CreativeSpore.SuperTilemapEditor;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TilemapPaletteCycling : MonoBehaviour
{
    public Texture2D defaultPalette;
    public PaletteCycle paletteCycle;
    public STETilemap[] tilemaps;
    public float cycleFrequency = 1 / 12f;
    private Material _sharedMaterial;
    private int _currentIndex;
    private float _cycleTimer;

    private void Awake()
    {
        if (tilemaps == null || tilemaps.Length <= 0)
        {
            tilemaps = GetComponentsInChildren<STETilemap>();
        }

        if(tilemaps.Length <= 0)
        {
            Debug.LogError("TilemapPaletteCycling attached to " + gameObject.name + " which has no tilemaps");
            Destroy(this);
            return;
        }

        _sharedMaterial = new Material(tilemaps[0].Material);
        foreach (var tilemap in tilemaps)
        {
            tilemap.Material = _sharedMaterial;
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
        foreach (var tilemap in tilemaps)
        {
            if (tilemap.Material.HasProperty("_Palette"))
            {
                tilemap.Material.SetTexture("_Palette", palette);
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
