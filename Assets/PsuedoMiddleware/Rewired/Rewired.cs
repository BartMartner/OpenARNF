using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/* This is a terrible hacky wrapper class I wrote to substitute for the AMAZING Rewired input pluggin.
 * It only supports single player Keyboard input without modification.
 * Purchase the real REWIRED from GUAVAMAN for quality input handling at: https://guavaman.com/projects/rewired/
 * You should be able to import Rewired, delete this, and not have to modify any code, but no guarantees.
 */

namespace Rewired
{
    public static class ReInput
    {
        public static ContollerStatusChanged ControllerConnectedEvent;
        public static ContollerStatusChanged ControllerDisconnectedEvent;
        public static ControllerHelper controllers = new ControllerHelper();
        public static PlayerHelper players = new PlayerHelper();
        public static Player SystemPlayer
        {
            get { return players.SystemPlayer; }
        
        }
        public static MappingHelper mapping = new MappingHelper();
    }

    public class MappingHelper
    {
        private Dictionary<string, InputAction> _map = new Dictionary<string, InputAction>()
        {
            {"Vertical", new InputAction(){ id = 0} },
            {"Horizontal", new InputAction(){ id = 1} },
            {"Attack", new InputAction(){ id = 4} },
            {"Jump", new InputAction(){ id = 5} },
            {"SpecialMove", new InputAction(){ id = 6} },
            {"ActivatedItem", new InputAction(){ id = 7} },
            {"AngleUp", new InputAction(){ id = 8} },
            {"AngleDown", new InputAction(){ id = 9} },
            {"PageRight", new InputAction(){ id = 10} },
            {"PageLeft", new InputAction(){ id = 11} },
            {"ExpandMap", new InputAction(){ id = 12} },
            {"UIHorizontal", new InputAction(){id = 14} },
            {"UIVertical", new InputAction(){id = 15} },
            {"UISubmit", new InputAction(){id = 16} },
            {"UICancel", new InputAction(){id = 17} },            
            {"UIUp", new InputAction(){id = 18} },
            {"UIDown", new InputAction(){id = 19} },
            {"UILeft", new InputAction(){id = 20} },
            {"UIRight", new InputAction(){id = 21} },
            {"UIScroll", new InputAction(){id = 31} },
            {"WeaponVertical", new InputAction(){ id = 32} },
            {"WeaponHorizontal", new InputAction(){ id = 33} },
            {"CoinSlot", new InputAction(){ id = 37} },
        };

        public InputAction GetAction(string name)
        {
            if (_map.TryGetValue(name, out InputAction action))
            {
                return action;
            }
            else
            {
                Debug.LogError("No InputAction defined for " + name);
                return null;
            }
        }
    }

    public delegate void ContollerStatusChanged(ControllerStatusChangedEventArgs args);

    public class ControllerStatusChangedEventArgs
    {
    }

    public class PlayerHelper : IEnumerable<Player>
    {
        public Player SystemPlayer = new Player();

        public Player GetSystemPlayer()
        {
            return SystemPlayer;
        }

        public int playerCount
        {
            //Only a single player is supported with this class. BUY REWIRED!
            get { return 1; }
        }

        public IEnumerator<Player> GetEnumerator()
        {
            yield return SystemPlayer;
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            throw new NotImplementedException();
        }

        public Player GetPlayer(int id)
        {
            if(id == 0)
            {
                return SystemPlayer;
            }
            else
            {
                return null;
            }
        }
    }

    public class Player
    {
        public readonly ControllerHelper controllers = new ControllerHelper();

        public int id
        {
            get { return 0; }
        }

        public string name
        {
            get { return "OPEN ARNF DEBUG"; }
        }

        public bool GetAnyButton()
        {
            throw (new System.NotImplementedException());
        }

