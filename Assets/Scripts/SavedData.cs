using System;
using UnityEngine;

/// <summary>
/// Serializable data transfer object representing the persistent state of the player's progression.
/// Managed exclusively by the SavesManager serialization pipeline.
/// </summary>
[Serializable]
public class SavedData
{
    [Header("Economy Snapshot")]
    [Tooltip("Total soft currency balance held by the user.")]
    public int coins;

    [Header("Progression Metrics")]
    [Tooltip("The maximum vertical depth threshold boundaries the fishing hook actor can travel (stored as negative spatial units).")]
    public float maxDepth;

    [Tooltip("The maximum inventory volume allowance for carrying items concurrently.")]
    public int maxCatch;

    /// <summary>
    /// Initializes a brand new instance of the dataset with baseline default starting parameters.
    /// </summary>
    public SavedData()
    {
        coins = 0;
        maxDepth = -15f;
        maxCatch = 3;
    }
}