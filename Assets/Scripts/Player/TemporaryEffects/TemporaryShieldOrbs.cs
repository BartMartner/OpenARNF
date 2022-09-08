using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TemporaryShieldOrbs : TemporaryPlayerEffect
{
    public List<Follower> followers = new List<Follower>();

    public override void Equip(Player player, float duration)
    {
        base.Equip(player, duration);
        StartCoroutine(EquipRoutine());
    }

    public IEnumerator EquipRoutine()
    {
        for (int i = 0; i < 3; i++)
        {
            var followerPrefab = Resources.Load<Follower>("Followers/ShieldOrb");
            var follower = Instantiate(followerPrefab);
            var animator = follower.GetComponent<Animator>();
            animator.Play("Appear");
            follower.type = MajorItem.None;
            follower.player = _player;
            follower.transform.position = _player.transform.position;
            followers.Add(follower);

            var renderer = follower.GetComponent<Renderer>();
            renderer.material = new Material(renderer.material);
            var paletteCyclingInstance = follower.gameObject.AddComponent<PaletteCycling>();
            paletteCyclingInstance.enabled = true;
            paletteCyclingInstance.paletteCycle = Resources.Load<PaletteCycle>("PaletteCycles/TemporaryShieldOrb");
            paletteCyclingInstance.defaultPalette = Resources.Load<Texture2D>("Palettes/Followers/ShieldOrbPalettePurple");
            paletteCyclingInstance.cycleFrequency = 0.1f;

            _player.followers.Add(follower);
            _player.ReorderFollowers();
            yield return new WaitForSeconds(0.5f);
        }
    }

    public override void Unequip()
    {
        StartCoroutine(UnequipRoutine());
    }

    public IEnumerator UnequipRoutine()
    {
        _equipped = false;
        foreach (var follower in followers)
        {
            _player.followers.Remove(follower);
            var animator = follower.GetComponent<Animator>();
            animator.Play("Disappear");
            yield return new WaitForSeconds(8f / 18f);
            Destroy(follower.gameObject);
            _player.ReorderFollowers();
        }

        Destroy(this);
    }
}
