//#define DrawTileCast

using CreativeSpore.SuperTilemapEditor;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class GravStarPathFinder : BasePathFinder
{
    protected override void SetNode(int x, int y)
    {
        var traversable = _tiles[x, y] == 0;
        var nodePosition = new Vector3(x + _minX, y + _minY);

        if (BlockedBySensitives(nodePosition))
        {
            traversable = false;
            return;
        }

        //only include floor nodes if using GravityWorldNodes
        if (traversable)
        {
            if (y == 0) return;

            var tile = _tiles[x, y - 1];
            if (tile == 0 && !BlockedBySensitives(nodePosition + Vector3.down))
            {
                traversable = false;
            }
            else
            {
                traversable = true;

                switch (tile)
                {
                    case 2:
                        nodePosition.y -= 0.5f;
                        break;
                    case 3:
                        nodePosition.y -= 0.25f;
                        break;
                    case 4:
                        nodePosition.y -= 0.75f;
                        break;
                }
            }
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

    /// <summary>
    /// Find neighboring nodes that the passed in node could potentially path to
    /// </summary>
    /// <param name="node">the node for which we're trying to find neighbors</param>
    public override void FindNeighbors(GravStarNode node)
    {
        var bestDistance = 0f;
        var bestClearance = node.verticalClearance;
        GravStarNode bestNode = null;

        node.neighbors.Clear();
        float pathTolerance = 2.01f - _sqrt2;
        float pathLength;
        float expectedDistance;
        int maxY = Mathf.Clamp(node.indexY + GravStarAgent.maxAgentVertLeap, 0, _height);
        int minX = Mathf.Clamp(node.indexX - GravStarAgent.maxAgentHorizLeap, 0, _width);
        int maxX = Mathf.Clamp(node.indexX + GravStarAgent.maxAgentHorizLeap, 0, _width);

        //explore rows and find closest reachable node in each row
        for (int y = node.indexY-1; y < maxY; y++) //explore all nodes in vertical range of max jump
        {
            if (y < 0) continue;

            //left search
            bestDistance = float.MaxValue;
            bestNode = null;

            for (int x = node.indexX; x >= minX; x--)
            {
                var potentialNeighbor = _nodeMap[x, y];
                if (potentialNeighbor == null || potentialNeighbor == node) continue;

                if (x == node.indexX - 1 && Math.Abs(potentialNeighbor.position.y - node.position.y) < 1)
                {
                    //the tile directly to the left is valid
                    bestClearance = Mathf.Min(potentialNeighbor.verticalClearance, node.verticalClearance);
                    bestNode = potentialNeighbor;
                    break;
                }

                if (y == node.indexY - 1) continue; //for the row directly below, only worry about slope connections checked above

                expectedDistance = GetExpectedDistance(node.position, potentialNeighbor.position);
                if (expectedDistance > bestDistance) { continue; }

                if (x == node.indexX)
                {
                    if ((node.leftEdge && potentialNeighbor.leftEdge) || (node.rightEdge && potentialNeighbor.rightEdge))
                        expectedDistance += _sqrt2;
                    else
                        continue;
                }

                pathLength = Vector2.Distance(potentialNeighbor.position, node.position);
                if (pathLength > bestDistance || pathLength > expectedDistance + pathTolerance) { continue; }
                var clearance = JumpTileCastClearance(node, potentialNeighbor);
                if (clearance == 0) { continue; }

                bestClearance = clearance;
                bestDistance = pathLength;
                bestNode = potentialNeighbor;
            }

            if (bestNode != null)// && !node.neighbors.Contains(bestNode))
            {
                node.neighbors.Add(new NeighborConnection(bestNode.indexX, bestNode.indexY, bestClearance));
            }

            //right search 
            bestDistance = float.MaxValue;
            bestNode = null;
            for (int x = node.indexX; x < maxX; x++)
            {
                var potentialNeighbor = _nodeMap[x, y];
                if (potentialNeighbor == null || potentialNeighbor == node) continue;

                //node to the right is connected
                if (x == node.indexX + 1 && Math.Abs(potentialNeighbor.position.y - node.position.y) < 1)
                {
                    bestClearance = Mathf.Min(potentialNeighbor.verticalClearance, node.verticalClearance);
                    bestNode = potentialNeighbor;
                    break;
                }

                if (y == node.indexY - 1) continue; //for the row directly below, only worry about slope connections checked above

                expectedDistance = GetExpectedDistance(node.position, potentialNeighbor.position);
                if (expectedDistance > bestDistance) { continue; }

                if (x == node.indexX)
                {
                    if ((node.leftEdge && potentialNeighbor.leftEdge) || (node.rightEdge && potentialNeighbor.rightEdge))
                        expectedDistance += _sqrt2;
                    else
                        continue;
                }

                pathLength = Vector2.Distance(node.position, potentialNeighbor.position);
                if (pathLength > bestDistance || pathLength > expectedDistance + pathTolerance) { continue; }
                var clearance = JumpTileCastClearance(node, potentialNeighbor);
                if (clearance == 0) { continue; }

                bestClearance = clearance;
                bestDistance = pathLength;
                bestNode = potentialNeighbor;
            }

            if (bestNode != null)//&& !node.neighbors.Contains(bestNode))
            {
                node.neighbors.Add(new NeighborConnection(bestNode.indexX, bestNode.indexY, bestClearance));
            }
        }

        //check if an edge node
        if (!node.leftEdge && !node.rightEdge) return;
        //don't proceed to fall checks if node is at the bottom of the grid
        if (node.indexY == 0) return;

        //exploring nodes you could fall to from the node being explored
        for (int y = node.indexY - 1; y > 0; y--)
        {
            for (int x = minX; x < maxX; x++)
            {
                GravStarNode potentialNeighbor = _nodeMap[x, y];
                if (potentialNeighbor == null) continue;
                var deltaX = potentialNeighbor.position.x - node.position.x;
                if (deltaX == 0) continue; //no need to explore a node direction below this one (though none should exist)

                //can you jump from this neighbor to this node? If so you can jump back.
                var clearance = JumpTileCastClearance(potentialNeighbor, node);

                //otherwise check if you can fall
                if (clearance == 0)
                {
                    var originX = node.indexX + (deltaX > 0 ? 1 : -1);
                    var originY = node.indexY - 1;

                    clearance = TileCastClearance(originX, originY, potentialNeighbor.indexX, potentialNeighbor.indexY);
                    if (clearance == 0) { continue; }

                    var oC = GetClearance(originX, node.indexY);
                    if (oC < clearance) { clearance = oC; }
                }

                node.neighbors.Add(new NeighborConnection(potentialNeighbor.indexX, potentialNeighbor.indexY, clearance));
            }
        }
    }

    public float GetExpectedDistance(Vector2 position1, Vector2 position2)
    {
        var xDist = position1.x - position2.x;
        xDist = xDist > 0 ? xDist : -xDist;
        var yDist = position1.y - position2.y;
        yDist = yDist > 0 ? yDist : -yDist;

        if (yDist > xDist)
        {
            var d = yDist;
            d += xDist * _sqrt2m1;
            return d;
        }
        else
        {
            var d = xDist;
            d += yDist * _sqrt2m1;
            return d;
        }
    }

    public bool JumpTileCast(GravStarNode startNode, GravStarNode endNode)
    {
        return JumpTileCastClearance(startNode, endNode) == 0;
    }

    public float JumpTileCastClearance(GravStarNode startNode, GravStarNode endNode)
    {
        var eDeltaX = endNode.position.x - startNode.position.x;
        var eDeltaY = endNode.position.y - startNode.position.y;

        var eTargetX = endNode.indexX;
        if (eDeltaX == 0)
        {
            if (startNode.leftEdge && endNode.leftEdge) eTargetX -= 1;
            if (startNode.rightEdge && endNode.rightEdge) eTargetX += 1;
            return TileCastClearance(startNode.indexX, startNode.indexY, eTargetX, endNode.indexY);
        }
        else if (eDeltaY != 0)
        {
            eTargetX += eDeltaX > 0 ? -1 : 1;
        }

        var midX = startNode.indexX + eDeltaX * 0.5f;
        var midY = startNode.indexY + eDeltaY * 0.5f;
        var eMag = Mathf.Sqrt(eDeltaX * eDeltaX + eDeltaY * eDeltaY);
        var eDir = new Vector3(eDeltaX / eMag, eDeltaY / eMag);
        var perpDir = Vector3.Cross(eDir, Vector3.forward);
        if (perpDir.x < 0) perpDir.x = -perpDir.x;

        var offsetMag = -eDeltaX * 0.333f;
        if (offsetMag < -3)
        {
            offsetMag = -3;
        }
        else if (offsetMag > 3)
        {
            offsetMag = 3;
        }

        var offsetX = perpDir.x * offsetMag;
        var offsetY = perpDir.y * offsetMag;
        if (offsetY < 0) offsetY = -offsetY;
        var perpMidX = eDeltaX > 0 ? (int)(midX + offsetX) : (int)(midX + offsetX + 0.5f); //Mathf.Ceil
        var fPerpMid = midY + offsetY;
        var perpMidY = fPerpMid > 0 ? (int)(fPerpMid + 0.5f) : (int)(fPerpMid - 0.5f); //Mathf.Round

        var startCast = TileCastClearance(startNode.indexX, startNode.indexY, perpMidX, perpMidY);
        if (startCast == 0) return 0;

        var endCast = TileCastClearance(perpMidX, perpMidY, eTargetX, endNode.indexY);
        return startCast < endCast ? startCast : endCast;
    }

    public void DrawGridPath(GravStarNode path, Color color)
    {
        var cNode = path;
        color *= 0.5f;
        color.a = 0.1f;
        while (cNode != null)
        {
            if (cNode.parent != null)
            {
                Debug.DrawLine(cNode.position, cNode.parent.position, color, 15f);
            }
            cNode = cNode.parent;
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
                else if (d > _sqrt2)
                {
                    var yDelta = n2.position.y - n.position.y;
                    if (yDelta == 0)
                    {
                        Extensions.DrawDashedLine(n.position, n2.position, .25f, Color.red, t);
                    }
                    else if (yDelta > 0)
                    {
                        Extensions.DrawDashedLine(n.position, n2.position, .25f, new Color(1, 1, 0, 1f), t);
                    }
                    else
                    {
                        Extensions.DrawDashedLine(n2.position, n.position, .25f, new Color(1, 0, 1, 1f), t, 1);
                    }
                }
                else
                {
                    Debug.DrawLine(n.position, n2.position, Color.blue, t);
                }
            }
        }
    } 
}