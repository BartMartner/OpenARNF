using UnityEngine;
using System.Collections;

public class MicrosoftRandom : System.Random
{
    public MicrosoftRandom() { }
    public MicrosoftRandom(int seed) : base(seed) { }

    public float value
    {
        get
        {
            return (float)Sample();
        }
    }

    /// <param name="min">inclusive</param>
    /// <param name="max">exclusive</param>
    /// <returns>a random int between min and max-1</returns>
    public int Range(int min, int max)
    {
        return Next(min, max);
    }

    public int ZeroToMaxInt()
    {
        return Next(0, int.MaxValue);
    }

    public Quaternion ZAngle(float min, float max)
    {
        return Quaternion.Euler(0, 0, min + value * (max - min));
    }

    public float Range(float min, float max)
    {
        return min + value * (max - min);
    }
}


public abstract class RandomNumberGenerator
{
    public abstract uint Next();

    public float Value()
    {
        return Next() / (float)uint.MaxValue;
    }

    /// <param name="min">inclusive</param>
    /// <param name="max">exclusive</param>
    /// <returns>a random int between min and max-1</returns>
    public int Range(int min, int max)
    {
        return min + (int)(Next() % (max - min));
    }

    /// <param name="min">inclusive</param>
    /// <param name="max">inclusive</param>
    /// <returns>a random float between min and max</returns>
    public float Range(float min, float max)
    {
        return min + (Next() * (float)(max - min)) / uint.MaxValue;
    }
};

public class XorShift : RandomNumberGenerator
{
    private uint _state;

    public XorShift(int seed)
    {
        if(seed <= 0)
        {
            seed = int.MaxValue;
        }
        _state = (uint)seed;
    }

    public override uint Next()
    {
        // Xorshift algorithm from George Marsaglia's paper.
        _state ^= (_state << 13);
        _state ^= (_state >> 17);
        _state ^= (_state << 5);
        return _state;
    }
};