using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class TutorialSmithBossController : MonoBehaviour
{
    public TutorialSmithBossForm1 form1;
    public TutorialSmithBossForm2 form2;

    public Pit[] pits;
    public ToggleBlockGroup left;
    public ToggleBlockGroup right;
    public ToggleBlockGroup[] blockGroups;
    public List<Enemy> enemies;

    [Header("GlitchCreepers")]
    public GameObject glitchCreeperPrefab;
    public Transform[] glitchCreeperSpawnPoints;

    [Header("FalseBeastProjectors")]
    public GameObject falseBeastProjectorPrefab;
    public Transform[] falseBeastProjectorSpawnPoints;

    [Header("HoodedAcolytes")]
    public GameObject hoodedAcolytePrefab;
    public Transform[] hoodedAcolyteSpawnPoints;

    private Enemy _enemy;
    public Enemy enemy { get { return _enemy; } }
    private bool _swapped;

    public void Awake()
    {
        _enemy = GetComponent<Enemy>();
        _enemy.onStartDeath.AddListener(Die);
    }

    public void Update()
    {
        if (enemies.Count > 0) 
        {
            enemies.RemoveAll(e => !e);
        }

        if(!_swapped && _enemy.health < _enemy.maxHealth * 0.45f)
        {
            _swapped = true;
            StartCoroutine(SwapForms());
        }
    }

    public void RaiseFarPlatforms()
    {
        right.Show();
        left.Show();
        blockGroups[1].Hide();
        blockGroups[blockGroups.Length-2].Hide();
        pits[1].Show();
        pits[pits.Length -2].Show();
    }

    public void LowerFarPlatforms()
    {
        right.Hide();
        left.Hide();
        blockGroups[1].Show();
        blockGroups[blockGroups.Length - 2].Show();
        pits[1].Hide();
        pits[pits.Length - 2].Hide();
    }

    public void SpawnForm1Monsters()
    {
        var random = Random.Range(0, 2);
        switch(random)
        {
            case 0:
                SpawnGlitchCreepers();
                break;
            case 1:
                SpawnFalseBeastProjectors();
                break;
            case 2:
                SpawnHoodedAcolytes();
                break;
        }
    }

    public void SpawnForm2Monsters()
    {
        var random = Random.Range(0, 2);
        switch (random)
        {
            case 0:
                SpawnHoodedAcolytes();
                break;
            case 1:
                SpawnFalseBeastProjectors();
                break;
        }
    }

    public void SpawnGlitchCreepers()
    {
        SpawnMonsters(glitchCreeperPrefab, glitchCreeperSpawnPoints);
    }

    public void SpawnFalseBeastProjectors()
    {
        SpawnMonsters(falseBeastProjectorPrefab, falseBeastProjectorSpawnPoints);
    }

    public void SpawnHoodedAcolytes()
    {
        SpawnMonsters(hoodedAcolytePrefab, hoodedAcolyteSpawnPoints);
    }

    public void SpawnMonsters(GameObject prefab, Transform[] transforms)
    {
        foreach (var t in transforms)
        {
            var e = Instantiate<GameObject>(prefab, t.position, t.rotation, transform.parent);
            var enemy = e.GetComponentInChildren<Enemy>();
            if (enemy)
            {
                enemies.Add(enemy);
                var spawnPickups = e.GetComponent<SpawnPickUpsOnDeath>();
                if (spawnPickups)
                {
                    spawnPickups.spawnChance = 0.1f;
                }
            }
            FXManager.instance.SpawnFX(FXType.AnimeSplode, t.position);
        }
    }

    public void Die()
    {
        StartCoroutine(Death());
    }

    public IEnumerator Death()
    {
        if (EnemyManager.instance) { EnemyManager.instance.HurtAllEnemies(100000, DamageType.Generic, true); }
        foreach (var enemy in enemies) { enemy.Hurt(10000); }

        var bossFight = GetComponentInParent<BossFight>();
        //end the boss fight early (before it reads the total hp as 0)
        if (bossFight) { bossFight.EndBossFight(); }
        //this allows for some of the ensuing shenanigans with personal teleporter / glitch key

        if (AchievementManager.instance.TutorialSmithDefeated())
        {
            yield return new WaitForSeconds(0.1f);
            while (AchievementScreen.instance.visible)
            {
                yield return new WaitForSeconds(0.1f);
            }
        }

        yield return StartCoroutine(form2.Death());

        Destroy(form2.gameObject);

        TransitionFade.instance.FadeOut(2, Color.white);

        var timer = 0f;
        while(timer < 2)
        {
            timer += Time.deltaTime;
            yield return null;
            if (LayoutManager.instance && LayoutManager.instance.transitioning)
            {
                TransitionFade.instance.FadeIn(2, Color.white);
                yield break;
            }
        }

        //End the run and record stats
        GameMode gameMode = GameMode.Normal;
        if (SaveGameManager.activeSlot != null)
        {
            SaveGameManager.activeSlot.RunCompleted();
            gameMode = SaveGameManager.activeGame.gameMode;
        }

        if (SaveGameManager.activeGame != null && SaveGameManager.activeGame.bossesDefeated.Contains(BossName.MegaBeastCore))
        {
            MainCamera.instance.SetGlitch(true);

            var dialogueInfo = new DialogueInfo();
            dialogueInfo.NPCName = "Error";

            string d = "";
            string everyThingYouKnow = "Everything you know is wrong";
            string thisWasAllDesignedByHand = "This was all designed by hand";
            int i = 0;
            int j = 0;

            foreach (var t in TutorialBot.helpfulTips)
            {
                d += t;
                d += " " + everyThingYouKnow[i] + " ";
                i = (i + 1) % everyThingYouKnow.Length;
                d += " " + thisWasAllDesignedByHand[j] + " ";
                j = (j + 1) % everyThingYouKnow.Length;
            }

            dialogueInfo.dialogue = d;
            NPCDialogueManager.instance.ShowDialogueScreen(dialogueInfo);
            yield return new WaitForSeconds(16f);
            SeedHelper.StartSpookyMode(false);
            yield break;
        }

        switch (gameMode)
        {
            case GameMode.Normal:
                SceneManager.LoadScene("EndScreen03");
                break;
            case GameMode.Spooky:
                SceneManager.LoadScene("SpookyEnding");
                break;
            default:
                SceneManager.LoadScene("Congratulations");
                break;
        }
    }

    public IEnumerator SwapForms()
    {
        while (form1.acting) yield return null;

        yield return StartCoroutine(form1.FadeOut());

        Destroy(form1.gameObject);

        enemy.notTargetable = false;
        enemy.altPosition = form2.transform;

        foreach (var p in blockGroups)
        {
            p.Hide();
        }

        foreach (var p in pits)
        {
            p.Show();
        }

        form2.gameObject.SetActive(true);
        yield return StartCoroutine(form2.Arise());
    }
}
