using UnityEngine;

[CreateAssetMenu(fileName = "NewEnemyStats", menuName = "Scriptable Objects/Enemy Stats")]
public class EnemyScriptableObject : ScriptableObject
{
    public enum EnemyType { Knight, Mage }
    public EnemyType enemyType;
    public int spawnCost;
    public int health;
    public int damage;
    public float attackSpeed;
    public float attackkRange;
    public float speed;
}
