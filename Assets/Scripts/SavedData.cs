using UnityEngine;

/// <summary>
/// Data container for the player's progress.
/// Serialized to JSON by the SavesManager.
/// </summary>
[System.Serializable]
public class SavedData
{
    [Header("Economy")]
    public int coins;

    [Header("Upgrades")]
    public float maxDepth;
    public int maxCatch;

    /// <summary>
    /// Default constructor sets the starting values for a brand new game.
    /// </summary>
    public SavedData()
    {
        coins = 0;
        maxDepth = -15f; // Profundidad inicial por defecto
        maxCatch = 3;    // Capacidad del anzuelo por defecto
    }
}