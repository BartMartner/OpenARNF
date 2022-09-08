using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using System;
using System.Linq;
using System.Text.RegularExpressions;

public class DebugRoom : MonoBehaviour
{
    public const int roomUnit = 64;

    public Sprite whiteRect;
    public Sprite whiteCircle;
    public Sprite startIcon;
    public Sprite branchIcon;
    public Sprite verticalJumpExit;
    public Sprite hortizontalJumpExit;
    public Sprite verticalGroundedSmallGapExit;
    public Sprite hortizontalGroundedSmallGapExit;
    public Sprite verticalIgnoreTerrainlExit;
    public Sprite hortizontalIgnoreTerrainExit;
    public Sprite verticalPhaseWallExit;
    public Sprite hortizontalPhaseWallExit;

    public GameObject heatEffect;
    public GameObject darknessEffect;
    public GameObject confusionEffect;
    public GameObject waterEffect;
    public GameObject boss;
    public GameObject save;
    public GameObject teleporter;
    public GameObject megaBeast;
    public GameObject orbSmith;
    public GameObject gunSmith;
    public GameObject artificer;
    public GameObject theTraitor;
    public GameObject bulucChabtan;
    public GameObject hephaestus;
    public GameObject orphiel;
    public GameObject tyr;
    public GameObject wadjetMikail;
    public GameObject zurvan;
    public Image highlight;
    
    public RoomAbstract roomAbstract;
    public Image background;
    public Image specialExpected;
    public Image icon;
    public Image[] walls;
    public RectTransform rectTransform;
    public bool match;
    public Text expCap;

    public List<Image> minorItems = new List<Image>();
    private List<Image> _exits = new List<Image>();
    private List<DebugTraversalPath> _traversalPaths = new List<DebugTraversalPath>();

    public void Awake()
    {
        if(!background)
        {
            background = GetComponent<Image>();
        }

        rectTransform = background.rectTransform;
    }

