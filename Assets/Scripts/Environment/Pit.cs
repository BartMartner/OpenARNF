using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Pit : MonoBehaviour
{
    private SpriteRenderer[] _spriteRenderers;
    private Color32 _pitColor = new Color32(255, 0, 32, 255);
    public float damage = 3;

    private void Awake()
    {
        _spriteRenderers = GetComponentsInChildren<SpriteRenderer>();
    }

    public void Show()
    {
        gameObject.SetActive(true);
        StartCoroutine(ColorLerp(Color.clear, _pitColor, 1f));
    }

    public void Hide()
    {
        StartCoroutine(ColorLerp(_pitColor, Color.clear, 1f));
    }

    public IEnumerator ColorLerp(Color start, Color end, float time)
    {
        var timer = 0f;
        while (timer < time)
        {
            timer += Time.deltaTime;
            foreach (var r in _spriteRenderers)
            {
                r.color = Color.Lerp(start, end, timer / time);
            }
            yield return null;
        }

        if(end == Color.clear)
        {
            gameObject.SetActive(false);
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        var player = collision.GetComponent<Player>();
        
        if (player)
        {
            StartCoroutine(CheckForPlayer(player));
            return;
        }

        var enemy = collision.GetComponent<Enemy>();

        if (enemy)
        {
            enemy.StartDeath();
        }
    }

    private IEnumerator CheckForPlayer(Player player)
    {
        player.Hurt(damage, gameObject, DamageType.Generic, true);

        for (int i = 0; i < 10; i++)
        {
            if (!player.mainRenderer.isVisible && player.state == DamageableState.Alive)
            {
                player.transform.position = player.lastSafePosition;
                if (player.gravityFlipped != player.lastSafeGravity)
                {
                    player.FlipGravity();
                }
                player.velocity = Vector3.zero;

                FXManager.instance.SpawnFX(FXType.Teleportation, player.transform.position, false, false, player.facing == Direction.Left, player.gravityFlipped);
                break;
            }
            yield return null;
        }

        MainCamera.instance.SetGlitch(true);
        player.paralyzed = true;
        yield return new WaitForSeconds(0.5f);
        player.paralyzed = false;
        MainCamera.instance.SetGlitch(false);
    }
}