        private Dictionary<string, KeyCode> _keyValues = new Dictionary<string, KeyCode>()
        {
            {"UISubmit", KeyCode.Return },
            {"UICancel", KeyCode.Escape },
            {"Pause", KeyCode.Escape },
            {"ExpandMap", KeyCode.M },
            {"Up", KeyCode.W },
            {"Down", KeyCode.S },
            {"Left", KeyCode.A },
            {"Right", KeyCode.D },
            {"Jump", KeyCode.Space },
            {"Attack", KeyCode.J },
            {"SpecialMove", KeyCode.L },
            {"ActivatedItem", KeyCode.U },
            {"AngleUp", KeyCode.I },
            {"AngleDown", KeyCode.K },
            {"PageRight", KeyCode.Comma },
            {"PageLeft", KeyCode.Period },
            {"CoinSlot", KeyCode.Insert },
        };

        public bool GetButtonDown(string label)
        {
            switch(label)
            {
                case "UIUp":
                    return Input.GetKeyDown(KeyCode.W) || Input.GetKeyDown(KeyCode.UpArrow);
                case "UIDown":
                    return Input.GetKeyDown(KeyCode.S) || Input.GetKeyDown(KeyCode.DownArrow);
                case "UILeft":
                    return Input.GetKeyDown(KeyCode.A) || Input.GetKeyDown(KeyCode.LeftArrow);
                case "UIRight":
                    return Input.GetKeyDown(KeyCode.D) || Input.GetKeyDown(KeyCode.RightArrow);
                default:
                    if(_keyValues.TryGetValue(label, out var keyCode))
                    {
                        return Input.GetKeyDown(keyCode);
                    }
                    else
                    {
                        Debug.LogError(label + " is not defined");
                        return false;
                    }
            }
        }

        public bool GetButtonUp(string label)
        {
            switch (label)
            {
                case "UIUp":
                    return Input.GetKeyUp(KeyCode.W) || Input.GetKeyUp(KeyCode.UpArrow);
                case "UIDown":
                    return Input.GetKeyUp(KeyCode.S) || Input.GetKeyUp(KeyCode.DownArrow);
                case "UILeft":
                    return Input.GetKeyUp(KeyCode.A) || Input.GetKeyUp(KeyCode.LeftArrow);
                case "UIRight":
                    return Input.GetKeyUp(KeyCode.D) || Input.GetKeyUp(KeyCode.RightArrow);
                default:
                    if (_keyValues.TryGetValue(label, out var keyCode))
                    {
                        return Input.GetKeyUp(keyCode);
                    }
                    else
                    {
                        Debug.LogError(label + " is not defined");
                        return false;
                    }                    
            }
        }

