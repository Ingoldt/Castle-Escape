using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Tilemaps;

public class Pathfinding : MonoBehaviour
{

    private const int STRAIGHT_COST = 10;
    private List<PathNode> openList;
    private List<PathNode> closedList;

    public bool containsPath(Vector2Int start, Vector2Int end, int width, int height, TileBase[,] currentState)
    {
        TileBase startTile = currentState[start.x, start.y];
        TileBase endTile = currentState[end.x, end.y];

        PathNode startNode = new PathNode(start.x, start.y, startTile);
        PathNode endNode = new PathNode(end.x, end.y, endTile);

        openList = new List<PathNode> { startNode };
        closedList = new List<PathNode>();

        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                TileBase tile = currentState[x, y];
                PathNode pathNode = new PathNode(x, y, tile);
                pathNode.gCost = int.MaxValue;
                pathNode.calculateFCost();
                pathNode.previousNode = null;
            }
        }

        startNode.gCost = 0;
        startNode.hCost = CaculateDistanceCost(startNode, endNode);
        startNode.calculateFCost();

        while (openList.Count > 0) 
        {
            PathNode currentNode = GetLowestFCostNode(openList);
            if (currentNode == endNode) 
            {
                // FInal node reached
                return true;
            }
            
            openList.Remove(currentNode);
            closedList.Add(currentNode);

            foreach (PathNode neighbourNode in GetNeighbourList(currentNode, currentState, width, height)) 
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
                    neighbourNode.calculateFCost();

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

    private List<PathNode> GetNeighbourList(PathNode currentNode, TileBase[,] currentState, int width, int height)
    {
        List<PathNode > neighbourList = new List<PathNode>();
        if (currentNode.x - 1 >= 0)
        {
            // Left
            neighbourList.Add(GetNode(currentNode.x - 1, currentNode.y, currentState));
        }
        if (currentNode.x + 1 < width)
        {
            // Right
            neighbourList.Add(GetNode(currentNode.x + 1, currentNode.y, currentState));
        }
        if (currentNode.y - 1 >= 0)
        {
            // Down
            neighbourList.Add(GetNode(currentNode.x, currentNode.y - 1, currentState));
        }
        if (currentNode.y + 1 < height)
        {
            // Up
            neighbourList.Add(GetNode(currentNode.x, currentNode.y + 1, currentState));
        }
        return neighbourList;
    }

    private PathNode GetNode(int x, int y, TileBase[,] currentState)
    {
        TileBase tile = currentState[x, y];
        return new PathNode(x, y, tile);
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
