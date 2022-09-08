using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Text))]
public class VersionNumber : MonoBehaviour
{
    private Text _text;
    public string conMessage;

    private void Start()
    {
        _text = GetComponent<Text>();
        SetText();
        SaveGameManager.instance.onSave += SetText;
    }

    public void SetText()
    {
        if (SaveGameManager.instance && SaveGameManager.instance.saveFileData != null && SaveGameManager.instance.saveFileData.conventionMode)
        {
            _text.text = SaveGameManager.version.ToString() + " " + conMessage;
        }
        else
        {
            _text.text = SaveGameManager.version.ToString();
        }
    }

    private void OnDestroy()
    {
        if (SaveGameManager.instance) { SaveGameManager.instance.onSave -= SetText; }
    }
}
