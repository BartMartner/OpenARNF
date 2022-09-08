using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System;

public class PlayerSpriteMaker : MonoBehaviour
{
    public string BackArm = "Base";
    public string Torso = "Base";
    public List<string> TorsoDecals = new List<string>();
    public string Head = "Base";
    public string Legs = "Base";
    public List<string> LegDecals = new List<string>();
    public string FrontArm = "Base";
    public List<string> ArmDecals = new List<string>();
    public string ShoulderPad = "Base";
    public Texture2D playerBaseTexture;
    public Dictionary<MajorItem, Dictionary<string, Sprite>> itemSprites = new Dictionary<MajorItem, Dictionary<string, Sprite>>();
    private Texture2D _originalPalette;
    private Color[] _originalColors;

    public bool makeSpriteTrigger;

    private Player _player;    
    private static Material _blitMaterial;
    private static int _calls;
    private static Texture2D _baseTorso;
    private static Texture2D _baseHead;
    private static Texture2D _baseLegs;

    public void Awake()
    {
        _player = GetComponent<Player>();
        if (_blitMaterial == null) { _blitMaterial = Resources.Load<Material>("Materials/SpriteMaker"); }
        if (_baseTorso == null) { _baseTorso = Resources.Load<Texture2D>("PlayerParts/BaseTorso"); }
        if (_baseHead == null) { _baseHead = Resources.Load<Texture2D>("PlayerParts/BaseHead"); }
        if (_baseLegs == null) { _baseLegs = Resources.Load<Texture2D>("PlayerParts/BaseLegs"); }
    }

    public void ClearItems()
    {
        BackArm = "Base";
        Torso = "Base";
        Head = "Base";
        Legs = "Base";
        FrontArm = "Base";
        ShoulderPad = "Base";
    }

    public void ClearDecals()
    {
        TorsoDecals.Clear();
        LegDecals.Clear();
        ArmDecals.Clear();
    }

    public string GetKey()
    {
        string all = BackArm + Torso + Head + Legs + FrontArm + ShoulderPad;
        TorsoDecals.ForEach((s) => all += s);
        LegDecals.ForEach((s) => all += s);
        ArmDecals.ForEach((s) => all += s);
        return all;
    }

