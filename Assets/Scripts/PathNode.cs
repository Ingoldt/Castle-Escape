using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.Tilemaps;

public class PathNode
{
    public int x;
    public int y;
    public int gCost;
    public int hCost;
    public int fCost;

    public bool isWalkable;
    public PathNode previousNode;

    private TileBase tile;

    private static readonly HashSet<string> nonWalkableTiles = new HashSet<string>
    {
        "Wall",
        "Pillar_1",
        "Pillar_2",
        "Pillar_3",
        "Pillar_4",
        "Plant_1",
        "Plant_2",
        "Plant_3",
    };

    public PathNode(int x, int y, TileBase tile)
    {
        this.x = x;
        this.y = y;
        this.tile = tile;
        isWalkable = !nonWalkableTiles.Contains(tile.name);
    }

    public void calculateFCost() 
    {
        fCost = gCost + hCost;
    
    }
}
