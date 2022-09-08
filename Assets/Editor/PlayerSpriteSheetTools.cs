using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

public class PlayerSpriteSheetTools : MonoBehaviour
{
    public static Int2D cellSize = new Int2D(48, 48);

    [MenuItem("Assets/SpriteSheetTools/Generate Parts Map")]
    public static void CompileTexture() //This is the one that analyzes my old sprites sheets and breaks them down into a smaller sheet with just 
    {
        bool revertImportSettings = false;
        var path = AssetDatabase.GetAssetPath(Selection.activeObject);
        Debug.Log("path: " + path);
        var importer = (TextureImporter)TextureImporter.GetAtPath(path);

        if (!importer.isReadable)
        {
            importer.filterMode = FilterMode.Point;
            importer.textureCompression = TextureImporterCompression.Uncompressed;
            importer.isReadable = true;
            revertImportSettings = true;
            AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceUpdate);
        }

        var selected = Selection.activeObject as Texture2D;
        var pixelMatrix = selected.GetColor32Matrix();

        var cellCountX = selected.width / cellSize.x;
        var cellCountY = selected.height / cellSize.y;
        List<PlayerSpriteSheetPartMapData> parts = new List<PlayerSpriteSheetPartMapData>();
        for (int cellX = 0; cellX < cellCountX; cellX++)
        {
            for (int cellY = 0; cellY < cellCountY; cellY++)
            {
                var cellMinX = cellX * cellSize.x;
                var cellMaxX = cellMinX + cellSize.x;
                var cellMinY = cellY * cellSize.y;
                var cellMaxY = cellMinY + cellSize.y;
                var minX = 960;
                var minY = 480;
                var maxX = 0;
                var maxY = 0;

                for (int x = cellMinX; x < cellMaxX; x++)
                {
                    for (int y = cellMinY; y < cellMaxY; y++)
                    {
                        var pixel = pixelMatrix[x, y];
                        if (pixel.a > 0)
                        {
                            if (x < minX) { minX = x; }
                            if (y < minY) { minY = y; }
                            if (x > maxX) { maxX = x; }
                            if (y > maxY) { maxY = y; }
                        }
                    }
                }

                var padding = 4f;
                minX = (int)Mathf.Clamp(minX - padding, cellMinX, cellMaxX-1);
                maxX = (int)Mathf.Clamp(maxX + padding, cellMinX, cellMaxX-1);
                minY = (int)Mathf.Clamp(minY - padding, cellMinY, cellMaxY-1);
                maxY = (int)Mathf.Clamp(maxY + padding, cellMinY, cellMaxY-1);

                if (minX < maxX && minY < maxY)
                {
                    var newPart = new PlayerSpriteSheetPartMapData(new Int2D(minX - cellMinX, minY - cellMinY), new Int2D(cellX, cellY), CropColor32Matrix(pixelMatrix, minX, maxX, minY, maxY));
                    parts.Add(newPart);
                }
            }
        }

        //eliminating duplicate parts
        var partsMapCondensed = new List<PlayerSpriteSheetPartMapData>();
        var partsMapCellWidth = 0;
        var partsMapCellHeight = 0;
        foreach (var part in parts)
        {
            int rotations = 0;
            PlayerSpriteSheetPartMapData matchingPart = null;
            var pixels = part.pixels.Clone() as Color32[,];

            for (int i = 0; i < 4; i++)
            {
                if(i > 0)
                {
                    pixels = RotateMatrix(pixels, false);
                }
                matchingPart = partsMapCondensed.FirstOrDefault((p) => ComparePixels(p.pixels, pixels));
                rotations = i;
                if (matchingPart != null) break;
            }
            
            if (matchingPart != null)
            {
                part.sourceData.ForEach((d) => d.rotations = rotations);                
                matchingPart.sourceData.AddRange(part.sourceData);
            }
            else
            {
                partsMapCondensed.Add(part);
            }

            if (part.width > partsMapCellWidth) { partsMapCellWidth = part.width; }
            if (part.height > partsMapCellHeight) { partsMapCellHeight = part.height; }
        }

