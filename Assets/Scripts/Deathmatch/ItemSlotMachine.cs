using Rewired;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ItemSlotMachine : MonoBehaviour
{
    public float initialSpeed = 50;
    public float speed = 50;
    public float speedUpFactor = 5f;
    public float iconSize = 32;
    public MajorItem[] slot0Items;
    public MajorItem[] slot1Items;
    public MajorItem[] slot2Items;
    public Image[] slot0Icons;
    public Image[] slot1Icons;
    public Image[] slot2Icons;
    public Image[] activeArrows;
    public Image[] blackOut;
    public bool complete;
    private int _currentSlot = 0;
    private int _slot0LastIndex;
    private int[] _slot0ItemIndices = new int[] { 0, 1 };
    private int _slot1LastIndex;
    private int[] _slot1ItemIndices = new int[] { 0, 1 };
    private int _slot2LastIndex;
    private int[] _slot2ItemIndices = new int[] { 0, 1 };
    private Rewired.Player _controller;
    private bool _initialized;
    public List<MajorItem> result = new List<MajorItem>();
    private float _timeLimit = 5;
    private float _timer = 0;

    private void Start()
    {
        if (!_initialized)
        {
            Initialize(0, null, null, null);
        }
    }

    public void Initialize(int id, MajorItem[] items0, MajorItem[] items1, MajorItem[] items2)
    {
        _currentSlot = 0;
        _controller = ReInput.players.GetPlayer(id);
        speed = initialSpeed;
        complete = false;
        result.Clear();

        if (items0 != null) slot0Items = items0;
        if (items1 != null) slot1Items = items1;
        if (items2 != null) slot2Items = items2;

        _slot0LastIndex = 1;
        slot0Icons[0].transform.localPosition = Vector3.zero;
        slot0Icons[0].sprite = ItemManager.items[slot0Items[_slot0ItemIndices[0]]].icon;
        slot0Icons[1].transform.localPosition = Vector3.up * iconSize;
        slot0Icons[1].sprite = ItemManager.items[slot0Items[_slot0ItemIndices[1]]].icon;

        _slot1LastIndex = 1;
        slot1Icons[0].transform.localPosition = Vector3.zero;
        slot1Icons[0].sprite = ItemManager.items[slot1Items[_slot1ItemIndices[0]]].icon;
        slot1Icons[1].transform.localPosition = Vector3.up * iconSize;
        slot1Icons[1].sprite = ItemManager.items[slot1Items[_slot1ItemIndices[1]]].icon;

        _slot2LastIndex = 1;
        slot2Icons[0].transform.localPosition = Vector3.zero;
        slot2Icons[0].sprite = ItemManager.items[slot2Items[_slot2ItemIndices[0]]].icon;
        slot2Icons[1].transform.localPosition = Vector3.up * iconSize;
        slot2Icons[1].sprite = ItemManager.items[slot2Items[_slot2ItemIndices[1]]].icon;

        _initialized = true;
    }

    public void Update ()
    {
        if (complete || !_initialized) return;

        if (_currentSlot == 0) { UpdateSlots(slot0Icons, _slot0ItemIndices, slot0Items, ref _slot0LastIndex); }
        if (_currentSlot == 1) { UpdateSlots(slot1Icons, _slot1ItemIndices, slot1Items, ref _slot1LastIndex); }
        if (_currentSlot == 2) { UpdateSlots(slot2Icons, _slot2ItemIndices, slot2Items, ref _slot2LastIndex); }

        _timer += Time.deltaTime;

        if(_controller.GetButtonDown("UISubmit") || _timer > _timeLimit)
        {
            UISounds.instance.Confirm();

            switch (_currentSlot)
            {
                case 0:
                    StartCoroutine(StopSlot(slot0Icons, _slot0ItemIndices, slot0Items));
                    break;
                case 1:
                    StartCoroutine(StopSlot(slot1Icons, _slot1ItemIndices, slot1Items));
                    break;
                case 2:
                    StartCoroutine(StopSlot(slot2Icons, _slot2ItemIndices, slot2Items));
                    break;
            }

            _currentSlot++;
            _timer = 0;

            if (_currentSlot > 2) { complete = true; }
        }

        for (int i = 0; i < activeArrows.Length; i++)
        {
            activeArrows[i].gameObject.SetActive(i == _currentSlot);
        }

        for (int i = 0; i < blackOut.Length; i++)
        {
            var image = blackOut[i];

            if (i == _currentSlot && _timer > _timeLimit-1)
            {
                image.gameObject.SetActive(true);
                image.color = _timer % 0.2 > 0.1f ? Color.clear : new Color(1,0,0,0.5f);
            }
            else
            {
                image.gameObject.SetActive(i > _currentSlot);
                image.color = new Color(0, 0, 0, 0.5f);
            }
            
        }
    }

    public void UpdateSlots(Image[] icons, int[] indices, MajorItem[] items, ref int lastIndex)
    {   
        for (int i = 0; i < icons.Length; i++)
        {
            var icon = icons[i];
            var pos = icon.transform.localPosition;
            pos += Vector3.down * Time.deltaTime * speed;
            pos.y = Mathf.Clamp(pos.y, -iconSize, iconSize);
            icon.transform.localPosition = pos;
        }

        for (int i = 0; i < icons.Length; i++)
        {
            var icon = icons[i];
            var pos = icon.transform.localPosition;
            if (pos.y <= -iconSize)
            {
                pos = (i == 0 ? icons[1] : icons[0]).transform.localPosition + Vector3.up * iconSize;
                lastIndex = (lastIndex + 1) % items.Length;
                indices[i] = lastIndex;
                icon.sprite = ItemManager.items[items[lastIndex]].icon;
                icon.transform.localPosition = pos;
            }
        }
    }

    private IEnumerator StopSlot(Image[] icons, int[] indices, MajorItem[] items)
    {
        var item0 = icons[0];
        var item1 = icons[1];

        var chosenIndex = item0.transform.localPosition.y > item1.transform.localPosition.y ? 0 : 1;

        result.Add(items[indices[chosenIndex]]);

        speed *= speedUpFactor;

        var chosenItem = icons[chosenIndex];
        var notChosenItem = icons[chosenIndex == 0 ? 1 : 0];

        while (icons[chosenIndex].transform.localPosition.y != 0)
        {
            icons[0].transform.localPosition += Vector3.down * Time.deltaTime * 50;
            icons[1].transform.localPosition += Vector3.down * Time.deltaTime * 50;

            if (chosenItem.transform.localPosition.y < 0)
            {
                chosenItem.transform.localPosition = Vector3.zero;
                notChosenItem.transform.localPosition = Vector3.up * iconSize;
            }

            yield return null;
        }
    }
}
