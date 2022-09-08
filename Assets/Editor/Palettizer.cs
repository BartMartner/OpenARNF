using UnityEngine;
using System.Collections;
using UnityEditor;
using System.Collections.Generic;
using System.IO;

public class Palettizer : MonoBehaviour
{
    //based on http://thedragonloft.blogspot.com/2015/04/sprite-palette-swapping-with-shaders-in.html
    [MenuItem("Assets/Palettizer/Palettize Selected Texture")]
    public static void PalettizeSelectedTexture()
    {
        PalettizeTexture(false);
    }

    [MenuItem("Assets/Palettizer/Palettize Palette Texture")]
    public static void PalettizePaletteTexture()
    {
        PalettizeTexture(true);
    }

    public static void PalettizeTexture(bool asPallette)
    {
        bool revertImportSettings = false;
        string path = AssetDatabase.GetAssetPath(Selection.activeObject);
        Debug.Log("path: " + path);
        TextureImporter importer = (TextureImporter)TextureImporter.GetAtPath(path);

        if (!importer.isReadable)
        {
            importer.filterMode = FilterMode.Point;
            importer.textureCompression = TextureImporterCompression.Uncompressed;
            importer.isReadable = true;
            revertImportSettings = true;
            AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceUpdate);
        }

        var selected = Selection.activeObject as Texture2D;
        var pixels = selected.GetPixels32();
        var palletteColors = new Dictionary<Color32, Color32>();
        for (int i = 0; i < pixels.Length; i++)
        {
            var pixel = pixels[i];
            Color32 palletizedColor;

            if (pixel.a == 0)
            {
                continue;
            }

            if (!palletteColors.ContainsKey(pixel))
            {
                palletizedColor = PalettizeColor(palletteColors.Count, pixel);
                if (palletteColors.Count >= 64)
                {
                    Debug.Log("Cannot Palettize Texture. More than 64 colors");
                    if (revertImportSettings) { RevertImportSettings(importer, path); }
                    return;
                }

                palletteColors.Add(pixel, palletizedColor);
            }
            else
            {
                palletizedColor = palletteColors[pixel];
            }

            pixels[i] = palletizedColor;
        }

        selected.SetPixels32(pixels);

        var savePath = Application.dataPath + path.Replace("Assets", string.Empty);
        if (!asPallette)
        { 
            Debug.Log("Resaving texture to " + savePath);
            File.WriteAllBytes(savePath, selected.EncodeToPNG());
        }
        
        string pallettePath;
        if(asPallette)
        {
            pallettePath = savePath;
        }
        else
        {
            var textureName = Path.GetFileNameWithoutExtension(savePath);
            pallettePath = Path.Combine(Path.GetDirectoryName(savePath), textureName + "Palette.png");
        }

        var palletteTexture = new Texture2D(8, 8);
        var palette = new Color32[64];

        foreach (var kvp in palletteColors)
        {
            var color = kvp.Value;
            if(color.a == 0)
            {
                Debug.Log("0 alpha: " + color.ToString());
            }
            int index = GetIndexFromColor(color);
            palette[index] = color;
            Debug.Log("Index: " + index + " = " + color.ToString());
        }

        palletteTexture.SetPixels32(palette);

        Debug.Log("Saving texture to " + pallettePath);
        File.WriteAllBytes(pallettePath, palletteTexture.EncodeToPNG());

        if (revertImportSettings) { RevertImportSettings(importer, path); }
    }

    [MenuItem("Assets/Palettizer/Match Palette")]
    public static void MatchPalette()
    {
        string selectedPath = AssetDatabase.GetAssetPath(Selection.activeObject);
        string path = EditorUtility.OpenFilePanel("Select Palette", selectedPath, "png");
        var selection = Selection.objects;

        if(path.Length != 0 && selection.Length > 0)
        {
            WWW www = new WWW("file:///" + path);
            Texture2D paletteTexture = new Texture2D(8,8);
            www.LoadImageIntoTexture(paletteTexture);
            Color32[] palette = paletteTexture.GetPixels32();
            Debug.Log("Opened Palette at " + path);

            foreach (var obj in selection)
            {
                var texture2D = obj as Texture2D;
                if(texture2D)
                {
                    MatchPalette(texture2D, palette);
                }
            }
        }
    }

    public static void MatchPalette(Texture2D texture, Color32[] palette)
    {
        Debug.Log("Match Pallette Called");

        bool revertImportSettings = false;
        string path = AssetDatabase.GetAssetPath(texture);
        TextureImporter importer = (TextureImporter)TextureImporter.GetAtPath(path);

        if (!importer.isReadable)
        {
            importer.isReadable = true;
            revertImportSettings = true;
            AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceUpdate);
        }

        var pixels = texture.GetPixels32();

        Dictionary<Color32, Color32> closestMatches = new Dictionary<Color32, Color32>();

        for (int i = 0; i < pixels.Length; i++)
        {
            var pixel = pixels[i];

            if(pixel.a == 0)
            {
                continue;
            }

            if (closestMatches.ContainsKey(pixel))
            {
                pixel = closestMatches[pixel];
            }
            else
            {
                Color32 closestMatch = new Color32();
                var closestDiff = int.MaxValue;

                foreach (var color in palette)
                {
                    var diff = Mathf.Abs(color.r - pixel.r) + Mathf.Abs(color.g - pixel.g) + Mathf.Abs(color.b - pixel.b);
                    if (diff < closestDiff)
                    {
                        closestMatch = color;
                        closestDiff = diff;
                    }
                }

                pixel = closestMatch;
            }

            pixels[i] = pixel;
        }

        texture.SetPixels32(pixels);

        var savePath = Application.dataPath + path.Replace("Assets", string.Empty);
        Debug.Log("Resaving texture to " + savePath);
        File.WriteAllBytes(savePath, texture.EncodeToPNG());        

        if (revertImportSettings) { RevertImportSettings(importer, path); }
        AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceUpdate);
    }

    public static void RevertImportSettings(TextureImporter importer, string path)
    {
        importer.isReadable = false;
        AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceUpdate);        
    }

    public static Color32 PalettizeColor(int index, Color32 color)
    {
        if(index >= 64)
        {
            Debug.LogError("Trying to palletize a color with an index > 64!");
        }

        var byteIndex = (byte)index;

        //(543210) byteIndex bit indices
        //(rrbbgg)

        //(76543210) color indices
        //(-----54) red
        //(-----32) green
        //(-----10) blue

        color.r = color.r.BoolSetBit(1, byteIndex.IsBitSet(5));
        color.r = color.r.BoolSetBit(0, byteIndex.IsBitSet(4));
        color.g = color.g.BoolSetBit(1, byteIndex.IsBitSet(3));
        color.g = color.g.BoolSetBit(0, byteIndex.IsBitSet(2));
        color.b = color.b.BoolSetBit(1, byteIndex.IsBitSet(1));
        color.b = color.b.BoolSetBit(0, byteIndex.IsBitSet(0));
        return color;
    }

    public static int GetIndexFromColor(Color32 color)
    {
        byte index = 0;
        index = index.BoolSetBit(5, color.r.IsBitSet(1));
        index = index.BoolSetBit(4, color.r.IsBitSet(0));
        index = index.BoolSetBit(3, color.g.IsBitSet(1));
        index = index.BoolSetBit(2, color.g.IsBitSet(0));
        index = index.BoolSetBit(1, color.b.IsBitSet(1));
        index = index.BoolSetBit(0, color.b.IsBitSet(0));
        return index;
    }
}
