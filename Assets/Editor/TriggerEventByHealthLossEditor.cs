using UnityEngine;
using System.Collections;
using UnityEditor;

[CustomEditor(typeof(TriggerEventByHealthLoss))]
public class TriggerEventByHealthLossEditor : Editor
{
    private float _lastPercentage;
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();
        
        var trigger = target as TriggerEventByHealthLoss;
        _lastPercentage = trigger.GetHealthPercentage();
        var percent = EditorGUILayout.Slider(_lastPercentage, 0, 1);
        if(percent != _lastPercentage)
        {
            Undo.RecordObject(trigger, "Undo Trigger Event By Health Loss Slide");
            trigger.SetHealthPercentage(percent);
        }
    }
}