        //copy parts with no duplicates to their own texture
        var partsMapDimensions = Mathf.CeilToInt(Mathf.Sqrt(partsMapCondensed.Count));
        var partsMapSize = new Int2D(partsMapCellWidth * partsMapDimensions, partsMapCellHeight * partsMapDimensions);
        var partsMapPixelMatrix = new Color32[partsMapSize.x, partsMapSize.y];

        var currentCellX = 0;
        var currentCellY = 0;
        var index = 0;

        foreach (var part in partsMapCondensed)
        {
            currentCellX = index % partsMapDimensions;
            currentCellY = (int)(index / partsMapDimensions);
            part.coordinate = new Int2D(currentCellX * partsMapCellWidth, currentCellY * partsMapCellHeight);
            for (int x = 0; x < part.pixels.GetLength(0); x++)
            {
                for (int y = 0; y < part.pixels.GetLength(1); y++)
                {
                    partsMapPixelMatrix[part.coordinate.x + x, part.coordinate.y + y] = part.pixels[x, y];
                }
            }
            //copy pixels it partsMapPixelMatrix;
            index++;
        }

        var partsMapPixels = FlattenColorMatrix(partsMapPixelMatrix);
        var savePath = Application.dataPath + path.Replace("Assets", string.Empty);
        var textureName = Path.GetFileNameWithoutExtension(savePath);
        var partsMapPath = Path.Combine(Path.GetDirectoryName(savePath), textureName + "PartsMap.png");
        var partsMapTexture = new Texture2D(partsMapSize.x, partsMapSize.y); //Determine the size of the chunk sheet
        partsMapTexture.SetPixels32(partsMapPixels);

        Debug.Log("Saving parts map image to " + partsMapPath);
        File.WriteAllBytes(partsMapPath, partsMapTexture.EncodeToPNG());

        string json = JsonConvert.SerializeObject(partsMapCondensed);
        savePath = Application.dataPath + path.Replace("Assets", string.Empty);
        savePath = Path.Combine(Path.GetDirectoryName(savePath), textureName + "PartsMapData.txt");
        Debug.Log("Saving parts map data to " + savePath);
        File.WriteAllText(savePath, json);