    public void MakeSprite()
    {
        _calls++;

        var itemList = new List<MajorItem>() { 0 };
        var count = _player.energyWeapons.Count;
        for (int i = 0; i < count; i++)
        {
            itemList.Add(_player.energyWeapons[i].item);
        }

        _blitMaterial.SetPass(0);
        _blitMaterial.mainTexture = null;

        var originalActive = RenderTexture.active;

        RenderTexture preArmTexture = RenderTexture.GetTemporary(playerBaseTexture.width, playerBaseTexture.height, 32, RenderTextureFormat.ARGB32, RenderTextureReadWrite.Default);
        preArmTexture.filterMode = FilterMode.Point;
        RenderTexture.active = preArmTexture;
        GL.Clear(true, true, Color.clear, 0);

        var torsoTexture = Torso == "Base" ? _baseTorso : Resources.Load<Texture2D>("PlayerParts/" + Torso + "Torso");
        Graphics.Blit(torsoTexture, preArmTexture, _blitMaterial);

        if (TorsoDecals != null)
        {
            for (int i = 0; i < TorsoDecals.Count; i++)
            {
                Graphics.Blit(Resources.Load<Texture2D>("PlayerParts/" + TorsoDecals[i]), preArmTexture, _blitMaterial);
            }
        }

        var headTexture = Head == "Base" ? _baseHead : Resources.Load<Texture2D>("PlayerParts/" + Head + "Head");
        Graphics.Blit(headTexture, preArmTexture, _blitMaterial);

        var legsTexture = Legs == "Base" ? _baseLegs : Resources.Load<Texture2D>("PlayerParts/" + Legs + "Legs");
        Graphics.Blit(legsTexture, preArmTexture, _blitMaterial);
        
        if (LegDecals != null)
        {
            for (int i = 0; i < LegDecals.Count; i++)
            {
                //using Resource.Load because these are apt to change.
                Graphics.Blit(Resources.Load<Texture2D>("PlayerParts/" + LegDecals[i]), preArmTexture, _blitMaterial);
            }
        }

        var shoulderPad = Resources.Load<Texture2D>("PlayerParts/" + ShoulderPad + "ShoulderPad");

        RenderTexture itemRenderTexture;

        var armDecalsFront = new Texture2D[ArmDecals.Count];
        var armDecalsBack = new Texture2D[ArmDecals.Count];

        //using ResourcePrefabManager.instance.LoadTexture2D because Decals stick around even when other items are collected.
        for (int i = 0; i < ArmDecals.Count; i++)
        {
            armDecalsFront[i] = ResourcePrefabManager.instance.LoadTexture2D("PlayerParts/" + ArmDecals[i] + "FrontArm");
            armDecalsBack[i] = ResourcePrefabManager.instance.LoadTexture2D("PlayerParts/" + ArmDecals[i] + "BackArm");
        }

        //using ResourcePrefabManager.instance.LoadTexture2D because these stick around even when other items are collected.
        for (int i = 0; i < itemList.Count; i++)
        {
            var item = itemList[i];

            if (item != MajorItem.None)
            {
                var info = ItemManager.items[item];
                if (info != null && info.noSprite) { continue; }
            }

            itemRenderTexture = RenderTexture.GetTemporary(preArmTexture.descriptor);
            RenderTexture.active = itemRenderTexture;
            GL.Clear(true, true, Color.clear, 0);

            var backArmPath = "PlayerParts/" + (item == 0 ? BackArm : item.ToString()) + "BackArm";
            var frontArmPath = "PlayerParts/" + (item == 0 ? FrontArm : item.ToString()) + "FrontArm";

            Graphics.Blit(ResourcePrefabManager.instance.LoadTexture2D(backArmPath), itemRenderTexture, _blitMaterial);

            if (item == 0)
            {
                for (int j = 0; j < armDecalsBack.Length; j++)
                {
                    if (armDecalsBack[j] != null)
                    {
                        Graphics.Blit(armDecalsBack[j], itemRenderTexture, _blitMaterial);
                    }
                }
            }

            Graphics.Blit(preArmTexture, itemRenderTexture, _blitMaterial);
            Graphics.Blit(ResourcePrefabManager.instance.LoadTexture2D(frontArmPath), itemRenderTexture, _blitMaterial);

            if (item == 0)
            {
                for (int j = 0; j < ArmDecals.Count; j++)
                {
                    Graphics.Blit(armDecalsFront[j], itemRenderTexture, _blitMaterial);
                }
            }

            Graphics.Blit(shoulderPad, itemRenderTexture, _blitMaterial);

            var scratchTexture = new Texture2D(playerBaseTexture.width, playerBaseTexture.height, TextureFormat.ARGB32, false);
            scratchTexture.filterMode = FilterMode.Point;
            scratchTexture.ReadPixels(new Rect(0, 0, itemRenderTexture.width, itemRenderTexture.height), 0, 0);
            scratchTexture.Apply();

            //clean up
            RenderTexture.active = originalActive;
            RenderTexture.ReleaseTemporary(itemRenderTexture);

            Dictionary<string, Sprite> currentSprites;
            if(!itemSprites.TryGetValue(item, out currentSprites))
            {
                currentSprites = new Dictionary<string, Sprite>();
                itemSprites[item] = currentSprites;
            }

            currentSprites.Clear();

            var width = 48;
            var height = 48;
            var xCount = playerBaseTexture.width / width;
            var yCount = playerBaseTexture.height / height;
            for (int y = 0; y < yCount; y++)
            {
                for (int x = 0; x < xCount; x++)
                {
                    //(yCount - y - 1) backwards y coords                
                    Sprite sprite = Sprite.Create(scratchTexture, new Rect(x * width, (yCount - y - 1) * height, width, height), new Vector2(0.5f, 0.5f), 16, 0, SpriteMeshType.FullRect);
                    currentSprites["PlayerBase_" + (x + (y * xCount)).ToString()] = sprite;
                }
            }
        }

        RenderTexture.active = originalActive;
        RenderTexture.ReleaseTemporary(preArmTexture);

        if (_calls >= 30)
        {
            _calls = 0;
            Resources.UnloadUnusedAssets();
        }
    }

    public void Update()
    {
        if (makeSpriteTrigger)
        {
            makeSpriteTrigger = false;
            MakeSprite();
        }
    }

    public void LateUpdate()
    {
        if (_player)
        {
            Sprite sprite;
            Dictionary<string, Sprite> currentSprites;
            var item = _player.selectedEnergyWeapon == null ? 0 : _player.selectedEnergyWeapon.item;
            if (itemSprites.TryGetValue(item, out currentSprites) && currentSprites.TryGetValue(_player.mainRenderer.sprite.name, out sprite))
            {
                _player.mainRenderer.sprite = sprite;
            }
        }
    }

