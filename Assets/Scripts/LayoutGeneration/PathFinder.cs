using CreativeSpore.SuperTilemapEditor;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[Serializable]
public class PathFinder
{
    private RoomLayout _layout;
    private float _inOrOutMod = 1;

    public List<Int2D> GetPath(RoomLayout layout, Int2D start, Int2D end, Func<RoomAbstract, Int2D, bool> validate, float inOrOutMod = 1)
    {
        _layout = layout;
        _inOrOutMod = inOrOutMod;
        var nodeMap = new AStarNode[layout.width, layout.height];
        AStarNode currentNode = null;
        AStarNode endNode = null;
        
        for (int y = 0; y < layout.height; y++)
        {
            for (int x = 0; x < layout.width; x++)
            {
                var gridPos = new Int2D(x, y);
                var room = layout.GetRoomAtPositon(gridPos);
                var node = new AStarNode()
                {
                    position = gridPos,
                    traversable = room == null || (validate != null && validate(room, gridPos)),
                };

                if (gridPos == start) { currentNode = node; }
                if (gridPos == end) { endNode = node; }

                nodeMap[gridPos.x, gridPos.y] = node;
            }
        }

        //Calculate H Values
        foreach (var node in nodeMap)
        {            
            node.heuristicMovementCost = (int)(Mathf.Abs(endNode.position.x - node.position.x) + Mathf.Abs(endNode.position.y - node.position.y));
        }

        bool endNodeReached = false;

        var openList = new HashSet<AStarNode>();
        openList.Add(currentNode);

        while (openList.Count > 0)
        {
            var lowestCostNode = openList.First();
            foreach (var node in openList)
            {
                //if two nodes are the same, using the last node added to the open list improves speed
                if (node.fullCost <= lowestCostNode.fullCost) { lowestCostNode = node; }
            }

            currentNode = lowestCostNode;
            if(currentNode == endNode)
            {
                endNodeReached = true;
                break;
            }

            currentNode.closed = true;

            ProcessAdjacentNodes(nodeMap, currentNode, openList);            

            openList.Remove(currentNode);
        }

        if (endNodeReached)
        {
            var path = new List<Int2D>();

            var previousPos = currentNode.position;
            path.Add(currentNode.position);            

            while (currentNode.parent != null)
            {
                var pos = currentNode.parent.position;
                //handle diagonals
                if(pos.x != previousPos.x && pos.y != previousPos.y)
                {
                    var corner = new Int2D(previousPos.x, pos.y);
                    if (nodeMap[corner.x, corner.y].traversable)
                    {
                        path.Add(corner);
                    }
                    else
                    {
                        corner = new Int2D(pos.x, previousPos.y);
                        if (nodeMap[corner.x, corner.y].traversable)
                        {
                            path.Add(corner);
                        }
                        else
                        {
                            Debug.LogError("Path Finder could not find corner between " + pos.ToString() + " and " + previousPos.ToString());
                        }
                    }
                }
                path.Add(pos);
                previousPos = pos;
                currentNode = currentNode.parent;
            }

            path.Reverse();
            return path;
        }

        return null;
    }

    public void ProcessAdjacentNodes(AStarNode[,] nodeMap, AStarNode parent, HashSet<AStarNode> openList)
    {        
        var width = nodeMap.GetUpperBound(0);
        var height = nodeMap.GetUpperBound(1);

        for (int y = -1; y <= 1; y++)
        {
            var gridY = parent.position.y + y;
            if (gridY > 0 && gridY <= height)
            {
                for (int x = -1; x <= 1; x++)
                {
                    var gridX = parent.position.x + x;
                    if (gridX > 0 && gridX < width)
                    {                        
                        var node = nodeMap[gridX, gridY];
                        var inOpenList = openList.Contains(node);
                        if (CompareNode(nodeMap, parent, node, inOpenList) && !inOpenList) { openList.Add(node); }
                    }
                }
            }
        }
    }

    public virtual bool CompareNode(AStarNode[,] nodeMap, AStarNode parent, AStarNode node, bool inOpenList)
    {
        if (node.traversable && !node.closed)
        {
            //check corners
            bool diagonol = false;
            if (node.position.x != parent.position.x && node.position.y != parent.position.y) //corner
            {
                diagonol = true;

                var corner = new Int2D(node.position.x, parent.position.y);
                if (nodeMap[corner.x,corner.y].traversable)
                {
                    if (_layout.GetRoomAtPositon(corner) != null) return false;
                }
                else
                {
                    return false;
                }

                corner = new Int2D(parent.position.x, node.position.y);
                if (nodeMap[corner.x, corner.y].traversable)
                {
                    if (_layout.GetRoomAtPositon(corner) != null) return false;
                }
                else
                {
                    return false;
                }
            }

            //can there's a room at parent can it connect to node?
            var roomAtParent = _layout.GetRoomAtPositon(parent.position);
            var roomAtNode = _layout.GetRoomAtPositon(node.position);
            var inOrOutOfRoom = roomAtParent != roomAtNode;

            if (inOrOutOfRoom) //if not moving within same room, check for exits between rooms
            {
                if (roomAtParent != null && !roomAtParent.HasAnyPathExitToGridPosition(node.position)) return false;
                if (roomAtNode != null && !roomAtNode.HasAnyPathExitToGridPosition(parent.position)) return false;
            }

            var moveCost = Int2D.Distance(parent.position, node.position);

            //if moving into room apply throughRoomMod
            if(roomAtNode != null && inOrOutOfRoom)
            {
                moveCost *= _inOrOutMod;
            }
            //if we're moving from a space in a room to another space in the same room it should cost less
            else if (roomAtParent != null && !inOrOutOfRoom)
            {
                moveCost *= (diagonol ? 0.5f : 0.8f);
            }

            var newGCost = parent.generatedMovementCost + moveCost;
            if (newGCost < node.generatedMovementCost || !inOpenList)
            {
                node.parent = parent;
                node.generatedMovementCost = newGCost;
                node.fullCost = node.heuristicMovementCost + node.generatedMovementCost;
            }

            return true;
        }

        return false;
    }
}

[Serializable]
public class AStarNode
{
    public Int2D position;
    public bool traversable;
    public float fullCost;
    public int heuristicMovementCost;
    public float generatedMovementCost;
    public AStarNode parent;
    public bool closed;
}
