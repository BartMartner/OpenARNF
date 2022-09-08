using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BeastGutsPortal : MonoBehaviour
{
    public Collider2D trigger;
    private SpriteRenderer _renderer;
    private float _offsetY;
    private bool _triggered;
    private void Awake()
    {
        _renderer = GetComponent<SpriteRenderer>();
    }

    public void Show()
    {
        gameObject.SetActive(true);
    }

    private void Update()
    {
        _offsetY += Time.deltaTime * 0.05f;
        _offsetY = _offsetY % 1;
        _renderer.material.SetFloat("_OffsetY", -_offsetY);
    }

    private void OnTriggerStay2D(Collider2D collision)
    {
        if (!_triggered)
        {
            _triggered = true;
            trigger.enabled = false;
            StartCoroutine(TransitionSequence());
            Debug.Log("triggered!");
        }
    }

    private IEnumerator TransitionSequence()
    {
        var player1 = PlayerManager.instance.player1;
        var timer = 0f;
        player1.enabled = false;
        player1.SetAnimatorInAir();

        while (timer < 2)
        {
            var position = player1.transform.position;
            position += Vector3.up * Time.deltaTime * 2f;
            position.x = Mathf.MoveTowards(position.x, transform.position.x, 3 * Time.deltaTime);
            player1.transform.position = position;
            timer += Time.deltaTime;
            yield return null;
        }

        if (LayoutManager.instance)
        {
            UISounds.instance.BeastGutsFade();
            LayoutManager.instance.TeleportToBeastGuts();
        }

        timer = 0f;
        while (timer < 10)
        {
            var position = player1.transform.position;
            position += Vector3.up * Time.deltaTime * 2f;
            position.x = Mathf.MoveTowards(position.x, transform.position.x, 3 * Time.deltaTime);
            player1.transform.position = position;
            timer += Time.deltaTime;
            yield return null;
        }
    }
}
