using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class DevilerChild : MonoBehaviour
{
    public bool active;    
    public DevilerParent parent;
    private SimpleAnimation[] _origMask;

    public int[] segments = new int[]
    { -1, 1, 1, 1,-1,
       1, 1, 1, 1, 1,
       1, 1, 1, 1, 1,
       1, 1, 1, 1, 1,
      -1, 1, 1, 1,-1,
       1, 1, 1, 1, 1, };

    public SimpleMaskAnimator[] masks;
    private SimpleDamageTriggerRect[] _damageTriggers;

    //--,01,02,03,--
    //05,06,07,08,09
    //10,11,12,13,14
    //15,16,17,18,19
    //--,21,22,23,--
    //25,26,27,28,29
    private int[] _usedIndices;
    public int[] usedIndices
    {
        get
        {
            if(_usedIndices == null)
            {
                var indices = new List<int>();
                for (int i = 0; i < segments.Length; i++)
                {
                    if (segments[i] == -1) { continue; }
                    indices.Add(i);
                }
                _usedIndices = indices.ToArray();
            }
            return _usedIndices;
        }
    }

    private void Awake()
    {
        _origMask = new SimpleAnimation[masks.Length];
        _damageTriggers = new SimpleDamageTriggerRect[masks.Length];
        for (int i = 0; i < masks.Length; i++)
        {
            _damageTriggers[i] = masks[i].GetComponent<SimpleDamageTriggerRect>();
            _origMask[i] = masks[i].simpleAnim;
        }
        RefreshSegments();
    }

    public void EnableAllSegments()
    {
        for (int i = 0; i < segments.Length; i++) { if (segments[i] != -1) segments[i] = 1; }
        RefreshSegments();
    }

    public void DisableAllSegments()
    {
        for (int i = 0; i < segments.Length; i++) { if (segments[i] != -1) segments[i] = 0; }
        RefreshSegments();
    }

    public void RefreshSegments()
    {
        var maskIndex = 0;
        for (int i = 0; i < segments.Length; i++)
        {
            var segment = segments[i];
            if (segment == -1) continue;
            var mask = masks[maskIndex];
            var damageTrigger = _damageTriggers[maskIndex];
            var anim = mask.GetComponent<SimpleMaskAnimator>();
            if(segment == 0)
            {
                var index = 0;
                if (i > 4 && segments[i - 5] == 1) { index += 1; }
                if ((i-4) % 5 != 0 && i < segments.Length-1 && segments[i + 1] == 1) { index += 2; }
                if (i < 25 && segments[i + 5] == 1) { index += 4; }
                if (i % 5 != 0 && segments[i - 1] == 1) { index += 8; }
                mask.simpleAnim = parent.gapAnims[index];
                mask.SetFrame();
                if (damageTrigger) { damageTrigger.enabled = false; }
            }
            else
            {
                mask.simpleAnim = _origMask[maskIndex];
                mask.SetFrame();
                if (damageTrigger) { damageTrigger.enabled = true; }
            }

            maskIndex++;
        }
    }

    public int GetMaskIndex(int index)
    {
        var maskIndex = 0;
        for (int i = 0; i < segments.Length; i++)
        {
            var segment = segments[i];
            if (segment == -1) continue;
            if (i == index) { return maskIndex; }
            maskIndex++;
        }

        return -1;
    }

    public Vector3 GetSegmentPos(int index)
    {
        var maskIndex = GetMaskIndex(index);
        return masks[maskIndex].transform.position;
    }

    public bool IsComplete()
    {
        return !segments.Any(s => s == 0);
    }
}