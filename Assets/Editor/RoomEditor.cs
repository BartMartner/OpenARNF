using UnityEngine;
using System.Collections;
using UnityEditor;
using UnityEditor.SceneManagement;
using System.Collections.Generic;

[CustomEditor(typeof(Room))]
public class RoomEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        Room room = (Room)target;
        var activeScene = EditorSceneManager.GetActiveScene();
        if (GUILayout.Button("Find and Assign Data"))
        {
            Undo.RecordObject(room, "FindAndAssignExits");

            //for water update
            if(room.roomInfo.traversalLimitations.requiredJumpHeight == 0)
            {
                room.roomInfo.traversalLimitations.requiredJumpHeight = Mathf.Floor(Constants.startingMaxJumpHeight);
            }

            var neededPositionToggles = FindObjectsOfType<NeedLocalPositionToggle>();
            foreach (var p in neededPositionToggles)
            {
                if (p.needed)
                {
                    p.needed.SetActive(true);
                }

                if (p.notNeeded)
                {
                    p.notNeeded.SetActive(false);
                }
            }

            if (room.roomInfo.size == Int2D.zero)
            {
                var cameraBounds = room.GetComponentsInChildren<CameraBoundsTrigger>();
                Bounds totalBounds = cameraBounds[0].GetBounds();
                var min = totalBounds.min;
                var max = totalBounds.max;

                for (int j = 1; j < cameraBounds.Length; j++)
                {
                    var bounds = cameraBounds[j].GetBounds();
                    if (bounds.min.x < min.x)
                        min.x = bounds.min.x;
                    if (bounds.min.y < min.y)
                        min.y = bounds.min.y;
                    if (bounds.max.x > max.x)
                        max.x = bounds.max.x;
                    if (bounds.max.y > max.y)
                        max.y = bounds.max.y;
                }

                totalBounds.min = min;
                totalBounds.max = max;

                room.roomInfo.size = new Int2D((int)(totalBounds.size.x / Constants.roomWidth), (int)(totalBounds.size.y / Constants.roomHeight));
            }

            room.roomInfo.sceneName = EditorSceneManager.GetActiveScene().name;
            room.name = room.roomInfo.sceneName;
            room.roomInfo.possibleExits = new List<ExitLimitations>();
            room.roomInfo.requiredExits = new List<ExitLimitations>();
            var exits = room.GetComponentsInChildren<RoomTransitionTrigger>(true);
            foreach (var exit in exits)
            {
                var limitations = new ExitLimitations();

                //assignData
                //Could compare to 0 but this is a little safer due to possible rounding errors
                if (Mathf.Abs(exit.transform.position.x % (Constants.roomWidth * 0.5f)) < float.Epsilon)
                {
                    exit.direction = exit.transform.position.y > 0 ? Direction.Up : Direction.Down;
                }
                else
                {
                    exit.direction = exit.transform.position.x > 0 ? Direction.Right : Direction.Left;
                }

                exit.localGridPosition = room.GetLocalGridPosition(exit.transform.position);

                limitations.direction = exit.direction;
                limitations.localGridPosition = exit.localGridPosition;
                limitations.toExit = exit.requiredExitLimitations;
                
                var parentToggle = exit.GetComponentInParent<ExitObjectToggle>();

                if (!parentToggle)
                {
                    room.roomInfo.requiredExits.Add(limitations);
                }

                room.roomInfo.possibleExits.Add(limitations);

                EditorUtility.SetDirty(exit);
            }

            var toggles = room.GetComponentsInChildren<ExitObjectToggle>();
            foreach (var toggle in toggles)
            {
                Undo.RecordObject(toggle, "Asssign Toggle " + toggle.name);
                var exit = toggle.exitMatch.GetComponentInChildren<RoomTransitionTrigger>(true);
                toggle.exitRequirements.direction = exit.direction;
                toggle.exitRequirements.localGridPosition = exit.localGridPosition;
                toggle.exitRequirements.toExit = exit.requiredExitLimitations;
                EditorUtility.SetDirty(toggle);
            }

            int i = 0;
            room.SetPermanentStateObjects();

            var minorItemSpawnPoints = room.GetComponentsInChildren<MinorItemSpawnPoint>(true);
            i = 0;
            room.roomInfo.minorItemLocations.Clear();
            foreach (var spawnPoint in minorItemSpawnPoints)
            {
                spawnPoint.info.localID = i;
                i++;
                var dependant = spawnPoint.GetComponentInParent<MinorItemDependantObject>();
                if(dependant)
                {
                    dependant.minorItemLocalID = spawnPoint.info.localID;
                    EditorUtility.SetDirty(dependant);
                }

                spawnPoint.info.localGridPosition = room.GetLocalGridPosition(spawnPoint.transform.position);

                room.roomInfo.minorItemLocations.Add(spawnPoint.info);
                EditorUtility.SetDirty(spawnPoint);
            }

            var majorItemSpawnPoint = room.GetComponentInChildren<MajorItemSpawnPoint>(true);
            if(majorItemSpawnPoint)
            {
                room.roomInfo.majorItemLocalPosition = room.GetLocalGridPosition(majorItemSpawnPoint.transform.position);
            }

            EditorSceneManager.MarkSceneDirty(activeScene);
        }

        if (!room.name.Contains("Template") && room.roomInfo.environmentType != EnvironmentType.GreyBox)
        {
            if (GUILayout.Button("Save Room Info"))
            {
                room.SaveRoomInfo();

                var sceneList = new List<string>();
                foreach (var scene in EditorBuildSettings.scenes)
                {
                    var scenePath = scene.path;
                    sceneList.Add(scenePath);
                }

                if (!sceneList.Contains(activeScene.path))
                {
                    Debug.Log("Adding Scene to BuildSettings");
                    var original = EditorBuildSettings.scenes;
                    var newSettings = new EditorBuildSettingsScene[original.Length + 1];
                    System.Array.Copy(original, newSettings, original.Length);
                    newSettings[newSettings.Length - 1] = new EditorBuildSettingsScene(activeScene.path, true);
                    EditorBuildSettings.scenes = newSettings;
                    EditorSceneManager.SaveScene(activeScene);
                }
            }
        }
    }
}
