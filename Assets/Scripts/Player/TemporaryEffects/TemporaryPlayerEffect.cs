using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TemporaryPlayerEffect : MonoBehaviour
{
    protected Player _player;
    protected bool _equipped;
    protected float _duration;

    protected virtual void Update()
    {
        if (!_equipped) { return; }

        if (_player.state != DamageableState.Alive ||
            (AchievementScreen.instance && AchievementScreen.instance.visible) ||
            (PauseMenu.instance && PauseMenu.instance.visible) ||
            (NPCDialogueManager.instance && NPCDialogueManager.instance.dialogueActive) ||
            (BossFightUI.instance && BossFightUI.instance.getReadyVisible))
        {
            return;
        }

        _duration -= Time.deltaTime;
        if (_duration <= 0) { Unequip(); }
    }

    public virtual void Equip(Player player, float duration)
    {
        _player = player;
        _equipped = true;
        _duration = duration;
    }

    public virtual void Unequip()
    {
        _equipped = false;
        Destroy(this);
    }

    private void OnDestroy()
    {
        //if Destroy somehow gets called from someplace other then Unequip
        if (_equipped) Unequip();
    }
}
