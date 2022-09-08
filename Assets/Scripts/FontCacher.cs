using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class FontCacher : MonoBehaviour
{
    public Font[] preCacheFonts;
    private static FontCacher _instance;

    private void Awake()
    {
        if(_instance)
        {
            Destroy(gameObject);
        }
        else
        {
            _instance = this;
            DontDestroyOnLoad(gameObject);
        }
    }

    public IEnumerator Start()
    {
        Debug.Log("Caching Fonts Started");
        var glyphs = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789!@#$%^&*()-_+=~`[]{}|\\:;\"'<>,.?/ ";
        var sizes = new int[] { 14,18,24,27,36};
        //var boldSizes = new int[] { 12, };
        foreach (var font in preCacheFonts)
        {
            foreach (var size in sizes)
            {
                //var bold = boldSizes.Contains(size);
                for (int i = 0; i < glyphs.Length; i++)
                {
                    font.RequestCharactersInTexture(glyphs[i].ToString(), size, FontStyle.Normal);
                    yield return null;
                    //if(bold)
                    //{
                        font.RequestCharactersInTexture(glyphs[i].ToString(), size, FontStyle.Bold);
                        yield return null;
                    //}
                }
            }
        }
        Debug.Log("Caching Fonts Complete");
    }

    private void OnDestroy()
    {
        if(_instance == this)
        {
            _instance = null;
        }
    }
}
