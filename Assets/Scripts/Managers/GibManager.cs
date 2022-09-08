using UnityEngine;
using System.Collections.Generic;
using System.Collections;
using System;
using Random = UnityEngine.Random;
using System.Linq;

[DisallowMultipleComponent]
public class GibManager : MonoBehaviour
{
    public static GibManager instance;

    public List<Gib> meatGibs;
    public List<Gib> paleMeatGibs;
    public List<Gib> pinkMeatGibs;
    public List<Gib> greenMeatGibs;
    public List<Gib> brownRockGibs;
    public List<Gib> caveMetalGibs;
    public List<Gib> glitchGibs;
    public List<Gib> explosiveDoorShield;
    public List<Gib> concreteGibs;
    public List<Gib> buriedCityRockGibs;
    public List<Gib> greenTechGibs;
    public List<Gib> boneDoorGibs;
    private Dictionary<GibType, List<Gib>> _gibs = new Dictionary<GibType, List<Gib>>();
    private Dictionary<GibType, int> _gibPrefabIndex = new Dictionary<GibType, int>();

    public List<SpriteRenderer> glitchSplatterPrefabs;
    private Dictionary<int, List<SpriteRenderer>> _glitchSplatters = new Dictionary<int, List<SpriteRenderer>>();
    public List<SpriteRenderer> bloodSplatterPrefabs;
    private Dictionary<int, List<SpriteRenderer>> _bloodSplatters = new Dictionary<int, List<SpriteRenderer>>();

    public ParticleSystem bloodParticlesPrefab;

    private void Awake()
    {
        instance = this;
    }

    private void Start()
    { 
        var gibTypes = Enum.GetValues(typeof(GibType));

        foreach (GibType gType in gibTypes)
        {
            _gibPrefabIndex.Add(gType, 0);
        }

        foreach (GibType gType in Enum.GetValues(typeof(GibType)))
        {
            _gibs.Add(gType, new List<Gib>());
            NewGib(gType);
        }

        for (int i = 0; i < bloodSplatterPrefabs.Count; i++)
        {
            _bloodSplatters.Add(i, new List<SpriteRenderer>());
        }

        for (int i = 0; i < glitchSplatterPrefabs.Count; i++)
        {
            _glitchSplatters.Add(i, new List<SpriteRenderer>());
        }

        if (LayoutManager.instance)
        {
            LayoutManager.instance.onTransitionComplete += OnTransitionComplete;
        }
    }

    public void ClearGibs(GibType[] gibTypes)
    {
        for (int i = 0; i < gibTypes.Length; i++)
        {
            var type = gibTypes[i];
            List<Gib> list;
            if (_gibs.TryGetValue(type, out list))
            {
                for (int j = 0; j < list.Count; j++) { Destroy(list[j].gameObject); }
                list.Clear();
            }
        }
    }

    public void OnTransitionComplete()
    {
        foreach (var kvp in _bloodSplatters)
        {
            foreach (var splatter in kvp.Value)
            {
                splatter.gameObject.SetActive(false);
            }
        }
    }

