using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;
using CreativeSpore.SuperTilemapEditor;
using System.Collections.Generic;
using System.Linq;
using System;
using Random = UnityEngine.Random;

public class LayoutManager : MonoBehaviour
{
    public static LayoutManager instance;

    public int itemCount;
    public RoomLayout layout;
    public Int2D startingRoomPosition;
    public Vector3 playerStart;
    public Direction playerFacing;

    public Action onRoomExited;
    public Action onRoomLoaded;
    public Action onTransitionComplete;

    public AudioClip glitchTransition;

    public EnvironmentType currentEnvironment;

    private bool _transitioning;
    public bool transitioning
    {
        get { return _transitioning; }
    }

    private Room _awaitedRoom;
    private bool _newGame;
    private bool _noRooms = true;
    private int _loadCounter;
    private Player _player;

    private Room _currentRoom;
    public Room currentRoom
    {
        get { return _currentRoom; }
    }

    public static Room CurrentRoom
    {
        get { return instance ? instance.currentRoom : null; }
    }

    public void Awake()
    {
        if (instance)
        {
            Destroy(gameObject);
            return;
        }

        instance = this;

        var activeGame = SaveGameManager.activeGame;
        if (activeGame != null)
        {
            layout = activeGame.layout;

            Debug.Log("Started game with seed: " + activeGame.seed);

            if (!activeGame.started)
            {
                var startingRoom = layout.GetRoomAtPositon(layout.startingPosition);

                switch(activeGame.layout.gameMode)
                {
                    case GameMode.ClassicBossRush:
                    case GameMode.BossRush:
                        activeGame.playerHealth = Constants.startingHealth;
                        break;
                    default:
                        _newGame = true;
                        break;
                }
                
                activeGame.started = true;
                activeGame.playerStart = startingRoom.worldPosition + startingRoom.assignedRoomInfo.playerStartOffset;
                playerFacing = startingRoom.assignedRoomInfo.playerStartFacing;
                activeGame.lastRoom = layout.startingPosition;
                SaveGameManager.instance.Save(false, true);
            }

            playerStart = activeGame.playerStart;
            startingRoomPosition = activeGame.lastRoom;
        }
        else
        {
            Debug.LogError("Error: SaveGameManager.instance.activeGame is null!");
            return;
        }
    }

    public IEnumerator Start()
    {
        StartCoroutine(LoadRoomAtPosition(startingRoomPosition, !_newGame));

        _player = PlayerManager.instance.player1;
        _player.transform.position = playerStart;

        if(playerFacing == Direction.Left)
        {
            _player.StartCoroutine(_player.ChangeFacing(Direction.Left));
        }

        _player.enabled = false;

        //Start won't get called on Player, so we have to pull this out here.
        var playerSpriteRenderer = _player.GetComponent<SpriteRenderer>();
        if(playerSpriteRenderer)
        {
            playerSpriteRenderer.enabled = false;
        }

        var originalCullMask = TransitionFade.instance.camera.cullingMask;
        TransitionFade.instance.camera.cullingMask = 0;

        if (_newGame)
        {
            var activeGame = SaveGameManager.activeGame;
            if (activeGame != null && activeGame.gameMode == GameMode.Normal)
            {
                _player.fatigued = true;
            }

            var envType = layout.environmentOrder[0];

            if (envType == EnvironmentType.Surface)
            {
                MusicController.instance.SurfaceStart();
            }
            else if (envType == EnvironmentType.ForestSlums)
            {
                MusicController.instance.ForestSlumsStart();
            }
        }

        var cameraStart = Constants.LayoutToWorldPosition(Constants.WorldToLayoutPosition(playerStart));
        MainCamera.instance.SetLimits(new Bounds(cameraStart, Constants.roomSize), false); // prevents the camera shooting back to centered on player after its snapped;
        MainCamera.instance.SnapCamera(cameraStart);

        while (_noRooms) //wait until currentRoom is set
        {
            yield return null;            
        }

        GC.Collect();

        _player.enabled = true;
        if (playerSpriteRenderer) { playerSpriteRenderer.enabled = true; }

        while (TransitionFade.instance.enabled) { yield return null; }

        TransitionFade.instance.camera.cullingMask = originalCullMask;

        if(_newGame)
        {
            var activeGame = SaveGameManager.activeGame;
            if (activeGame != null && activeGame.layout.startingWeapon != MajorItem.None)
            {
                var wheel = FindObjectOfType<WeaponWheel>();
                if (wheel) { wheel.ShowWheel(0, 2); }
            }
        }
    }

    public void OnDestroy()
    {
        if (instance == this) { instance = null; }
    }

