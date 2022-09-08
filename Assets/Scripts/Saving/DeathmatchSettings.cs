using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;

[Serializable]
public class DeathmatchSettings
{
    [JsonProperty(PropertyName = "mode")]
    public DeathmatchMode mode = DeathmatchMode.TimeLimit;
    [JsonProperty(PropertyName = "tmLmt")]
    public int timeLimit = 600;
    [JsonProperty(PropertyName = "frgLmt")]
    public int fragLimit = 10;
    [JsonProperty(PropertyName = "rltItm")]
    public bool rouletteItems = true;
    [JsonProperty(PropertyName = "spnRmItm")]
    public bool spawnRoomItems = true;
    [JsonProperty(PropertyName = "itmMd")]
    public DeathmatchItemMode itemMode = DeathmatchItemMode.EnergyOnly;
    [JsonProperty(PropertyName = "mpRot")]
    public List<string> mapRotation = new List<string>(DeathmatchManager.allMaps);
    [JsonProperty(PropertyName = "mlmnSpnRt")]
    public int molemanSpawnRate = 0; //number of molemen that spawn per minute
    [JsonProperty(PropertyName = "mxMlmn")]
    public int maxMolemen = 1;
}