    public void MatchAbstract(RoomAbstract roomAbstract, Vector2 offset)
    {
        this.roomAbstract = roomAbstract;

        expCap.text = "ExpCap: " + roomAbstract.expectedCapabilitiesIndex.ToString();

        icon.rectTransform.sizeDelta = new Vector2(roomUnit / 2, roomUnit / 2);

        var newSize = new Vector2(roomAbstract.width, roomAbstract.height) * roomUnit;
        if (rectTransform.sizeDelta != newSize)
        {
            rectTransform.sizeDelta = newSize;
        }

        Color color = Color.white;
        var newPosition = (roomAbstract.gridPosition.Vector2() - offset) * roomUnit;
        if (rectTransform.anchoredPosition != newPosition)
        {
            rectTransform.anchoredPosition = newPosition;
        }

        if (roomAbstract.isStartingRoom) //starting room
        {
            gameObject.name = "Start";
            icon.enabled = true;
            icon.sprite = startIcon;
            icon.color = Color.white;
        }
        else if (roomAbstract.majorItem == 0) //normal room
        {
            icon.enabled = false;
            color = Color.white;
            //color = GetPathColor(roomAbstract.GetPathItems());

            string roomInfoName = "Unassigned Room";
            if(roomAbstract.assignedRoomInfo != null && !string.IsNullOrEmpty(roomAbstract.assignedRoomInfo.sceneName))
            {
                roomInfoName = roomAbstract.assignedRoomInfo.sceneName;
            }            
            gameObject.name = roomInfoName + " " + roomAbstract.gridPosition;

            if (roomAbstract.branchRoom) //start of a spoke path
            {
                icon.enabled = true;
                icon.sprite = branchIcon;
                icon.color = roomAbstract.isEnvironmentStart ? Color.green : Color.white;
            }
        }
        else //has major item
        {
            icon.enabled = true;

            var itemIcon = Resources.Load<Sprite>("Sprites/Items/" + roomAbstract.majorItem.ToString());

            if (itemIcon)
            {
                icon.sprite = itemIcon;
                icon.color = Color.white;
            }
            else
            {
                icon.sprite = whiteCircle;

                Color32 dColor;
                var damageType = Constants.DamageTypeFromItem(roomAbstract.majorItem);
                if (Constants.damageTypeColors.TryGetValue(damageType, out dColor))
                {
                    icon.color = dColor;
                }
            }

            gameObject.name = roomAbstract.majorItem.ToString();
        }

        if (roomAbstract.assignedRoomInfo == null || string.IsNullOrEmpty(roomAbstract.assignedRoomInfo.sceneName))
        {
            color.a = 0.33f;
        }

        foreach (var wall in walls)
        {
            if (roomAbstract.preBossRoom != BossName.None)
            {
                wall.color = new Color(1,0.5f,0.5f);
            }
            else
            {
                switch (roomAbstract.altPalette)
                {
                    case 1:
                        wall.color = roomAbstract.useAltTileset ? Color.cyan : Color.blue;
                        break;
                    case 2:
                        wall.color = roomAbstract.useAltTileset ? Color.yellow : Color.green;
                        break;
                    case 3:
                        wall.color = roomAbstract.useAltTileset ? Color.magenta : Color.red;
                        break;
                    default:
                        wall.color = roomAbstract.useAltTileset ? Color.gray : Color.white;
                        break;
                }
            }
        }

        heatEffect.SetActive(roomAbstract.environmentalEffect.HasFlag(EnvironmentalEffect.Heat));
        darknessEffect.SetActive(roomAbstract.environmentalEffect.HasFlag(EnvironmentalEffect.Darkness));
        confusionEffect.SetActive(roomAbstract.environmentalEffect.HasFlag(EnvironmentalEffect.Confusion));
        waterEffect.SetActive(roomAbstract.environmentalEffect.HasFlag(EnvironmentalEffect.Underwater));
        boss.SetActive(roomAbstract.assignedRoomInfo.boss != 0);
        save.SetActive(roomAbstract.assignedRoomInfo.roomType == RoomType.SaveRoom);
        teleporter.SetActive(roomAbstract.assignedRoomInfo.roomType == RoomType.Teleporter);
        megaBeast.SetActive(roomAbstract.assignedRoomInfo.roomType == RoomType.MegaBeast);
        orbSmith.SetActive(roomAbstract.assignedRoomInfo.shopType == ShopType.OrbSmith);
        gunSmith.SetActive(roomAbstract.assignedRoomInfo.shopType == ShopType.GunSmith);
        artificer.SetActive(roomAbstract.assignedRoomInfo.shopType == ShopType.Artificer);
        theTraitor.SetActive(roomAbstract.assignedRoomInfo.shopType == ShopType.TheTraitor);
        bulucChabtan.SetActive(roomAbstract.assignedRoomInfo.shrineType == ShrineType.BulucChabtan);
        hephaestus.SetActive(roomAbstract.assignedRoomInfo.shrineType == ShrineType.Hephaestus);
        orphiel.SetActive(roomAbstract.assignedRoomInfo.shrineType == ShrineType.Orphiel);
        tyr.SetActive(roomAbstract.assignedRoomInfo.shrineType == ShrineType.Tyr);
        wadjetMikail.SetActive(roomAbstract.assignedRoomInfo.shrineType == ShrineType.WadjetMikail);
        zurvan.SetActive(roomAbstract.assignedRoomInfo.shrineType == ShrineType.Zurvan);

        background.color = color;
        if (roomAbstract.assignedRoomInfo != null)
        {
            switch (roomAbstract.assignedRoomInfo.environmentType)
            {
                case EnvironmentType.Surface:
                    background.sprite = LayoutDebug.instance.surfaceRoom;
                    break;
                case EnvironmentType.Cave:
                    background.sprite = LayoutDebug.instance.caveRoom;
                    break;
                case EnvironmentType.CoolantSewers:
                    background.sprite = LayoutDebug.instance.coolantSewersRoom;
                    break;
                case EnvironmentType.Factory:
                    background.sprite = LayoutDebug.instance.factoryRoom;
                    break;
                case EnvironmentType.CrystalMines:
                    background.sprite = LayoutDebug.instance.crystalMinesRoom;
                    break;
                case EnvironmentType.BuriedCity:
                    background.sprite = LayoutDebug.instance.buriedCityRoom;
                    break;
                case EnvironmentType.BeastGuts:
                    background.sprite = LayoutDebug.instance.beastGutsRoom;
                    break;
                case EnvironmentType.Glitch:
                    background.sprite = LayoutDebug.instance.glitchRoom;
                    break;
                case EnvironmentType.ForestSlums:
                    background.sprite = LayoutDebug.instance.forestSlumsRoom;
                    break;
                default:
                    background.sprite = LayoutDebug.instance.blank;
                    break;
            }
        }

        while(_exits.Count < roomAbstract.exits.Count)
        {
            CreateExit();
        }

        for (int i = 0; i < _exits.Count; i++)
        {
            var exit = _exits[i];
            if(i < roomAbstract.exits.Count)
            {
                exit.gameObject.SetActive(true);
                MatchExit(exit, roomAbstract.exits[i]);
            }
            else
            {
                exit.gameObject.SetActive(false);
            }
        }

        while(_traversalPaths.Count < roomAbstract.assignedRoomInfo.traversalPaths.Count)
        {
            CreateTraversalPath();
        }

        for (int i = 0; i < _traversalPaths.Count; i++)
        {
            var tp = _traversalPaths[i];
            if (i < roomAbstract.assignedRoomInfo.traversalPaths.Count && i < roomAbstract.traversalPathRequirements.Count)
            {
                tp.gameObject.SetActive(true);
                tp.name = "Traversal Path " + i;
                MatchTraversalPath(tp, roomAbstract.assignedRoomInfo.traversalPaths[i], roomAbstract.traversalPathRequirements[i]);
            }
            else
            {
                tp.gameObject.SetActive(false);
            }
        }

        for (int i = 0; i < minorItems.Count; i++)
        {
            if (i < roomAbstract.minorItems.Count)
            {
                minorItems[i].gameObject.SetActive(true);
                string path = "Sprites/Pickups/" + roomAbstract.minorItems[i].type.ToString();
                var sprite = Resources.Load<Sprite>(path);
                if(!sprite)
                {
                    sprite = Resources.Load<Sprite>(path + "_0");
                }

                minorItems[i].sprite = sprite;

                //switch (roomAbstract.minorItems[i].type)
                //{
                //    case MinorItemType.DamageModule:
                //        minorItems[i].color = Constants.damageTypeColors[DamageType.Explosive];
                //        break;
                //    case MinorItemType.EnergyModule:
                //        minorItems[i].color = Constants.damageTypeColors[DamageType.Electric];
                //        break;
                //    case MinorItemType.HealthTank:
                //        minorItems[i].color = Constants.blasterGreen;
                //        break;
                //}
            }
            else
            {
                minorItems[i].gameObject.SetActive(false);
            }
        }
    }

