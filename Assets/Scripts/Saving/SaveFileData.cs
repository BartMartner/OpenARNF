using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class SaveFileData
{
    [JsonProperty(PropertyName = "lastVersion")]
    private string _lastVersion = SaveGameManager.version.ToString();
    [JsonIgnore]
    public string lastVersion
    {
        get { return _lastVersion; }
        set
        {
            _lastVersion = value;
            dirty = true;
        }
    }

    [JsonProperty(PropertyName = "musVol")]
    private float _musicVolume = 0.7f;
    [JsonIgnore]
    public float musicVolume
    {
        get { return _musicVolume; }
        set
        {
            _musicVolume = value;
            dirty = true;
        }
    }

    [JsonProperty(PropertyName = "sndVol")]
    private float _soundVolume = 1;
    [JsonIgnore]
    public float soundVolume
    {
        get { return _soundVolume; }
        set
        {
            _soundVolume = value;
            dirty = true;
        }
    }

    [JsonProperty(PropertyName = "fScr")]
    private bool _fullScreen = true;
    [JsonIgnore]
    public bool fullScreen
    {
        get { return _fullScreen; }
        set
        {
            _fullScreen = value;
            dirty = true;
        }
    }

    [JsonProperty(PropertyName = "vSync")]
    private bool _vSync = true;
    [JsonIgnore]
    public bool vSync
    {
        get { return _vSync; }
        set
        {
            _vSync = value;
            dirty = true;
        }
    }

    [JsonProperty(PropertyName = "res")]
    private Int2D _resolution = new Int2D(768, 448);
    [JsonIgnore]
    public Int2D resolution
    {
        get { return _resolution; }
        set
        {
            _resolution = resolution; //WHY THO?!
            dirty = true;
        }
    }

    //1.4.0.25
    [JsonProperty(PropertyName = "con")]
    private bool _conventionMode = false;
    [JsonIgnore]
    public bool conventionMode
    {
        get { return _conventionMode; }
        set
        {
#if ARCADE
            _conventionMode = value;
            dirty = true;
#endif
        }
    }

#if ARCADE
    //1.6.0.35
    [JsonProperty(PropertyName = "freePlay")]
    private bool _freePlay = false;
    [JsonIgnore]
    public bool freePlay
    {
        get { return _freePlay; }
        set
        {
            _freePlay = value;
            dirty = true;
        }
    }
#endif


    //1.5.0.29
    [JsonProperty(PropertyName = "pxlPft")]
    private bool _pixelPefect = false;
    [JsonIgnore]
    public bool pixelPerfect
    {
        get { return _pixelPefect; }
        set
        {
            _pixelPefect = value;
            dirty = true;
        }
    }

    [JsonIgnore]
    public Dictionary<int, SaveSlotData> saveSlots = new Dictionary<int, SaveSlotData>();

    [JsonIgnore]
    public bool dirty;
}
