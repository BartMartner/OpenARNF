using CreativeSpore.SuperTilemapEditor;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GravStarNode
{
    public int indexX;
    public int indexY;
    public Vector2 position;
    public bool leftEdge;
    public bool rightEdge;
    public float verticalClearance;
    public List<NeighborConnection> neighbors = new List<NeighborConnection>();
    public Vector2Int index { get { return new Vector2Int(indexX, indexY); } }

    //pathing vars
    public float fullCost;
    public float pathDistanceEnd;
    public bool open;
    public bool closed;
    public bool heuristicFound;
    public GravStarNode parent;
    public float heuristicMovementCost;
    public float generatedMovementCost;

    public virtual void Reset()
    {
        fullCost = 0;
        pathDistanceEnd = 0;
        closed = false;
        open = false;
        heuristicFound = false;
        parent = null;
        generatedMovementCost = 0;
    }
}

public struct NeighborConnection
{
    public float clearance;
    public int x;
    public int y;

    public NeighborConnection(int x, int y, float clearance)
    {
        this.x = x;
        this.y = y;
        this.clearance = clearance;
    }

    public bool MatchesNode(GravStarNode node)
    {
        return node.indexX == x && node.indexY == y;
    }
}
