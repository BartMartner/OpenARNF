using UnityEngine;
using System.Collections;
using System;
using System.Collections.Generic;
using Newtonsoft.Json;

[Serializable]
public struct Int2DDirection
{
    [JsonProperty(PropertyName = "pos")]
    public Int2D position;
    [JsonProperty(PropertyName = "dir")]
    public Direction direction;

    public override int GetHashCode()
    {
        return position.GetHashCode() ^ direction.GetHashCode();
    }

    public override bool Equals(object obj)
    {
        if (obj == null || !(obj is Int2DDirection))
        {
            return false;
        }

        var p = (Int2DDirection)obj;
        return p.position == position && p.direction == direction;
    }

    public static bool operator ==(Int2DDirection one, Int2DDirection two)
    {
        return one.position == two.position && one.direction == two.direction;
    }

    public static bool operator !=(Int2DDirection one, Int2DDirection two)
    {
        return one.position != two.position || one.direction != two.direction;
    }
}

[Serializable]
public struct Rect2D
{
    public int yMin;
    public int yMax;
    public int xMin;
    public int xMax;    
}

[Serializable]
public struct Buffer2D
{
    public static Buffer2D zero = new Buffer2D(0, 0, 0, 0);
    public static Buffer2D one = new Buffer2D(1, 1, 1, 1);

    public float top;
    public float bottom;
    public float left;
    public float right;

    public Buffer2D(float top, float bottom, float left, float right)
    {
        this.top = top;
        this.bottom = bottom;
        this.left = left;
        this.right = right;
    }
}

[Serializable]
public struct Int2D
{
    public static Int2D zero = new Int2D(0, 0);
    public static Int2D negOne = new Int2D(-1, -1);
    public static Int2D up = new Int2D(0, 1);
    public static Int2D down = new Int2D(0, -1);
    public static Int2D left = new Int2D(-1, 0);
    public static Int2D right = new Int2D(1, 0);

    public int x;
    public int y;

    public Int2D(int x, int y)
    {
        this.x = x;
        this.y = y;
    }

    public Int2D(uint x, uint y)
    {
        this.x = (int)x;
        this.y = (int)y;
    }

    public Vector2 Vector2()
    {
        return new Vector2(x, y);
    }

    public override string ToString()
    {
        return "{" + x + "," + y + "}";
    }

    public override int GetHashCode()
    {
        return x.GetHashCode() ^ y.GetHashCode();
    }

    public override bool Equals(object obj)
    {
        if (obj == null || !(obj is Int2D))
        {
            return false;
        }

        var p = (Int2D)obj;
        return p.x == x && p.y == y;
    }

    public static Int2D GetRoomPosFromRect(Rect rect)
    {
        return new Int2D((int)rect.xMin, (int)rect.yMax - 1);
    }

    public static float Distance(Int2D a, Int2D b)
    {
        return Vector3.Distance(a.Vector2(), b.Vector2());
    }

    public static int ManhattanDistance(Int2D a, Int2D b)
    {
        return Mathf.Abs(a.x - b.x) + Mathf.Abs(a.y - b.y);
    }

    public static Int2D Lerp(Int2D a, Int2D b, float time)
    {
        int x = (int)((1 - time) * a.x + time * b.x);
        int y = (int)((1 - time) * a.y + time * b.y);
        return new Int2D(x, y);
    }

    public static Vector2 Direction(Int2D from, Int2D to)
    {
        return new Vector2(to.x - from.x, to.y - from.y).normalized;
    }

    public static Int2D operator +(Int2D a, Int2D b)
    {
        return new Int2D(a.x + b.x, a.y + b.y);
    }

    public static Int2D operator -(Int2D a, Int2D b)
    {
        return new Int2D(a.x - b.x, a.y - b.y);
    }

    public static bool operator ==(Int2D one, Int2D two)
    {
        return one.x == two.x && one.y == two.y;
    }

    public static bool operator ==(Vector2 v2, Int2D i2)
    {
        return v2.x == i2.x && v2.y == i2.y;
    }

    public static bool operator !=(Int2D one, Int2D two)
    {
        return one.x != two.x || one.y != two.y;
    }

    public static bool operator !=(Vector2 v2, Int2D i2)
    {
        return v2.x != i2.x || v2.y != i2.y;
    }
}

[Serializable]
public struct Vector3Data
{
    public float x;
    public float y;
    public float z;

    public Vector3Data(float x, float y, float z)
    {
        this.x = x;
        this.y = y;
        this.z = z;
    }

    public override bool Equals(object obj)
    {
        return obj is Vector3Data && this == (Vector3Data)obj;
    }

    public override int GetHashCode()
    {
        return x.GetHashCode() + y.GetHashCode() + z.GetHashCode();
    }

    public static bool operator ==(Vector3Data a, Vector3Data b)
    {
        return a.x == b.x && a.y == b.y && a.z == b.z;
    }

    public static bool operator !=(Vector3Data a, Vector3Data b)
    {
        return !(a == b);
    }
}

[Serializable]
public class MinorItemData
{
    public MinorItemData(int globalID)
    {
        this.globalID = globalID;
    }

    [JsonProperty(PropertyName = "id")]
    public int globalID;
    [JsonProperty(PropertyName = "spnNfo")]
    public MinorItemSpawnInfo spawnInfo = new MinorItemSpawnInfo();
    public MinorItemType type;
    [JsonProperty(PropertyName = "rrls")]
    public int rerolls;
}

[Serializable]
public class TraversalPathPoint
{
    [JsonProperty(PropertyName = "minPos")]
    public Int2D minGridPosition;
    [JsonProperty(PropertyName = "maxPos")]
    public Int2D maxGridPosition;

