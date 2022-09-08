using UnityEngine;
using System.Collections;
using System;
using System.Linq;
using System.Collections.Generic;

public static partial class Extensions
{
    public static bool HasFlag(this Direction keys, Direction flag)
    {
        return (keys & flag) == flag;
    }

    public static bool HasFlag(this GameMode keys, GameMode flag)
    {
        return (keys & flag) == flag;
    }

    public static bool HasFlag(this EnvironmentalEffect keys, EnvironmentalEffect flag)
    {
        return (keys & flag) == flag;
    }

    public static IEnumerable<EnvironmentalEffect> GetFlags(this EnvironmentalEffect keys)
    {
        return Enum.GetValues(typeof(EnvironmentalEffect)).Cast<EnvironmentalEffect>().Where(f => keys.HasFlag(f));
    }

    public static string ToParsedString(this BossName original)
    {
        switch (original)
        {
            case BossName.BeakLord:
                return "Beak Lord";
            case BossName.FleshAdder:
                return "Flesh Adder";
            case BossName.MegaBeastCore:
                return "Megabeast Core";
            case BossName.MetalPatriarch:
                return "Metal Patriarch";
            case BossName.MolemanShaman:
                return "Moleman Shaman";
            case BossName.MouthMeat:
                return "Mouth Meat";
            case BossName.MouthMeatSenior:
                return "Mouth Meat Senior";
            case BossName.WallCreep:
                return "Wall Creep";
            case BossName.WhiteWyrm:
                return "White Wyrm";
            case BossName.OozeHart:
                return "Ooze Hart";
            case BossName.Blightbark:
                return "Blightbark";
            case BossName.SkinDeviler:
                return "Skin Deviler";
            case BossName.CorruptedMiner:
                return "Corrupted Miner";
            default:
                return original.ToString();
        }
    }

    public static string ToParsedString(this DeathmatchMode original)
    {
        switch (original)
        {
            case DeathmatchMode.FragLimit:
                return "Frag Limit";
            case DeathmatchMode.TimeLimit:
                return "Time Limit";
            //case DeathmatchMode.LastRobotStanding:
            //    return "Last Robot Standing";
            default:
                return original.ToString();
        }
    }

    public static string ToParsedString(this DeathmatchItemMode original)
    {
        switch (original)
        {
            case DeathmatchItemMode.EnergyOnly:
                return "Energy Only";
            default:
                return original.ToString();
        }
    }

    public static void AddRange<T>(this HashSet<T> hashSet, IEnumerable<T> range)
    {
        foreach (T item in range)
        {
            hashSet.Add(item);
        }
    }

    public static void Shuffle<T>(this IList<T> list)
    {
        int n = list.Count;
        while(n > 1)
        {
            n--;
            int k = UnityEngine.Random.Range(0, n + 1);
            T value = list[k];
            list[k] = list[n];
            list[n] = value;
        }
    }

    public static void Shuffle<T>(this IList<T> list, MicrosoftRandom random)
    {
        int n = list.Count;
        while (n > 1)
        {
            n--;
            int k = random.Range(0, n + 1);
            T value = list[k];
            list[k] = list[n];
            list[n] = value;
        }
    }

    public static Int2D Int2D(this Vector2 v2)
    {
        return new Int2D((int)v2.x, (int)v2.y);
    }

    public static Vector2 GetCardinalVector (this Vector2 direction)
    {
        if(Mathf.Abs(direction.x) > Mathf.Abs(direction.y))
        {
            return new Vector2(Mathf.Sign(direction.x), 0);
        }
        else
        {
            return new Vector2(0, Mathf.Sign(direction.y));
        }
    }

    public static bool RequiresTraversalAbility(this EnvironmentalEffect effect)
    {
        foreach (var travEffect in Constants.traversalEnvEffects)
        {
            if (effect.HasFlag(travEffect)) return true;
        }

        return false;
    }

    public static Direction GetCardinalDirection(this Vector2 direction)
    {
        if (Mathf.Abs(direction.x) > Mathf.Abs(direction.y))
        {
            return Mathf.Sign(direction.x) > 0 ? Direction.Right : Direction.Left;
        }
        else
        {
            return Mathf.Sign(direction.y) > 0 ? Direction.Up : Direction.Down;
        }
    }

    public static bool isHorizontal(this Direction direction)
    {
        return direction == Direction.Left || direction == Direction.Right;
    }

    public static Direction Opposite(this Direction direction)
    {
        switch (direction)
        {
            case Direction.Up:
                return Direction.Down;
            case Direction.Right:
                return Direction.Left;
            case Direction.Down:
                return Direction.Up;
            case Direction.Left:
                return Direction.Right;
            default:
                return Direction.None;
        }
    }

