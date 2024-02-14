using System.Collections;
using System.Collections.Generic;
using Unity.MLAgents;
using UnityEngine;

public class TrainEnvironment : MonoBehaviour
{
    public LevelGeneratorAgent mlAgent;
    public LevelGeneration level;

    [Header("Environment Parameters")]
    public int episodes;
    public int iteration;
    public int maxChanges;
    public int maxIterations;

    // Called when the environment starts
    void Start()
    {
        // Initialization code here
        // You may want to initialize your Level and ML Agent components
    }

    // Called when the environment is reset
    public void ResetEnvironment()
    {
        // Reset the level component
        //level.ResetLevel();

        // Reset the ML Agent
        //mlAgent.ResetAgent();
    }

    // Called when the environment updates
    void Update()
    {
        // Update logic here
    }
}
