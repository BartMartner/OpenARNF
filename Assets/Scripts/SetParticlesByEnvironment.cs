using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SetParticlesByEnvironment : MonoBehaviour, IAbstractDependantObject
{
    public Sprite[] underwaterSprites;
    public int m_priority { get; set; }

    private ParticleSystem _particleSystem;

    public void Awake()
    {
        _particleSystem = GetComponent<ParticleSystem>();
    }

    public void CompareWithAbstract(RoomAbstract roomAbstract)
    {
        if(roomAbstract.environmentalEffect == EnvironmentalEffect.Underwater)
        {
            SetUnderWater(); 
        }
    }

    public void SetUnderWater()
    {
        if (underwaterSprites != null)
        {
            var tsa = _particleSystem.textureSheetAnimation;
            for (int i = 0; i < underwaterSprites.Length; i++)
            {
                if (i < tsa.spriteCount) { tsa.SetSprite(i, underwaterSprites[i]); }
                else
                {
                    tsa.AddSprite(underwaterSprites[i]);
                }
            }
        }
    }
}