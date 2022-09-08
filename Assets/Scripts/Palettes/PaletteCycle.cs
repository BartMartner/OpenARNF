using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[CreateAssetMenu(fileName = "PaletteCycle", menuName = "Palette/Create Palette Cycle", order = 1)]
public class PaletteCycle : ScriptableObject
{
    public Texture2D[] palettes;
}
