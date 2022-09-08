using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CorruptedMiner : MonoBehaviour
{
    public Transform botStartLeft;
    public Transform botStartRight;
    public Transform trackStartLeft;
    public Transform trackStartRight;
    public Transform trackEnterPos;
    public Animator leftBayDoor;
    public Animator rightBayDoor;
    public CorruptedMinerBot bot;
    public CorruptedMinerTrack track;
    public AudioClip meatHurtSound;
    public AudioClip deathSound;
    private bool _trackSummoned;
    private bool _meatAppear;
    private bool _meatEye;
    private Enemy _enemy;
    private bool _dying;

    private void Awake()
    {
        _enemy = GetComponent<Enemy>();
    }

    private void Start()
    {
        if (PlayerManager.instance)
        {
            var player = PlayerManager.instance.GetClosestPlayer(transform.position);
            if (player)
            {
                var lDist = Vector3.Distance(player.position, botStartLeft.position);
                var rDist = Vector3.Distance(player.position, botStartRight.position);
                bot.transform.position = lDist > rDist ? botStartLeft.position : botStartRight.position;
            }
        }
    }

    public void Update()
    {
        if (_dying) return;

        if(_enemy.state != DamageableState.Alive)
        {
            StartCoroutine(Die());
            return;
        }

        var healthRatio = _enemy.health / _enemy.maxHealth;

        if (healthRatio < 0.80f && !_meatEye)
        {
            _meatEye = true;
            _enemy.hurtSound = meatHurtSound;
            bot.MeatEye();
        }

        if (healthRatio < 0.66f && !_trackSummoned)
        {
            _trackSummoned = true;
            StartCoroutine(TrackEnter());
        }

        if(healthRatio < 0.33 && !_meatAppear && !bot.busy)
        {
            _meatAppear = true;
            bot.EnableMeat();
        }
    }

    public IEnumerator TrackEnter()
    {
        while (bot.busy) { yield return null; }
        bot.pacer.enabled = false;
        bot.busy = true;
        var player = PlayerManager.instance.GetClosestPlayer(transform.position);
        var start = Vector3.SqrMagnitude(player.position - trackStartLeft.position) < Vector3.SqrMagnitude(player.position - trackStartRight.position) ? trackStartLeft.position : trackStartRight.position;
        var door = start == trackStartLeft.position ? leftBayDoor : rightBayDoor;
        door.Play("Open");
        yield return new WaitForSeconds(2);
        bot.pacer.enabled = true;
        bot.busy = false;
        track.transform.position = start;
        track.gameObject.SetActive(true);
        yield return StartCoroutine(track.EnterRoomRoutine(start, trackEnterPos.position));
        door.Play("Close");
        yield return new WaitForSeconds(2);
    }

    public IEnumerator Die()
    {
        _dying = true;
        bot.enabled = false;
        track.enabled = false;
        var timer = 0f;
        var time = 3f;
        var start = bot.transform.position;
        var fadeStarted = false;

        while (timer < time)
        {
            timer += Time.deltaTime;
            bot.transform.position = Vector3.Lerp(start, track.transform.position, timer / time);
            yield return null;
            if (timer > 2.5f && ! fadeStarted)
            {
                fadeStarted = true;
                TransitionFade.instance.FadeOut(0.5f, Color.white);
            }
        }

        var size = new Vector2(8, 3);
        var rect = new Rect((track.transform.position - (Vector3)size * 0.5f), size);
        GibManager.instance.SpawnGibs(GibType.GreenTech, rect, 30);
        GibManager.instance.SpawnGibs(GibType.Meat, rect, 20);
        FXManager.instance.SpawnFXsOverTime(FXType.ExplosionMedium, new Bounds(track.transform.position, size), 20, 0.5f);
        TransitionFade.instance.FadeIn(0.25f, Color.white);
        MainCamera.instance.Shake(0.5f);
        AudioManager.instance.PlayOneShot(deathSound);
        Destroy(gameObject);
    }
}