    public static Int2D ToInt2D(this Direction direction)
    {
        switch (direction)
        {
            case Direction.Up:
                return new Int2D(0,1);
            case Direction.Right:
                return new Int2D(1, 0);
            case Direction.Down:
                return new Int2D(0, -1);
            case Direction.Left:
                return new Int2D(-1, 0);
            default:
                return new Int2D(0, 0);
        }
    }

    public static Vector2 ToVector2(this Direction direction)
    {
        switch (direction)
        {
            case Direction.Up:
                return Vector2.up;
            case Direction.Right:
                return Vector2.right;
            case Direction.Down:
                return Vector2.down;
            case Direction.Left:
                return Vector2.left;
            default:
                return Vector2.zero;
        }
    }

    public static Vector3 ToVector3(this Direction direction)
    {
        switch (direction)
        {
            case Direction.Up:
                return Vector3.up;
            case Direction.Right:
                return Vector3.right;
            case Direction.Down:
                return Vector3.down;
            case Direction.Left:
                return Vector3.left;
            default:
                return Vector3.zero;
        }
    }

    /// <summary>
    /// Returns the minimal element of the given sequence, based on
    /// the given projection.
    /// </summary>
    /// <remarks>
    /// If more than one element has the minimal projected value, the first
    /// one encountered will be returned. This overload uses the default comparer
    /// for the projected type. This operator uses immediate execution, but
    /// only buffers a single result (the current minimal element).
    /// </remarks>
    /// <typeparam name="TSource">Type of the source sequence</typeparam>
    /// <typeparam name="TKey">Type of the projected element</typeparam>
    /// <param name="source">Source sequence</param>
    /// <param name="selector">Selector to use to pick the results to compare</param>
    /// <returns>The minimal element, according to the projection.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="source"/> or <paramref name="selector"/> is null</exception>
    /// <exception cref="InvalidOperationException"><paramref name="source"/> is empty</exception>

    public static TSource MinBy<TSource, TKey>(this IEnumerable<TSource> source,
        Func<TSource, TKey> selector)
    {
        return source.MinBy(selector, null);
    }

    /// <summary>
    /// Returns the minimal element of the given sequence, based on
    /// the given projection and the specified comparer for projected values.
    /// </summary>
    /// <remarks>
    /// If more than one element has the minimal projected value, the first
    /// one encountered will be returned. This operator uses immediate execution, but
    /// only buffers a single result (the current minimal element).
    /// </remarks>
    /// <typeparam name="TSource">Type of the source sequence</typeparam>
    /// <typeparam name="TKey">Type of the projected element</typeparam>
    /// <param name="source">Source sequence</param>
    /// <param name="selector">Selector to use to pick the results to compare</param>
    /// <param name="comparer">Comparer to use to compare projected values</param>
    /// <returns>The minimal element, according to the projection.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="source"/>, <paramref name="selector"/> 
    /// or <paramref name="comparer"/> is null</exception>
    /// <exception cref="InvalidOperationException"><paramref name="source"/> is empty</exception>

    public static TSource MinBy<TSource, TKey>(this IEnumerable<TSource> source,
        Func<TSource, TKey> selector, IComparer<TKey> comparer)
    {
        if (source == null) throw new ArgumentNullException("source");
        if (selector == null) throw new ArgumentNullException("selector");
        comparer = comparer ?? Comparer<TKey>.Default;

        using (var sourceIterator = source.GetEnumerator())
        {
            if (!sourceIterator.MoveNext())
            {
                throw new InvalidOperationException("Sequence contains no elements");
            }
            var min = sourceIterator.Current;
            var minKey = selector(min);
            while (sourceIterator.MoveNext())
            {
                var candidate = sourceIterator.Current;
                var candidateProjected = selector(candidate);
                if (comparer.Compare(candidateProjected, minKey) < 0)
                {
                    min = candidate;
                    minKey = candidateProjected;
                }
            }
            return min;
        }
    }

    /// <summary>
    /// Returns the maximal element of the given sequence, based on
    /// the given projection.
    /// </summary>
    /// <remarks>
    /// If more than one element has the maximal projected value, the first
    /// one encountered will be returned. This overload uses the default comparer
    /// for the projected type. This operator uses immediate execution, but
    /// only buffers a single result (the current maximal element).
    /// </remarks>
    /// <typeparam name="TSource">Type of the source sequence</typeparam>
    /// <typeparam name="TKey">Type of the projected element</typeparam>
    /// <param name="source">Source sequence</param>
    /// <param name="selector">Selector to use to pick the results to compare</param>
    /// <returns>The maximal element, according to the projection.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="source"/> or <paramref name="selector"/> is null</exception>
    /// <exception cref="InvalidOperationException"><paramref name="source"/> is empty</exception>

    public static TSource MaxBy<TSource, TKey>(this IEnumerable<TSource> source,
        Func<TSource, TKey> selector)
    {
        return source.MaxBy(selector, null);
    }