    public Gib NewGib(GibType gType)
    {
        Gib newGib = null;

        int gibCount = 0;

        switch (gType)
        {
            case GibType.Meat:
                newGib = Instantiate(meatGibs[_gibPrefabIndex[gType]]) as Gib;
                gibCount = meatGibs.Count;
                break;
            case GibType.PaleMeat:
                newGib = Instantiate(paleMeatGibs[_gibPrefabIndex[gType]]) as Gib;
                gibCount = paleMeatGibs.Count;
                break;
            case GibType.PinkMeat:
                newGib = Instantiate(pinkMeatGibs[_gibPrefabIndex[gType]]) as Gib;
                gibCount = pinkMeatGibs.Count;
                break;
            case GibType.GreenMeat:
                newGib = Instantiate(greenMeatGibs[_gibPrefabIndex[gType]]) as Gib;                
                gibCount = greenMeatGibs.Count;
                break;
            case GibType.BrownRock:
                newGib = Instantiate(brownRockGibs[_gibPrefabIndex[gType]]) as Gib;
                gibCount = brownRockGibs.Count;
                break;
            case GibType.CaveMetal:
                newGib = Instantiate(caveMetalGibs[_gibPrefabIndex[gType]]) as Gib;
                gibCount = caveMetalGibs.Count;
                break;
            case GibType.ExplosiveDoorShield:
                newGib = Instantiate(explosiveDoorShield[_gibPrefabIndex[gType]]) as Gib;
                gibCount = explosiveDoorShield.Count;
                break;
            case GibType.Concrete:
                newGib = Instantiate(concreteGibs[_gibPrefabIndex[gType]]) as Gib;
                gibCount = concreteGibs.Count;
                break;
            case GibType.BuriedCityRock:
                newGib = Instantiate(buriedCityRockGibs[_gibPrefabIndex[gType]]) as Gib;
                gibCount = buriedCityRockGibs.Count;
                break;
            case GibType.GlitchGib:
                newGib = Instantiate(glitchGibs[_gibPrefabIndex[gType]]) as Gib;
                gibCount = glitchGibs.Count;
                break;
            case GibType.GreenTech:
                newGib = Instantiate(greenTechGibs[_gibPrefabIndex[gType]]) as Gib;
                gibCount = greenTechGibs.Count;
                break;
            case GibType.BoneDoorShield:
                newGib = Instantiate(boneDoorGibs[_gibPrefabIndex[gType]]) as Gib;
                gibCount = boneDoorGibs.Count;
                break;
        }

        if (newGib)
        {
            newGib.transform.parent = transform;
            newGib.gameObject.SetActive(false);
            if (_gibs[gType].Count < 50)
            {
                _gibs[gType].Add(newGib);
            }
            else
            {
                newGib.recycle = false;
            }
        }

        _gibPrefabIndex[gType] = (_gibPrefabIndex[gType] + 1) % gibCount;

        return newGib;
    }

    public void AddBloodParticles(Transform parent)
    {
        var bloodParticles = Instantiate(bloodParticlesPrefab);
        bloodParticles.transform.SetParent(parent);
        bloodParticles.transform.localPosition = Vector3.zero;
    }

    public void SpawnBloodSplatter(Vector3 position, float scale, bool glitch = false)
    {
        if(scale <= 0)
        {
            return;
        }

        List<SpriteRenderer> splatters;
        var ceilScale = Mathf.CeilToInt(scale);
        var dict = glitch ? _glitchSplatters : _bloodSplatters;
        var list = glitch ? glitchSplatterPrefabs : bloodSplatterPrefabs;

        var index = Mathf.Clamp(ceilScale - 1, 0, list.Count);

        if(dict.TryGetValue(index, out splatters))
        {
            SpriteRenderer splatter = null;
            SpriteRenderer backUpSplatter = null;
            for (int i = 0; i < splatters.Count; i++)
            {
                if(!backUpSplatter && !splatters[i].isVisible)
                {
                    backUpSplatter = splatters[i];
                }

                if(!splatters[i].gameObject.activeInHierarchy)
                {
                    splatter = splatters[i];
                    break;
                }
            }

            if(!splatter)
            {
                if (splatters.Count < 300)
                {
                    splatter = Instantiate(list[index]);
                    splatter.transform.parent = transform;
                    dict[index].Add(splatter);
                }
                else
                {
                    if (backUpSplatter)
                    {
                        splatter = backUpSplatter;
                    }
                    else
                    {
                        splatter = splatters.FirstOrDefault((s) => !s.isVisible);
                        if (!splatter) { return; }
                    }
                }
            }

            splatter.gameObject.SetActive(true);
            
            position.x = Mathf.Round(position.x / 0.0625f) * 0.0625f;
            position.y = Mathf.Round(position.y / 0.0625f) * 0.0625f;
            position.z = 0;
            
            splatter.transform.position = position;
            splatter.transform.rotation = Quaternion.Euler(0, 0, Random.Range(0, 3) * 90);
            splatter.sortingLayerName = (LayoutManager.instance && LayoutManager.instance.currentRoom.hasFadeAways) ? "AboveFadeAways" : "AboveTiles";

            var grow = splatter.GetComponent<GrowBehaviour>();
            var targetScale = Vector3.one * (scale / (float)ceilScale);
            if (grow)
            {
                splatter.transform.localScale = Vector3.zero;
                grow.Grow(targetScale);
            }
            else
            {
                splatter.transform.localScale = targetScale;
            }
        }
    }

