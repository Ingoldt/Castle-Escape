using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

[CreateAssetMenu(fileName = "TileLists", menuName = "Scriptable Objects/Tile Lists")]
[System.Serializable]
public class TileTypesScriptableObject : ScriptableObject
{
    [SerializeField]
    private List<TileBase> wallList = new List<TileBase>();
    public List<TileBase> WallList => wallList;
    [SerializeField]
    private List<TileBase> floorList = new List<TileBase>();
    public List<TileBase> FloorList => floorList;
    [SerializeField]
    private List<TileBase> plantList = new List<TileBase>();
    public List<TileBase> PlantList => plantList;
    [SerializeField]
    private List<TileBase> pillarList = new List<TileBase>();
    public List<TileBase> PillarList => pillarList;
    [SerializeField]
    private List<TileBase> barrelList = new List<TileBase>();
    public List<TileBase> BarrelList => barrelList;
    [SerializeField]
    private List<TileBase> spawnList = new List<TileBase>();
    public List<TileBase> SpawnList => spawnList;
    [SerializeField]
    private List<TileBase> doorList = new List<TileBase>();
    public List<TileBase> DoorList => doorList;

    private List<TileBase> indestructibleList
    {
        get
        {
            List<TileBase> indestructibleList = new List<TileBase>();
            indestructibleList.AddRange(wallList);
            indestructibleList.AddRange(plantList);
            indestructibleList.AddRange(pillarList);
            return indestructibleList;
        }
    } 
    public List<TileBase> IndestructibleList => indestructibleList;
}
