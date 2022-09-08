using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Rewired;
using System;

public class PlayerManager : MonoBehaviour
{
    public const int maxCoOpBots = 3;

    public static PlayerManager instance;

    /// <summary>
    /// Intended to simply track the items the players have collected. Includes activated items.
    /// Used for shops, achievements, and destroying collected items in rooms.
    /// </summary>
    public HashSet<MajorItem> itemsCollected = new HashSet<MajorItem>();

    public CanvasGroup joinAlert;
    public Player playerPrefab;
    public CoOpPlayer coOpOrbPrefab;
    public CoOpControlsHint[] controlsHint;
    private CoOpPlayer[] _coOpPlayers = new CoOpPlayer[maxCoOpBots];
    public CoOpPlayer[] coOpPlayers { get { return _coOpPlayers; } }
    public Material orb3Material;
    public Material orb4Material;
    public Material player2Material;
    public Material player3Material;
    public Material player4Material;
    private Player _player1;
    public Player player1 { get { return _player1; } }
    private Rewired.Player[] _controllers = new Rewired.Player[maxCoOpBots];
    private List<Player> _players = new List<Player>();
    public List<Player> players { get { return _players; } }
    public float coOpMod = 1f;
    private bool _trueCoOp = false;
    public bool trueCoOp { get { return _trueCoOp; } }
    public bool allowWallJump = false;

    private IEnumerator _alertRoutine;

    public Vector3 cameraPosition
    {
        get
        {
            if (CoOpCamera.instance && _trueCoOp && players.Count > 1)
            {
                return CoOpCamera.instance.transform.position;
            }
            else
            {
                return MainCamera.instance.transform.position;
            }
        }
    }

    public BaseCamera mainCamera
    {
        get
        {
            if (CoOpCamera.instance && _trueCoOp && players.Count > 1)
            {
                return CoOpCamera.instance;
            }
            else
            {
                return MainCamera.instance;
            }
        }
    }

    private void Awake()
    {
        if (instance)
        {
            Destroy(gameObject);
        }
        else
        {
            instance = this;
            _player1 = Player.instance;
            if (_player1) { _players.Add(_player1); }
        }
    }

    private void Start()
    {        
        for (int i = 0; i < maxCoOpBots; i++)
        {
            _controllers[i] = ReInput.players.GetPlayer(i+1);
        }

        if (SaveGameManager.activeGame == null)
        {
            allowWallJump = true;
        }
        else
        {
            if (!DeathmatchManager.instance)
            {
                var activeGame = SaveGameManager.activeGame;
                _trueCoOp = activeGame.gameMode == GameMode.TrueCoOp;                
                itemsCollected.AddRange(activeGame.itemsCollected.ToList());
            }
            else
            {
                _trueCoOp = false;
            }

            allowWallJump = SaveGameManager.activeSlot.achievements.Contains(AchievementID.WallJump);
        }
    }

    // Update is called once per frame
    private void Update()
    {
        if (!DeathmatchManager.instance && Time.timeScale != 0)
        {
            var count = Mathf.Min(_coOpPlayers.Length, _controllers.Length);
            for (int i = 0; i < count; i++)
            {
                var controller = _controllers[i];
                if(_trueCoOp && controller == _player1.controller) { continue; }

                if (_controllers[i].GetButtonDown("CoOpEnterGame"))
                {
                    var shouldJoin = _trueCoOp ? !_players.Any(p => p.controller == controller) : _coOpPlayers[i] == null;
                    if (shouldJoin)
                    {
                        if (_trueCoOp)
                        {
                            TrueCoOpPlayerJoin(controller);
                        }
                        else
                        {
                            CoOpPlayerJoin(i);
                        }
                    }
                    else
                    {
                        if (controlsHint[i].gameObject.activeInHierarchy)
                        {
                            controlsHint[i].StopAllCoroutines();
                            controlsHint[i].gameObject.SetActive(false);
                        }

                        if (_trueCoOp)
                        {
                            //TODO: Despawn True CoOp Player
                        }
                        else
                        {
                            _coOpPlayers[i].Despawn(true);
                        }
                    }
                }
                else if (joinAlert && _controllers[i].GetAnyButton())
                {
                    bool showAlert = _trueCoOp ? !_players[i] : !_coOpPlayers[i];
                    if (showAlert)
                    {
                        if (_alertRoutine != null) { StopCoroutine(_alertRoutine); }
                        _alertRoutine = ShowJoinAlert(_controllers[i].id);
                        StartCoroutine(_alertRoutine);
                    }
                }
            }

            if (!player1.enabled && player1.paused &&
                (!AchievementScreen.instance || !AchievementScreen.instance.visible) &&
                (!PauseMenu.instance || !PauseMenu.instance.visible) &&
                (!NPCDialogueManager.instance || !NPCDialogueManager.instance.dialogueActive))
            {
                player1.UpdateEnergyWeapon();
            }
        }
    }

