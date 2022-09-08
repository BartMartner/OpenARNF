using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;

public class DeathScreen : MonoBehaviour
{
    public static DeathScreen instance;
    public bool visible { get { return _visible; } }
    private bool _visible;

    public void Awake()
    {
        instance = this;
        gameObject.SetActive(false);
    }

    public void Show()
    {
        _visible = true;
        Time.timeScale = 0;
        gameObject.SetActive(true);
    }

    public void BackToStartScreen()
    {
        _visible = false;
        Time.timeScale = 1;
        SaveGameManager.instance.ClearActiveGame();
        SceneManager.LoadScene("StartScreen");
    }

    public void Restart()
    {
        Time.timeScale = 1;
        Debug.Log("New Game on Slot: " + SaveGameManager.instance.selectedSaveSlot);
        var activeGame = SaveGameManager.activeGame;        
        var oldPassword = activeGame.password;
        SaveGameManager.instance.NewGame(activeGame.gameMode, activeGame.raceMode); //this sets layout to null
        if (!string.IsNullOrEmpty(oldPassword)) { SaveGameManager.activeGame.password = oldPassword; }
        SaveGameManager.instance.Save(false, true);
        SceneManager.LoadScene("NewGame");
    }

    public void OnDestroy()
    {
        if(instance = this)
        {
            instance = null;
        }
    }
}