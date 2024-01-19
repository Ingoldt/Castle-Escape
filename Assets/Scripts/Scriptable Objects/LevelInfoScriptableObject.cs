using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

[CreateAssetMenu(fileName = "New Level Info", menuName = "Scriptable Objects/Level Info")]
[System.Serializable]
public class LevelInfoScriptableObject : ScriptableObject
{
    public enum BaseType { Small, Medium, Large }
    public enum Variation { V1, V2, V3, V4 }
    public Variation PublicVariation => _variation;
    public BaseType PublicBaseType => _baseType;
    public Variation _Variation => _variation;
    public TileBase[,] InitialState;
    [HideInInspector]
    public int width;
    [HideInInspector]
    public int height;
    [HideInInspector]
    public int tileCount;

    [SerializeField]
    private BaseType _baseType;
    [SerializeField]
    private Variation _variation;


}