    public void TrueCoOpPlayerJoin(Rewired.Player controller)
    {
        var id = _players.Count;
        var player = Instantiate(playerPrefab, _player1.transform.parent);
        _players.Add(player);
        player.transform.parent = _player1.transform.parent;
        player.playerId = id;
        if (id == 1) { player.material = player2Material; }
        if (id == 2) { player.material = player3Material; }
        if (id == 3) { player.material = player4Material; }
        player.controller = controller;
        var position = _player1.position;
        player.transform.position = position;
        if (CoOpCamera.instance)
        {
            var camera = new GameObject().AddComponent<MainCamera>();
            camera.name = "Player " + (id + 1) + " Dummy Camera";
            camera.player = player;
            player.mainCamera = camera;
        }
        FXManager.instance.SpawnFX(FXType.Teleportation, position);
    }

    public void CoOpPlayerJoin(int id)
    {
        _coOpPlayers[id] = Instantiate(coOpOrbPrefab, _player1.transform.parent);
        var controller = _controllers[id];
        var coOpPlayer = _coOpPlayers[id];
        coOpPlayer.transform.parent = _player1.transform.parent;
        coOpPlayer.Spawn(_player1, controller);
        switch(id)
        {
            case 0:
                if (SaveGameManager.activeGame != null) { SaveGameManager.activeGame.player2Entered = true; }
                break;
            case 1:
                if (SaveGameManager.activeGame != null) { SaveGameManager.activeGame.player3Entered = true; }
                coOpPlayer.SetMaterial(orb3Material);
                break;
            case 2:
                if(SaveGameManager.activeGame != null) { SaveGameManager.activeGame.player4Entered = true; }
                coOpPlayer.SetMaterial(orb4Material);
                break;
        }
        SaveGameManager.instance.Save();

        controlsHint[id].Show(coOpPlayer.transform, controller.id);
        CalculateCoOpDamageMod();

        bool controllerConflict = false;
        for (int i = 0; i < _coOpPlayers.Length; i++)
        {
            var p1 = _coOpPlayers[i];
            if(p1)
            {
                for (int j = i+1; j < _coOpPlayers.Length; j++)
                {
                    var p2 = _coOpPlayers[j];
                    if(p2 && p1.controller == p2.controller)
                    {
                        controllerConflict = true;
                    }
                }
            }
        }

        if(controllerConflict)
        {
            Debug.LogError("Whoops! 2 Co-op bots were assigned the same controller! Correcting!");
            for (int i = 0; i < _controllers.Length; i++)
            {
                _controllers[i] = ReInput.players.GetPlayer(i + 1);
                if(i < _coOpPlayers.Length && _coOpPlayers[i])
                {
                    _coOpPlayers[i].controller = _controllers[i];
                }
            }
        }
    }