    public Image CreateExit()
    {
        var exit = new GameObject().AddComponent<Image>();
        exit.transform.SetParent(transform);
        exit.transform.localScale = Vector3.one;
        _exits.Add(exit);
        return exit;
    }

    public DebugTraversalPath CreateTraversalPath()
    {
        var traversalPath = new GameObject().AddComponent<DebugTraversalPath>();
        traversalPath.transform.SetParent(transform);
        traversalPath.transform.localScale = Vector3.one;
        traversalPath.transform.localPosition = Vector3.zero;
        traversalPath.rectTransform = traversalPath.GetComponent<RectTransform>();
        _traversalPaths.Add(traversalPath);
        return traversalPath;
    }

    public void MatchTraversalPath(DebugTraversalPath debugTraversalPath, TraversalPath traversalPath, TraversalRequirements requirements)
    {
        debugTraversalPath.rectTransform.anchorMin = Vector2.zero;
        debugTraversalPath.rectTransform.anchorMax = Vector2.one;
        debugTraversalPath.rectTransform.offsetMax = Vector2.zero;
        debugTraversalPath.rectTransform.offsetMin = Vector2.zero;

        debugTraversalPath.toRect.anchorMin = debugTraversalPath.toRect.anchorMax = new Vector2(0, 0);
        debugTraversalPath.toRect.anchoredPosition = traversalPath.to.GetMidPoint() * roomUnit;
        debugTraversalPath.to.sprite = whiteCircle;
        debugTraversalPath.to.color = traversalPath.reciprocal ? Color.yellow : Color.red;
        debugTraversalPath.toRect.sizeDelta = new Vector2(12, 12);

        debugTraversalPath.fromRect.anchorMin = debugTraversalPath.fromRect.anchorMax = new Vector2(0, 0);
        debugTraversalPath.fromRect.anchoredPosition = traversalPath.from.GetMidPoint() * roomUnit;
        debugTraversalPath.from.sprite = whiteCircle;
        debugTraversalPath.from.color = traversalPath.reciprocal ? Color.yellow : Color.green;
        debugTraversalPath.fromRect.sizeDelta = new Vector2(12, 12);

        var to = debugTraversalPath.toRect.localPosition;
        var from = debugTraversalPath.fromRect.localPosition;
        var midPoint = Vector2.Lerp(from, to, 0.5f);
        debugTraversalPath.limitations.transform.localPosition = midPoint;
        debugTraversalPath.limitations.sprite = whiteRect;
        debugTraversalPath.limitationsRect.sizeDelta = new Vector2(16, 8);

        if (requirements.minEffectiveJumpHeight > Constants.startingMaxJumpHeight)
        {
            debugTraversalPath.limitations.sprite = verticalJumpExit;
        }
        else if (requirements.requiresGroundedSmallGaps)
        {
            debugTraversalPath.limitations.sprite = verticalGroundedSmallGapExit;
        }
        else if(requirements.requiresShotIgnoresTerrain)
        {
            debugTraversalPath.limitations.sprite = verticalIgnoreTerrainlExit;            
        }
        else
        {
            debugTraversalPath.limitations.sprite = whiteRect;
        }

        Color32 limitationColor;
        if (Constants.damageTypeColors.TryGetValue(requirements.requiredDamageType, out limitationColor))
        {
            debugTraversalPath.limitations.color = limitationColor;
        }
        else if (requirements.requiresPhaseProof)
        {
            debugTraversalPath.limitations.color = Color.gray;
        }
        else
        {
            debugTraversalPath.limitations.color = Color.white;
        }

        debugTraversalPath.connectorRect.sizeDelta = new Vector2(2, Vector2.Distance(from, to));
        debugTraversalPath.connectorRect.localPosition = midPoint;
        debugTraversalPath.connectorRect.localRotation = Quaternion.FromToRotation(Vector2.up, (to - from).normalized);
    }

