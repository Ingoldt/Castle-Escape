using UnityEngine;

[CreateAssetMenu(fileName = "NewEnemyStats", menuName = "Scriptable Objects/Enemy Stats")]
public class EnemyScriptableObject : ScriptableObject
{
    public int spawnCost;
    public int health;
    public int damage;
    public float speed;
    // Add any other stats as needed
}
