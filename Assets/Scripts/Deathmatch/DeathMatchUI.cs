using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DeathMatchUI : MonoBehaviour
{
    public Text joinPrompt;
    public Text frags;
    public CanvasGroup notification;
    public Text notificationText;
    public HealthBar healthBar;
    public EnergyBar energyBar;
    public WeaponWheel weaponWheel;
    public ActivatedItemUI activatedItemUI;
    public ItemSlotMachine slotMachine;
    public Image blackOut;
    public Team team;

    private IEnumerator _notification;

    public void AssignPlayer(DeathmatchPlayer player)
    {
        team = player.team;
        joinPrompt.gameObject.SetActive(false);
        frags.gameObject.SetActive(true);
        frags.material = new Material(frags.material);
        frags.material.SetColor("_FlashColor", Color.white);

        healthBar.player = player;
        healthBar.gameObject.SetActive(true);
        if (healthBar.started) { healthBar.SetupPlayer(); }

        energyBar.player = player;
        energyBar.gameObject.SetActive(true);
        if (energyBar.started) { StartCoroutine(energyBar.SetupPlayer()); }

        weaponWheel.player = player;
        weaponWheel.gameObject.SetActive(true);

        activatedItemUI.player = player;
        activatedItemUI.gameObject.SetActive(true);
    }

    public void DropPlayer(DeathmatchPlayer player)
    {
        joinPrompt.gameObject.SetActive(true);
        frags.gameObject.SetActive(false);
        healthBar.UnassignPlayer();
        healthBar.gameObject.SetActive(false);
        energyBar.UnassignPlayer();
        energyBar.gameObject.SetActive(false);
        weaponWheel.player = null;
        weaponWheel.gameObject.SetActive(false);
        activatedItemUI.player = null;
        activatedItemUI.gameObject.SetActive(false);
        slotMachine.gameObject.SetActive(false);
    }

    public void Update()
    {
        if(DeathmatchManager.instance.score.ContainsKey(team))
        {
            var score = DeathmatchManager.instance.score[team];
            frags.text = "Frags: " + score;

            if (DeathmatchManager.instance.leader == team && !DeathmatchManager.instance.tie)
            {
                frags.material.SetFloat("_FlashAmount", Mathf.PingPong(Time.unscaledTime, 0.5f));
            }
            else
            {
                frags.material.SetFloat("_FlashAmount", 0);
            }
        }
    }

    public void ShowNotification(string text, float time = 2)
    {
        if(_notification != null) { StopCoroutine(_notification); }
        _notification = ShowNotifcationRoutine(text, time);
        StartCoroutine(_notification);
    }

    private IEnumerator ShowNotifcationRoutine(string text, float time)
    {
        notification.gameObject.SetActive(true);
        notification.alpha = 1;
        notificationText.text = text;

        yield return new WaitForSecondsRealtime(time);

        var fadeTime = 0.5f;
        var timer = fadeTime;
        while (timer > 0)
        {
            timer -= Time.unscaledDeltaTime;
            notification.alpha = timer / fadeTime;
            yield return null;
        }

        notification.gameObject.SetActive(false);
    }
}
