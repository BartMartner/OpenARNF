using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using System.Linq;
using CreativeSpore.SuperTilemapEditor;

public class RoomTransitionTrigger : MonoBehaviour
{
    public bool skipAssigningDoorDamageType;
    public Direction direction;
    public Int2D localGridPosition;

    public string parentScene
    {
        get
        {
            return transform.root.name.ToLowerInvariant();
        }
    }

    [HideInInspector]
    public Int2D parentGridPosition;
    [HideInInspector]
    public Int2D targetGridPosition;

    public TraversalLimitations requiredExitLimitations;

    public Vector3 exitPoint
    {
        get
        {
            switch(direction)
            {
                case Direction.Up:
                    return transform.position + Vector3.down * 2.25f;
                case Direction.Down:
                    return transform.position + Vector3.up * 2.25f;                    
                case Direction.Left:
                    return transform.position + Vector3.right * 1.5f;
                case Direction.Right:
                    return transform.position + Vector3.left * 1.5f;
                default:
                    return transform.position;
            }
        }
    }

    [HideInInspector]
    public bool ready;
    public BoxCollider2D trigger;
    public Door door;

    public void Awake()
    {
        trigger = GetComponent<BoxCollider2D>();
    }

    public void OnTriggerEnter2D(Collider2D collision)
    {
        if ((!ready && collision.gameObject != Player.instance.gameObject) ||
            (door && door.isClosed))
        {
            return;
        }

        if (LayoutManager.instance)
        {
            var player = collision.GetComponent<Player>();
            if (player) { LayoutManager.instance.StartTransition(this, player); }
        }        
    }

    //Consider removing these variables and just having the exit abstract? Check if exit abstract is null instead of checking ready?
    public bool AssignExitAbstract(ExitAbstract exitAbstract)
    {
        targetGridPosition = exitAbstract.TargetPosition();
        parentGridPosition = exitAbstract.globalGridPosition;
        localGridPosition = exitAbstract.localGridPosition;

        var damageType = exitAbstract.toExit.requiredDamageType;

        if(damageType.GetFlags().Count() > 1)
        {
            Debug.LogError("Exit Abstract " + exitAbstract.globalGridPosition.ToString() + " has multiple supported damage types!");
            return false;
        }

        var activeGame = SaveGameManager.activeGame;
        if (door)
        {
            door.id = exitAbstract.id;
            if (!skipAssigningDoorDamageType && damageType != 0 && !activeGame.doorsOpened.Contains(door.id))
            {
                door.SetImmunitiesAndColor(damageType);
            }

            if(activeGame.gameMode == GameMode.Exterminator)
            {
                StartCoroutine(SetupForExterminator(exitAbstract));
            }
            else if (activeGame.gameMode == GameMode.BossRush)
            {
                SetupForBossRush(exitAbstract);
            }

            if(exitAbstract.toConfusionRoom)
            {
                door.toConfusionRoom = true;
                door.SetupDoor();
            }

            if(exitAbstract.toHeatRoom)
            {
                door.toHeatRoom = true;
                door.SetupDoor();
            }

            if(exitAbstract.toBossRoom)
            {
                door.toBossRoom = true;
                door.SetupDoor();
            }

            if(exitAbstract.toWaterRoom)
            {
                door.toWaterRoom = true;
                door.SetupDoor();
            }
        }
        ready = true;

        return ready;
    }

    public void SetupForBossRush(ExitAbstract exitAbstract)
    {
        var activeGame = SaveGameManager.activeGame;
        if (activeGame == null) return;

        var currentRoomInfo = LayoutManager.instance != null ? LayoutManager.instance.currentRoom.roomInfo : null;
        if (currentRoomInfo == null) return;

        var nextRoom = activeGame.layout.GetRoomAtPositon(exitAbstract.TargetPosition());
        if (nextRoom == null) return;

        bool toFinal = nextRoom.assignedRoomInfo.environmentType == EnvironmentType.BuriedCity &&
          nextRoom.assignedRoomInfo.sceneName.Contains("BeastRemnant");

        if (!toFinal && nextRoom.assignedRoomInfo.environmentType == currentRoomInfo.environmentType) { return; }

        foreach (var e in activeGame.layout.environmentOrder)
        {
            if (e == currentRoomInfo.environmentType) break; //current room comes first
            if (e == nextRoom.assignedRoomInfo.environmentType) return; //next room env comes first, doesn't need lock
        }
        
        HashSet<BossName> bosses;
        if (!Constants.envBosses.TryGetValue(currentRoomInfo.environmentType, out bosses)) return;

        bool needLock = false;
        foreach (var boss in bosses)
        {
            if (!activeGame.layout.bossesAdded.Contains(boss)) { continue; }
            if (!activeGame.layout.roomAbstracts.Any(r => r.assignedRoomInfo.environmentType == currentRoomInfo.environmentType && r.assignedRoomInfo.boss == boss)) { continue; }
            if (!activeGame.bossesDefeated.Contains(boss))
            {
                needLock = true;
                break;
            }
        }

        if (needLock) { door.HardLock(0); }
    }

    public IEnumerator SetupForExterminator(ExitAbstract exitAbstract)
    {
        while(!PlayerManager.instance || !PlayerManager.instance.player1.started)
        {
            yield return null;
        }

        var activeGame = SaveGameManager.activeGame;
        var room = activeGame.layout.GetRoomAtPositon(exitAbstract.TargetPosition());

        //Safety Check
        if (activeGame.roomsVisited.Contains(room.roomID) || room.majorItem != MajorItem.None ||
            room.assignedRoomInfo.environmentType == EnvironmentType.Glitch ||
            room.assignedRoomInfo.environmentType == EnvironmentType.BeastGuts)
        {
            yield break;
        }

        int eci;
        int playerCap = PlayerManager.instance.player1.GetPlayerCapabilitiesIndex();
        if(GetExterminatorIndex(exitAbstract, playerCap, out eci))
        {
            door.HardLock(eci);
        }
    }

    public static bool GetExterminatorIndex(ExitAbstract exitAbstract, int capabilitiesIndex, out int eci)
    {
        var activeGame = SaveGameManager.activeGame;
        var room = activeGame.layout.GetRoomAtPositon(exitAbstract.TargetPosition());

        eci = room.expectedCapabilitiesIndex;
        bool result = false;

        //Does the room expect capabilities beyond what the player possesses
        if (eci > capabilitiesIndex) { result = true; }

        //does the connected exit require abilities the player hasn't acquired yet (slide or phase)?
        var capabilities = activeGame.layout.traversalCapabilities[capabilitiesIndex];
        var toExit = activeGame.layout.GetConnectedExit(exitAbstract).toExit;
        if (!toExit.CapabilitesSufficient(capabilities))
        {
            var ni = activeGame.layout.GetIndexOfFirstSuitableCapabilities(toExit);
            if(ni > eci) { eci = ni; }            
            result = true;
        }

        //does the room contain a traversal path that can't be traversed?
        for (int i = 0; i < room.traversalPathRequirements.Count; i++)
        {
            var req = room.traversalPathRequirements[i];
            if (!req.CapabilitesSufficient(capabilities))
            {
                var ni = activeGame.layout.GetIndexOfFirstSuitableCapabilities(req);
                if (ni > eci) { eci = ni; }
                result = true;
            }
        }

        //does the room have an environment effect the player can't navigate
        if (room.environmentalEffect.RequiresTraversalAbility() && !capabilities.environmentalResistance.HasFlag(room.environmentalEffect))
        {
            result = true;
        }

        return result;
    }
}
