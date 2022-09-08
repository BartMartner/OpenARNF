using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ImageScroller : MonoBehaviour
{
    public float speed = 1;    
    public Color hazeColor;
    public float hazeDepth;

    private Image[] _images;
    private RectTransform _rectTransform;
    private List<float> _locationFloats;
    private Vector2 _lastScreenSize;
    private float _scollMod;

	void Start ()
    {
        _rectTransform = GetComponent<RectTransform>();
        _images = GetComponentsInChildren<Image>();
        var newMaterial = new Material(_images[0].material);
        newMaterial.SetColor("_FlashColor", hazeColor);
        newMaterial.SetFloat("_FlashAmount", hazeDepth);

        Vector3[] containerCorners = new Vector3[4];
        _rectTransform.GetWorldCorners(containerCorners);        
        _locationFloats = new List<float>();

        foreach (var i in _images)
        {
            i.material = newMaterial;
            var position = i.transform.localPosition.x / 768;
            _locationFloats.Add(position + 0.5f);
        }
	}
	
	void Update ()
    {
        var screenSize = new Vector2(Screen.width, Screen.height);
        if (_lastScreenSize != screenSize)
        {
            _lastScreenSize = screenSize;
            return;
        }
        _lastScreenSize = screenSize;

        Vector3[] containerCorners = new Vector3[4];
        _rectTransform.GetWorldCorners(containerCorners);
        var size = containerCorners[2] - containerCorners[0];

        _scollMod += Time.deltaTime * (speed/size.x);

        for (int i = 0; i < _images.Length; i++)
        {
            var image = _images[i];
            var p = image.transform.position;
            var t = (_locationFloats[i] - _scollMod) % 1;
            if(t < 0)
            {
                t = 1 + t;
            }
            p.x = Mathf.Lerp(containerCorners[0].x, containerCorners[3].x, t);
            image.transform.position = p;
            //image.transform.position += Vector3.left * speed * Time.deltaTime;
            //if(!OnScreen(image))
            //{
            //    var p = image.transform.position;
            //    p.x = (size.x + image.sprite.bounds.size.x)/2 -1;
            //    image.transform.position = p;
            //}
        }
    }

    //private bool OnScreen(Image image)
    //{        
    //    Vector3[] imageCorners = new Vector3[4];
    //    image.rectTransform.GetWorldCorners(imageCorners);
    //    Vector3[] containerCorners = new Vector3[4];
    //    _rectTransform.GetWorldCorners(containerCorners);
    //    var bounds = new Rect();
    //    bounds.yMin = containerCorners[0].y;
    //    bounds.xMin = containerCorners[0].x;
    //    bounds.yMax = containerCorners[2].y;
    //    bounds.xMax = containerCorners[2].x;

    //    foreach (var corner in imageCorners)
    //    {
    //        if(bounds.Contains(corner))
    //        {
    //            return true;
    //        }
    //    }
    //    return false;
    //}
}