    public bool AllShopsOutOfStock()
    {
        return !(layout.roomAbstracts.Any((r) => r.shopOfferings != null && r.shopOfferings.Count > 0 && r.shopOfferings.Any((i) => !PlayerManager.instance.itemsCollected.Contains(i))));
    }

    public int ShopsOutOfStock()
    {
        int shops = 0;
        foreach (var r in layout.roomAbstracts)
        {
            if(r.shopOfferings != null && r.shopOfferings.Count > 0 && !r.shopOfferings.Any((i) => !PlayerManager.instance.itemsCollected.Contains(i)))
            {
                shops++;
            }
        }

        return shops;
    }

    public void RegisterRoom(Room room) { _awaitedRoom = room; }

    public IEnumerator LoadRoomAtPosition(Int2D position, bool triggerMusic = true)
    {
        _awaitedRoom = null;
        //TODO: assign scene names to RoomAbstracts;
        var roomAbstract = layout.GetRoomAtPositon(position);
        
        if(roomAbstract == null || !Application.CanStreamedLevelBeLoaded(roomAbstract.assignedRoomInfo.sceneName))
        {
            GlitchToEnvironmentStart(EnvironmentType.Glitch);
            yield break;
        }

        SceneManager.LoadScene(roomAbstract.assignedRoomInfo.sceneName, LoadSceneMode.Additive);
        while(!_awaitedRoom)
        {
            yield return null;
        }

        _currentRoom = _awaitedRoom;
        _awaitedRoom.AssignAbstract(roomAbstract);
        currentEnvironment = _awaitedRoom.roomInfo.environmentType;
        if (triggerMusic)
        {
            MusicController.instance.SetMusicFromRoom(roomAbstract);
        }

        _noRooms = false;
        _awaitedRoom = null;
    }

    public void StartTransition(RoomTransitionTrigger enteredTrigger, Player player)
    {
        if(_transitioning)
        {
            Debug.LogWarning(enteredTrigger.name + " tried to start a transition while already LayoutManager was already transitioning!");
            return;
        }
        StartCoroutine(Transition(enteredTrigger, player));
    }

    public void RespawnInCurrentRoom()
    {
        StartCoroutine(LoadCurrentRoomAndRespawnThere());
    }

    public void RespawnAtLastSaveRoom()
    {
        if (SaveGameManager.activeGame != null)
        {
            var saveRoomPos = SaveGameManager.activeGame.activeSaveRoomPositions.Last();         
            StartCoroutine(LoadSaveRoomAndRespawnThere(saveRoomPos));
        }
    }

    public void GlitchToEnvironmentStart(EnvironmentType envType)
    {
        var room = layout.GetEnvironmentStart(envType);

        AudioManager.instance.PlayOneShot(glitchTransition);

        var activeGame = SaveGameManager.activeGame;
        if (activeGame != null && activeGame.activeSaveRoomPositions.Count > 0)
        {
            activeGame.destroyedSaveRoomPositions.AddRange(activeGame.activeSaveRoomPositions);
            activeGame.activeSaveRoomPositions.Clear();
        }

        StartCoroutine(TeleportToRoom(room.gridPosition, TransitionFadeType.Glitch));
    }

    public void TeleportToBeastGuts()
    {
        var room = layout.GetEnvironmentStart(EnvironmentType.BeastGuts);

        var activeGame = SaveGameManager.activeGame;
        if (activeGame != null && activeGame.activeSaveRoomPositions.Count > 0)
        {
            activeGame.destroyedSaveRoomPositions.AddRange(activeGame.activeSaveRoomPositions);
            activeGame.activeSaveRoomPositions.Clear();
        }

        StartCoroutine(TeleportToRoom(room.gridPosition, TransitionFadeType.BeastGuts));
    }
    
    public void TeleportToPosition(Int2D position, TransitionFadeType fadeType = TransitionFadeType.Normal)
    {
        StartCoroutine(TeleportToRoom(position, fadeType));
    }

