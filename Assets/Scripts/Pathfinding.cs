using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Tilemaps;

public class Pathfinding
{

    private const int STRAIGHT_COST = 10;
    private List<PathNode> openList;
    private List<PathNode> closedList;
    private PathNode[,] nodeGrid;

    public Pathfinding() { }
    public bool ContainsPath(Vector2Int start, Vector2Int end, int width, int height, TileBase[,] currentState)
    {
        nodeGrid = new PathNode[width, height];

        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                TileBase tile = currentState[x, y];
                PathNode pathNode = new PathNode(x, y, tile);
                pathNode.gCost = int.MaxValue;
                pathNode.CalculateFCost();
                pathNode.previousNode = null;

                nodeGrid[x, y] = pathNode;
            }
        }

        PathNode startNode = nodeGrid[start.x, start.y];
        PathNode endNode = nodeGrid[end.x, end.y];

        startNode.gCost = 0;
        startNode.hCost = CaculateDistanceCost(startNode, endNode);
        startNode.CalculateFCost();

        // Debug statements after processing
        //Debug.Log("Start Node: " + $"({startNode.x}, {startNode.y})");
        //Debug.Log("End Node: " + $"({endNode.x}, {endNode.y})");

        openList = new List<PathNode> { startNode };
        closedList = new List<PathNode>();

        while (openList.Count > 0) 
        {
            PathNode currentNode = GetLowestFCostNode(openList);

            // Debug statements before processing
            //Debug.Log("Open List: " + string.Join(", ", openList.Select(node => $"({node.x}, {node.y})")));
            //Debug.Log("Closed List: " + string.Join(", ", closedList.Select(node => $"({node.x}, {node.y})")));
            //Debug.Log("current Node: " + $"({currentNode.x}, {currentNode.y})");

            if (currentNode == endNode) 
            {
                // FInal node reached
                return true;
            }
            
            openList.Remove(currentNode);
            closedList.Add(currentNode);

            // Debug statements after processing
            //Debug.Log("FCosts in Open List: " + string.Join(", ", openList.Select(node => $"{node.fCost}")));

            foreach (PathNode neighbourNode in GetNeighbourList(currentNode, width, height)) 
            {
                if (closedList.Contains(neighbourNode)) continue;
                if (!neighbourNode.isWalkable)
                {
                    closedList.Add(neighbourNode);
                    continue;
                }

                int tentativeGCost = currentNode.gCost + CaculateDistanceCost(currentNode, neighbourNode);
                if (tentativeGCost < neighbourNode.gCost)
                {
                    neighbourNode.previousNode = currentNode;
                    neighbourNode.gCost = tentativeGCost;
                    neighbourNode.hCost = CaculateDistanceCost(neighbourNode, endNode);
                    neighbourNode.CalculateFCost();

                    if (!openList.Contains(neighbourNode)) 
                    {
                        openList.Add(neighbourNode);
                    }
                }

            }
        }
        // Out of nodes on openList
        //path could be found
        return false;
    }

    private List<PathNode> GetNeighbourList(PathNode currentNode,int width, int height)
    {
        List<PathNode > neighbourList = new List<PathNode>();
        if (currentNode.x - 1 >= 0)
        {
            // Left
            neighbourList.Add(GetNode(currentNode.x - 1, currentNode.y));
        }
        if (currentNode.x + 1 < width)
        {
            // Right
            neighbourList.Add(GetNode(currentNode.x + 1, currentNode.y));
        }
        if (currentNode.y - 1 >= 0)
        {
            // Down
            neighbourList.Add(GetNode(currentNode.x, currentNode.y - 1));
        }
        if (currentNode.y + 1 < height)
        {
            // Up
            neighbourList.Add(GetNode(currentNode.x, currentNode.y + 1));
        }

        return neighbourList;
    }

    private PathNode GetNode(int x, int y)
    {
        
        return nodeGrid[x, y];
    }

    private int CaculateDistanceCost(PathNode a, PathNode b)
    {
        int xDistance = Mathf.Abs(a.x - b.x);
        int yDistance = Mathf.Abs(a.y - b.y);

        // no diagonal Movement
        return STRAIGHT_COST * (xDistance + yDistance);
    }

    private PathNode GetLowestFCostNode(List<PathNode> pathNodeList)
    {
        PathNode lowestFCostNode = pathNodeList[0];
        for (int i = 0; i < pathNodeList.Count; i++) 
        {
            if (pathNodeList[i].fCost < lowestFCostNode.fCost)
            {
                lowestFCostNode = pathNodeList[i];
            }
        }
        return lowestFCostNode;
    }
}
