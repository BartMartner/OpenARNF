using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class CityBackgroundGenerator : MonoBehaviour
{
    public int layers = 3;
    [Range(0, 1)]
    public float minParallax = 0.2f;
    [Range(0, 1)]
    public float maxParallax = 0.8f;

    [Header("Buildings")]
    public SpriteRenderer[] buildingPrefabs;
    public Material buildingMaterial;
    public Color[] hazeColors;
    public float maxYOffset = 1;
    public float minBuildingSpacing = 0;
    public float maxBuildingSpacing = 2;
    public bool allowFlipping = true;
    public float debugCorruption;

    [Header("Lasers")]
    public Sprite[] laserSprites;
    public Color[] laserColors;
    public int minLasersPerLayer = 0;
    public int maxLasersPerLayer = 3;
    public float minLaserSpeed = 5;
    public float maxLaserSpeed = 30;


    public void GenerateBackground()
    {
        var room = GetComponentInParent<Room>();

        if (!room)
        {
            Debug.LogError("SurfaceCityBackgroundGenerator must be the child of a gameObject with the Room script attached");
            return;
        }

        List<GameObject> children = new List<GameObject>();
        foreach (Transform child in transform)
        {
            children.Add(child.gameObject);
        }

        foreach (var child in children)
        {
            DestroyImmediate(child);
        }

        var minX = -Constants.roomWidth * 0.5f;
        var maxX = minX + room.roomInfo.size.x * Constants.roomWidth;

        for (int i = 0; i < layers; i++)
        {
            var layer = new GameObject();
            layer.name = "Layer " + i;
            layer.transform.parent = transform;
            layer.transform.localPosition = new Vector3(0, (room.roomInfo.size.y - 1) * -Constants.roomHeight, 0); //this makes parallax work right.

            var depth = layers > 1 ? Mathf.Lerp(minParallax, maxParallax, i / (layers - 1f)) : minParallax;

            var parallax = layer.AddComponent<Parallax>();
            parallax.amount = depth;

            var sortingOrder = -5 + i * -5;

            var buildings = new GameObject();
            buildings.name = "Buildings";
            buildings.transform.parent = layer.transform;
            buildings.transform.localPosition = new Vector3(0, -Constants.roomHeight * 0.5f, 0);
            if (hazeColors.Length > 0)
            {
                var corruptHaze = buildings.AddComponent<SetColorBasedOnCorruption>();
                corruptHaze.colors = hazeColors;
                corruptHaze.target = "_FlashColor";
                corruptHaze.setSpriteRendererColor = false;
                corruptHaze.debugCorruption = debugCorruption;
            }

            var layerMaterial = new Material(buildingMaterial);
            layerMaterial.SetColor("_FlashColor", hazeColors[0]);
            layerMaterial.SetFloat("_FlashAmount", depth);

            var lastX = minX;

            var buildingCount = 1;
            var pMaxX = maxX * (1 - depth);
            while(lastX < pMaxX)
            {
                var prefab = buildingPrefabs[Random.Range(0, buildingPrefabs.Length)];
                lastX += Random.Range(minBuildingSpacing, maxBuildingSpacing) + prefab.bounds.extents.x;
                var localPosition = new Vector3(lastX, i * 0.1f + Random.Range(maxYOffset, 0));
                var building = Instantiate<SpriteRenderer>(prefab);
                var corruption = building.GetComponent<SetSpriteBasedOnCorruption>();
                if (corruption)
                {
                    corruption.minCheck = Random.Range(0.1f, 0.9f);
                    corruption.maxCheck = corruption.minCheck + 0.1f;
                    corruption.debugCorruption = debugCorruption;
                }
                building.transform.SetParent(buildings.transform);
                building.transform.localPosition = localPosition;
                building.sortingOrder = sortingOrder;
                if (allowFlipping)
                {
                    building.flipX = Random.value > 0.5f;
                }
                building.material = layerMaterial;
                lastX += prefab.bounds.extents.x;
                buildingCount++;
            }

            var laserCount = Random.Range(minLasersPerLayer, maxLasersPerLayer);

            if (laserCount > 0)
            {
                var lasers = new GameObject();
                lasers.name = "Lasers";
                lasers.transform.parent = layer.transform;
                lasers.transform.localPosition = new Vector3(0, -Constants.roomHeight * 0.5f, 0);

                var laserHeight = room.roomInfo.size.y * Constants.roomHeight;

                for (int j = 0; j < laserCount; j++)
                {
                    var t = 1f/(laserCount+1) * (j+1);
                    //for t(lasertCount, j) 
                    //t(1, 0) = 0.5f
                    //t(2, 0) = 0.33f and t(2, 1) = 0.66f
                    //t(3, 0) = 0.25f and t(3, 1) = 0.5f, t(3, 2) = 0.75

                    var localPosition = new Vector3(Mathf.Lerp(minX + Random.Range(minX, -minX), maxX + Random.Range(minX, -minX), t), 0, 0);

                    if (localPosition.x < minX)
                    {
                        continue;
                    }

                    var laserPoint = new GameObject();
                    laserPoint.name = "LaserPoint";
                    laserPoint.transform.parent = lasers.transform;
                    laserPoint.transform.localPosition = localPosition;
                    laserPoint.transform.localRotation = Quaternion.Euler(0, 0, Random.Range(-15, 15));
                    var lazerPointRotate = laserPoint.AddComponent<PingPongRotate>();
                    lazerPointRotate.speed = Random.Range(minLaserSpeed, maxLaserSpeed);
                    lazerPointRotate.minAngle = -15;
                    lazerPointRotate.maxAngle = 15;
                    var laserBeam = new GameObject().AddComponent<SpriteRenderer>();
                    laserBeam.name = "LaserBeam";
                    laserBeam.sprite = laserSprites[Random.Range(0, laserSprites.Length)];
                    laserBeam.transform.parent = laserPoint.transform;
                    laserBeam.transform.localScale = new Vector3((1-depth), laserHeight, 1);
                    laserBeam.transform.localPosition = new Vector3(0, laserHeight / 2, 0);
                    laserBeam.transform.localRotation = Quaternion.identity;
                    laserBeam.sortingLayerName = "Background1";
                    laserBeam.sortingOrder = sortingOrder - 1;
                    
                    var color = laserColors[Random.Range(0, laserColors.Length)];
                    color.a = (1-depth) * 0.75f;
                    laserBeam.color = color;
                }
            }
        }
    }
}

