using System.Collections.Generic;
using UnityEngine;

public class EnemySpawner : MonoBehaviour
{
    [SerializeField]
    private List<GameObject> _enemyPrefabs;
    [SerializeField]
    private GameObject _keyPrefab;
    private LevelInfoScriptableObject _levelInfo;
    private List<Vector3> tempLocations;
    private int enemyCounter;
    public int minDist = 5;
    public bool spawn = false;

    // Subscribing to the LevelGenerated event
    private void OnEnable()
    {
        GameController.OnSpawnEnemies += HandleSpawnEnemies;
        EnemyScript.OnEnemyDeath += HandleEnemyDied;
    }

    // Unsubscribing from the LevelGenerated event
    private void OnDisable()
    {
        GameController.OnSpawnEnemies -= HandleSpawnEnemies;
        EnemyScript.OnEnemyDeath -= HandleEnemyDied;
    }
    public void StartSpawning()
    {
        spawn = true;
    }

    private void HandleSpawnEnemies(List<Vector3> locations, Vector3 playerPos)
    {
        Debug.Log("EnemySPawner: handling spawn enemies request");
        enemyCounter = 0;
        _levelInfo = GameObject.FindGameObjectWithTag("LevelAgent").GetComponent<LevelGeneration>().GetLevelInfo;

        tempLocations = locations;
        StartSpawning();

        if (spawn && locations.Count > 0)
        {
            // Choose enemies until there are no more spawn tickets left
            List<GameObject> enemiesToSpawn = new List<GameObject>();
            int remainingTickets = _levelInfo.spawnTickets;

            while (remainingTickets > 0)
            {
                GameObject selectedEnemyPrefab = GetRandomEnemyPrefab();

                // subtract cost from the spawning tickets
                if (selectedEnemyPrefab != null)
                {
                    int enemyCost = selectedEnemyPrefab.GetComponent<EnemyScript>().GetEnemyCost();

                    if (remainingTickets > enemyCost)
                    {
                        remainingTickets -= enemyCost;
                        // Store the selected enemy in spawning list
                        enemiesToSpawn.Add(selectedEnemyPrefab);
                    }
                    else
                    {
                        // Not enough spawn tickets for this enemy, break the loop
                        break;
                    }
                }
                else
                {
                    Debug.LogError("There are no Enemy prefabs assigned to the  Enemy Spawner");
                    break;
                }
            }

            foreach (GameObject enemyPrefab in enemiesToSpawn)
            {
                (List<Vector3> updatedLocations, Vector3 spawnPosition) = GetRandomSpawnPosition(locations, playerPos);

                if (spawnPosition != Vector3.zero)
                {
                    // Spawn the enemy at the chosen position
                    SpawnEnemy(enemyPrefab, spawnPosition);
                }
                else
                {
                    Debug.LogWarning("No suitable spawn position found for an enemy.");
                }

                // Update the locations list
                locations = updatedLocations;

            }

        }
        spawn = false;
    }

    private GameObject GetRandomEnemyPrefab()
    {
        if (_enemyPrefabs.Count > 0)
        {
            return _enemyPrefabs[Random.Range(0, _enemyPrefabs.Count)];
        }
        else
        {
            Debug.LogWarning("Cant Spawn Enemies, no enemy prefabs available.");
            return null;
        }
    }

    private (List<Vector3>, Vector3) GetRandomSpawnPosition(List<Vector3> locations, Vector3 playerPos)
    {
        while (locations.Count > 0)
        {
            int randomIndex = Random.Range(0, locations.Count);
            Vector3 pos = locations[randomIndex];

            if (minDist < Vector3.Distance(pos, playerPos))
            {
                if (HasAdjacentPosition(pos, tempLocations))
                {
                    // Remove the chosen position from the available locations
                    locations.RemoveAt(randomIndex);

                    Debug.Log("Added Position to Spawn Position for Enemies: " + pos);

                    // Return the chosen position and the updated locations list
                    return (locations, pos);
                }
                Debug.Log("Tried to spawn Enemy at Position that might be not reachable for the Player");
            }
            else
            {
                // Remove the position if it doesn't meet the distance criteria
                locations.RemoveAt(randomIndex);
                Debug.Log("Removed Position due to proximity to player.");
            }
        }

        Debug.Log("No suitable spawn position found.");
        return (locations, Vector3.zero);
    }

    private bool HasAdjacentPosition(Vector3 pos, List<Vector3> locations)
    {
        List<Vector3> adjacentPositions = new List<Vector3>();

        adjacentPositions.Add(pos + Vector3.up);
        adjacentPositions.Add(pos + Vector3.down);
        adjacentPositions.Add(pos + Vector3.left);
        adjacentPositions.Add(pos + Vector3.right);

        foreach (Vector3 adjPos in adjacentPositions)
        {
            if (locations.Contains(adjPos))
            {
                // At least one adjacent position found
                return true;
            }
        }
        return false;
    }

    private void SpawnEnemy(GameObject enemyPrefab, Vector3 position)
    {
        Instantiate(enemyPrefab, position, Quaternion.identity, transform);
        enemyCounter++;
    }

    private void HandleEnemyDied(Vector3 pos)
    {
        Debug.Log("Enemy Count before enemy died:  " + enemyCounter);
        // check if all enemies are dead
        enemyCounter--;

        Debug.Log("Enemy Count after enemy died:  " + enemyCounter);

        if (enemyCounter <= 0)
        {
            // instantiate key so player can move on to next level
            Instantiate(_keyPrefab, pos, Quaternion.identity);
        }

    }
}
