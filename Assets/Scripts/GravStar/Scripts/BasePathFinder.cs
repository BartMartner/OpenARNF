using CreativeSpore.SuperTilemapEditor;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public abstract class BasePathFinder : MonoBehaviour
{
    public Action onNodesChange;
    public List<GravStarNode> nonNullNodes { get { return _nonNullNodes; } }
    public HashSet<IPathFindingSensitive> sensitives;
    public bool autoUpdateTiles = true;

    public bool ready { get; private set; }

    protected const float _sqrt2 = 1.4142135623f;
    protected const float _sqrt2m1 = .4142135623f;

    protected int _width;
    protected int _height;
    protected GravStarNode[,] _nodeMap;
    protected int[,] _tiles;
    protected float _minX;
    protected float _minY;
    protected List<STETilemap> _tilemaps;
    public List<STETilemap> tilemaps { get { return _tilemaps; } set { _tilemaps = value; } }
    protected bool _needsRefresh;
    protected List<GravStarNode> _nonNullNodes = new List<GravStarNode>();
    protected List<GravStarNode> _openList = new List<GravStarNode>();
    protected Stack<GravStarNode> _scratchStack = new Stack<GravStarNode>();

    private void Start()
    {
        //_tilemaps = GetComponentsInChildren<STETilemap>().ToList();
        _needsRefresh = true;
    }

    public bool BlockedBySensitives(Vector3 position)
    {
        if (sensitives == null) return false;
        foreach (var s in sensitives)
        {
            if (s.pathFindingSensitive && s.collider2Ds != null)
            {
                foreach (var c in s.collider2Ds)
                {
                    if (c.enabled && c.bounds.Contains(position)) { return true; }
                }
            }
        }
        return false;
    }

    public virtual bool CompareNode(GravStarNode parent, GravStarNode node)
    {
        if (node.closed) return false;

        //Vector2 delta = new Vector2(node.position.x - parent.position.x, node.position.y - parent.position.y);
        var xComp = node.position.x - parent.position.x;
        var yComp = node.position.y - parent.position.y;
        var sqrMag = Mathf.Sqrt(xComp * xComp + yComp * yComp);

        var newGCost = parent.generatedMovementCost + sqrMag;

        if (!node.open || newGCost < node.generatedMovementCost)
        {
            node.parent = parent;
            node.generatedMovementCost = newGCost;
            node.fullCost = node.heuristicMovementCost + node.generatedMovementCost;
        }

        return true;
    }

    public List<GravStarNode> ComputeNodePath(GravStarNode currentNode)
    {
        var path = new List<GravStarNode>();
        currentNode.fullCost = 0;
        path.Add(currentNode);

        float distance = 0;
        while (currentNode.parent != null)
        {
            distance += Vector2.Distance(currentNode.position, currentNode.parent.position);
            path.Add(currentNode.parent);
            currentNode = currentNode.parent;
            currentNode.pathDistanceEnd = distance;
        }
        path.Reverse();
        return path;
    }

    public virtual List<Vector2> ComputeVectorPath(GravStarNode currentNode)
    {
        var path = new List<Vector2>();
        path.Add(currentNode.position);
        while (currentNode.parent != null)
        {
            path.Add(currentNode.parent.position);
            currentNode = currentNode.parent;
        }

        path.Reverse();
        return path;
    }

    public abstract void DebugDrawNodes(BaseAgentStats stats);

    public abstract void FindNeighbors(GravStarNode node);

    public List<List<GravStarNode>> FindNodeGroups(BaseAgentStats agentStats)
    {
        Refresh();
        TryRefresh();
        //Kosaraju
        List<GravStarNode> graph = _nonNullNodes;
        bool[] visited = new bool[graph.Count];
        bool[] sorted = new bool[graph.Count];
        var l = new List<GravStarNode>();
        List<List<GravStarNode>> sortedNodes = new List<List<GravStarNode>>();

        //Visit
        Action<GravStarNode> Visit = null;
        Visit = (GravStarNode g) =>
        {
            var index = graph.IndexOf(g);
            if (!visited[index])
            {
                visited[index] = true;
                foreach (var c in g.neighbors)
                {
                    var n = _nodeMap[c.x, c.y];
                    if (agentStats == null || (agentStats.height <= c.clearance && agentStats.CanNavigate(g, n)))
                    {
                        Visit(n);
                    }
                }

                if (!l.Contains(g)) l.Add(g);
            }
        };

        Action<GravStarNode, GravStarNode> Assign = null;
        Assign = (GravStarNode g1, GravStarNode g2) =>
        {
            var index = graph.IndexOf(g1);
            if (!sorted[index])
            {
                if (g1 == g2)
                {
                    sortedNodes.Add(new List<GravStarNode>());
                }

                sortedNodes[sortedNodes.Count - 1].Add(g1);
                sorted[index] = true;

                //"transpose"
                foreach (var n in graph)
                {
                    if (n.neighbors.Any(c => c.x == g1.indexX && c.y == g1.indexY))
                    {
                        var connection = n.neighbors.FirstOrDefault(c => c.x == g1.indexX && c.y == g1.indexY);
                        if (agentStats == null || (agentStats.height <= connection.clearance && agentStats.CanNavigate(n, g1)))
                        {
                            Assign(n, g2);
                        }
                    }
                }
            }
        };

        foreach (var n in graph)
        {
            Visit(n);
        }

        l.Reverse();
        foreach (var n in l)
        {
            Assign(n, n);
        }

        return sortedNodes;
    }

    public List<GravStarNode> FindNodes(Func<GravStarNode, bool> match)
    {
        TryRefresh();
        return _nonNullNodes.Where(match).ToList();
    }

    public int GetClearance(int indexX, int indexY)
    {
        int clearance = 0;
        for (int y = indexY + 1; y < _height; y++)
        {
            if (_tiles[indexX, y] > 0)
            {
                break;
            }
            else
            {
                clearance = 1 + (y - indexY);
            }
        }

        return clearance;
    }

    public GravStarNode GetClosestNode(Vector2 pos)
    {
        return GetClosestNode(pos, null);
    }

    public virtual GravStarNode GetClosestNode(Vector2 pos, Func<GravStarNode, bool> verify)
    {
        TryRefresh();

        if (_nonNullNodes.Count == 0) return null;

        float bestSqrMag = 0;
        GravStarNode closest = null;
        float deltaX, deltaY, d;        

        var initialNode = PositionToIndex(pos);
        GravStarNode node = NodeAtIndex(initialNode);
        if (node != null && (verify == null || verify(node))) { return node; }

        var range = 1;

        while (closest == null)
        {
            var minX = initialNode.x - range;
            var maxX = initialNode.x + range;
            var minY = initialNode.y - range;
            var maxY = initialNode.y + range;

            if (minY < 0 && minX < 0 && maxX >= _width && maxY >= _height) { break; }

            if (minY >= 0 || maxY < _height)
            {
                var startX = minX < 0 ? 0 : minX;
                var endX = maxX >= _width ? _width - 1 : maxX;
                for (int x = startX; x <= endX; x++)
                {
                    //CheckTop
                    if(maxY < _height)
                    {
                        node = _nodeMap[x, maxY];
                        if (node != null)
                        {
                            deltaX = node.position.x - pos.x;
                            deltaY = node.position.y - pos.y;
                            d = deltaX * deltaX + deltaY * deltaY;

                            if ((closest == null || d < bestSqrMag) && (verify == null || verify(node)))
                            {
                                closest = node;
                                bestSqrMag = d;
                            }
                        }
                    }

                    //CheckBottom
                    if (minY >= 0)
                    {
                        node = _nodeMap[x, minY];
                        if (node != null)
                        {
                            deltaX = node.position.x - pos.x;
                            deltaY = node.position.y - pos.y;
                            d = deltaX * deltaX + deltaY * deltaY;

                            if ((closest == null || d < bestSqrMag) && (verify == null || verify(node)))
                            {
                                closest = node;
                                bestSqrMag = d;
                            }
                        }
                    }
                }
            }//end horzontal checks

            if (minX >= 0 || maxX < _width)
            {
                var startY = minY < 0 ? 0 : minY;
                var endY = maxY >= _height ? _height-1 : maxY;
                for (int y = startY; y <= endY; y++)
                {
                    //CheckRight
                    if (maxX < _width)
                    {
                        node = _nodeMap[maxX,y];
                        if (node != null)
                        {
                            deltaX = node.position.x - pos.x;
                            deltaY = node.position.y - pos.y;
                            d = deltaX * deltaX + deltaY * deltaY;

                            if ((closest == null || d < bestSqrMag) && (verify == null || verify(node)))
                            {
                                closest = node;
                                bestSqrMag = d;
                            }
                        }
                    }

                    //CheckLeft
                    if (minX >= 0)
                    {
                        node = _nodeMap[minX, y];
                        if (node != null)
                        {
                            deltaX = node.position.x - pos.x;
                            deltaY = node.position.y - pos.y;
                            d = deltaX * deltaX + deltaY * deltaY;

                            if ((closest == null || d < bestSqrMag) && (verify == null || verify(node)))
                            {
                                closest = node;
                                bestSqrMag = d;
                            }
                        }
                    }
                }
            }//end vertical checks

            range++;
        }
        
        return closest;
    }

    public virtual void GetClosestNodes(Vector2 start, Vector2 end, BaseAgentStats stats, out GravStarNode sNode, out GravStarNode eNode)
    {
        TryRefresh();

        sNode = null;
        eNode = null;
        float sqrDistanceStart = 0;
        float sqrDistanceTarget = 0;

        foreach (var node in _nonNullNodes)
        {
            if (node.verticalClearance < stats.height) continue;

            var d = (end - node.position).sqrMagnitude;
            if (eNode == null || d < sqrDistanceTarget)
            {
                eNode = node;
                sqrDistanceTarget = d;
            }

            d = (start - node.position).sqrMagnitude;
            if (sNode == null || d < sqrDistanceStart)
            {
                sNode = node;
                sqrDistanceStart = d;
            }
        }
    }

    public bool TileCast(Vector2 start, Vector2 end)
    {
        var si = PositionToIndex(start);
        var ei = PositionToIndex(end);
        return TileCast(si.x, si.y, ei.x, ei.y);
    }

    public bool TileCast(int startX, int startY, int endX, int endY)
    {
        if (startX < 0 || startX >= _width ||
            startY < 0 || startY >= _height ||
            endX < 0 || endX >= _width ||
            endY < 0 || endY >= _height)
        {
            return true;
        }

        if (_tiles[startX, startY] != 0) { return true; }
        if (_tiles[endX, endY] != 0) { return true; }
        if (startX == endX && startY == endY) { return false; }

        var indexX = startX;
        var indexY = startY;
        var deltaX = endX - startX;
        var deltaY = endY - startY;
        var dirX = deltaX == 0 ? 0 : (deltaX > 0 ? 1 : -1);
        var dirY = deltaY == 0 ? 0 : (deltaY > 0 ? 1 : -1);
        float ySlope = deltaX == 0 ? deltaY : (float)deltaY / deltaX;
        if (ySlope < 0) { ySlope = -ySlope; }

        //if ySlope is less than 1 use "xSlope"
        float xSlope;
        int moves;

        if (ySlope >= 1)
        {
            xSlope = 0;
            moves = (int)ySlope >> 1; //fast divide by 2
        }
        else
        {
            xSlope = deltaY == 0 ? deltaX : (float)deltaX / deltaY;
            if (xSlope < 0) { xSlope = -xSlope; }
            moves = (int)xSlope >> 1;
        }

        float xSlopeThreshold = xSlope;
        float ySlopeThreshold = ySlope;

        while (indexX != endX || indexY != endY)
        {
            if (_tiles[indexX, indexY] != 0) { return true; }

            if (ySlope >= 1)
            {
                if (moves < ySlopeThreshold - 0.00001f)
                {
                    indexY += dirY;
                    moves++;
                }
                else
                {
                    indexX += dirX;
                    ySlopeThreshold += ySlope;
                }
            }
            else
            {
                if (moves < xSlopeThreshold - 0.00001f)
                {
                    indexX += dirX;
                    moves++;
                }
                else
                {
                    indexY += dirY;
                    xSlopeThreshold += xSlope;
                }
            }
        }

        return false;
    }

    public bool TileCast(int startX, int startY, int endX, int endY, out Vector2Int index)
    {
        index = new Vector2Int(startX,startY);

        if (startX < 0 || startX >= _width ||
            startY < 0 || startY >= _height ||
            endX < 0 || endX >= _width ||
            endY < 0 || endY >= _height)
        {
            return true;
        }

        if (_tiles[startX, startY] != 0) { return true; }

        if (startX == endX && startY == endY) { return false; }

        var indexX = startX;
        var indexY = startY;
        var deltaX = endX - startX;
        var deltaY = endY - startY;
        var dirX = deltaX == 0 ? 0 : (deltaX > 0 ? 1 : -1);
        var dirY = deltaY == 0 ? 0 : (deltaY > 0 ? 1 : -1);
        float ySlope = deltaX == 0 ? deltaY : (float)deltaY / deltaX;
        if (ySlope < 0) { ySlope = -ySlope; }

        //if ySlope is less than 1 use "xSlope"
        float xSlope;
        int moves;

        if (ySlope >= 1)
        {
            xSlope = 0;
            moves = (int)ySlope >> 1; //fast divide by 2
        }
        else
        {
            xSlope = deltaY == 0 ? deltaX : (float)deltaX / deltaY;
            if (xSlope < 0) { xSlope = -xSlope; }
            moves = (int)xSlope >> 1;
        }

        float xSlopeThreshold = xSlope;
        float ySlopeThreshold = ySlope;

        while (indexX != endX || indexY != endY)
        {
            if (_tiles[indexX, indexY] != 0) { return true; }
            index.x = indexX;
            index.y = indexY;

            if (ySlope >= 1)
            {
                if (moves < ySlopeThreshold - 0.00001f)
                {
                    indexY += dirY;
                    moves++;
                }
                else
                {
                    indexX += dirX;
                    ySlopeThreshold += ySlope;
                }
            }
            else
            {
                if (moves < xSlopeThreshold - 0.00001f)
                {
                    indexX += dirX;
                    moves++;
                }
                else
                {
                    indexY += dirY;
                    xSlopeThreshold += xSlope;
                }
            }
        }

        if (_tiles[endX, endY] != 0)
        {
            index = new Vector2Int(endX,endY);
            return true;
        }

        return false;
    }

    /// <returns>Vertical Clearance. Means the cast hit something.</returns>
    public float TileCastClearance(int startX, int startY, int endX, int endY)
    {
#if DrawTileCast
        Extensions.DrawX(IndexToPosition(startX, startY), 0.3f, Color.green, 3);
        Extensions.DrawX(IndexToPosition(endX, endY), 0.3f, Color.blue, 3);
        Debug.DrawLine(IndexToPosition(startX, startY), IndexToPosition(endX, endY), Color.yellow, 3f);
#endif

        if (startX < 0 || startX >= _width ||
            startY < 0 || startY >= _height ||
            endX < 0 || endX >= _width ||
            endY < 0 || endY >= _height)
        {
            return 0;
        }

        var clearance = GetClearance(startX, startY);
        if (startX == endX && startY == endY)
        {
            if (_tiles[startX, startY] != 0)
            {
                return 0;
            }
            else
            {
                return clearance;
            }
        }

        var eClearance = GetClearance(endX, endY);
        if (eClearance < clearance) { clearance = eClearance; }

        var indexX = startX;
        var indexY = startY;
        var deltaX = endX - startX;
        var deltaY = endY - startY;
        var dirX = deltaX == 0 ? 0 : (deltaX > 0 ? 1 : -1);
        var dirY = deltaY == 0 ? 0 : (deltaY > 0 ? 1 : -1);
        float ySlope = deltaX == 0 ? deltaY : (float)deltaY / deltaX;
        if (ySlope < 0) { ySlope = -ySlope; }

        //if ySlope is less than 1 use "xSlope"
        float xSlope;
        int moves;

        if (ySlope >= 1)
        {
            xSlope = 0;
            moves = (int)ySlope >> 1; //fast divide by 2
        }
        else
        {
            xSlope = deltaY == 0 ? deltaX : (float)deltaX / deltaY;
            if (xSlope < 0) { xSlope = -xSlope; }
            moves = (int)xSlope >> 1;
        }

        float xSlopeThreshold = xSlope;
        float ySlopeThreshold = ySlope;

        while (indexX != endX || indexY != endY)
        {

            if (_tiles[indexX, indexY] != 0)
            {
                return 0;
#if DrawTileCast
                Extensions.DrawX(IndexToPosition(indexX, indexY), 0.2f, Color.red, 3);
#endif
            }

            else
            {
                var c = GetClearance(indexX, indexY);
                if (c < clearance) { clearance = c; }
#if DrawTileCast
                Extensions.DrawX(IndexToPosition(indexX, indexY), 0.2f, Color.yellow, 3);
#endif
            }

            if (ySlope >= 1)
            {
                if (moves < ySlopeThreshold - 0.00001f)
                {
                    indexY += dirY;
                    moves++;
                }
                else
                {
                    indexX += dirX;
                    ySlopeThreshold += ySlope;
                }
            }
            else
            {
                if (moves < xSlopeThreshold - 0.00001f)
                {
                    indexX += dirX;
                    moves++;
                }
                else
                {
                    indexY += dirY;
                    xSlopeThreshold += xSlope;
                }
            }
        }

        if (_tiles[endX, endY] != 0)
        {
            return 0;
        }
        else
        {
            return clearance;
        }
    }

    public List<Vector2Int> GetLine(Vector2 start, Vector2 end)
    {
        var si = PositionToIndex(start);
        var ei = PositionToIndex(end);
        return GetLine(si.x, si.y, ei.x, ei.y);
    }

    public List<Vector2Int> GetLine(int startX, int startY, int endX, int endY)
    {
        var line = new List<Vector2Int>();

        if (startX == endX && startY == endY)
        {
            line.Add(new Vector2Int(startX, startY));
            return line;
        }

        var indexX = startX;
        var indexY = startY;
        var deltaX = endX - startX;
        var deltaY = endY - startY;
        var dirX = deltaX == 0 ? 0 : (deltaX > 0 ? 1 : -1);
        var dirY = deltaY == 0 ? 0 : (deltaY > 0 ? 1 : -1);
        var ySlope = deltaX == 0 ? deltaY : (float)deltaY / deltaX;
        if (ySlope < 0) { ySlope = -ySlope; }

        //if ySlope is less than 1 use "xSlope"
        float xSlope;
        int moves;

        if (ySlope >= 1)
        {
            xSlope = 0;
            moves = (int)ySlope >> 1; //fast divide by 2
        }
        else
        {
            xSlope = deltaY == 0 ? deltaX : (float)deltaX / deltaY;
            if (xSlope < 0) { xSlope = -xSlope; }
            moves = (int)xSlope >> 1;
        }

        float xSlopeThreshold = xSlope;
        float ySlopeThreshold = ySlope;

        while (indexX != endX || indexY != endY)
        {
            //Overshot X (rounding error)
            if ((deltaX > 0 && indexX > endX) || (deltaX < 0 && indexX < endX))
            {
                break;
            }

            //Overshot Y (rounding error)
            if ((deltaY > 0 && indexY > endY) || (deltaY < 0 && indexY < endY))
            {
                break;
            }

            line.Add(new Vector2Int(indexX, indexY));

            if (ySlope >= 1)
            {
                if (moves < ySlopeThreshold - 0.00001f)
                {
                    indexY += dirY;
                    moves++;
                }
                else
                {
                    indexX += dirX;
                    ySlopeThreshold += ySlope;
                }
            }
            else
            {
                if (moves < xSlopeThreshold - 0.00001f)
                {
                    indexX += dirX;
                    moves++;
                }
                else
                {
                    indexY += dirY;
                    xSlopeThreshold += xSlope;
                }
            }
        }

        line.Add(new Vector2Int(endX, endY));
        return line;
    }

    public List<Vector2> GetPath(GravStarNode startNode, GravStarNode endNode, BaseAgentStats stats, bool greedy = false)
    {
        var currentNode = GetPathLinkedList(startNode, endNode, stats, greedy);
        if (currentNode == null) return null;
        return ComputeVectorPath(currentNode);
    }

    public List<Vector2> GetPath(Vector2 start, Vector2 end, BaseAgentStats stats)
    {
        TryRefresh();
        GravStarNode startNode = null;
        GravStarNode endNode = null;
        GetClosestNodes(start, end, stats, out startNode, out endNode);
        if (startNode == null || endNode == null) { return null; }
        return GetPath(startNode, endNode, stats);
    }

    public virtual GravStarNode GetPathLinkedList(GravStarNode currentNode, GravStarNode endNode, BaseAgentStats stats, bool greedy = false)
    {
        if(greedy) { return GetPathLinkedListGreedy(currentNode, endNode, stats); }

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
                if (node.fullCost <= currentNode.fullCost) //A*
                {
                    currentNode = node;
                    currentIndex = i;
                }
            }

            if (currentNode == endNode) { return currentNode; }

            //process neighbors
            var nbrCnt = currentNode.neighbors.Count;
            for (int i = 0; i < nbrCnt; i++)
            {
                var connection = currentNode.neighbors[i];
                var node = _nodeMap[connection.x, connection.y];
                if (node.open) { continue; }
                SetHeuristicCost(node, endNode);
                if (connection.clearance < stats.height || !stats.CanNavigate(currentNode, node)) { continue; }

                if (CompareNode(currentNode, node))
                {
                    node.open = true;
                    _openList.Add(node);
                }
            }

            currentNode.closed = true;
            currentNode.open = false;

            _openList.RemoveAt(currentIndex);
            count = _openList.Count;       
        }

        return null;
    }

    public GravStarNode GetPathLinkedListGreedy(GravStarNode currentNode, GravStarNode end, BaseAgentStats stats)
    {
        TryRefresh();

        foreach (var node in _nonNullNodes)
        {
            node.Reset();
        }

        _openList.Clear();
        SetHeuristicCost(currentNode, end);
        _openList.Add(currentNode);
        int currentIndex = 0;
        int count = 1;

        while (count > 0)
        {
            currentIndex = 0;
            currentNode = _openList[0];
            for (int i = 0; i < count; i++)
            {
                var node = _openList[i];
                if (node.heuristicMovementCost <= currentNode.heuristicMovementCost)
                {
                    currentNode = node;
                    currentIndex = i;
                }
            }

            if (currentNode == end) return currentNode;

            var nbrCnt = currentNode.neighbors.Count;
            for (int i = 0; i < nbrCnt; i++)
            {
                var connection = currentNode.neighbors[i];
                var node = _nodeMap[connection.x, connection.y];
                if (node.open) { continue; }
                if (connection.clearance < stats.height || !stats.CanNavigate(currentNode, node)) { continue; }
                if (!node.closed)
                {
                    node.parent = currentNode;
                    node.open = true;
                    SetHeuristicCost(node, end);
                    _openList.Add(node);
                }
            }

            currentNode.closed = true;
            currentNode.open = false;
            _openList.RemoveAt(currentIndex);
            count = _openList.Count;            
        }

        return null;
    }

    public GravStarNode GetPathLinkedList(Vector2 start, Vector2 end, BaseAgentStats stats)
    {
        TryRefresh();

        GravStarNode startNode = null;
        GravStarNode endNode = null;
        GetClosestNodes(start, end, stats, out startNode, out endNode);

        if (startNode == null || endNode == null) { return null; }

        return GetPathLinkedList(startNode, endNode, stats);
    }

    //SLOWER than Get Path :(
    public bool IsConnected(GravStarNode start, GravStarNode end, BaseAgentStats stats)
    {
        if(start.neighbors.Count == 0) { return false; }
        foreach (var node in _nonNullNodes) { node.Reset(); }

        _scratchStack.Clear();
        _scratchStack.Push(start);

        while(_scratchStack.Count > 0)
        {
            var current = _scratchStack.Pop();
            if(current == end) { return true; }
            if(current.closed) { continue; }
            current.closed = true;

            var neighbors = current.neighbors.OrderByDescending((c) =>{ 
                var node = _nodeMap[c.x, c.y];
                var deltaX = node.indexX - end.indexX;
                var deltaY = node.indexY - end.indexY;
                return deltaX * deltaX + deltaY * deltaY;
            });

            foreach (var connection in neighbors)
            {
                var node = _nodeMap[connection.x, connection.y];
                if (!node.closed) { _scratchStack.Push(node); }
            }
        }

        return false;
    }

    public Vector2 IndexToPosition(int indexX, int indexY)
    {
        return new Vector2(indexX + _minX, indexY + _minY);
    }

    public Vector2Int NodeIndexToTilemapIndex(STETilemap tilemap, int x, int y)
    {
        var pos = new Vector2(x + _minX, y + _minY);
        var localPos = tilemap.transform.InverseTransformPoint(pos);
        var cellSize = tilemap.CellSize;
        var gridX = Mathf.FloorToInt((localPos.x + Vector2.kEpsilon) / cellSize.x);
        var gridY = Mathf.FloorToInt((localPos.y + Vector2.kEpsilon) / cellSize.y);
        return new Vector2Int(gridX, gridY);
    }

    public GravStarNode NodeAtIndex(Vector2Int index)
    {
        return NodeAtIndex(index.x, index.y);
    }

    public GravStarNode NodeAtIndex(int x, int y, bool tryRefresh = true)
    {
        if (tryRefresh) { TryRefresh(); }
        if (_nodeMap == null) return null;
        if (x < 0 || x >= _width || y < 0 || y >= _height) { return null; }
        return _nodeMap[x, y];
    }

    public void OnTileChanged(STETilemap tilemap, int gridX, int gridY, uint tileData)
    {
        if (!enabled || !autoUpdateTiles)
        {
            _needsRefresh = true;
            return;
        }

        var worldPos = TilemapUtils.GetGridWorldPos(tilemap, gridX, gridY);
        var index = PositionToIndex(worldPos);

        if (index.x >= _nodeMap.GetLength(0) || index.y >= _nodeMap.GetLength(1))
        {
            _needsRefresh = true;
            return;
        }

        SetTile(tilemap, index.x, index.y);

        for (int y = -1; y <= 1; y++)
        {
            var indexY = index.y + y;
            if (indexY >= 0 && indexY < _height)
            {
                _nodeMap[index.x, indexY] = null;
                _nonNullNodes.RemoveAll((n) => n.indexX == index.x && n.indexY == indexY);
                SetNode(index.x, indexY);
            }
        }

        foreach (var n in _nonNullNodes)
        {
            if (n.indexX == index.x) { SetVerticalClearance(n); }
            if (n.indexX >= index.x - 1 && n.indexX <= index.x + 1) { SetEdgeStatus(n); }
            n.neighbors.Clear();
            FindNeighbors(n);
        }

        if (onNodesChange != null) { onNodesChange(); }
    }

    public Vector2Int PositionToIndex(Vector2 position)
    {
        Vector2Int result = new Vector2Int();
        var x = position.x - _minX;
        var y = position.y - _minY;
        result.x = x >= 0 ? (int)(x + 0.5f) : (int)(x - 0.5f);
        result.y = y >= 0 ? (int)(y + 0.5f) : (int)(y - 0.5f);
        return result;
    }

    public void Refresh() { _needsRefresh = true; }    

    protected bool RefreshNodes(List<STETilemap> tilemaps)
    {
        if (tilemaps == null)
        {
            Debug.LogError("RefreshNodes passed null tilemap List!");
            return false;
        }

        if (sensitives != null)
        {
            foreach (var s in sensitives)
            {
                if (s.pathFindingSensitive)
                {
                    s.pfTryRefresh = null;
                    s.pfTryRefresh += Refresh;
                }
            }
        }

        _tilemaps = tilemaps;

        _minX = float.MaxValue;
        var maxX = float.MinValue;
        _minY = float.MaxValue;
        var maxY = float.MinValue;
        foreach (var tilemap in _tilemaps)
        {
            var min = TilemapUtils.GetGridWorldPos(tilemap, tilemap.MinGridX, tilemap.MinGridY);
            var max = TilemapUtils.GetGridWorldPos(tilemap, tilemap.MaxGridX, tilemap.MaxGridY);
            if (min.x < _minX) _minX = min.x;
            if (max.x > maxX) maxX = max.x;
            if (min.y < _minY) _minY = min.y;
            if (max.y > maxY) maxY = max.y;
        }

        _width = (int)(maxX - _minX + 1);
        _height = (int)(maxY - _minY + 1);
        _tiles = new int[_width, _height];
        _nodeMap = new GravStarNode[_width, _height];
        _nonNullNodes.Clear();
        _openList.Clear();
        _scratchStack.Clear();

        for (int x = 0; x < _width; x++)
        {
            for (int y = 0; y < _height; y++)
            {
                if (x == 0 || x == _width - 1 || y == 0 || y == _height - 1)
                {
                    _tiles[x, y] = 1;
                    continue;
                }
            }
        }

        foreach (var tilemap in tilemaps)
        {
            if (!tilemap || tilemap.ColliderType == eColliderType.None) continue;

            tilemap.OnTileChanged -= OnTileChanged;
            tilemap.OnTileChanged += OnTileChanged;

            for (int x = 0; x < _width; x++)
            {
                for (int y = 0; y < _height; y++)
                {
                    if (_tiles[x, y] == 0) { SetTile(tilemap, x, y); }
                }
            }
        }

        for (int x = 0; x < _width; x++)
        {
            for (int y = 0; y < _height; y++)
            {
                SetNode(x, y);
            }
        }

        foreach (var node in _nodeMap)
        {
            if (node == null) continue;
            FindNeighbors(node);
        }

        //onNodesChange might call Refresh
        _needsRefresh = false;

        if (onNodesChange != null) { onNodesChange(); }
        ready = true;
        return true;
    }

    protected void SetEdgeStatus(GravStarNode n)
    {
        var x = n.indexX;
        var y = n.indexY;
        var slope = y > 0 && _tiles[x, y - 1] > 1;
        var edgeMinY = slope ? 1 : 0;
        var belowY = slope ? y - 2 : y - 1;
        n.leftEdge = y > edgeMinY && x > 0 && _tiles[x - 1, belowY] == 0;
        n.rightEdge = y > edgeMinY && x < _width - 1 && _tiles[x + 1, belowY] == 0;
    }

    public void SetHeuristicCost(GravStarNode startNode, GravStarNode endNode)
    {
        if (startNode.heuristicFound) return;
        var deltaX = endNode.position.x - startNode.position.x;
        var deltaY = endNode.position.y - startNode.position.y;
        startNode.heuristicMovementCost = Mathf.Sqrt(deltaX * deltaX + deltaY * deltaY);
        startNode.heuristicFound = true;
    }

    protected abstract void SetNode(int x, int y);

    private void SetTile(STETilemap tilemap, int x, int y)
    {
        if (x >= _tiles.GetLength(0) || y >= _tiles.GetLength(1))
        {
            _needsRefresh = true;
            return;
        }

        var gridPos = NodeIndexToTilemapIndex(tilemap, x, y);
        var tile = tilemap.GetTile(gridPos.x, gridPos.y);

        if (tile != null && tile.collData.type != eTileCollider.None)
        {
            if (tile.collData.type == eTileCollider.Polygon)
            {
                var verts = tile.collData.vertices;
                if (verts.Length == 3)
                {
                    if (verts.Any((v) => v.y == 0.5f))
                    {
                        _tiles[x, y] = 4;
                    }
                    else
                    {
                        _tiles[x, y] = 2;
                    }
                }
                else
                {
                    _tiles[x, y] = 3;
                }
            }
            else
            {
                _tiles[x, y] = 1;
            }
        }
        else
        {
            _tiles[x, y] = 0;
        }
    }

    public bool GetTile(Vector2 position, out int tile)
    {
        var index = PositionToIndex(position);
        tile = 0;
        if (index.x < 0 || index.y < 0 || index.x >= _width || index.y >= _height) { return false; }
        tile = _tiles[index.x, index.y];
        return tile > 0;
    }

    public bool GetTile(Vector2Int index, out int tile)
    {
        tile = 0;
        if(index.x < 0 || index.y < 0 || index.x >= _width || index.y >= _height) { return false; }
        tile = _tiles[index.x, index.y];
        return tile > 0;
    }

    protected void SetVerticalClearance(GravStarNode n)
    {
        n.verticalClearance = 1;
        for (int y = n.indexY + 1; y < _height; y++)
        {
            if (_tiles[n.indexX, y] > 0)
            {
                break;
            }
            else
            {
                n.verticalClearance = 1 + (y - n.indexY);
            }
        }
    }

    public void TryRefresh()
    {
        if (_needsRefresh)
        {
            _tilemaps.RemoveAll((t) => !t);
            if (RefreshNodes(_tilemaps))
            {
                _needsRefresh = false;
            }
        }
    }
}
