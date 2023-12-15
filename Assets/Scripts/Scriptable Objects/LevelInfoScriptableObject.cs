using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

[CreateAssetMenu(fileName = "New Level Info", menuName = "Scriptable Objects/Level Info")]
public class LevelInfoScriptableObject : ScriptableObject
{
    public enum BaseType { Small, Medium, Large }
    public enum Variation { V1, V2, V3, V4 }

    [SerializeField]
    private BaseType _baseType;
    public BaseType PublicBaseType => _baseType;

    [SerializeField]
    private Variation _variation;
    public Variation PublicVariation => _variation;
    public Variation _Variation => _variation;
    public TileBase[,] initialState;



}