    public void SpawnGibs(GibType gType, Vector3 position, int amount, float force = 10, float lifeSpan = 5)
    {
        List<Gib> eList;

        if (_gibs.TryGetValue(gType, out eList))
        {
            for (int a = 0; a < amount; a++)
            {
                Gib gib = null;

                for (int i = 0; i < eList.Count; i++)
                {
                    if (!eList[i].gameObject.activeInHierarchy)
                    {
                        gib = eList[i];
                        break;
                    }
                }

                if (!gib && eList.Count < 100)
                {
                    gib = NewGib(gType);
                }

                if (gib)
                {
                    SpawnGib(gib, gType, position, force, lifeSpan);
                }
            }
        }
    }

    public void SpawnGibs(GibType gType, Rect area, int amount, float force  = 10, float lifeSpan = 5)
    {
        List<Gib> eList;

        if (_gibs.TryGetValue(gType, out eList))
        {
            for (int a = 0; a < amount; a++)
            {
                Gib gib = null;

                for (int i = 0; i < eList.Count; i++)
                {
                    if (!eList[i].gameObject.activeInHierarchy)
                    {
                        gib = eList[i];
                        break;
                    }
                }

                if (!gib)
                {
                    gib = NewGib(gType);
                }

                var position = new Vector2(Random.Range(area.xMin, area.xMax), Random.Range(area.yMin, area.yMax));

                SpawnGib(gib, gType, position, force, lifeSpan);
            }
        }
    }

    private void SpawnGib(Gib gib, GibType gType, Vector3 position, float force, float lifeSpan)
    {
        if (gType == GibType.Meat || gType == GibType.PaleMeat || gType == GibType.PinkMeat || gType == GibType.GreenMeat)
        {
            var blood = gib.GetComponentInChildren<ParticleSystem>();
            var activeSlot = SaveGameManager.activeSlot;
            if (activeSlot != null)
            {
                if (activeSlot.blood && !blood)
                {
                    AddBloodParticles(gib.transform);
                }
                else if (!activeSlot.blood && blood)
                {
                    Destroy(blood.gameObject);
                }
            }
        }
        gib.Spawn(gType, position, force, lifeSpan);
    }

    public Gib GetClosestGib(Vector3 position, float distance, GibType[] validTypes, bool inRoom)
    {
        var lowestSqrMagnitude = float.MaxValue;
        Gib closest = null;

        foreach (var kvp in _gibs)
        {
            if (validTypes.Contains(kvp.Key))
            {
                var gibList = kvp.Value;
                for (int i = 0; i < gibList.Count; i++)
                {
                    var gib = gibList[i];
                    if (gib.gameObject.activeInHierarchy && !gib.vorbTargeted)
                    {
                        var currentRoom = LayoutManager.CurrentRoom;
                        if(inRoom && currentRoom && !currentRoom.worldBounds.Contains(gib.transform.position))
                        {
                            continue;
                        }

                        var ePosition = gib.transform.position;
                        var magnitude = (ePosition - position).sqrMagnitude;
                        if (magnitude < lowestSqrMagnitude)
                        {
                            lowestSqrMagnitude = magnitude;
                            closest = gib;
                        }
                    }
                }

            }
        }

        if (Mathf.Sqrt(lowestSqrMagnitude) < distance)
        {
            return closest;
        }
        else
        {
            return null;
        }
    }
}
