using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.Tilemaps;

public class LevelGeneration : MonoBehaviour
{
    [SerializeField]
    private List<GameObject> levelPrefabs;
    private GameObject levelInstance;
    private LevelInfoScriptableObject selectedLevel;
    //Debugging
    /*
    void OnDrawGizmos()
    {
        Tilemap tilemap = GetComponent<Tilemap>();

        if (tilemap != null)
        {
            // Draw the tilemap bounds using Gizmos
            Gizmos.color = Color.red;
            Gizmos.DrawWireCube(tilemap.localBounds.center, tilemap.localBounds.size);
        }
    }
    */

    // Add a property to get the current levelInstance
    public GameObject GetLevelInstance
    {
        get { return levelInstance; }
    }

    // Reset the environment and chooses a random initialization
    public void ResetEnvironment()
    {
        // clean up left over instances 
        if (levelInstance != null)
        {
            Destroy(levelInstance);
        }

        if (levelPrefabs != null && levelPrefabs.Count > 0) 
        {
            levelInstance = ChooseRandomLevel();
            // Apply the initial state
            InitializeTilemap(levelInstance);
        }
    }

    private GameObject ChooseRandomLevel()
    {
        // Randomly choose an prebuild Level from the list
        int randomIndex = Mathf.FloorToInt(Random.Range(0, levelPrefabs.Count));

        // Return the selected prebuilt level
        return Instantiate(levelPrefabs[randomIndex], transform.localPosition,Quaternion.identity);
    }

    void InitializeTilemap(GameObject levelInstance)
    {
        selectedLevel = levelInstance.GetComponent<LevelInfoScriptableObject>();

        TileBase[,] initialState = selectedLevel.InitialState;
        TileBase[,] currentState = selectedLevel.CurrentState;

        // Ensure the dimensions match
        int width = Mathf.Min(initialState.GetLength(0), currentState.GetLength(0));
        int height = Mathf.Min(initialState.GetLength(1), currentState.GetLength(1));

        BoundsInt bounds = new BoundsInt(0, 0, 0, width, height, 0);

        for (int x = 0; x < bounds.size.x; x++)
        {
            for (int y = 0; y < bounds.size.y; y++)
            {
                currentState[x, y] = initialState[x, y];
            }
        }
    }

    void UpdateTilemap(GameObject levelInstance)
    {
        selectedLevel = levelInstance.GetComponent<LevelInfoScriptableObject>();

        Tilemap tilemap = GetComponent<Tilemap>();
        TileBase[,] currentState = selectedLevel.CurrentState;

        // Ensure the dimensions match
        int width = currentState.GetLength(0);
        int height = currentState.GetLength(1);

        BoundsInt bounds = new BoundsInt(0, 0, 0, width, height, 0);

        for (int x = 0; x < bounds.size.x; x++)
        {
            for (int y = 0; y < bounds.size.y; y++)
            {
                TileBase tile = currentState[x, y];
                Vector3Int position = new Vector3Int(x, y, 0);
                tilemap.SetTile(position, tile);
            }
        }
    }
}