    private IEnumerator TeleportToRoom(Int2D roomPos, TransitionFadeType fadeType)
    {
        _transitioning = true;

        var players = PlayerManager.instance.players;
        var parentScene = FindObjectOfType<Room>().name;

        var fadeTime = Constants.transitionFadeTime;
        TransitionFade.instance.SetCullingMask(LayerMask.GetMask());

        switch(fadeType)
        {
            case TransitionFadeType.Glitch:
                fadeTime = 4;
                MainCamera.instance.FadeGlitchIn(fadeTime, 1, 1, 1, 1, 1);
                break;
            case TransitionFadeType.BeastGuts:
                fadeTime = 2;
                TransitionFade.instance.FadeOut(fadeTime, Color.red);
                break;
            case TransitionFadeType.Teleporter:
                fadeTime = 1;
                MainCamera.instance.FadeGlitchIn(fadeTime, 0f,0.1f,0.1f,0.25f,0.45f);
                TransitionFade.instance.FadeOut(fadeTime, Constants.blasterGreen);
                break;
            default:
                TransitionFade.instance.FadeOut(fadeTime, Color.black);
                break;
        }

        foreach (var player in players)
        {
            player.controller2D.enabled = false;
            player.enabled = false;
        }        

        yield return new WaitForSeconds(fadeTime);

        //Load Room at position

        _awaitedRoom = null;
        var roomAbstract = layout.GetRoomAtPositon(roomPos);

        SceneManager.LoadScene(roomAbstract.assignedRoomInfo.sceneName, LoadSceneMode.Additive);

        while (!_awaitedRoom) { yield return null; }

        _currentRoom = _awaitedRoom;
        _awaitedRoom.AssignAbstract(roomAbstract);
        currentEnvironment = _awaitedRoom.roomInfo.environmentType;

        foreach (var player in players)
        {
            player.controller2D.enabled = false;
            player.enabled = false;
        }

        MusicController.instance.SetMusicFromRoom(roomAbstract);

        var exitPoint = _awaitedRoom.roomAbstract.worldPosition + _awaitedRoom.roomAbstract.assignedRoomInfo.playerStartOffset;
        var lastRoomPosition = _awaitedRoom.roomAbstract.gridPosition;
        var roomEnv = _awaitedRoom.roomAbstract.assignedRoomInfo.environmentType;
        _awaitedRoom = null;
        //End Load Room At Position

        var scene = SceneManager.GetSceneByName("Core");
        SceneManager.SetActiveScene(scene);

        yield return SceneManager.UnloadSceneAsync(parentScene);
        ManageResources();

        switch (fadeType)
        {
            case TransitionFadeType.Glitch:
                MainCamera.instance.FadeGlitchOut(fadeTime);
                TransitionFade.instance.ResetCullingMask();
                break;
            case TransitionFadeType.BeastGuts:
                TransitionFade.instance.FadeIn(fadeTime, Color.red, true);
                break;
            case TransitionFadeType.Teleporter:
                MainCamera.instance.FadeGlitchOut(fadeTime);
                TransitionFade.instance.FadeIn(fadeTime, Constants.blasterGreen, true);
                break;
            default:
                TransitionFade.instance.FadeIn(Constants.transitionFadeTime, Color.black, true);
                break;
        }

        var game = SaveGameManager.activeGame;
        if (game != null)
        {
            game.lastRoom = lastRoomPosition;
            game.playerStart = exitPoint;

            if (game.allowAchievements)
            {
                var environmentsDiscovered = SaveGameManager.activeSlot.environmentsDiscovered;
                if (!environmentsDiscovered.Contains(roomEnv))
                {
                    environmentsDiscovered.Add(roomEnv);
                    AchievementManager.instance.EnvironmentDiscovered(roomEnv);
                }
            }

            SaveGameManager.instance.Save();
        }
        else
        {
            Debug.LogWarning("SaveGameManager.instance.activeGame is null. Progress not saved");
        }

        foreach (var player in players)
        {
            player.transform.position = exitPoint;
            player.SetLastSafePosition();
            player.enabled = true;
            player.Respawn();
        }
        
        _transitioning = false;
    }

    public IEnumerator LoadSaveRoomAndRespawnThere(Int2D saveRoomPos)
    {
        _transitioning = true;
        _awaitedRoom = null;

        var parentScene = FindObjectOfType<Room>().name;
        var players = PlayerManager.instance.players;

        foreach (var player in players)
        {
            player.controller2D.enabled = false;
            player.enabled = false;
        }
        
        TransitionFade.instance.FadeOut(Constants.transitionFadeTime);
        yield return new WaitForSeconds(Constants.transitionFadeTime);

        //Load Room at position
        _awaitedRoom = null;
        var roomAbstract = layout.GetRoomAtPositon(saveRoomPos);

        SceneManager.LoadScene(roomAbstract.assignedRoomInfo.sceneName, LoadSceneMode.Additive);

        while (!_awaitedRoom)
        {
            yield return null;
        }

        _currentRoom = _awaitedRoom;
        _awaitedRoom.AssignAbstract(roomAbstract);
        currentEnvironment = _awaitedRoom.roomInfo.environmentType;

        var save = _awaitedRoom.GetComponentInChildren<SaveStation>();

        MusicController.instance.SetMusicFromRoom(roomAbstract);

        _awaitedRoom = null;
        //End Load Room At Position

        while (!save.ready)
        {
            yield return null;
        }

        var scene = SceneManager.GetSceneByName("Core");
        SceneManager.SetActiveScene(scene);

        if (onRoomLoaded != null)
        {
            onRoomLoaded();
        }

        yield return SceneManager.UnloadSceneAsync(parentScene);
        ManageResources();

        var activeGame = SaveGameManager.activeGame;
        if (activeGame != null)
        {
            activeGame.activeSaveRoomPositions.Remove(saveRoomPos);
            activeGame.destroyedSaveRoomPositions.Add(saveRoomPos);
            activeGame.lastRoom = saveRoomPos;
            activeGame.playerStart = save.playerPosition.position;
            SaveGameManager.instance.Save(false, true);
        }

        TransitionFade.instance.FadeIn(Constants.transitionFadeTime);

        foreach (var player in players)
        {
            save.RespawnPlayer(player);
        }

        _transitioning = false;
    }

