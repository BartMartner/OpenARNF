using UnityEngine;
using System.Collections;

public class PixelPerfect : MonoBehaviour
{
    public int referenceScreenSize;
    public float referencePixelsPerUnit;

    private int lastSize = 0;

    // Use this for initialization
    void Start()
    {
        UpdateOrthoSize();
    }

    void UpdateOrthoSize()
    {
        lastSize = Screen.height;

        // first find the reference orthoSize
        float refOrthoSize = (referenceScreenSize / referencePixelsPerUnit) * 0.5f;

        // then find the current orthoSize
        float ppu = referencePixelsPerUnit;
        float orthoSize = (lastSize / ppu) * 0.5f;

        // the multiplier is to make sure the orthoSize is as close to the reference as possible
        float multiplier = Mathf.Max(1, Mathf.Round(orthoSize / refOrthoSize));

        // then we rescale the orthoSize by the multipler
        orthoSize /= multiplier;

        // set it
        this.GetComponent<Camera>().orthographicSize = orthoSize;

        Debug.Log(lastSize + " " + orthoSize + " " + multiplier + " " + ppu);
    }

#if UNITY_EDITOR
    void Update()
    {
        if (lastSize != Screen.height)
        {
            UpdateOrthoSize();
        }
    }
#endif
}