        if (revertImportSettings) { RevertImportSettings(importer, path); }
    }

    [MenuItem("Assets/SpriteSheetTools/Build Sprite Sheet")]
    public static void BuildSpriteSheet() //This is the one that takes the chunks and positions them 
    {
        var path = AssetDatabase.GetAssetPath(Selection.activeObject);
        var selected = Selection.activeObject as TextAsset;
        List<PlayerSpriteSheetPartMapData> data = JsonConvert.DeserializeObject<List<PlayerSpriteSheetPartMapData>>(selected.text);

        var texturePath = path.Replace("Data.txt", ".png");
        var texture = AssetDatabase.LoadAssetAtPath<Texture2D>(texturePath);

        var textureColors = texture.GetColor32Matrix();
        Texture2D spriteSheet = new Texture2D(960, 480);
        spriteSheet.SetPixels32(new Color32[960 * 480]);

        foreach (var partData in data)
        {
            var partColors = CropColor32Matrix(textureColors, partData.coordinate, partData.width, partData.height);

            foreach (var cellData in partData.sourceData)
            {
                var colors = cellData.rotations > 0 ? partColors.Clone() as Color32[,] : partColors;
                for (int i = 0; i < cellData.rotations; i++)
                {
                    colors = RotateMatrix(colors, true);
                }

                var flat = FlattenColorMatrix(colors);
                var x = cellData.cellCoordinate.x * cellSize.x + cellData.localPosition.x;
                var y = cellData.cellCoordinate.y * cellSize.y + cellData.localPosition.y;
                spriteSheet.SetPixels32(x,y, colors.GetLength(0), colors.GetLength(1), flat, 0);
            }
        }

        spriteSheet.Apply();
        var spriteSheetPath = texturePath.Replace("PartsMap.png", ".png");
        File.WriteAllBytes(spriteSheetPath, spriteSheet.EncodeToPNG());
        Debug.Log("Saving sprite sheet to " + spriteSheetPath);
    }

    [MenuItem("Assets/SpriteSheetTools/CopyToBlankFrames")]
    public static void CopyFromBaseToBlankFrames()
    {
        bool revertImportSettings = false;
        var path = AssetDatabase.GetAssetPath(Selection.activeObject);
        Debug.Log("path: " + path);
        var importer = (TextureImporter)TextureImporter.GetAtPath(path);
        var sheetTypes = new string[] { "FrontArm", "BackArm", "Torso", "Legs", "Head", "ShoulderPad",};
        string type = string.Empty;

        foreach (var t in sheetTypes)
        {
            if(path.Contains(t))
            {
                Debug.Log("Sprite Sheet Type: " + t);
                type = t;
                break;
            }
        }

        if (!importer.isReadable)
        {
            importer.filterMode = FilterMode.Point;
            importer.textureCompression = TextureImporterCompression.Uncompressed;
            importer.isReadable = true;
            revertImportSettings = true;
            AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceUpdate);
        }

        var selected = Selection.activeObject as Texture2D;
        var pixelMatrix = selected.GetColor32Matrix();

        var cellX = 13;
        var cellY = 1;

        var cellMinX = cellX * cellSize.x;
        var cellMaxX = cellMinX + cellSize.x - 1;
        var cellMinY = cellY * cellSize.y;
        var cellMaxY = cellMinY + cellSize.y - 1;
        
        var framePixels = CropColor32Matrix(pixelMatrix, cellMinX, cellMaxX, cellMinY, cellMaxY);

        var sheetPaths = new List<string>();
        var pathPrefix = "Assets/Resources/PlayerParts/";
        foreach (var item in ItemManager.items.Values)
        {
            switch(type)
            {
                case "FrontArm":                    
                    if (item.armDecal || item.armSprite || item.isEnergyWeapon) { sheetPaths.Add(pathPrefix + item.type.ToString() + "FrontArm.png"); }
                    break;
                case "BackArm":
                    if (item.armDecal || item.armSprite || item.isEnergyWeapon) { sheetPaths.Add(pathPrefix + item.type.ToString() + "BackArm.png"); }
                    break;
                case "Torso":
                    if (item.torsoDecal) { sheetPaths.Add(pathPrefix + item.type.ToString() + ".png"); }
                    if (item.torsoSprite) { sheetPaths.Add(pathPrefix + item.type.ToString() + "Torso.png"); }
                    break;
                case "Legs":
                    if (item.legDecal) { sheetPaths.Add(pathPrefix + item.type.ToString() + ".png"); }
                    if (item.legSprite) { sheetPaths.Add(pathPrefix + item.type.ToString() + "Legs.png"); }
                    break;
                case "Head":
                    if (item.headSprite) { sheetPaths.Add(pathPrefix + item.type.ToString() + "Head.png"); }
                    break;
                case "ShoulderPad":
                    if (item.shoulderPadSprite) { sheetPaths.Add(pathPrefix + item.type.ToString() + "ShoulderPad.png"); }
                    break;
            }
        }

        foreach (var p in sheetPaths)
        {
            Debug.Log(p);
            var texture = AssetDatabase.LoadAssetAtPath<Texture2D>(p);
            var pixels = texture.GetColor32Matrix();

            bool empty = true;
            for (int x = cellMinX; x < cellMaxX; x++)
            {
                for (int y = cellMinY; y < cellMaxY; y++)
                {
                    var pixel = pixels[x, y];
                    if (pixel.a > 0)
                    {
                        empty = false;
                        break;
                    }
                }

                if (!empty) break;
            }

            if (empty)
            {
                for (int x = 0; x < framePixels.GetLength(0); x++)
                {
                    for (int y = 0; y < framePixels.GetLength(1); y++)
                    {
                        pixels[cellMinX + x, cellMinY + y] = framePixels[x, y];
                    }
                }
                Debug.Log("Writing to " + p);
                texture.SetPixels32(FlattenColorMatrix(pixels));
                File.WriteAllBytes(p, texture.EncodeToPNG());
            }
        }

        if (revertImportSettings) { RevertImportSettings(importer, path); }
    }

    public static bool ComparePixels(Color32[,] pixels1, Color32[,] pixels2)
    {
        var p1x = pixels1.GetLength(0);
        var p1y = pixels1.GetLength(1);
        var p2x = pixels2.GetLength(0);
        var p2y = pixels2.GetLength(1);
        if (p1x != p2x || p1y != p2y) return false;

        for (int x = 0; x < p1x; x++)
        {
            for (int y = 0; y < p1y; y++)
            {
                if ((Color)pixels1[x, y] != (Color)pixels2[x, y])
                {
                    return false;
                }
            }
        }

        return true;
    }

    public static Color32[] FlattenColorMatrix(Color32[,] matrix)
    {
        var pixels = new Color32[matrix.Length];
        int i = 0;

        for (int y = 0; y < matrix.GetLength(1); y++)
        {
            for (int x = 0; x < matrix.GetLength(0); x++)
            {
                pixels[i] = matrix[x, y];
                i++;
            }
        }
        return pixels;
    }

    public static Color32[,] RotateMatrix(Color32[,] source, bool clockwise)
    {
        var sourceWidth = source.GetLength(0);
        var sourceHeight = source.GetLength(1);

        var rotated = new Color32[sourceHeight,sourceWidth];

        //rotate
        for (int x = 0; x < sourceWidth; x++)
        {
            for (int y = 0; y < sourceHeight; y++)
            {
                if (clockwise)
                {
                    rotated[sourceHeight-1-y, x] = source[x, y];
                }
                else
                {
                    rotated[y, sourceWidth - 1 - x] = source[x, y];
                }
            }
        }

        return rotated;
    }

    public static Color32[,] CropColor32Matrix(Color32[,] source, Int2D positon, int width, int height)
    {
        return CropColor32Matrix(source, positon.x, positon.x + width-1, positon.y, positon.y + height-1);
    }

    public static Color32[,] CropColor32Matrix(Color32[,] source, int minX, int maxX, int minY, int maxY)
    {
        var size = new Int2D(1 + maxX - minX, 1 + maxY - minY);
        var chunk = new Color32[size.x, size.y];

        for (int x = minX; x <= maxX; x++)
        {
            for (int y = minY; y <= maxY; y++)
            {
                var chunkCoord = new Int2D(x - minX, y - minY);
                var pixel = source[x, y];
                chunk[chunkCoord.x, chunkCoord.y] = pixel.a > 0 ? pixel : (Color32)Color.clear;
            }
        }

        return chunk;
    }

    public static void RevertImportSettings(TextureImporter importer, string path)
    {
        importer.isReadable = false;
        AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceUpdate);
    }

    [Serializable]
    public class CellData
    {
        public Int2D localPosition;
        public Int2D cellCoordinate;
        public int rotations;        

        public CellData(Int2D localPosition, Int2D cellCoordinate, int rotations)
        {
            this.localPosition = localPosition;
            this.cellCoordinate = cellCoordinate;
            this.rotations = rotations;
        }
    }

    [Serializable]
    public class PlayerSpriteSheetPartMapData
    {
        /// <summary>
        /// The pixel coordinate where this part can be found on the partsMap (always starts at 0,0 of that cell)
        /// </summary>
        public Int2D coordinate;

        /// <summary>
        /// A list of cells and local positions within those cells where this chunk goes on the working sprite sheet
        /// </summary>
        public List<CellData> sourceData = new List<CellData>();

        [NonSerialized]
        public Color32[,] pixels;

        public int width;
        public int height;

        public PlayerSpriteSheetPartMapData() { }

        public PlayerSpriteSheetPartMapData(Int2D localPosition, Int2D cellCoordinate, Color32[,] pixels)
        {
            sourceData.Add(new CellData(localPosition, cellCoordinate, 0));
            this.pixels = pixels;
            width = pixels.GetLength(0);
            height = pixels.GetLength(1);
        }
    }
}