    /// <summary>
    /// Returns the maximal element of the given sequence, based on
    /// the given projection and the specified comparer for projected values. 
    /// </summary>
    /// <remarks>
    /// If more than one element has the maximal projected value, the first
    /// one encountered will be returned. This operator uses immediate execution, but
    /// only buffers a single result (the current maximal element).
    /// </remarks>
    /// <typeparam name="TSource">Type of the source sequence</typeparam>
    /// <typeparam name="TKey">Type of the projected element</typeparam>
    /// <param name="source">Source sequence</param>
    /// <param name="selector">Selector to use to pick the results to compare</param>
    /// <param name="comparer">Comparer to use to compare projected values</param>
    /// <returns>The maximal element, according to the projection.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="source"/>, <paramref name="selector"/> 
    /// or <paramref name="comparer"/> is null</exception>
    /// <exception cref="InvalidOperationException"><paramref name="source"/> is empty</exception>

    public static TSource MaxBy<TSource, TKey>(this IEnumerable<TSource> source,
        Func<TSource, TKey> selector, IComparer<TKey> comparer)
    {
        if (source == null) throw new ArgumentNullException("source");
        if (selector == null) throw new ArgumentNullException("selector");
        comparer = comparer ?? Comparer<TKey>.Default;

        using (var sourceIterator = source.GetEnumerator())
        {
            if (!sourceIterator.MoveNext())
            {
                throw new InvalidOperationException("Sequence contains no elements");
            }
            var max = sourceIterator.Current;
            var maxKey = selector(max);
            while (sourceIterator.MoveNext())
            {
                var candidate = sourceIterator.Current;
                var candidateProjected = selector(candidate);
                if (comparer.Compare(candidateProjected, maxKey) > 0)
                {
                    max = candidate;
                    maxKey = candidateProjected;
                }
            }
            return max;
        }
    }

    public static bool ContainsAll<T>(this IEnumerable<T> source, IEnumerable<T> compare)
    {
        foreach (var c in compare)
        {
            if (!source.Contains(c)) return false;
        }
        return true;
    }

    public static Vector2 randomInsideBounds(Bounds bounds)
    {
        var position = bounds.center;
        position.x += UnityEngine.Random.Range(-bounds.extents.x, bounds.extents.x);
        position.y += UnityEngine.Random.Range(-bounds.extents.y, bounds.extents.y);
        return position;
    }

    public static int SignOrZero(float number)
    {
        if(number == 0)
        {
            return 0;
        }
        else
        {
            return number > 0 ? 1 : -1;
        }
    }

    public static int SignOrZero(int number)
    {
        if (number == 0)
        {
            return 0;
        }
        else
        {
            return number > 0 ? 1 : -1;
        }
    }

    //http://derekwill.com/2015/03/05/bit-processing-in-c/
    public static bool IsBitSet(this byte b, int pos)
    {
        if (pos < 0 || pos > 7)
            throw new ArgumentOutOfRangeException("pos", "Index must be in the range of 0-7.");

        return (b & (1 << pos)) != 0;
    }

    public static byte BoolSetBit(this byte b, int pos, bool value)
    {
        if (pos < 0 || pos > 7)
            throw new ArgumentOutOfRangeException("pos", "Index must be in the range of 0-7.");

        return value ? b.SetBit(pos) : b.UnsetBit(pos);
    }

    public static byte SetBit(this byte b, int pos)
    {
        if (pos < 0 || pos > 7)
            throw new ArgumentOutOfRangeException("pos", "Index must be in the range of 0-7.");

        return (byte)(b | (1 << pos));
    }

    public static byte UnsetBit(this byte b, int pos)
    {
        if (pos < 0 || pos > 7)
            throw new ArgumentOutOfRangeException("pos", "Index must be in the range of 0-7.");

        return (byte)(b & ~(1 << pos));
    }

    public static string ToBinaryString(this byte b)
    {
        return Convert.ToString(b, 2).PadLeft(8, '0');
    }

    public static bool IsEventHandleRegistered(this Action action, Delegate prospectiveHandler)
    {
        if(action != null)
        {
            return action.GetInvocationList().Contains(prospectiveHandler);
        }
        return false;
    }

    public static Color32[,] GetColor32Matrix(this Texture2D texture)
    {
        var pixels = texture.GetPixels32();
        var pixelMatrix = new Color32[texture.width, texture.height];
        var i = 0;

        for (int y = 0; y < texture.height; y++)
        {
            for (int x = 0; x < texture.width; x++)
            {
                pixelMatrix[x, y] = pixels[i];
                i++;
            }
        }

        return pixelMatrix;
    }

    public static Color[,] GetColorMatrix(this Texture2D texture)
    {
        var pixels = texture.GetPixels();
        var pixelMatrix = new Color[texture.width, texture.height];
        var i = 0;

        for (int y = 0; y < texture.height; y++)
        {
            for (int x = 0; x < texture.width; x++)
            {
                pixelMatrix[x, y] = pixels[i];
                i++;
            }
        }

        return pixelMatrix;
    }
}
