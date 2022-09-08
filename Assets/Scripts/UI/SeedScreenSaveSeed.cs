using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class SeedScreenSaveSeed : MonoBehaviour
{
    public const string blankSeed = "------ ------ ------ ------";
    private Text _text;
    private Button _button;
    private string _seed;

    public void Awake()
    {
        _text = GetComponent<Text>();
        _button = GetComponent<Button>();
    }

    public void SetSeed(string seed)
    {
        if(seed == string.Empty)
        {
            _button.interactable = false;
            _text.text = blankSeed;
            return;
        }

        _seed = seed;
        _button.interactable = true;

        var parsedSeed = _seed;
        parsedSeed = parsedSeed.Insert(6, " ");
        parsedSeed = parsedSeed.Insert(13, "\n");
        parsedSeed = parsedSeed.Insert(20, " ");
        switch (_seed)
        {
            case SeedHelper.ClassicBossRush:
                _text.text = "CLASSIC BOSS RUSH";
                break;
            case SeedHelper.Spooky:
                _text.text = "SPOOKY";
                break;
            case SeedHelper.MirrorWorld:
                _text.text = "MIRROR WORLD";
                break;
#if TRUECOOP
            case SeedHelper.TrueCoOp:
                _text.text = "TRUE COOP";
                break;
#endif
            default:
                _text.text = parsedSeed;
                break;
        }
    }

    public void OnClick()
    {
        if (string.IsNullOrEmpty(_seed) || _seed == blankSeed)
        {
            UISounds.instance.UIFail();
        }
        else
        {
            switch (_seed)
            {
                case SeedHelper.ClassicBossRush:
                    SeedHelper.StartClassicBossRush();
                    break;
                case SeedHelper.Spooky:
                    SeedHelper.StartSpookyMode();
                    break;
                case SeedHelper.MirrorWorld:
                    SeedHelper.StartMirrorWorldMode();
                    break;
#if TRUECOOP
                case SeedHelper.TrueCoOp:
                    SeedHelper.StartTrueCoOpMode();
                    break;
#endif
                default:
                    var parameters = SeedHelper.KeyToParameters(_seed);
                    if (parameters != null)
                    {
                        var inputSeedScreen = GetComponentInParent<InputSeedScreen>();
                        var raceMode = inputSeedScreen ? inputSeedScreen.raceMode : false;
                        Debug.Log("Seeded Game on Slot: " + SaveGameManager.instance.selectedSaveSlot);
                        InputHelper.instance.AssignPlayer1LastActiveController();
                        SaveGameManager.instance.NewGame(GameMode.Normal, raceMode);
                        SaveGameManager.instance.Save();
                        SaveGameManager.activeGame.password = _seed;
                        UISounds.instance.Confirm();
                        SceneManager.LoadScene("NewGame");
                    }
                    break;
            }
        }
    }
}
