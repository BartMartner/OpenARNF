using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[DisallowMultipleComponent]
public class ResourcePrefabManager : MonoBehaviour
{
    public static ResourcePrefabManager instance;
    public Dictionary<string, GameObject> gameObjects = new Dictionary<string, GameObject>();
    public Dictionary<string, PaletteCycle> paletteCylces = new Dictionary<string, PaletteCycle>();
    public Dictionary<string, Texture2D> texture2Ds = new Dictionary<string, Texture2D>();
    public Dictionary<string, Sprite> sprites = new Dictionary<string, Sprite>();
    public Dictionary<string, Material> materials = new Dictionary<string, Material>();
    public Dictionary<string, StatusEffect> statusEffects = new Dictionary<string, StatusEffect>();

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void OnDestroy()
    {
        if (instance == this)
        {
            instance = null;
        }
    }

    public GameObject LoadGameObject(string path)
    {
        GameObject result;
        if (!gameObjects.TryGetValue(path, out result))
        {
            result = Resources.Load<GameObject>(path);
            gameObjects.Add(path, result);
        }

        return result;
    }

    public Sprite LoadSprite(string path)
    {
        Sprite result;
        if (!sprites.TryGetValue(path, out result))
        {
            result = Resources.Load<Sprite>(path);
            sprites.Add(path, result);
        }

        return result;
    }

    public Texture2D LoadTexture2D(string path)
    {
        Texture2D result;
        if(!texture2Ds.TryGetValue(path, out result))
        {
            result = Resources.Load<Texture2D>(path);
            texture2Ds.Add(path, result);
        }

        return result;
    }

    public Material LoadMaterial(string path)
    {
        Material result;
        if (!materials.TryGetValue(path, out result))
        {
            result = Resources.Load<Material>(path);
            materials.Add(path, result);
        }

        return result;
    }

    public PaletteCycle LoadPaletteCycle(string path)
    {
        PaletteCycle result;
        if (!paletteCylces.TryGetValue(path, out result))
        {
            result = Resources.Load(path) as PaletteCycle;
            paletteCylces.Add(path, result);
        }

        return result;
    }

    public StatusEffect LoadStatusEffect(string path)
    {
        StatusEffect result;
        if (!statusEffects.TryGetValue(path, out result))
        {
            result = Resources.Load(path) as StatusEffect;
            statusEffects.Add(path, result);
        }

        return result;
    }
}