        public bool GetButton(string label)
        {
            switch (label)
            {
                case "UIUp":
                    return Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.UpArrow);
                case "UIDown":
                    return Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.DownArrow);
                case "UILeft":
                    return Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.LeftArrow);
                case "UIRight":
                    return Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.RightArrow);
                default:
                    if (_keyValues.TryGetValue(label, out var keyCode))
                    {
                        return Input.GetKey(keyCode);
                    }
                    else
                    {
                        Debug.LogError(label + " is not defined");
                        return false;
                    }
            }
        }

        public bool GetAnyButtonDown()
        {
            foreach (KeyCode keyCode in Enum.GetValues(typeof(KeyCode)))
            {
                if(Input.GetKeyDown(keyCode))
                {
                    return true;
                }
            }
            return false;            
        }
        public float GetAxis(string label)
        {
            if(label == "Vertical")
            {
                if (Input.GetKey(KeyCode.W))
                {
                    return 1;
                }
                else if (Input.GetKey(KeyCode.S))
                {
                    return -1;
                }
                else
                {
                    return 0;
                }
            }
            else if(label == "Horizontal")
            {
                if (Input.GetKey(KeyCode.D))
                {
                    return 1;
                }
                else if (Input.GetKey(KeyCode.A))
                {
                    return -1;
                }
                else
                {
                    return 0;
                }
            }
            else if (label == "WeaponVertical")
            {
                if (Input.GetKey(KeyCode.UpArrow))
                {
                    return 1;
                }
                else if (Input.GetKey(KeyCode.DownArrow))
                {
                    return -1;
                }
                else
                {
                    return 0;
                }
            }
            else if (label == "WeaponHorizontal")
            {
                if (Input.GetKey(KeyCode.RightArrow))
                {
                    return 1;
                }
                else if (Input.GetKey(KeyCode.LeftArrow))
                {
                    return -1;
                }
                else
                {
                    return 0;
                }
            }

            Debug.LogError("Axis " + label + " is not defined");
            return 0;
        }
    }

    public class ControllerHelper
    {
        public MapHelper maps = new MapHelper();

        public bool hasKeyboard
        {
            get { return _Keyboard != null; }
        }

        public int joystickCount
        {
            get { return _Joysticks.Count; }
        }

        private List<Controller> _Controllers = new List<Controller>();
        public List<Controller> Controllers
        {
            get { return _Controllers; }
        }

        private List<Joystick> _Joysticks = new List<Joystick>();
        public List<Joystick> Joysticks { get { return _Joysticks; } }

        private Keyboard _Keyboard = new Keyboard();
        public Keyboard Keyboard
        {
            get { return _Keyboard; }
        }

        public bool IsControllerAssignedToPlayer(ControllerType type, int x, int y)
        {
            throw new System.NotImplementedException();
        }

        public void AddController(Controller controller, bool removeFromOtherPlayers)
        {
            if (removeFromOtherPlayers)
            {
                Debug.LogWarning("removeFromOtherPlayers will not function as expect. OPENARNF only supports a single player using keyboard input without modification");
            }

            _Controllers.Add(controller);

            var joystick = controller as Joystick;
            if (joystick != null) 
            {
                _Joysticks.Add(joystick);
            }
        }

        public void ClearAllControllers()
        {
            _Controllers.Clear();
            _Joysticks.Clear();
        }

        public Controller GetLastActiveController()
        {
            //throw new System.NotImplementedException();
            //OpenARNF only supports Keyboard input without modification
            return Keyboard;
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
        public override ControllerType type
        {
            get { return ControllerType.Keyboard; }
        }

        public override string name 
        { 
            get { return "Keyboard"; }
        }

        public bool GetKey(KeyCode keyCode)
        {
            return Input.GetKey(keyCode);
        }

        public bool GetKeyDown(KeyCode keyCode)
        {
            return Input.GetKeyDown(keyCode);            
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

    public class Joystick : Controller 
    {
        public override ControllerType type
        {
            get { return ControllerType.Joystick; }
        }
    }

    public class InputAction
    {
        public int id;
    }

    public class MapHelper
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

        private Dictionary<int, ActionElementMap> _aems = new Dictionary<int, ActionElementMap>()
        {
            {6, new ActionElementMap()
                {
                    elementIdentifierName = "L",
                }
            },
            {7, new ActionElementMap()
                {
                    elementIdentifierName = "U",
                }
            },
            {16, new ActionElementMap()
                {
                    elementIdentifierName = "Return",
                }
            },
            {17, new ActionElementMap()
                {
                    elementIdentifierName = "Escape",
                }
            },
        };

        public ActionElementMap GetFirstElementMapWithAction(Controller controller, int id, bool skipDisabledMaps)
        {
            if(controller != ReInput.SystemPlayer.controllers.Keyboard)
            {
                Debug.LogError("Only the keyboard for the system player is supported by default in OpenARNF. ");
                return null;
            }

            if(_aems.TryGetValue(id, out var aem))
            {
                return aem;
            }
            else
            {
                Debug.LogError("No action element map could be found for id " + id);
                return null;
            }            
        }
    }

    public class Controller
    {
        public virtual ControllerType type { get; private set; }

        public int id
        {
            get { throw (new System.NotImplementedException()); }
        }

        public virtual string name { get; }

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
            get; set;
        }
        public int elementIdentifierId
        {
            get; private set;
        }
        public AxisRange axisRange
        {
            get; private set;
        }
        public AxisType axisType
        {
            get; private set;
        }
        public Pole axisContribution
        {
            get; private set;
        }
        public ControllerMap controllerMap
        {
            get; private set;
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