    public IEnumerator LoadCurrentRoomAndRespawnThere()
    {
        if (SaveGameManager.activeGame == null) { yield break; }

        var players = PlayerManager.instance.players;

        var saveRoomPos = SaveGameManager.activeGame.lastRoom;

        _transitioning = true;
        _awaitedRoom = null;

        var parentScene = FindObjectOfType<Room>().name;

        foreach (var player in players)
        {
            player.controller2D.enabled = false;
            player.enabled = false;
        }

        TransitionFade.instance.FadeOut(Constants.transitionFadeTime);
        yield return new WaitForSeconds(Constants.transitionFadeTime);

        //Load Room at position
        _awaitedRoom = null;
        var roomAbstract = layout.GetRoomAtPositon(saveRoomPos);

        SceneManager.LoadScene(roomAbstract.assignedRoomInfo.sceneName, LoadSceneMode.Additive);

        while (!_awaitedRoom) { yield return null; }

        _currentRoom = _awaitedRoom;
        _awaitedRoom.AssignAbstract(roomAbstract);
        currentEnvironment = _awaitedRoom.roomInfo.environmentType;

        MusicController.instance.SetMusicFromRoom(roomAbstract);

        _awaitedRoom = null;
        //End Load Room At Position

        
        yield return null;        

        var scene = SceneManager.GetSceneByName("Core");
        SceneManager.SetActiveScene(scene);

        if (onRoomLoaded != null) { onRoomLoaded(); }

        yield return SceneManager.UnloadSceneAsync(parentScene);
        ManageResources();

        foreach (var player in players)
        {
            player.transform.position = SaveGameManager.activeGame.playerStart;
            player.enabled = true;
            player.Respawn();
            player.fatigued = false;
        }

        TransitionFade.instance.FadeIn(Constants.transitionFadeTime);

        _transitioning = false;
    }

