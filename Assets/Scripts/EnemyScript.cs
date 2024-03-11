using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyScript : MonoBehaviour
{
    [SerializeField]
    private EnemyScriptableObject enemyStats;

    public int GetEnemyCost()
    {
        return enemyStats.spawnCost;
    }

    private void Awake()
    {
        if (enemyStats == null)
        {
            Debug.LogWarning("EnemyScriptableObject not assigned to enemy prefab.");
            return;
        }
    }
}
