using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Rewired
{
    public static class ReInput
    {
        public static ContollerStatusChanged ControllerConnectedEvent;
        public static ContollerStatusChanged ControllerDisconnectedEvent;
        public static ControllerList controllers;
        public static Players players;
        public static Player SystemPlayer;
        public static Mapping mapping;
    }

    public class Mapping
    {
        public InputAction GetAction(string name)
        {
            throw new NotImplementedException();
        }
    }

    public delegate void ContollerStatusChanged(ControllerStatusChangedEventArgs args);

    public class ControllerStatusChangedEventArgs
    {
    }

    public class Players : IEnumerable<Player>
    {
        public Player SystemPlayer;

        public Player GetSystemPlayer()
        {
            throw (new System.NotImplementedException());
        }

        public int playerCount
        {
            get { throw (new System.NotImplementedException()); }
        }

        public IEnumerator<Player> GetEnumerator()
        {
            throw new NotImplementedException();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            throw new NotImplementedException();
        }

        public Player GetPlayer(int id)
        {
            throw new NotImplementedException();
        }
    }

    public class Player
    {
        public int id
        {
            get { throw (new System.NotImplementedException()); }
        }

        public string name
        {
            get { throw (new System.NotImplementedException()); }
        }

        public bool GetAnyButton()
        {
            throw (new System.NotImplementedException());
        }

        public bool GetButtonDown(string label)
        {
            throw (new System.NotImplementedException());
        }

        public bool GetButtonUp(string label)
        {
            throw (new System.NotImplementedException());
        }

        public bool GetButton(string label)
        {
            throw (new System.NotImplementedException());
        }

        public bool GetAnyButtonDown()
        {
            throw (new System.NotImplementedException());
        }
        public float GetAxis(string label)
        {
            throw (new System.NotImplementedException());
        }

        public ControllerList controllers
        {
            get { throw (new System.NotImplementedException()); }
        }
    }

    public class ControllerList
    {
        public ControllerMaps maps
        {
            get { throw (new System.NotImplementedException()); }
        }

        public bool hasKeyboard
        {
            get { throw (new System.NotImplementedException()); }
        }

        public int joystickCount
        {
            get { throw (new System.NotImplementedException()); }
        }

        public IEnumerable<Controller> Controllers
        {
            get { throw (new System.NotImplementedException()); }
        }

        public List<Joystick> Joysticks
        {
            get { throw (new System.NotImplementedException()); }
        }

        public Keyboard Keyboard
        {
            get { throw (new System.NotImplementedException()); }
        }

        public bool IsControllerAssignedToPlayer(ControllerType type, int x, int y)
        {
            throw new System.NotImplementedException();
        }

        public void AddController(Controller controller, bool someBool)
        {
            throw new System.NotImplementedException();
        }

        public void ClearAllControllers()
        {
            throw new System.NotImplementedException();
        }

        public Controller GetLastActiveController()
        {
            throw new System.NotImplementedException();
        }

        public bool IsJoystickAssignedToPlayer(int id, int other)
        {
            throw new System.NotImplementedException();
        }

        public bool GetAnyButton()
        {
            throw new System.NotImplementedException();
        }
    }

    public class Keyboard : Controller
    {
        public bool GetKey(KeyCode keyCode)
        {
            throw new System.NotImplementedException();
        }

        public bool GetKeyDown(KeyCode keyCode)
        {
            throw new System.NotImplementedException();
        }

        public IEnumerable<PollingInfo> PollForAllKeysDown()
        {
            throw new System.NotImplementedException();
        }
    }

    public class PollingInfo
    {
        public KeyCode keyboardKey
        {
            get { throw (new System.NotImplementedException()); }
        }
    }

    public class Joystick : Controller { }

    public class InputAction
    {
        public int id
        {
            get { throw (new System.NotImplementedException()); }
        }
    }

    public class ControllerMaps
    {
        public void SetAllMapsEnabled(bool someBool)
        {
            throw new System.NotImplementedException();
        }

        public IEnumerable<ActionElementMap> ButtonMapsWithAction(int id, bool someBool)
        {
            throw new System.NotImplementedException();
        }
        public IEnumerable<ActionElementMap> ElementMapsWithAction(int id, bool someBool)
        {
            throw new System.NotImplementedException();
        }

        public ActionElementMap GetFirstElementMapWithAction(Controller controller, int id, bool someBool)
        {
            throw new System.NotImplementedException();
        }
    }

    public class Controller
    {
        public ControllerType type;

        public int id
        {
            get { throw (new System.NotImplementedException()); }
        }

        public string name
        {
            get { throw (new System.NotImplementedException()); }
        }

        public Guid hardwareTypeGuid
        {
            get { throw (new System.NotImplementedException()); }
        }
    }

    public class ControllerMap
    {
        public int controllerId
        {
            get { throw (new System.NotImplementedException()); }
        }
    }

    public class ActionElementMap
    {
        public string elementIdentifierName
        {
            get { throw (new System.NotImplementedException()); }
        }
        public int elementIdentifierId
        {
            get { throw (new System.NotImplementedException()); }
        }
        public AxisRange axisRange
        {
            get { throw (new System.NotImplementedException()); }
        }
        public AxisType axisType
        {
            get { throw (new System.NotImplementedException()); }
        }
        public Pole axisContribution
        {
            get { throw (new System.NotImplementedException()); }
        }
        public ControllerMap controllerMap
        {
            get { throw (new System.NotImplementedException()); }
        }
    }

    public class UserDataStore
    {

    }

    public enum AxisType
    {
        None,
    }

    public enum ControllerType
    {
        Joystick,
        Keyboard,
        Mouse,
    }

    public enum AxisRange
    {
        Full,
        Positive,
        Negative,
    }

    public enum Pole
    {
        Positive,
        Negative,
    }
}

namespace Rewired.Data.Mapping
{ 
    public class HardwareJoystickMap
    {
        public Guid Guid;
    }
}

namespace Rewired.UI.ControlMapper
{
    public partial class ControlMapper
    {
        public ControlMapperButtons references;
        public Action ScreenClosedEvent;
        public bool isOpen;
        public void Open()
        {
            throw (new System.NotImplementedException());
        }

        public void Close(bool someBool)
        {
            throw (new System.NotImplementedException());
        }
    }

    public class ControlMapperButtons
    {
        public UnityEngine.UI.Button removeControllerButton;
        public UnityEngine.UI.Button assignControllerButton;
        public UnityEngine.UI.Button calibrateControllerButton;
    }
}

