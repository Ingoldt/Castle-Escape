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

    // Start is called before the first frame update
    void Start()
    {
        if (enemyStats == null)
        {
            Debug.LogWarning("EnemyScriptableObject not assigned to enemy prefab.");
            return;
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
