using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SaveStation : MonoBehaviour, IAbstractDependantObject, IRepairable
{
    public int priority;
    public int m_priority
    {
        get { return priority; }
        set { priority = value; }
    }

    private Int2D _gridPosition;
    public Int2D gridPosition
    {
        get { return _gridPosition; }
    }

    private bool _ready;
    public bool ready
    {
        get { return _ready; }
    }

    public bool unusable;
    public Transform playerPosition;

    private Animator _animator;
    private BoxCollider2D _trigger;
    private bool _inCoroutine;

    public void Awake()
    {
        _animator = GetComponent<Animator>();
        _trigger = GetComponent<BoxCollider2D>();
    }

#if UNITY_EDITOR
    public IEnumerator Start()
    {
        yield return new WaitForSeconds(0.1f);
        if(!_ready && !FindObjectOfType<LayoutManager>())
        {
            _ready = true;
        }
    }
#endif

    public void RespawnPlayer(Player player)
    {
        StartCoroutine(RespawnAndDestroy(player));
    }

    public IEnumerator RespawnAndDestroy(Player player)
    {
        _inCoroutine = true;
        var originalParent = player.transform.parent;
        var originalScene = player.gameObject.scene;

        _animator.Play("Respawn");
        player.transform.parent = playerPosition;
        player.transform.localPosition = Vector3.zero;
        player.enabled = false;
        player.mainRenderer.enabled = false;

        yield return new WaitForSeconds(3);
        EnemyManager.instance.DestroyAllEnemies();

        player.transform.parent = originalParent;
        SceneManager.MoveGameObjectToScene(player.gameObject, originalScene);

        player.enabled = true;
        player.Respawn();
        player.SetLastSafePosition();
        _inCoroutine = false;
    }

    public void CompareWithAbstract(RoomAbstract roomAbstract)
    {
        if(roomAbstract.assignedRoomInfo.roomType != RoomType.SaveRoom)
        {
            Debug.Log("Error! Save Station is in a room that doesn't have RoomType.SaveRoom");
            Destroy(gameObject);
        }

        _gridPosition = roomAbstract.gridPosition;

        var activeGame = SaveGameManager.activeGame;
        if (activeGame != null)
        {
            if (activeGame.activeSaveRoomPositions.Contains(_gridPosition))
            {
                unusable = true;
                _animator.Play("Activated");
            }

            if (activeGame.destroyedSaveRoomPositions.Contains(_gridPosition))
            {
                unusable = true;
                _animator.Play("Destroyed");
            }
        }

        //Set to destroyed state, active state, loading state etc.
        
        _trigger.enabled = !unusable;
        _ready = true;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        var player = collision.GetComponent<Player>();
        if (player && _ready && !unusable)
        {
            var slide = player.activeSpecialMove as PlayerSlide;
            if (LayoutManager.instance && (player.activeSpecialMove is PlayerDash || player.activeSpecialMove is PlayerBuzzsawShell || (slide && slide.playerDash)))
            {
                player.activeSpecialMove.DeathStop();
                Debug.Log("Dashed into save station");
                LayoutManager.instance.GlitchToEnvironmentStart(EnvironmentType.Glitch);
            }
            else
            {
                if (slide) { slide.DeathStop(); }
                StartCoroutine(Save(player));
            }
        }
    }

    public IEnumerator Save(Player player)
    {
        _inCoroutine = true;
        player.enabled = false;
        player.ResetAnimatorAndCollision();

        var originalParent = player.transform.parent;
        var originalScene = player.gameObject.scene;

        player.transform.parent = playerPosition;
        player.transform.localPosition = Vector3.zero;

        _animator.SetTrigger("Close");

        var activeGame = SaveGameManager.activeGame;
        if(activeGame != null && !activeGame.activeSaveRoomPositions.Contains(_gridPosition))
        {
            activeGame.activeSaveRoomPositions.Add(_gridPosition);
            SaveGameManager.instance.Save();
        }

        unusable = true;
        _trigger.enabled = false;

        yield return new WaitForSeconds(3f);

        _animator.SetTrigger("Open");

        yield return new WaitForSeconds(8/12f);

        player.transform.parent = originalParent;
        SceneManager.MoveGameObjectToScene(player.gameObject, originalScene);

        player.enabled = true;
        _inCoroutine = false;
    }

    public bool CanRepair()
    {
        return !_inCoroutine && SaveGameManager.activeGame != null && SaveGameManager.activeGame.destroyedSaveRoomPositions.Contains(_gridPosition);
    }

    public void Repair()
    {
        if(CanRepair())
        {
            SaveGameManager.activeGame.destroyedSaveRoomPositions.Remove(_gridPosition);
            SaveGameManager.instance.Save();
            UISounds.instance.ScreenFlash();
            StartCoroutine(RepairRoutine());
        }
    }

    private IEnumerator RepairRoutine()
    {
        unusable = true;
        TransitionFade.instance.FadeOut(0.25f, Color.white);
        yield return new WaitForSeconds(0.25f);
        _animator.Play("Default");
        TransitionFade.instance.FadeIn(2, Color.white);
        yield return new WaitForSeconds(2f);
        unusable = false;
        _trigger.enabled = !unusable;
        _ready = true;
    }
}
