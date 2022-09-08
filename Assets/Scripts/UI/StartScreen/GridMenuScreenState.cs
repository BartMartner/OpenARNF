using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public abstract class GridMenuScreenState : ScreenState
{
    public int perPage = 60;
    public int columns = 12;
    public ScreenState previousState;
    public GridLayoutGroup gridLayoutGroup;
    public Color darkenedColor = new Color(0,0,0,0.5f);
    new public Text name;
    public Text descriptor;
    public Image cursor;
    public Image pageBox;
    public Text pageNumber;
    protected List<Image> _icons = new List<Image>();
    protected List<Image> _activeIcons = new List<Image>();
    protected abstract IList _collection { get; }
    protected List<int> _activeCollectionIndices = new List<int>();
    public int activeIndex
    {
        get { return _cursorIndex + _currentPage * perPage; }
    }

    protected int _currentPage = 0;
    protected int _cursorIndex = 0;
    protected float _lastOptionChangeDelay;
    protected float _optionChangeDelay;

    public abstract string GetName(int collectionIndex);
    public abstract string GetDescriptor(int collectionIndex);

    /// <summary>
    /// Should the sprite at collection grid index appear bright? Otherwise its color is set to darkenedColor
    /// </summary>
    /// <param name="collectionIndex"></param>
    /// <returns></returns>
    public abstract Sprite GetSprite(int collectionIndex);

    /// <summary>
    /// Should the sprite associated with this collection index appear bright? Otherwise its color is set to darkenedColor
    /// </summary>
    /// <param name="collectionIndex"></param>
    /// <returns></returns>
    public abstract bool GetEnabled(int collectionIndex);

    /// <summary>
    /// Should the gameObject associated with the collection item at this index be active?
    /// </summary>
    /// <param name="collectionIndex"></param>
    /// <returns></returns>
    public abstract bool GetActive(int collectionIndex);

    protected override void Start()
    {
        base.Start();

        for (int i = 0; i < perPage; i++)
        {
            var image = new GameObject().AddComponent<Image>();
            image.transform.SetParent(gridLayoutGroup.transform);
            image.transform.localScale = Vector3.one;
            _icons.Add(image);
        }

        cursor.rectTransform.position = _icons[0].rectTransform.position;

        Refresh();
    }

    public void Refresh()
    {
        if (_icons.Count <= 0) { return; }

        _activeIcons.Clear();
        _activeCollectionIndices.Clear();

        for (int i = 0; i < _collection.Count; i++)
        {
            if(GetActive(i))
            {
                _activeCollectionIndices.Add(i);
            }
        }

        for (int i = 0; i < _icons.Count; i++)
        {
            var activeIndex = i + _currentPage * perPage;

            if (activeIndex >= _activeCollectionIndices.Count)
            {
                _icons[i].gameObject.SetActive(false);
                continue;
            }

            var collectionIndex = _activeCollectionIndices[activeIndex];

            _icons[i].sprite = GetSprite(collectionIndex);
            _icons[i].color = GetEnabled(collectionIndex) ? Color.white : darkenedColor;
            _icons[i].gameObject.SetActive(GetActive(collectionIndex));

            if (_icons[i].gameObject.activeSelf)
            {
                _activeIcons.Add(_icons[i]);
            }
        }

        var pages = Mathf.CeilToInt(_activeCollectionIndices.Count / (float)perPage);

        if(pages <= 1)
        {
            pageBox.gameObject.SetActive(false);
        }
        else
        {
            pageBox.gameObject.SetActive(true);
            pageNumber.text = (_currentPage + 1).ToString() + "/" + pages;
        }
    }

    public override void ReadyUpdate()
    {
        if (_controller.GetButtonDown("UICancel"))
        {
            UISounds.instance.Cancel();
            GoToState(previousState);
        }

        if (_optionChangeDelay > 0) { _optionChangeDelay -= Time.deltaTime; }

        var rows = Mathf.CeilToInt(_activeIcons.Count / (float)columns);
        var currentRow = _cursorIndex / columns;
        var currentColumn = _cursorIndex % columns;

        if (GetButton("UIRight"))
        {
            currentColumn = (currentColumn + 1);
            if (currentColumn >= GetColumnsInRow(currentRow))
            {
                currentColumn = 0;
                currentRow += 1;
            }
        }

        if (GetButton("UILeft"))
        {
            currentColumn = (currentColumn - 1);
            if (currentColumn < 0)
            {
                currentRow -= 1;
                currentColumn = GetColumnsInRow(currentRow) - 1;
            }
        }

        if (GetButton("UIUp")) { currentRow = (currentRow - 1); }
        if (GetButton("UIDown")) { currentRow = (currentRow + 1); }

        var pages = Mathf.CeilToInt(_activeCollectionIndices.Count / (float)perPage);

        if (currentRow >= rows)
        {
            if(pages > 1)
            {
                _currentPage = (_currentPage + 1) % pages;
                Refresh();
                currentRow = 0;
            }
            else
            {
                currentRow = currentRow % rows;
            }
        }
        else if(currentRow < 0)
        {
            if(pages > 1)
            {
                _currentPage = _currentPage - 1;
                if (_currentPage < 0) { _currentPage = pages - 1; }

                Refresh();
                rows = Mathf.CeilToInt(_activeIcons.Count / (float)columns);
                currentRow = rows - 1;
            }
            else
            {
                currentRow = rows - 1;
            }
        }

        _cursorIndex = (currentRow * columns) + currentColumn;
        _cursorIndex = Mathf.Clamp(_cursorIndex, 0, _activeIcons.Count - 1);

        var collectionIndex = _activeCollectionIndices[_cursorIndex + _currentPage * perPage];
        cursor.rectTransform.position = _activeIcons[_cursorIndex].rectTransform.position;
        name.text = GetName(collectionIndex);
        descriptor.text = GetDescriptor(collectionIndex);
    }

    private int GetColumnsInRow(int row)
    {        
        var rows = Mathf.CeilToInt(_activeIcons.Count / (float)columns);
        var columnsInRow = row < (rows - 1) ? columns : _activeIcons.Count % columns;
        if (columnsInRow == 0) columnsInRow = columns;
        return columnsInRow;
    }

    public override void AppearStart()
    {
        _currentPage = 0;
        _cursorIndex = 0;
        base.AppearStart();
        Refresh();
    }

    public bool GetButton(string button)
    {
        if (controller.GetButtonDown(button))
        {
            _optionChangeDelay = 0.5f;
            _lastOptionChangeDelay = 0.5f;
            return true;
        }
        else if (_optionChangeDelay <= 0 && controller.GetButton(button))
        {
            _optionChangeDelay = Mathf.Clamp(_lastOptionChangeDelay * 0.65f, 0.05f, 0.5f);
            _lastOptionChangeDelay = _optionChangeDelay;
            return true;
        }
        else
        {
            return false;
        }
    }
}
