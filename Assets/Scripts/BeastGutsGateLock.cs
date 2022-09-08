using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class BeastGutsGateLock : MonoBehaviour, IAbstractDependantObject
{
    public int priority;
    public int m_priority
    {
        get { return priority; }
        set { priority = value; }
    }

    public Room parentRoom;

    private string _waveKey;
    private string _gateKey;
    private int _state;

    public SpriteRenderer spriteRenderer;
    public ButtonTriggerBounds buttonTrigger;
    public GameObject buttonHint;
    public Sprite off;
    public Sprite on;
    public GameObject greenLight;
    public GameObject redLight;
    public AudioSource audioSource;
    public AudioClip waveSound;
    public SpriteRenderer lightDim;

    public Enemy[] wave0Prefabs;
    public Enemy[] wave1Prefabs;
    public Enemy[] wave2Prefabs;

    private List<Enemy> _enemiesSpawned = new List<Enemy>();

    public void CompareWithAbstract(RoomAbstract roomAbstract)
    {
        if (SaveGameManager.activeGame != null)
        {
            var otherInts = SaveGameManager.activeGame.otherInts;

            _gateKey = "BeastGutsGate" + roomAbstract.roomID;
            _waveKey = "BeastGutsWave" + roomAbstract.roomID;
            int waveState;

            if (otherInts.TryGetValue(_gateKey, out _state))
            {
                SetState(_state);

                if (_state == 1 && otherInts.TryGetValue(_waveKey, out waveState) && waveState == 0)
                { 
                    BeginWaves();
                }
            }
        }
    }

    public void Press()
    {
        Debug.Log("Pressed");
        audioSource.Play();
        SetState(1);
        BeginWaves();
        if(SaveGameManager.activeGame != null)
        {
            SaveGameManager.activeGame.otherInts.Add(_gateKey, _state);
            SaveGameManager.instance.Save();
        }
    }

    public void SetState(int state)
    {
        if (state == 0)
        {
            redLight.SetActive(true);
            greenLight.SetActive(false);
            spriteRenderer.sprite = off;            
            _state = 0;
        }
        else
        {
            redLight.SetActive(false);
            greenLight.SetActive(true);
            spriteRenderer.sprite = on;
            _state = 1;
            Destroy(buttonTrigger.gameObject);
            Destroy(buttonHint);            
        }
    }

    public void BeginWaves()
    {
        StartCoroutine(WaveRoutine());
    }

    private IEnumerator WaveRoutine()
    {
        parentRoom.StartLockDown();
        MusicController.instance.PlayBlackFleshMusic();
        yield return new WaitForSeconds(3f);
        for (int i = 0; i < 6; i++)
        {
            yield return StartCoroutine(WaveStart());
            int count = 1;
            float spawnDelay = 0.75f;
            float waveWait = 1;
            Enemy[] prefabs = null;
            switch (i)
            {
                case 0:
                    prefabs = wave0Prefabs;
                    count = 10;
                    spawnDelay = 0.55f;
                    waveWait = 8f;
                    break;
                case 1:
                    prefabs = wave0Prefabs;
                    count = 8;
                    spawnDelay = 0.375f;
                    waveWait = 10f;
                    break;
                case 2:
                    prefabs = wave1Prefabs;
                    count = 7;
                    spawnDelay = 1f;
                    waveWait = 3f;
                    break;
                case 3:
                    prefabs = wave1Prefabs;
                    count = 5;
                    spawnDelay = 0.5f;
                    waveWait = 12f;
                    break;
                case 4:
                    prefabs = wave2Prefabs;
                    count = 1;
                    spawnDelay = 0;
                    waveWait = 10f;
                    break;
                case 5:
                    prefabs = wave2Prefabs;
                    count = 2;
                    spawnDelay = 0.5f;
                    waveWait = 0;
                    break;
            }

            for (int j = 0; j < count; j++)
            {
                var spawnPoint = GetSpawnPoint(4);
                var e = Instantiate(prefabs[Random.Range(0, prefabs.Length)],spawnPoint, Quaternion.identity, parentRoom.transform);
                _enemiesSpawned.Add(e);
                yield return new WaitForSeconds(spawnDelay);
            }

            var timer = 0f;
            while(timer < waveWait && _enemiesSpawned.Any(e => e.state == DamageableState.Alive))
            {
                timer += Time.deltaTime;
                yield return null;
            }
        }

        while (_enemiesSpawned.Any(e => e.state == DamageableState.Alive))
        {
            yield return new WaitForSeconds(0.5f);
        }

        yield return new WaitForSeconds(1f);
        MusicController.instance.PlayEnvironmentMusic();
        parentRoom.EndLockDown();
        if (SaveGameManager.activeGame != null)
        {
            SaveGameManager.activeGame.otherInts.Add(_waveKey, 1);
            SaveGameManager.instance.Save();
        }
    }

    private IEnumerator WaveStart()
    {
        audioSource.PlayOneShot(waveSound);
        MainCamera.instance.Shake(1);
        var timer = 0f;
        var color = lightDim.color;
        while(timer < 0.5f)
        {
            timer += Time.deltaTime;
            color.a = timer / 0.5f;
            lightDim.color = color;
            yield return null;
        }

        yield return new WaitForSeconds(0.1f);

        timer = 0.5f;
        while (timer > 0)
        {
            timer -= Time.deltaTime;
            color.a = timer / 0.5f;
            lightDim.color = color;
            yield return null;
        }

        color.a = 0;
        lightDim.color = color;
    }

    public Vector3 GetSpawnPoint(float padding)
    {
        var b = parentRoom.worldBounds;
        var tl = new Vector3(b.min.x - padding, b.max.y + padding);
        var tr = new Vector3(b.max.x + padding, b.max.y + padding);
        var bl = new Vector3(b.min.x - padding, b.min.y - padding);
        var br = new Vector3(b.max.x + padding, b.min.y - padding);
        var l = Random.value;
        var r = Random.Range(0, 4);

        switch (r)
        {
            case 0:
                return Vector3.Lerp(tl, tr, l);
            case 1:
                return Vector3.Lerp(tl, bl, l);
            case 2:
                return Vector3.Lerp(tr, br, l);
            case 3:
                return Vector3.Lerp(bl, br, l);
        }

        return b.center;
    }
}
