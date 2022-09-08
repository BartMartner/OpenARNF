using UnityEngine;
using System.Collections;
using UnityEngine.UI;

[RequireComponent(typeof(Text))]
public class FPSCounter : MonoBehaviour
{
    public float frequency = 0.5F; // The update frequency of the fps
    public int nbDecimal = 1; // How many decimal do you want to display

    private Text _text;
    private float accum = 0f; // FPS accumulated over the interval
    private int frames = 0; // Frames drawn over the interval

    void Start()
    {
        _text = GetComponent<Text>();
        StartCoroutine(FPS());
    }

    void Update()
    {
        accum += Time.timeScale / Time.deltaTime;
        ++frames;
    }
    
    private IEnumerator FPS()
    {
        // Infinite loop executed every "frenquency" secondes.
        while (true)
        {
            // Update the FPS
            float fps = accum / frames;
            _text.text = fps.ToString("f" + Mathf.Clamp(nbDecimal, 0, 10));            

            accum = 0.0F;
            frames = 0;

            yield return new WaitForSeconds(frequency);
        }
    }
}