    public void SaveTexture(Texture2D texture)
    {
#if !UNITY_SWITCH
        var bytes = texture.EncodeToPNG();
        var dir = Application.dataPath + "/" + "Temp/";
        if (!Directory.Exists(dir))
        {
            Directory.CreateDirectory(dir);
        }

        File.WriteAllBytes(dir + "LastPlayerSprites.png", bytes);        
#endif
    }

    //public Texture2D AveragePalettesRGB(List<Texture2D> palettes)
    //{
    //    var firstPalette = palettes.First();
    //    var length = firstPalette.GetPixels().Length;
    //    var colorSums = new Vector4[length];

    //    foreach (var texture in palettes)
    //    {
    //        var colors = texture.GetPixels();
    //        for (int i = 0; i < colors.Length; i++)
    //        {
    //            colorSums[i].w += Mathf.Pow(colors[i].r, 2);
    //            colorSums[i].x += Mathf.Pow(colors[i].g, 2);
    //            colorSums[i].y += Mathf.Pow(colors[i].b, 2);
    //            colorSums[i].z += Mathf.Pow(colors[i].a, 2);
    //        }
    //    }

    //    var newColors = new Color[length];

    //    for (int i = 0; i < colorSums.Length; i++)
    //    {
    //        var sum = colorSums[i] / palettes.Count;
    //        var r = Mathf.Sqrt(sum.w);
    //        var g = Mathf.Sqrt(sum.x);
    //        var b = Mathf.Sqrt(sum.y);
    //        var a = Mathf.Sqrt(sum.z);
    //        newColors[i] = new Color(r,g,b,a);
    //    }

    //    Texture2D summedTexture = new Texture2D(firstPalette.width, firstPalette.height, TextureFormat.ARGB32, false);
    //    summedTexture.filterMode = FilterMode.Point;
    //    summedTexture.SetPixels(newColors);
    //    summedTexture.Apply();

    //    return summedTexture;
    //}

    //public Texture2D AveragePalettesComboSqr(List<Texture2D> palettes)
    //{
    //    var firstPalette = palettes.First();
    //    var length = firstPalette.GetPixels().Length;
    //    var slaSums = new Vector3[length];
    //    var rgbs = new Vector4[length];
    //    foreach (var texture in palettes)
    //    {
    //        var colors = texture.GetPixels();
    //        for (int i = 0; i < colors.Length; i++)
    //        {
    //            var hsl = RGBToHSL(colors[i]);

    //            rgbs[i].w += Mathf.Pow(colors[i].r, 2);
    //            rgbs[i].x += Mathf.Pow(colors[i].g, 2);
    //            rgbs[i].y += Mathf.Pow(colors[i].b, 2);
    //            rgbs[i].z += Mathf.Pow(colors[i].a, 2);

    //            slaSums[i].x += hsl.x;
    //            slaSums[i].y += hsl.y;
    //            slaSums[i].z += hsl.z;
    //        }
    //    }

    //    var newColors = new Color[length];

    //    for (int i = 0; i < length; i++)
    //    {
    //        var sum = new Vector4();

    //        var rgbSum = rgbs[i] / palettes.Count;
    //        var rgbColor = new Color();
    //        rgbColor.r = Mathf.Sqrt(rgbSum.w);
    //        rgbColor.g = Mathf.Sqrt(rgbSum.x);
    //        rgbColor.b = Mathf.Sqrt(rgbSum.y);
    //        rgbColor.a = Mathf.Sqrt(rgbSum.z);
    //        sum.w = RGBToHSL(rgbColor).w;

    //        sum.x = slaSums[i].x / palettes.Count;
    //        sum.y = slaSums[i].y / palettes.Count;
    //        sum.z = slaSums[i].z / palettes.Count;
    //        newColors[i] = HSLToRGB(sum);
    //    }

    //    Texture2D summedTexture = new Texture2D(firstPalette.width, firstPalette.height, TextureFormat.ARGB32, false);
    //    summedTexture.filterMode = FilterMode.Point;
    //    summedTexture.SetPixels(newColors);
    //    summedTexture.Apply();

    //    return summedTexture;
    //}