    private IEnumerator Transition(RoomTransitionTrigger enteredTrigger, Player player)
    {
        _transitioning = true;

        var previousEnv = _currentRoom.roomAbstract.assignedRoomInfo.environmentType;
        var previousEnvFx = _currentRoom.roomAbstract.environmentalEffect;

        if (onRoomExited != null)
        {
            onRoomExited();
        }

        var players = PlayerManager.instance.players;

        foreach (var p in players)
        {
            p.controller2D.enabled = false;
            p.enabled = false;
        }
        enteredTrigger.trigger.enabled = false;

        TransitionFade.instance.FadeOut(Constants.transitionFadeTime);
        yield return new WaitForSeconds(Constants.transitionFadeTime);
        yield return StartCoroutine(LoadRoomAtPosition(enteredTrigger.targetGridPosition));

        var scene = SceneManager.GetSceneByName("Core");
        SceneManager.SetActiveScene(scene);

        if (onRoomLoaded != null)
        {
            onRoomLoaded();
        }

        yield return null;

        var doorTriggers = FindObjectsOfType<RoomTransitionTrigger>();
        var validTriggers = doorTriggers.Where(t => t.direction == enteredTrigger.direction.Opposite() && t.parentGridPosition == enteredTrigger.targetGridPosition);
        if (validTriggers.Count() <= 0)
        {
            var message = "DoorTransitionTrigger " + gameObject.name + " at " + transform.position + " could find no valid connected triggers";
            if(currentRoom) { message += " in room " + _currentRoom.name + "."; }
            throw new Exception(message);
        }

        var connectedTrigger = validTriggers.First();

        connectedTrigger.StopAllCoroutines();
        connectedTrigger.trigger.enabled = false;

        if (connectedTrigger.door)
        {
            connectedTrigger.door.Open(true);
        }

        Vector3 exitPoint = connectedTrigger.exitPoint;

        if (enteredTrigger.direction.isHorizontal())
        {
            exitPoint.y = player.transform.position.y;
        }
        else
        {
            exitPoint.x = player.transform.position.x;
        }


        foreach (var p in players)
        {
            p.transform.position = exitPoint;
            p.SetLastSafePosition();
        }

        //wait for camera bounds to be hit
        var frames = 0; ///FIX THIS! (ZOOMING BREAKS IT!!!)
        while (!MainCamera.instance.tweening && frames < 60) //trying to give this up to a second to catch the camera tweening
        {
            frames++;
            yield return null;
        }

        Time.timeScale = 0;

        //wait for camera bounds to finish
        while (MainCamera.instance.tweening)
        {
            yield return null;
        }

        if (onTransitionComplete != null) { onTransitionComplete(); }
        TransitionFade.instance.FadeIn(Constants.transitionFadeTime);
        Time.timeScale = 1;

        var activeGame = SaveGameManager.activeGame;
        if (activeGame != null)
        {
            activeGame.lastRoom = enteredTrigger.targetGridPosition;
            activeGame.playerStart = exitPoint;

            if (activeGame.allowAchievements)
            {
                var environment = layout.GetRoomAtPositon(enteredTrigger.targetGridPosition).assignedRoomInfo.environmentType;
                var slot = SaveGameManager.activeSlot;
                if (!slot.environmentsDiscovered.Contains(environment))
                {
                    slot.environmentsDiscovered.Add(environment);
                    AchievementManager.instance.EnvironmentDiscovered(environment);
                }

                if(!slot.achievements.Contains(AchievementID.WallJump))
                {
                    player.UpdateTraversalCapabilites();
                    var current = player.traversalCapabilities;
                    if (CheckForWallJump(_currentRoom, current, previousEnvFx, previousEnv))
                    {
                        PlayerManager.instance.allowWallJump = true;
                        AchievementManager.instance.TryEarnAchievement(AchievementID.WallJump);
                    }
                }
            }

            SaveGameManager.instance.Save();
        }
        else
        {
            Debug.LogWarning("SaveGameManager.instance.activeGame is null. Progress not saved");
        }

        if (connectedTrigger.door) { connectedTrigger.door.Close(); }

        yield return SceneManager.UnloadSceneAsync(enteredTrigger.parentScene);
        ManageResources();

        foreach (var p in players)
        {
            p.enabled = true;
            p.controller2D.enabled = true;
        }
        connectedTrigger.trigger.enabled = true;

        _transitioning = false;
    }

    public bool CheckForWallJump(Room room, TraversalCapabilities current, EnvironmentalEffect previousEnvFX, EnvironmentType previousEnv)
    {
        if(room.roomAbstract.assignedRoomInfo.environmentType == EnvironmentType.Glitch) { return false; }

        if (room.roomAbstract.environmentalEffect.RequiresTraversalAbility() &&
            room.roomAbstract.environmentalEffect != previousEnvFX) { return false; }

        var expected = room.roomAbstract.expectedCapabilities;
        if (current.effectiveJumpHeight < (expected.effectiveJumpHeight - 0.2f) ||
            (!current.shotIgnoresTerrain && expected.shotIgnoresTerrain) ||
            (!current.canTraverseGroundedSmallGaps && expected.canTraverseGroundedSmallGaps) ||
            (!current.canPhaseThroughWalls && expected.canPhaseThroughWalls))
        {
            return true;
        }

        if (expected.damageTypes != 0)
        {
            var dTypes = expected.damageTypes.GetFlags();
            foreach (var d in dTypes)
            {
                if (d == DamageType.Generic) continue;
                if (!current.damageTypes.HasFlag(d)) return true;
            }
        }

        if (expected.environmentalResistance != EnvironmentalEffect.None &&
            !current.environmentalResistance.HasFlag(expected.environmentalResistance) &&
            (room.roomAbstract.assignedRoomInfo.environmentType != previousEnv ||
            room.roomAbstract.assignedRoomInfo.roomType == RoomType.BossRoom ||
            room.roomAbstract.assignedRoomInfo.roomType == RoomType.ItemRoom))
        {
            return true;
        }

        return false;
    }

    public void ManageResources()
    {
        _loadCounter++;
        if (_loadCounter >= 15)
        {
            _loadCounter = 0;
            Resources.UnloadUnusedAssets();
            Debug.Log("Unloading Unused Assets!");
        }
        else
        {
            GC.Collect();
        }
    }
}
