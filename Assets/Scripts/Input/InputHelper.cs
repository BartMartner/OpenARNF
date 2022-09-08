using System.Collections;

using System.Collections.Generic;
using UnityEngine;
using Rewired.UI.ControlMapper;
using Rewired;
using System;
using System.IO;
using System.Linq;
#if UNITY_SWITCH
using Rewired.Platforms.Switch;
#endif

public class InputHelper : MonoBehaviour
{
    public static InputHelper instance;
    public Action OnAutoReassign;

    private ControlMapper _controlMapper;
    public ControlMapper controlMapper
    {
        get { return _controlMapper; }
    }

#if UNITY_SWITCH
    private bool _allowApplet;
#endif

    public void Awake()
    {
        instance = this;
        AssignSystemPlayerAllControllers(null);
        ReInput.ControllerConnectedEvent += AssignSystemPlayerAllControllers;
        ReInput.ControllerDisconnectedEvent += AssignSystemPlayerAllControllers;
    }

#if UNITY_SWITCH
    public IEnumerator Start()
    {
        _controlMapper = FindObjectOfType<ControlMapper>();
        if (_controlMapper)
        {
            _controlMapper.ScreenClosedEvent += OnRemap;
            _controlMapper.ScreenClosedEvent += SaveAction;
            _controlMapper.showAssignedControllers = false;
            _controlMapper.showAssignedControllersGroupLabel = false;
            _controlMapper.showControllerNameLabel = false;
            _controlMapper.showControllerGroupLabel = false;
            _controlMapper.showSettingsGroupLabel = false;
            _controlMapper.showInputBehaviorSettings = false;
            _controlMapper.showKeyboard = false;
            _controlMapper.assignControllerButton.gameObject.SetActive(false);
            _controlMapper.removeControllerButton.gameObject.SetActive(false);
            _controlMapper.calibrateControllerButton.gameObject.SetActive(false);
        }

        ReInput.ControllerConnectedEvent += ShowControllerApplet;
        ReInput.ControllerDisconnectedEvent += ShowControllerApplet;
        yield return null;
        _allowApplet = true;

        OnRemap(); //to prevent weird bug where player2 can't join
    }
#else
    public void Start()
    {
        _controlMapper = FindObjectOfType<ControlMapper>();
        if (_controlMapper)
        {
            _controlMapper.ScreenClosedEvent += OnRemap;
            _controlMapper.ScreenClosedEvent += SaveAction;
        }
    }
#endif

    private void OnDestroy()
    {
#if UNITY_SWITCH
        ReInput.ControllerConnectedEvent -= ShowControllerApplet;
        ReInput.ControllerDisconnectedEvent -= ShowControllerApplet;
#endif
        ReInput.ControllerConnectedEvent -= AssignSystemPlayerAllControllers;
        ReInput.ControllerDisconnectedEvent -= AssignSystemPlayerAllControllers;

        if (_controlMapper)
        {
            _controlMapper.ScreenClosedEvent -= OnRemap;
            _controlMapper.ScreenClosedEvent -= SaveAction;
        }
    }

    public void SaveAction() { if (SaveGameManager.instance) { SaveGameManager.instance.Save(false, true); } }

#if DEBUG && !UNITY_SWITCH
    private void LateUpdate()
    {
        if (Input.GetKeyDown(KeyCode.F12))
        {
            string path;
            if (Application.isEditor)
            {
                path = Application.dataPath + "/../Screenshots/";
            }
            else
            {
                path = Application.dataPath + "/Screenshots/";
            }

            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }

            var files = Directory.GetFiles(path);
            var name = "Screenshot" + files.Length + ".png";
            ScreenCapture.CaptureScreenshot(path + name);
            Debug.Log("Screenshot Taken! " + path + name);
        };
    }
#endif

    public void AssignSystemPlayerAllControllers(ControllerStatusChangedEventArgs args)
    {
        var systemPlayer = ReInput.players.SystemPlayer;
        systemPlayer.controllers.ClearAllControllers();
        foreach (var controller in ReInput.controllers.Controllers)
        {
            systemPlayer.controllers.AddController(controller, false);
        }
    }

#if UNITY_SWITCH
    public void AssignPlayer1LastActiveController() { }