    //public Texture2D AveragePalettesCombo(List<Texture2D> palettes)
    //{
    //    var firstPalette = palettes.First();
    //    var length = firstPalette.GetPixels().Length;
    //    var slaSums = new Vector3[length];
    //    var rgbs = new Vector4[length];
    //    foreach (var texture in palettes)
    //    {
    //        var colors = texture.GetPixels();
    //        for (int i = 0; i < colors.Length; i++)
    //        {
    //            var hsl = RGBToHSL(colors[i]);
    //            rgbs[i] += (Vector4)colors[i];
    //            slaSums[i].x += hsl.x;
    //            slaSums[i].y += hsl.y;
    //            slaSums[i].z += hsl.z;
    //        }
    //    }

    //    var newColors = new Color[length];

    //    for (int i = 0; i < length; i++)
    //    {
    //        var sum = new Vector4();

    //        sum.w = RGBToHSL(rgbs[i]/palettes.Count).w;
    //        sum.x = slaSums[i].x / palettes.Count;
    //        sum.y = slaSums[i].y / palettes.Count;
    //        sum.z = slaSums[i].z / palettes.Count;
    //        newColors[i] = HSLToRGB(sum);
    //    }

    //    Texture2D summedTexture = new Texture2D(firstPalette.width, firstPalette.height, TextureFormat.ARGB32, false);
    //    summedTexture.filterMode = FilterMode.Point;
    //    summedTexture.SetPixels(newColors);
    //    summedTexture.Apply();

    //    return summedTexture;
    //}

    public Texture2D AveragePalettesHSL(List<Texture2D> palettes)
    {
        if (!_originalPalette)
        {
            switch (_player.team)
            {
                case Team.DeathMatch0:
                    _originalPalette = Resources.Load<Texture2D>("Palettes/Player/Deathmatch/DeathmatchPlayer1");
                    break;
                case Team.DeathMatch1:
                    _originalPalette = Resources.Load<Texture2D>("Palettes/Player/Deathmatch/DeathmatchPlayer2");
                    break;
                case Team.DeathMatch2:
                    _originalPalette = Resources.Load<Texture2D>("Palettes/Player/Deathmatch/DeathmatchPlayer3");
                    break;
                case Team.DeathMatch3:
                    _originalPalette = Resources.Load<Texture2D>("Palettes/Player/Deathmatch/DeathmatchPlayer4");
                    break;
                default:
                    _originalPalette = Resources.Load<Texture2D>("Palettes/Player/PlayerPalette");
                    break;
            }

            _originalColors = _originalPalette.GetPixels();
        }

        var length = _originalColors.Length;
        var hSums = new Vector2[length];
        var hslaSums = new Vector4[length];

        var ramps = new Dictionary<int, int>()
        {
            {0,0},{1,0},{2,0},{3,0},{4,0}, {5,1},{6,1},{7,1},
            {8,2},{9,2},{10,2},{11,2}, {12,3},{13,3},{14,3},{15,3},
            {16,4},{17,4},{18,4},{19,4}, {20,5},{21,5},{22,5},{23,5},
            {24,6},{25,6},{26,6},{27,6}, {28,7},{29,7},{30,7}, {31,8},
            {32,9},{33,9},{34,9},{35,9},
        };

        for (int i = 36; i < 64; i++) ramps.Add(i, 10);       

        foreach (var texture in palettes)
        {
            var colors = texture.GetPixels();
            for (int i = 0; i < colors.Length; i++)
            {
                var hsl = RGBToHSL(colors[i]);
                hSums[i].x += Mathf.Cos(hsl.w * Mathf.Deg2Rad);
                hSums[i].y += Mathf.Sin(hsl.w * Mathf.Deg2Rad);
                hslaSums[i].w += hsl.w;
                hslaSums[i].x += hsl.x;
                hslaSums[i].y += hsl.y;
                hslaSums[i].z += hsl.z;
            }
        }

        var newColors = new Color[length];

        var previousHue = 0f;
        for (int i = 0; i < length; i++)
        {
            //Don't color deathmatch shoulders
            if (DeathmatchManager.instance && i >= 12 && i < 16)
            {
                newColors[i] = _originalColors[i];
                continue;
            }

            var sum = new Vector4();

            var hx = hSums[i].x / palettes.Count;
            var hy = hSums[i].y / palettes.Count;
            sum.w = Mathf.Atan2(hy, hx) * Mathf.Rad2Deg;

            if (i != 0)
            {
                var hueDelta = Mathf.DeltaAngle(previousHue, sum.w);
                if (Mathf.Abs(hueDelta) > 90 && ramps[i] == ramps[i - 1])
                {
                    sum.w = (sum.w + 180) % 360;
                }
            }

            previousHue = sum.w;

            sum.x = hslaSums[i].x / palettes.Count;
            sum.y = hslaSums[i].y / palettes.Count;
            sum.z = hslaSums[i].z / palettes.Count;
            newColors[i] = HSLToRGB(sum);
        }

        Texture2D summedTexture = new Texture2D(_originalPalette.width, _originalPalette.height, TextureFormat.ARGB32, false);
        summedTexture.filterMode = FilterMode.Point;
        summedTexture.SetPixels(newColors);
        summedTexture.Apply();

        return summedTexture;
    }

