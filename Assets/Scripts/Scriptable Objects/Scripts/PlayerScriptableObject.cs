using UnityEngine;

[CreateAssetMenu(fileName = "NewPlayerStats", menuName = "Scriptable Objects/Player Stats")]
public class PlayerScriptableObject : ScriptableObject
{
    public int health;
    public int damage;
    public float movementSpeed;
    public float attackSpeed;
    public float meleeRange;
}
