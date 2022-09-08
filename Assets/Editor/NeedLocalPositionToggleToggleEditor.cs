using UnityEngine;
using System.Collections;
using UnityEditor;
using UnityEditor.SceneManagement;

[CustomEditor(typeof(NeedLocalPositionToggle))]
public class NeedLocalPositionToggleToggleEditor : Editor
{
    public override void OnInspectorGUI()
    {
        var needLocalPositionToggle = target as NeedLocalPositionToggle;

        base.OnInspectorGUI();

        bool dirty = false;

        if(!needLocalPositionToggle.needed)
        {
            var needed = new GameObject();
            needed.name = "Needed";
            needed.transform.SetParent(needLocalPositionToggle.transform);
            needed.transform.localPosition = Vector3.zero;
            needLocalPositionToggle.needed = needed;
            dirty = true;
        }

        if (!needLocalPositionToggle.notNeeded)
        {
            var notNeeded = new GameObject();
            notNeeded.name = "NotNeeded";
            notNeeded.transform.SetParent(needLocalPositionToggle.transform);
            notNeeded.transform.localPosition = Vector3.zero;
            needLocalPositionToggle.notNeeded = notNeeded;
            dirty = true;
        }

        if (needLocalPositionToggle.needed.activeInHierarchy)
        {
            if (GUILayout.Button("Toggle Not Needed"))
            {
                needLocalPositionToggle.needed.SetActive(false);
                needLocalPositionToggle.notNeeded.SetActive(true);
                dirty = true;
            }
        }
        else if (needLocalPositionToggle.notNeeded.activeInHierarchy)
        {
            if (GUILayout.Button("Toggle Needed"))
            {
                needLocalPositionToggle.needed.SetActive(true);
                needLocalPositionToggle.notNeeded.SetActive(false);
                dirty = true;
            }
        }

        if(dirty)
        {
            EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
        }
    }
}