    public Vector4 RGBToHSL(Color rgb)
    {
        Vector4 hsl = new Vector4();

        float r = rgb.r;
        float g = rgb.g;
        float b = rgb.b;
        float a = rgb.a;

        hsl.z = a;

        float min = Mathf.Min(Mathf.Min(r, g), b);
        float max = Mathf.Max(Mathf.Max(r, g), b);
        float delta = max - min;

        hsl.y = (max + min) / 2;

        if (delta == 0)
        {
            hsl.w = 0;
            hsl.x = 0.0f;
        }
        else
        {
            hsl.x = (hsl.y <= 0.5) ? (delta / (max + min)) : (delta / (2 - max - min));

            float hue;

            if (r == max)
            {
                hue = ((g - b) / 6) / delta;
            }
            else if (g == max)
            {
                hue = (1.0f / 3) + ((b - r) / 6) / delta;
            }
            else
            {
                hue = (2.0f / 3) + ((r - g) / 6) / delta;
            }

            if (hue < 0)
                hue += 1;
            if (hue > 1)
                hue -= 1;

            hsl.w = (int)(hue * 360);
        }

        return hsl;
    }

    public Color HSLToRGB(Vector4 hsl)
    {
        float r = 0;
        float g = 0;
        float b = 0;

        if (hsl.x == 0)
        {
            r = g = b = (hsl.y);
        }
        else
        {
            float v1, v2;
            float hue = (float)hsl.w / 360;

            v2 = (hsl.y < 0.5) ? (hsl.y * (1 + hsl.x)) : ((hsl.y + hsl.x) - (hsl.y * hsl.x));
            v1 = 2 * hsl.y - v2;

            r = (HueToRGB(v1, v2, hue + (1.0f / 3)));
            g = (HueToRGB(v1, v2, hue));
            b = (HueToRGB(v1, v2, hue - (1.0f / 3)));
        }

        return new Color(r, g, b, hsl.z);
    }

    private float HueToRGB(float v1, float v2, float vH)
    {
        if (vH < 0)
            vH += 1;

        if (vH > 1)
            vH -= 1;

        if ((6 * vH) < 1)
            return (v1 + (v2 - v1) * 6 * vH);

        if ((2 * vH) < 1)
            return v2;

        if ((3 * vH) < 2)
            return (v1 + (v2 - v1) * ((2.0f / 3) - vH) * 6);

        return v1;
    }

    private Texture2D[] GetAllPalettes(Func<List<Texture2D>, Texture2D> averageMethod)
    {
        var items = new MajorItem[] { MajorItem.AspectShell, MajorItem.BrightShell, MajorItem.HeatShell, MajorItem.SpeedShell, MajorItem.SpeedShell, MajorItem.ViridianShell, MajorItem.WreckingShell };
        var basePalettes = new List<Texture2D>();
        for (int i = 0; i < items.Length; i++)
        {
            basePalettes.Add(Resources.Load<Texture2D>("Palettes/Player/PlayerPalette" + items[i]));
        }

        var allCombos = Constants.GetAllCombos(basePalettes);
        var croppedCombos = allCombos.Where((c) => c.Count <= 3).ToArray();
        var palettes = new Texture2D[croppedCombos.Length];

        for (int i = 0; i < croppedCombos.Length; i++)
        {
            palettes[i] = averageMethod(croppedCombos[i]);
        }

        return palettes;
    }

    public void TestPalettes()
    {
        var allPalettes = GetAllPalettes(AveragePalettesHSL);
        PaletteCycle cycle = new PaletteCycle();
        cycle.palettes = allPalettes;
        PaletteCycling cycling = _player.gameObject.AddComponent<PaletteCycling>();
        cycling.paletteCycle = cycle;
        cycling.cycleFrequency = 0.5f;
    }
}

public enum SpriteMakerMode
{
    Automatic,
    Force,
    Skip,
}