#else
    public void RestoreDefaultAssignments()
    {
        for (int i = 0; i < ReInput.players.playerCount; i++)
        {
            var p = ReInput.players.GetPlayer(i);
            p.controllers.ClearAllControllers();
            Debug.Log("Unassigning Controllers from " + p.name);
        }

        for (int i = 0; i < ReInput.players.playerCount; i++)
        {
            var p = ReInput.players.GetPlayer(i);
            var maxP = i;
            var controller = ReInput.controllers.Controllers.FirstOrDefault(c =>
            {
                if (c.type != ControllerType.Joystick) { return false; }
                for (int j = 0; j < i; j++)
                {
                    if (ReInput.controllers.IsControllerAssignedToPlayer(c.type, c.id, j)) { return false; }
                }
                return true;
            });

            if (controller != null)
            {
                Debug.Log("Assigned " + controller.name + " to player " + i + ".");
                p.controllers.AddController(controller, true);
            }
            else
            {
                Debug.LogWarning("No Unassigned Controller found for " + p.name);
            }
        }

        AssignSystemPlayerAllControllers(null);
    }

    public void AssignPlayer1LastActiveController()
    {
        var lastControllerUsed = ReInput.controllers.GetLastActiveController();
        if (lastControllerUsed.type == ControllerType.Mouse)
        {
            return;
        }

        var player1 = ReInput.players.GetPlayer(0);

#if ARCADE
        if (lastControllerUsed.type == ControllerType.Keyboard) { return; }

        Debug.Log("Controllers: " + ReInput.controllers.controllerCount);

        for (int i = 0; i < ReInput.players.playerCount; i++)
        {
            var p = ReInput.players.GetPlayer(i);
            p.controllers.ClearAllControllers();
            Debug.Log("Unassigning Controllers from " + p.name);
        }

        player1.controllers.AddController(lastControllerUsed, true);
        ReInput.players.SystemPlayer.controllers.AddController(lastControllerUsed, false);
        Debug.Log("Assigned " + lastControllerUsed.name + " to " + player1.name);

        for (int i = 1; i < ReInput.players.playerCount; i++)
        {
            var p = ReInput.players.GetPlayer(i);
            var maxP = i;
            var controller = ReInput.controllers.Controllers.FirstOrDefault(c =>
            {
                if(c.type != ControllerType.Joystick) { return false; }
                for (int j = 0; j < i; j++)
                {
                    if(ReInput.controllers.IsControllerAssignedToPlayer(c.type, c.id, j)) { return false; }
                }
                return true;
            });

            if (controller != null)
            {
                Debug.Log("Assigned " + controller.name + " to player " + i + ".");
                p.controllers.AddController(controller, true);
            }
            else
            {
                Debug.LogWarning("No Unassigned Controller found for " + p.name);
            }
        }
#else
        var player2 = ReInput.players.GetPlayer(1);

        Debug.Log("Assigning " + lastControllerUsed.name + " to player 1.");
        player1.controllers.ClearAllControllers();
        player1.controllers.AddController(lastControllerUsed, true);

        ReInput.players.SystemPlayer.controllers.AddController(lastControllerUsed, false);

        player2.controllers.ClearAllControllers();
        foreach (var joystick in ReInput.controllers.Joysticks)
        {
            if (!ReInput.controllers.IsControllerAssignedToPlayer(joystick.type, joystick.id, 0))
            {
                Debug.Log("Assigning " + joystick.name + " to player 2.");
                player2.controllers.AddController(joystick, false);
            }
        }

        if (lastControllerUsed != ReInput.controllers.Keyboard)
        {
            Debug.Log("Assigning keyboard to player 2.");
            player2.controllers.AddController(ReInput.controllers.Keyboard, false);
        }
#endif
        if (OnAutoReassign != null) { OnAutoReassign(); }
    }
#endif

    private void OnRemap()
    {
        Debug.Log("OnRemap Called!");
        var players = 4;

        for (int i = 1; i < players; i++)
        {
            var player = ReInput.players.GetPlayer(i);
            if (player.controllers.joystickCount <= 0)
            {
                foreach (var joystick in ReInput.controllers.Joysticks)
                {
                    bool assigned = false;
                    for (int j = 0; j < i; j++)
                    {
                        if (ReInput.controllers.IsJoystickAssignedToPlayer(joystick.id, j))
                        {
                            assigned = true;
                            break;
                        }
                    }

                    if (!assigned)
                    {
                        Debug.Log("Assigning " + joystick.name + " to player " + (i + 1) + ".");
                        player.controllers.AddController(joystick, false);
                        player.controllers.maps.SetAllMapsEnabled(true);
                        break;
                    }
                }
            }
        }

        if (!DeathmatchManager.instance && PlayerManager.instance && PlayerManager.instance.coOpPlayers.Length > 0)
        {
            for (int i = 0; i < PlayerManager.maxCoOpBots; i++)
            {
                if (!PlayerManager.instance.coOpPlayers[i]) continue;

                var rPlayer = ReInput.players.GetPlayer(i+1);
                if (rPlayer.controllers.joystickCount == 0 && !rPlayer.controllers.hasKeyboard)
                {
                    PlayerManager.instance.coOpPlayers[i].Despawn(true);
                }
            }
        }

        if (OnAutoReassign != null) { OnAutoReassign(); }
    }

#if UNITY_SWITCH
    void ShowControllerApplet(ControllerStatusChangedEventArgs args) { if (_allowApplet) { StartCoroutine(LaunchApplet()); } }

    IEnumerator LaunchApplet()
    {

        Debug.Log("Showing Applet!");
        _allowApplet = false;
#if !UNITY_EDITOR
        // Set the options to pass to the Controller Applet
        ControllerAppletOptions options = new ControllerAppletOptions();
        options.playerCountMin = 0;
        options.playerCountMax = 4;
        options.showColors = true;
        options.showLabels = true;
        options.players[0].color = Color.red;
        options.players[0].label = "Player 1";
        options.players[1].color = Color.blue;
        options.players[1].label = "Player 2";
        options.players[2].color = Color.green;
        options.players[2].label = "Player 3";
        options.players[3].color = Color.yellow;
        options.players[3].label = "Player 4";
        
        // Show the controller applet
        UnityEngine.Switch.Applet.Begin(); // See Unity documentation for explanation of this function
        SwitchInput.ControllerApplet.Show(options);
        UnityEngine.Switch.Applet.End();
#endif
        yield return new WaitForSeconds(0.5f);

        OnRemap();

        _allowApplet = true;
    }
#endif
    }