    public IEnumerator ShowJoinAlert(int id)
    {
        var control = joinAlert.GetComponentInChildren<SetImageForControl>();
        control.playerID = id;
        control.SetImage();

        joinAlert.gameObject.SetActive(true);
        joinAlert.alpha = 1;

        yield return new WaitForSeconds(0.5f);

        var timer = 0.5f;
        while (timer > 0)
        {
            timer -= Time.unscaledDeltaTime;
            joinAlert.alpha = timer / 0.5f;
            yield return null;
        }

        joinAlert.gameObject.SetActive(false);
        _alertRoutine = null;
    }

    public Transform GetClosestInArc(Vector3 position, Vector3 direction, float distance, float angle)
    {
        var lowestDistance = distance;
        Transform closest = null;
        Transform player;

        var toCheck = new List<Transform>();
        foreach (var p in players)
        {
            if (p.targetable && p.state == DamageableState.Alive) toCheck.Add(p.transform);
        }

        for (int i = 0; i < _coOpPlayers.Length; i++)
        {
            var p = _coOpPlayers[i];
            if (p && p.targetable && p.state == DamageableState.Alive) { toCheck.Add(p.transform); }
        }

        for (int i = 0; i < toCheck.Count; i++)
        {
            player = toCheck[i];
            var pPosition = player.position;
            var pAngle = Mathf.Abs(Quaternion.FromToRotation(direction, (pPosition - position).normalized).eulerAngles.z);
            if (pAngle > 180)
            {
                pAngle = 360 - pAngle;
            }

            var pDistance = Vector3.Distance(pPosition, position);
            if (pDistance < lowestDistance && pAngle < angle * 0.5f)
            {
                lowestDistance = pDistance;
                closest = player;
            }
        }

        return closest;
    }

    public Transform GetClosestPlayerInRange(Transform origin, float range, bool inFrontOnly)
    {
        var bestDistance = float.PositiveInfinity;
        Transform closest = null;

        foreach (var p in players)
        {
            if (p.targetable && Vector3.Distance(p.transform.position, origin.position) <= range &&
            (!inFrontOnly || Mathf.Sign(p.transform.position.x - origin.position.x) == Mathf.Sign(origin.right.x)))
            {
                var d = Vector3.Distance(p.transform.position, origin.position);
                if (d < bestDistance)
                {
                    closest = p.transform;
                    bestDistance = d;
                }
            }
        }

        foreach (var p in _coOpPlayers)
        {
            if (!p) continue;

            if (!p.targetable && Vector3.Distance(p.transform.position, origin.position) <= range &&
            (!inFrontOnly || Mathf.Sign(p.transform.position.x - origin.position.x) == Mathf.Sign(origin.right.x)))
            {
                var d = Vector3.Distance(p.transform.position, origin.position);
                if (d < bestDistance)
                {
                    closest = p.transform;
                    bestDistance = d;
                }
            }
        }

        return closest;
    }

    public Player GetClosestPlayer(Vector3 point)
    {
        Player closestPlayer = null;
        var bestDistance = float.PositiveInfinity;

        foreach (var player in _players)
        {
            if (player && player.targetable)
            {
                var distance = Vector3.SqrMagnitude(player.transform.position - point);
                if (distance < bestDistance)
                {
                    closestPlayer = player;
                    bestDistance = Vector3.SqrMagnitude(player.transform.position - point);
                }
            }
        }

        return closestPlayer;
    }

    public Transform GetClosestPlayerTransform(Vector3 point, Func<Player, bool> validate = null)
    {
        Transform closestPlayer = null;
        var bestDistance = float.PositiveInfinity;

        foreach (var player in _players)
        {
            if(validate != null && !validate(player)) { continue; }

            if (player && player.targetable)
            {
                var distance = Vector3.SqrMagnitude(player.transform.position - point);
                if (distance < bestDistance)
                {
                    closestPlayer = player.transform;
                    bestDistance = Vector3.SqrMagnitude(player.transform.position - point);
                }
            }
        }

        for (int i = 0; i < _coOpPlayers.Length; i++)
        {
            if (_coOpPlayers[i] && _coOpPlayers[i].targetable)
            {
                var distance = Vector3.SqrMagnitude(_coOpPlayers[i].transform.position - point);
                if (distance < bestDistance)
                {
                    closestPlayer = _coOpPlayers[i].transform;
                    bestDistance = distance;
                }
            }
        }

        return closestPlayer;
    }

