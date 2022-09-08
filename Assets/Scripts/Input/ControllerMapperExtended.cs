using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Rewired.UI.ControlMapper
{
    public partial class ControlMapper : MonoBehaviour
    {
        public Button removeControllerButton { get { return references.removeControllerButton; } }
        public Button assignControllerButton { get { return references.assignControllerButton; } }
        public Button calibrateControllerButton { get { return references.calibrateControllerButton; } }
    }
}