    public void MatchExit(Image exit, ExitAbstract exitAbstract)
    {
        exit.gameObject.name = exitAbstract.direction.ToString() + exitAbstract.localGridPosition.ToString();
        var wideAxis = roomUnit / 4f;
        var narrowAxis = roomUnit / 8f;
        var inset = new Vector2();
        inset.x = exitAbstract.localGridPosition.x * roomUnit;
        inset.y = exitAbstract.localGridPosition.y * roomUnit;
        bool vertical = false;
        switch (exitAbstract.direction)
        {
            case Direction.Up:
                vertical = true;
                inset.x += (roomUnit - wideAxis) * 0.5f;
                inset.y += roomUnit - narrowAxis;
                exit.rectTransform.SetInsetAndSizeFromParentEdge(RectTransform.Edge.Bottom, inset.y, narrowAxis);
                exit.rectTransform.SetInsetAndSizeFromParentEdge(RectTransform.Edge.Left, inset.x, wideAxis);
                break;
            case Direction.Down:
                vertical = true;
                inset.x += (roomUnit - wideAxis) * 0.5f;
                exit.rectTransform.SetInsetAndSizeFromParentEdge(RectTransform.Edge.Bottom, inset.y, narrowAxis);
                exit.rectTransform.SetInsetAndSizeFromParentEdge(RectTransform.Edge.Left, inset.x, wideAxis);
                break;
            case Direction.Left:
                inset.y += (roomUnit - wideAxis) * 0.5f;
                exit.rectTransform.SetInsetAndSizeFromParentEdge(RectTransform.Edge.Bottom, inset.y, wideAxis);
                exit.rectTransform.SetInsetAndSizeFromParentEdge(RectTransform.Edge.Left, inset.x, narrowAxis);
                break;
            case Direction.Right:
                inset.y += (roomUnit - wideAxis) * 0.5f;
                inset.x += roomUnit - narrowAxis;
                exit.rectTransform.SetInsetAndSizeFromParentEdge(RectTransform.Edge.Bottom, inset.y, wideAxis);
                exit.rectTransform.SetInsetAndSizeFromParentEdge(RectTransform.Edge.Left, inset.x, narrowAxis);
                break;
        }

        Color32 color = Color.white;
        if (exitAbstract.toExit.minEffectiveJumpHeight > Constants.startingMaxJumpHeight)
        {
            exit.sprite = vertical ? verticalJumpExit : hortizontalJumpExit;
        }
        else if (exitAbstract.toExit.requiresGroundedSmallGaps)
        {
            exit.sprite = vertical ? verticalGroundedSmallGapExit : hortizontalGroundedSmallGapExit;
            if (exitAbstract.toExit.requiresPhaseProof) { exit.color = Color.gray; }
        }
        else if (exitAbstract.toExit.requiresShotIgnoresTerrain)
        {
            exit.sprite = vertical ? verticalIgnoreTerrainlExit : hortizontalIgnoreTerrainExit;
        }
        else if (exitAbstract.toExit.requiresPhaseThroughWalls)
        {
            exit.sprite = vertical ? verticalPhaseWallExit : hortizontalPhaseWallExit;
        }
        else
        {
            exit.sprite = whiteRect;
        }

        if (Constants.damageTypeColors.TryGetValue(exitAbstract.toExit.requiredDamageType, out color))
        {
            exit.color = color;
        }
        else if (exitAbstract.toExit.requiresPhaseProof)
        {
            exit.color = Color.gray;
        }
        else
        {
            exit.color = Color.white;
        }
    }
}

public class DebugTraversalPath : MonoBehaviour
{
    public RectTransform rectTransform;
    public Image to;
    public RectTransform toRect;        
    public Image from;
    public RectTransform fromRect;
    public Image connector;
    public RectTransform connectorRect;
    public Image limitations;
    public RectTransform limitationsRect;

    public void Awake()
    {
        rectTransform = gameObject.AddComponent<RectTransform>();
        to = new GameObject().AddComponent<Image>();
        to.name = "to";
        to.transform.SetParent(transform);
        toRect = to.GetComponent<RectTransform>();
        from = new GameObject().AddComponent<Image>();
        from.name = "from";
        from.transform.SetParent(transform);
        fromRect = from.GetComponent<RectTransform>();
        connector = new GameObject().AddComponent<Image>();
        connector.name = "connector";
        connector.transform.SetParent(transform);
        connectorRect = connector.GetComponent<RectTransform>();
        limitations = new GameObject().AddComponent<Image>();
        limitations.name = "limitations";
        limitations.transform.SetParent(transform);
        limitationsRect = limitations.GetComponent<RectTransform>();
    }
}
