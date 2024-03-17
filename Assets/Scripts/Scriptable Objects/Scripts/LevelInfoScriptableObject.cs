using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

[CreateAssetMenu(fileName = "New Level Info", menuName = "Scriptable Objects/Level Info")]
[System.Serializable]
public class LevelInfoScriptableObject : ScriptableObject
{
    public enum BaseType { Small, Medium, Large, Chest, Boss }
    public enum Variation { V1, V2, V3, V4 }
    public Variation PublicVariation => _variation;
    public BaseType PublicBaseType => _baseType;
    public int spawnTickets;

    public TileBase[,] InitialState;
    [HideInInspector]
    public int width;
    [HideInInspector]
    public int height;
    [HideInInspector]
    public int tileCount;
    public bool playability = false;

    [SerializeField]
    private BaseType _baseType;
    [SerializeField]
    private Variation _variation;
}