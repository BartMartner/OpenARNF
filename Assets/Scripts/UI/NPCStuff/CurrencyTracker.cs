using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CurrencyTracker : MonoBehaviour
{
    public Text grayCurrency;
    public Text redCurrency;
    public Text greenCurrency;
    public Text blueCurrency;
    public GameObject grayBox;
    public GameObject redBox;
    public GameObject greenBox;
    public GameObject blueBox;
    public Player player;
    private Image _grayImage;
    private Image _redImage;
    private Image _greenImage;
    private Image _blueImage;
    private Color _originalBGColor;
    public bool forShop;
    private bool _expanded = true;

    private void Awake()
    {
        _grayImage = grayBox.GetComponent<Image>();
        _redImage = redBox.GetComponent<Image>();
        _greenImage = greenBox.GetComponent<Image>();
        _blueImage = blueBox.GetComponent<Image>();

        _originalBGColor = new Color32(0,0,0,64);
    }

    private void Start()
    {
        if (!player) { player = PlayerManager.instance.player1; }

        var alwaysVisible = SaveGameManager.activeSlot != null && SaveGameManager.activeSlot.scrapAlwaysVisible;
        if(!forShop && !alwaysVisible)
        {
            _expanded = false;
            grayBox.gameObject.SetActive(false);
            redBox.gameObject.SetActive(false);
            greenBox.gameObject.SetActive(false);
            blueBox.gameObject.SetActive(false);
        }
    }

    private void Update()
    {
        var alwaysVisible = SaveGameManager.activeSlot != null && SaveGameManager.activeSlot.scrapAlwaysVisible;
        if (!forShop && !alwaysVisible)
        {
            _expanded = player.controller.GetButton("ExpandMap");
        }

        if (player && (_expanded || alwaysVisible || forShop) && 
            (forShop || (!NPCDialogueManager.instance.dialogueActive && !Automap.instance.gridSelectMode)))
        {
            grayCurrency.text = player.grayScrap.ToString();
            redCurrency.text = player.redScrap.ToString();
            greenCurrency.text = player.greenScrap.ToString();
            blueCurrency.text = player.blueScrap.ToString();

            grayBox.gameObject.SetActive(true);
            redBox.gameObject.SetActive(true);
            greenBox.gameObject.SetActive(true);
            blueBox.gameObject.SetActive(true);

            if (_grayImage)
            {
                var color = alwaysVisible ? _originalBGColor : Color.clear;
                _grayImage.color = color;
                _redImage.color = color;
                _greenImage.color = color;
                _blueImage.color = color;
            }
        }
        else
        {
            if ((!forShop && (player.artificeMode || player.selectedEnergyWeapon is ArtificeBeam)))
            {
                grayCurrency.text = player.grayScrap.ToString();
                grayBox.gameObject.SetActive(true);
            }
            else
            {
                grayBox.gameObject.SetActive(false);
            }
            redBox.gameObject.SetActive(false);
            greenBox.gameObject.SetActive(false);
            blueBox.gameObject.SetActive(false);
        }
    }
}
