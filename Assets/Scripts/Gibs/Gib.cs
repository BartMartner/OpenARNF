using UnityEngine;
using System.Collections;

[RequireComponent(typeof(Rigidbody2D))]
public class Gib : MonoBehaviour
{
    private SpriteRenderer _spriteRenderer;
    private Rigidbody2D _rigidbody2D;
    private Color _orignalColor;
    private float _lifeSpan;
    private float _lifeCounter;
    private Bounds? _currentRoomBounds;
    public bool recycle = true;
    public bool vorbTargeted = false;

    public void Awake ()
    {
        _rigidbody2D = GetComponentInChildren<Rigidbody2D>();
        _spriteRenderer = GetComponentInChildren<SpriteRenderer>();
        _orignalColor = _spriteRenderer.color;
    }

    public void Update ()
    {
        if (_currentRoomBounds.HasValue && !_currentRoomBounds.Value.Contains(transform.position))
        {            
            Recycle();
        }

        _lifeCounter += Time.deltaTime;

        if (_lifeCounter > _lifeSpan)
        {
            if (_spriteRenderer.isVisible)
            {
                StartCoroutine(FadeOut());
            }
            else
            {
                Recycle();
            }
        }
    }

    public void Spawn(GibType gType, Vector3 origin, float force, float lifeSpan)
    {
        vorbTargeted = false;
        transform.position = origin;
        _lifeSpan = lifeSpan;
        _lifeCounter = 0;
        _spriteRenderer.color = _orignalColor;
        gameObject.SetActive(true);
        gameObject.layer = LayerMask.NameToLayer("DefaultOnly");
        _rigidbody2D.AddForce(Random.insideUnitCircle.normalized * force, ForceMode2D.Impulse);
        _currentRoomBounds = null;

#if UNITY_EDITOR
        var currentRoom = LayoutManager.CurrentRoom ?? FindObjectOfType<Room>();
#else
        var currentRoom = LayoutManager.CurrentRoom;
#endif
        if(currentRoom)
        {
            _currentRoomBounds = currentRoom.worldBounds;
            _currentRoomBounds.Value.Expand(1);
        }        
    }

    public IEnumerator FadeOut()
    {
        var targetColor = _spriteRenderer.color;
        targetColor.a = 0f;
        var timer = 0f;
        var fadeTime = 1f;
        while (timer < fadeTime)
        {
            timer += Time.deltaTime;
            _spriteRenderer.color = Color.Lerp(_orignalColor, targetColor, timer / fadeTime);
            yield return null;
        }
        Recycle();
    }

    public void Recycle()
    {
        if (recycle)
        {
            transform.parent = GibManager.instance.transform;
            gameObject.SetActive(false);
        }
        else
        {
            Destroy(gameObject);
        }
    }
}