    public bool ContainsLocalGridPosition(Int2D localGridPosition)
    {
        return localGridPosition.x >= minGridPosition.x && localGridPosition.x <= maxGridPosition.x &&
            localGridPosition.y >= minGridPosition.y && localGridPosition.y <= maxGridPosition.y;
    }

    /// <summary>
    /// For drawing things in DebugRoom
    /// </summary>
    /// <returns></returns>
    public Vector2 GetMidPoint()
    {
        var delta = maxGridPosition - minGridPosition;
        return new Vector2(0.5f + minGridPosition.x + delta.x * 0.5f, 0.5f + minGridPosition.y + delta.y * 0.5f);
    }

    public List<Int2D> AllGridPositions()
    {
        if(minGridPosition == maxGridPosition)
        {
            return new List<Int2D> { minGridPosition };
        }

        var gridPositions = new List<Int2D>();

        for (int x = minGridPosition.x; x <= maxGridPosition.x; x++)
        {
            for (int y = minGridPosition.y; y <= maxGridPosition.y; y++)
            {
                gridPositions.Add(new Int2D(x, y));
            }
        }

        return gridPositions;
    }
}

public struct DummyRoomInfo
{
    public EnvironmentType environmentType;
    public Int2D size;
    public int amount;

    public DummyRoomInfo(EnvironmentType environmentType, Int2D size, int amount)
    {
        this.environmentType = environmentType;
        this.size = size;
        this.amount = amount;
    }
}

public struct ItemPrice
{
    public int grayScrapCost;
    public int redScrapCost;
    public int greenScrapCost;
    public int blueScrapCost;

    public static ItemPrice zero = new ItemPrice(0, 0, 0, 0);

    public static bool operator ==(ItemPrice price1, ItemPrice price2)
    {
        return price1.grayScrapCost == price2.grayScrapCost &&
            price1.redScrapCost == price2.redScrapCost &&
            price1.greenScrapCost == price2.greenScrapCost &&
            price1.blueScrapCost == price2.blueScrapCost;
    }

    public static bool operator !=(ItemPrice price1, ItemPrice price2)
    {
        return price1.grayScrapCost != price2.grayScrapCost ||
            price1.redScrapCost != price2.redScrapCost ||
            price1.greenScrapCost != price2.greenScrapCost ||
            price1.blueScrapCost != price2.blueScrapCost;
    }

    public override bool Equals(object obj)
    {
        if (obj == null || GetType() != obj.GetType())
        {
            return false;
        }

        var price2 = (ItemPrice)obj;
        return grayScrapCost == price2.grayScrapCost &&
        redScrapCost == price2.redScrapCost &&
        greenScrapCost == price2.greenScrapCost &&
        blueScrapCost == price2.blueScrapCost;
    }

    public override int GetHashCode() { return base.GetHashCode(); }

    public int this[CurrencyType index]
    {
        get
        {
            switch (index)
            {
                case CurrencyType.Red:
                    return redScrapCost;
                case CurrencyType.Green:
                    return greenScrapCost;
                case CurrencyType.Blue:
                    return blueScrapCost;
                case CurrencyType.Gray:
                default:
                    return grayScrapCost;
            }
        }
        set
        {
            switch (index)
            {
                case CurrencyType.Red:
                    redScrapCost = value;
                    break;
                case CurrencyType.Green:
                    greenScrapCost = value;
                    break;
                case CurrencyType.Blue:
                    blueScrapCost = value;
                    break;
                case CurrencyType.Gray:
                default:
                    grayScrapCost = value;
                    break;
            }
        }
    }

    public ItemPrice(int gray, int red, int green, int blue)
    {
        grayScrapCost = gray;
        redScrapCost = red;
        greenScrapCost = green;
        blueScrapCost = blue;
    }

    public ItemPrice(ItemInfo itemInfo, ShopInfo shopInfo, bool halfOff)
    {
        redScrapCost = Mathf.CeilToInt(itemInfo.redScrapCost * shopInfo.redPriceMod);
        greenScrapCost = halfOff && (redScrapCost > 0) ? 0 : Mathf.CeilToInt(itemInfo.greenScrapCost * shopInfo.bluePriceMod);
        blueScrapCost = halfOff && (redScrapCost > 0 || greenScrapCost > 0) ? 0 : Mathf.CeilToInt(itemInfo.blueScrapCost * shopInfo.greenPriceMod);
        grayScrapCost = Mathf.CeilToInt(itemInfo.grayScrapCost * shopInfo.grayPriceMod * (halfOff ? 0.5f : 1));
    }

    public bool CanAfford(Player player)
    {
        if (!player) return false;

        return player.grayScrap >= grayScrapCost && player.redScrap >= redScrapCost &&
            player.greenScrap >= greenScrapCost && player.blueScrap >= blueScrapCost;
    }

    public void chargePlayer(Player player)
    {
        player.grayScrap -= grayScrapCost;
        player.redScrap -= redScrapCost;
        player.greenScrap -= greenScrapCost;
        player.blueScrap -= blueScrapCost;
    }

    public List<ItemCost> GetCost()
    {
        var costs = new List<ItemCost>();

        if (grayScrapCost > 0)
        {
            costs.Add(new ItemCost { type = CurrencyType.Gray, amount = grayScrapCost });
        }

        if (redScrapCost > 0)
        {
            costs.Add(new ItemCost { type = CurrencyType.Red, amount = redScrapCost });
        }

        if (greenScrapCost > 0)
        {
            costs.Add(new ItemCost { type = CurrencyType.Green, amount = greenScrapCost });
        }

        if (blueScrapCost > 0)
        {
            costs.Add(new ItemCost { type = CurrencyType.Blue, amount = blueScrapCost });
        }

        return costs;
    }
}



