using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GridStarPathFinder : BasePathFinder
{
    public override GravStarNode GetPathLinkedList(GravStarNode currentNode, GravStarNode endNode, BaseAgentStats stats, bool greedy = false)
    {
        //jump point only works for uniform heights
        if(stats.height > 1) { return base.GetPathLinkedList(currentNode, endNode, stats, greedy); }

        TryRefresh();
        foreach (var node in _nonNullNodes) { node.Reset(); }
        _openList.Clear();
        _openList.Add(currentNode);
        currentNode.open = true;
        int currentIndex = 0;
        int count = 1;

        //Navigate nodes and find a path
        while (count > 0)
        {
            currentIndex = 0;
            currentNode = _openList[0];
            for (int i = 0; i < count; i++)
            {
                var node = _openList[i];
                if (node.fullCost <= currentNode.fullCost)
                {
                    currentNode = node;
                    currentIndex = i;
                }
            }

            currentNode.closed = true;
            currentNode.open = false;
            _openList.RemoveAt(currentIndex);

            if (currentNode == endNode) { return currentNode; }

            //process neighbors
            Vector2Int? jumpPoint;
            GravStarNode jumpNode;

            var neighbors = FindJumpNeighbors(stats, currentNode);

            foreach (var node in neighbors)
            {
                jumpPoint = Jump(node.indexX, node.indexY, currentNode.indexX, currentNode.indexY, endNode);

                if (jumpPoint != null)
                {
                    jumpNode = _nodeMap[jumpPoint.Value.x, jumpPoint.Value.y];
                    if (jumpNode == null || jumpNode.closed) { continue; }

                    SetHeuristicCost(jumpNode, endNode);

                    if (CompareNode(currentNode, jumpNode))
                    {
                        jumpNode.open = true;
                        _openList.Add(jumpNode);
                    }
                }
            }

            count = _openList.Count;
        }

        return null;
    }

    private List<GravStarNode> FindJumpNeighbors(BaseAgentStats stats, GravStarNode currentNode)
    {
        int currentX = currentNode.indexX;
        int currentY = currentNode.indexY;
        int parentX, parentY, directionX, directionY;
        List<GravStarNode> neighbors = new List<GravStarNode>();

        // directed pruning: can ignore most neighbors, unless forced.
        if (currentNode.parent != null)
        {
            parentX = currentNode.parent.indexX;
            parentY = currentNode.parent.indexY;
            // get the normalized direction of travel
            directionX = (currentX - parentX) / Math.Max(Math.Abs(currentX - parentX), 1);
            directionY = (currentY - parentY) / Math.Max(Math.Abs(currentY - parentY), 1);

            // search diagonally
            if (directionX != 0 && directionY != 0)
            {
                var yNode = NodeAtIndex(currentX, currentY + directionY, false);
                if (yNode != null) { neighbors.Add(yNode); }

                var xNode = NodeAtIndex(currentX + directionX, currentY, false);
                if (xNode != null) { neighbors.Add(xNode); }

                var dNode = NodeAtIndex(currentX + directionX, currentY + directionY, false);
                if (dNode != null && xNode != null && yNode != null) { neighbors.Add(dNode); }

                var negXDNode = NodeAtIndex(currentX - directionX, currentY + directionY, false);
                if (negXDNode != null && yNode != null)
                {
                    var negXNode = NodeAtIndex(currentX - directionX, currentY, false);
                    if (negXNode != null) { neighbors.Add(negXDNode); }
                }

                var negYDNode = NodeAtIndex(currentX + directionX, currentY - directionY);
                if (negYDNode != null && xNode != null)
                {
                    var negYNode = NodeAtIndex(currentX, currentY - directionY, false);
                    if (negYNode != null) { neighbors.Add(negYDNode); }
                }
            }
            else // search horizontally/vertically
            {
                if (directionX != 0)
                {
                    var xNode = NodeAtIndex(currentX + directionX, currentY, false);
                    var upNode = NodeAtIndex(currentX, currentY + 1, false);
                    var downNode = NodeAtIndex(currentX, currentY - 1, false);

                    if (xNode != null)
                    {
                        neighbors.Add(xNode);
                        var xUpNode = NodeAtIndex(currentX + directionX, currentY + 1, false);
                        var xDownNode = NodeAtIndex(currentX + directionX, currentY - 1, false);

                        if (xUpNode != null && upNode != null) { neighbors.Add(xUpNode); }
                        if (xDownNode != null && downNode != null) { neighbors.Add(xDownNode); }
                    }

                    if (upNode != null) { neighbors.Add(upNode); }
                    if (downNode != null) { neighbors.Add(downNode); }
                }
                else
                {
                    var yNode = NodeAtIndex(currentX, currentY + directionY, false);
                    var rightNode = NodeAtIndex(currentX + 1, currentY, false);
                    var leftNode = NodeAtIndex(currentX - 1, currentY, false);

                    if (yNode != null)
                    {
                        neighbors.Add(yNode);

                        var yRightNode = NodeAtIndex(currentX + 1, currentY + directionY, false);
                        var yLeftNode = NodeAtIndex(currentX - 1, currentY + directionY, false);

                        if (yRightNode != null && rightNode != null) { neighbors.Add(yRightNode); }
                        if (yLeftNode != null && leftNode != null) { neighbors.Add(yLeftNode); }
                    }

                    if (rightNode != null) { neighbors.Add(rightNode); }
                    if (leftNode != null) { neighbors.Add(leftNode); }
                }
            }
        }
        else //return all neighbors
        {
            //clearance height breaks it!
            for (int x = -1; x <= 1; x++)
            {
                for (int y = -1; y <= 1; y++)
                {
                    if (x == 0 && y == 0) { continue; }
                    var iX = currentNode.indexX + x;
                    var iY = currentNode.indexY + y;
                    if (iX >= 0 && iX < _width && iY >= 0 && iY < _height)
                    {
                        var diagonal = x != 0 && y != 0;
                        if (diagonal && (_nodeMap[iX, currentNode.indexY] == null || _nodeMap[currentNode.indexX, iY] == null)) { continue; }
                        var potentialNeighbor = _nodeMap[iX, iY];
                        if (potentialNeighbor == null) continue;
                        neighbors.Add(potentialNeighbor);
                    }
                }
            }
        }

        return neighbors;
    }

    private Vector2Int? Jump(int iX, int iY, int parentX, int parentY, GravStarNode endNode)
    {
        var iNode = NodeAtIndex(iX, iY, false);

        if (iNode == null)
        {
            return null;
        }
        else if (iNode == endNode)
        {
            return new Vector2Int(iX, iY);
        }

        int directionX = iX - parentX;
        int directionY = iY - parentY;
        GravStarNode xNode, yNode;

        // check for forced neighbors
        // along the diagonal
        if (directionX != 0 && directionY != 0)
        {
            var dNode = NodeAtIndex(iX + directionX, iY + directionY, false);
            yNode = NodeAtIndex(iX, iY + directionY, false);
            xNode = NodeAtIndex(iX + directionX, iY, false);
            if (dNode != null && (yNode == null || xNode == null))
            {
                return new Vector2Int(iX, iY);
            }
        }
        // horizontally/vertically
        else
        {
            if (directionX != 0)
            {
                var upNode = NodeAtIndex(iX, iY + 1, false);
                var downNode = NodeAtIndex(iX, iY - 1, false);
                var negXUpNode = NodeAtIndex(iX - directionX, iY + 1, false);
                var negXDownNode = NodeAtIndex(iX - directionX, iY - 1, false);

                // moving along x
                if ((upNode != null && negXUpNode == null) ||
                    (downNode != null && negXDownNode == null))
                {
                    return new Vector2Int(iX, iY);
                }
            }
            else
            {
                var rightNode = NodeAtIndex(iX + 1, iY, false);
                var leftNode = NodeAtIndex(iX - 1, iY, false);
                var negYRightNode = NodeAtIndex(iX + 1, iY - directionY, false);
                var negYLeftNode = NodeAtIndex(iX - 1, iY - directionY, false);

                if ((rightNode != null && negYRightNode == null) ||
                    (leftNode != null && negYLeftNode == null))
                {
                    return new Vector2Int(iX, iY);
                }
            }
        }

        // when moving diagonally, must check for vertical/horizontal jump points
        if (directionX != 0 && directionY != 0)
        {
            if (Jump(iX + directionX, iY, iX, iY, endNode) != null) return new Vector2Int(iX, iY);
            if (Jump(iX, iY + directionY, iX, iY, endNode) != null) return new Vector2Int(iX, iY);
        }

        // moving diagonally, must make sure both of the vertical/horizontal
        // neighbors is open to allow the path
        xNode = NodeAtIndex(iX + directionX, iY, false);
        yNode = NodeAtIndex(iX, iY + directionY, false);
        if (xNode != null && yNode != null)
        {
            return Jump(iX + directionX, iY + directionY, iX, iY, endNode);
        }
        else
        {
            return null;
        }
    }

    public override void DebugDrawNodes(BaseAgentStats stats)
    {
        var t = 1 / 30f;
        if (_nodeMap == null) return;

        foreach (var n in _nodeMap)
        {
            if (n == null) continue;

            var vcs = n.position + Vector2.down * 0.5f;
            var vce = vcs + Vector2.up * n.verticalClearance;
            Extensions.DrawDashedLine(vcs, vce, 0.1f, new Color(.5f, .5f, .5f, .5f), t);

            var nodeColor = n.leftEdge || n.rightEdge ? Color.cyan : Color.green;
            Extensions.DrawX(n.position, 0.5f, nodeColor, 1 / 30f);

            foreach (var connection in n.neighbors)
            {
                var n2 = _nodeMap[connection.x, connection.y];
                var d = (n2.position - n.position).sqrMagnitude;

                if (connection.clearance < stats.height || !stats.CanNavigate(n, n2))
                {
                    Extensions.DrawDashedLine(n.position, n2.position, .25f, new Color(0.5f, 0.5f, 0.5f, 0.5f), t);
                }
                else
                {
                    Debug.DrawLine(n.position, n2.position, Color.blue, t);
                }
            }
        }
    }

    public override void FindNeighbors(GravStarNode node)
    {
        for (int x = -1; x <= 1; x++)
        {
            for (int y = -1; y <= 1; y++)
            {
                if (x == 0 && y == 0) continue;
                var iX = node.indexX + x;
                var iY = node.indexY + y;
                if (iX >= 0 && iX < _width && iY >= 0 && iY < _height)
                {
                    var diagonal = (x == 1 || x == -1) && (y == 1 || y == -1);
                    if (diagonal && (_nodeMap[iX, node.indexY] == null || _nodeMap[node.indexX, iY] == null)) { continue; }

                    var potentialNeighbor = _nodeMap[iX, iY];
                    if(potentialNeighbor != null)
                    {
                        node.neighbors.Add(new NeighborConnection(iX, iY, potentialNeighbor.verticalClearance));
                    }
                }
            }
        }
    }
    
    protected override void SetNode(int x, int y)
    {
        var traversable = _tiles[x, y] == 0;
        var nodePosition = new Vector3(x + _minX, y + _minY);

        if (BlockedBySensitives(nodePosition))
        {
            traversable = false;
            return;
        }

        if (traversable)
        {
            var node = new GravStarNode()
            {
                indexX = x,
                indexY = y,
                position = nodePosition,
            };

            SetEdgeStatus(node);
            SetVerticalClearance(node);

            _nodeMap[x, y] = node;
            _nonNullNodes.Add(node);
        }
    }

    public override List<Vector2> ComputeVectorPath(GravStarNode currentNode)
    {
        var path = new List<Vector2>();
        path.Add(currentNode.position);
        var lastNodeAdded = currentNode;
        while (currentNode.parent != null)
        {
            var parent = currentNode.parent;
            var parentParent = currentNode.parent.parent;
            if (parentParent != null)
            {
                if (lastNodeAdded.indexX == parentParent.indexX || lastNodeAdded.indexY == parentParent.indexY)
                {
                    currentNode = parent;
                    continue;
                }

                var delta = lastNodeAdded.index - parentParent.index;                
                if (Mathf.Abs(delta.x) == Mathf.Abs(delta.y))
                {
                    var index = parentParent.index;
                    var dX = (int)Mathf.Sign(delta.x);
                    var dY = (int)Mathf.Sign(delta.y);
                    bool clear = true;
                    while (index != lastNodeAdded.index)
                    {
                        var iX = index.x + dX;
                        var iY = index.y + dY;
                        if (_nodeMap[iX, index.y] == null || _nodeMap[index.x, iY] == null)
                        {
                            clear = false;
                            break;
                        }
                        index.x = iX;
                        index.y = iY;
                    }

                    if(clear)
                    {
                        currentNode = parent;
                        continue;
                    }
                }
                else if (!TileCast(parentParent.indexX, parentParent.indexY, lastNodeAdded.indexX, lastNodeAdded.indexY))
                {
                    currentNode = parent;
                    continue;
                }
            }
            path.Add(parent.position);
            currentNode = parent;
            lastNodeAdded = currentNode;
        }

        path.Reverse();
        return path;
    }
}
