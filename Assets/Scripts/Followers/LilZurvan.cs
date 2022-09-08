using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LilZurvan : LilShrine
{
    public AudioClip burstSound;
    public GameObject pulsePrefab;

    public override IEnumerator Start()
    {
        _holyNumber = 5;
        effects.Add(TempRegen());
        effects.Add(SearchBurst());
        yield return base.Start();
    }    

    public IEnumerable TempRegen()
    {
        yield return null;
        var tempRegen = player.gameObject.AddComponent<TemporaryRegeneration>();
        var tempBoostCount = player.GetComponentsInChildren<TemporaryRegeneration>().Length;
        var bonus = 1f / (tempBoostCount > 3 ? tempBoostCount - 2 : 1);
        tempRegen.Equip(player, 30 * player.blessingTimeMod, bonus);
        FXManager.instance.SpawnFX(FXType.AnimeSplode, player.position);
    }

    public IEnumerable SearchBurst()
    {
        if (burstSound) { _audioSource.PlayOneShot(burstSound); }
        Instantiate(pulsePrefab, transform.position, Quaternion.identity, player.transform.parent);

        yield return new WaitForSeconds(0.33f);
        if (Automap.instance) Automap.instance.RevealMap(player.gridPosition, 1, true);
        yield return new WaitForSeconds(0.33f);
        if (Automap.instance) Automap.instance.RevealMap(player.gridPosition, 2, true);
        yield return new WaitForSeconds(0.33f);
        if (Automap.instance) Automap.instance.RevealMap(player.gridPosition, 3, true);

        SaveGameManager.instance.Save();
    }
}
