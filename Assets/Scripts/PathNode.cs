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
        "Plant_1_1",
        "Plant_1_2",
        "Plant_1_3",
        "Plant_1_4",
        "Plant_2_1",
        "Plant_2_2",
        "Plant_2_3",
        "Plant_2_4",
        "Plant_3_1",
        "Plant_3_2",
        "Plant_3_3",
        "Plant_3_4",
    };

    public PathNode(int x, int y, TileBase tile)
    {
        this.x = x;
        this.y = y;
        this.tile = tile;
        isWalkable = tile != null && !nonWalkableTiles.Contains(tile.name);
    }

    public void CalculateFCost() 
    {
        fCost = gCost + hCost;
    
    }
}
