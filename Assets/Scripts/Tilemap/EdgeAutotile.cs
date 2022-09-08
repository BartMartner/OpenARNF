using CreativeSpore.SuperTilemapEditor;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class EdgeAutotile : MonoBehaviour
{
    public STETilemap parentTilemap;
    private STETilemap _edgeMap;
    private bool _initialized = false;

    public TilesetBrush fill;
    public TilesetBrush edge; //0
    public TilesetBrush leftEdge; //1
    public TilesetBrush upEdge; //2
    public TilesetBrush rightEdge; //4
    public TilesetBrush downEdge; //8

    private int _minX;
    private int _maxX;
    private int _minY;
    private int _maxY;
    private bool _dirty;
    private bool _updating;

    private void Initialize()
    {
        _initialized = true;
        _edgeMap = GetComponent<STETilemap>();
        _edgeMap.OnTileChanged += OnTileChanged;
        if (parentTilemap)
        {
            parentTilemap.OnTileChanged += OnTileChanged;
            for (int x = parentTilemap.MinGridX; x <= parentTilemap.MaxGridX; x++)
            {
                for (int y = parentTilemap.MinGridY; y < parentTilemap.MaxGridY; y++)
                {
                    SetTile(x, y);
                }
            }
            _edgeMap.SetMapBounds(parentTilemap.MinGridX, parentTilemap.MinGridY, parentTilemap.MaxGridX, parentTilemap.MaxGridY);
        }
    }

    private void OnTileChanged(STETilemap tilemap, int gridX, int gridY, uint tileData)
    {
        if (_updating) return;
        if(parentTilemap && tilemap == parentTilemap)
        {
            _edgeMap.SetMapBounds(parentTilemap.MinGridX, parentTilemap.MinGridY, parentTilemap.MaxGridX, parentTilemap.MaxGridY);
        }

        _dirty = true;
        var minX = tilemap.MinGridX > gridX - 1 ? tilemap.MinGridX : gridX - 1;
        var maxX = tilemap.MaxGridX < gridX + 1 ? tilemap.MaxGridX : gridX + 1;
        var minY = tilemap.MinGridY > gridY - 1 ? tilemap.MinGridY : gridY - 1;
        var maxY = tilemap.MaxGridY < gridY + 1 ? tilemap.MaxGridY : gridY + 1;

        if (minX < _minX) { _minX = minX; }
        if (maxX > _maxX) { _maxX = maxX; }
        if (minY < _minY) { _minY = minY; }
        if (maxY > _maxY) { _maxY = maxY; }
    }

    private void SetTile(int x, int y)
    {
        var et = _edgeMap.GetBrush(x, y);
        if (et == fill) { return; }
        if (et && et != edge) { return; }
        if (!et && _edgeMap.GetTile(x, y) != null) { return; }

        if (parentTilemap)
        {
            var pt = parentTilemap.GetTile(x, y);
            if (pt == null)
            {
                _edgeMap.SetTileData(x, y, Tileset.k_TileData_Empty);
                return;
            }
        }

        bool r = x < _edgeMap.MaxGridX && _edgeMap.GetBrush(x + 1, y) == fill;
        bool l = x > _edgeMap.MinGridX && _edgeMap.GetBrush(x - 1, y) == fill;
        bool u = y < _edgeMap.MaxGridY && _edgeMap.GetBrush(x, y + 1) == fill;
        bool d = y > _edgeMap.MinGridY && _edgeMap.GetBrush(x, y - 1) == fill;

        bool dl = _edgeMap.GetBrush(x - 1, y - 1) == fill;
        bool dr = _edgeMap.GetBrush(x + 1, y - 1) == fill;
        bool ul = _edgeMap.GetBrush(x - 1, y + 1) == fill;
        bool ur = _edgeMap.GetBrush(x + 1, y + 1) == fill;

        var idID = 0;
        bool skip = false;

        if (parentTilemap)
        {
            if (dl && (!l || !d))
            {
                skip = parentTilemap.GetTile(x - 1, y) == null || parentTilemap.GetTile(x, y - 1) == null;
            }
            else if (ul && (!u || !l))
            {
                skip = parentTilemap.GetTile(x - 1, y) == null || parentTilemap.GetTile(x, y + 1) == null;
            }
            else if (dr && (!r || !d))
            {
                skip = parentTilemap.GetTile(x + 1, y) == null || parentTilemap.GetTile(x, y - 1) == null;
            }
            else if (ur && (!u || !r))
            {
                skip = parentTilemap.GetTile(x + 1, y) == null || parentTilemap.GetTile(x, y + 1) == null;
            }
        }

        if (skip)
        {
            _edgeMap.SetTileData(x, y, Tileset.k_TileData_Empty);
            return;
        }

        if (l || dl || ul) { idID += 1; }
        if (u || ul || ur) { idID += 2; }
        if (r || dr || ur) { idID += 4; }
        if (d || dl || ul) { idID += 8; }

        if (idID == 0)
        {
            _edgeMap.SetTileData(x, y, Tileset.k_TileData_Empty);
            return;
        }

        TilesetBrush tBrush = edge;

        if (parentTilemap)
        {
            r = x < parentTilemap.MaxGridX && parentTilemap.GetTile(x + 1, y) == null;
            l = x > parentTilemap.MinGridX && parentTilemap.GetTile(x - 1, y) == null;
            u = y < parentTilemap.MaxGridY && parentTilemap.GetTile(x, y + 1) == null;
            d = y > parentTilemap.MinGridY && parentTilemap.GetTile(x, y - 1) == null;
            dl = ul = dr = ur = false;

            idID = 0;
            if (l || dl || ul) { idID += 1; }
            if (u || ul || ur) { idID += 2; }
            if (r || dr || ur) { idID += 4; }
            if (d || dl || ul) { idID += 8; }

            switch (idID)
            {
                case 1:
                    if (leftEdge) { tBrush = leftEdge; }
                    break;
                case 2:
                    if (upEdge) { tBrush = upEdge; }
                    break;
                case 4:
                    if (rightEdge) { tBrush = rightEdge; }
                    break;
                case 8:
                    if (downEdge) { tBrush = downEdge; }
                    break;
            }
        }

        var tId = tBrush.Tileset.FindBrushId(tBrush.name);
        _edgeMap.SetTile(x, y, 0, tId);
    }

    private void Update()
    {
        _updating = true;

        if (!_initialized) { Initialize(); }
        
        if (_dirty)
        {
            _dirty = false;
            for (int x = _minX; x <= _maxX; x++)
            {
                for (int y = _minY; y <= _maxY; y++)
                {
                    SetTile(x, y);
                }
            }
            _edgeMap.Refresh();

            _minY = _minX = int.MaxValue;
            _maxY = _maxX = int.MinValue;
        }

        _updating = false;
    }

    private void OnDisable()
    {
        _initialized = false;
        if(parentTilemap) { parentTilemap.OnTileChanged -= OnTileChanged; }
    }
}
