using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "PhantasmalOrbs", menuName = "Player Activated Items/Phantasmal Orbs", order = 1)]
public class PhantasmalOrbs : PlayerActivatedItem
{
    public List<Follower> followers = new List<Follower>();
    private float _timer;
    private bool _active;
    private bool _activating;
    private bool _deactivating;
    private float _aegisTimer;

    public override void Initialize(Player player)
    {
        base.Initialize(player);        
    }

    public override void ButtonDown()
    {
        base.ButtonDown();

        if (!_activating && !_deactivating)
        {
            if (_active)
            {
                Deactivate();
            }
            else if (Usable() && followers.Count == 0)
            {
                Activate();
            }
        }
    }

    public override void Update()
    {
        base.Update();

        if (_active && !_activating && !_deactivating)
        {
            if (_aegisTimer > 0) { _aegisTimer -= Time.deltaTime; }
            if (_player.energy <= 0) { Deactivate(); }
        }
    }

    public void Activate()
    {
        _active = true;        
        _player.StartCoroutine(ActivateRoutine());
    }

    public void Deactivate()
    {
        _active = false;
        _player.StartCoroutine(DeactivateRoutine());
    }

    public IEnumerator ActivateRoutine()
    {
        while (_deactivating) yield return null;

        if(!Usable()) yield break;

        _activating = true;
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
            follower.followerIndex = -1;
            follower.onHurt.AddListener(onDamage);

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
        _activating = false;
    }

    public override ActivatedItemPickUp Unequip(Vector3 position, Transform parent, bool setJustSpawned = true)
    {
        if (_active) _player.StartCoroutine(DeactivateRoutine());
        return base.Unequip(position, parent, setJustSpawned);
    }

    public void onDamage(float d)
    {
        if (_aegisTimer > 0) return;
        if (_player.energy > 0)
        {
            _aegisTimer = 1f;
            _player.energy -= Mathf.Clamp(d, 2, 3);
        }
    }

    public IEnumerator DeactivateRoutine()
    {
        while (_activating) yield return null;

        _deactivating = true;

        for (int i = 0; i < followers.Count; i++)
        {
            var follower = followers[i];
            _player.followers.Remove(follower);
            var animator = follower.GetComponent<Animator>();
            animator.Play("Disappear");
            yield return new WaitForSeconds(8f / 18f);
            Destroy(follower.gameObject);
            _player.ReorderFollowers();
        }
        followers.Clear();

        _deactivating = false;
    }

    public void OnDestroy()
    {
        if (_active) _player.StartCoroutine(DeactivateRoutine());
    }

    public override bool Usable()
    {
        return (base.Usable() && !_activating && !_deactivating);
    }
}