    public IDamageable GetClosestPlayerDamageable(Vector3 point)
    {
        var closest = GetClosestPlayerTransform(point);
        return closest ? closest.gameObject.GetComponent<IDamageable>() : null;
    }
    
    public static bool CanTarget(IDamageable target)
    {
        return target != null && !target.Equals(null) && target.targetable;
    }

    public void CalculateCoOpDamageMod()
    {
        var active = 0f;
        for (int i = 0; i < coOpPlayers.Length; i++)
        {
            var p = _coOpPlayers[i];
            if (p && p.state == DamageableState.Alive && p.gameObject.activeInHierarchy)
            {
                active++;
            }
        }

        if (active <= 1)
        {
            coOpMod = 1;
        }
        else if (active == 2)
        {
            coOpMod = 0.83f;
        }
        else
        {
            coOpMod = 0.66f;
        }
    }

    public bool IntersectsAnyPlayerBounds(Bounds bounds)
    {
        foreach (var p in _players)
        {
            if (bounds.Intersects(p.controller2D.collider2D.bounds))
            {
                return true;
            }
        }

        foreach (var p in _coOpPlayers)
        {
            if (p && bounds.Intersects(p.collider2D.bounds))
            {
                return true;
            }
        }

        return false;
    }

    public void PauseAllPlayers()
    {
        for (int i = 0; i < players.Count; i++)
        {
            if (players[i]) { players[i].Pause(); }
        }

        for (int i = 0; i < _coOpPlayers.Length; i++)
        {
            if(_coOpPlayers[i]) { _coOpPlayers[i].Pause(); }
        }        
    }

    public void UnpauseAllPlayers()
    {        
        for (int i = 0; i < players.Count; i++)
        {
            if (players[i]) { players[i].Unpause(); }
        }

        for (int i = 0; i < _coOpPlayers.Length; i++)
        {
            if (_coOpPlayers[i]) { _coOpPlayers[i].Unpause(); }
        }        
    }

    private void OnDestroy()
    {
        if (instance = this) { instance = null; }
    }

    public void AddPlayer(Player player)
    {
        if (!_players.Contains(player)) { _players.Add(player); }
    }

    public void RemovePlayer(Player player)
    {
        _players.Remove(player);
    }

    public void ItemCollected(MajorItem item)
    {
        itemsCollected.Add(item);

        if (AchievementManager.instance)
        {
            switch (item)
            {
                case MajorItem.TheRedKey: //To Dare
                    AchievementManager.instance.WaitTryEarnAchievement(1, AchievementID.GlitchShell);
                    break;
                case MajorItem.TheBlueKey: //To Know
                    AchievementManager.instance.WaitTryEarnAchievement(1, AchievementID.GlitchMap);
                    break;
                case MajorItem.TheGreenKey: //To Want
                    AchievementManager.instance.WaitTryEarnAchievement(1, AchievementID.ModuleTransmogrifier);
                    break;
                case MajorItem.TheBlackKey: //To Be Silent
                    AchievementManager.instance.WaitTryEarnAchievement(1, AchievementID.TheThief);
                    break;
            }

            if (itemsCollected.ContainsAll(new MajorItem[] { MajorItem.TheRedKey, MajorItem.TheGreenKey, MajorItem.TheBlueKey, MajorItem.TheBlackKey }))
            {
                AchievementManager.instance.WaitTryEarnAchievement(1, AchievementID.TheGlitchedKey);
            }
        }

        var activeGame = SaveGameManager.activeGame;
        if (activeGame != null)
        {
            activeGame.itemsCollected = itemsCollected.ToList();
            SaveGameManager.activeSlot.itemsCollected.UnionWith(itemsCollected);
            SaveGameManager.instance.Save();
        }
    }
}
