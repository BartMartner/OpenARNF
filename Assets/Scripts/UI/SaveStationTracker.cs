using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SaveStationTracker : MonoBehaviour
{
    public static SaveStationTracker instance;
    public Sprite active;
    public Sprite destroyed;
    private Image[] _sprites;

	// Use this for initialization
	IEnumerator Start ()
    {
        _sprites = GetComponentsInChildren<Image>();

        while(!SaveGameManager.instance)
        {
            yield return null;
        }

        SaveGameManager.instance.onSave += Refresh;
        Refresh();
	}
	
	// Update is called once per frame
	void Refresh ()
    {
        if(SaveGameManager.activeGame == null)
        {
            return;
        }

        int statesSet = 0;

        foreach (var save in SaveGameManager.activeGame.destroyedSaveRoomPositions)
        {
            if (statesSet > _sprites.Length)
            {
                break;
            }

            _sprites[statesSet].gameObject.SetActive(true);
            _sprites[statesSet].sprite = destroyed;
            _sprites[statesSet].color = Color.white;
            statesSet++;
        }

        foreach (var save in SaveGameManager.activeGame.activeSaveRoomPositions)
        {
            if(statesSet > _sprites.Length)
            {
                break;
            }

            _sprites[statesSet].gameObject.SetActive(true);
            _sprites[statesSet].sprite = active;
            _sprites[statesSet].color = Color.white;
            statesSet++;
        }

        for (int i = statesSet; i < _sprites.Length; i++)
        {
            _sprites[i].gameObject.SetActive(false);
            _sprites[i].sprite = null;
            _sprites[i].color = Color.clear;
        }
    }

    private void OnDestroy()
    {
        if (SaveGameManager.instance)
        {
            SaveGameManager.instance.onSave -= Refresh;
        }
    }
}
