using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Manages the dynamic pooling and spawning of different fish prefabs based on depth zones.
/// Uses a density-based algorithm to prevent overcrowding in shallow waters.
/// </summary>
public class FishSpawner : MonoBehaviour
{
    [Header("Dependencies")]
    [Tooltip("Array of all available fish types in the game.")]
    [SerializeField] private FishData[] allAvailableFishes;

    [Header("Spawn Configuration")]
    [Tooltip("The maximum depth the player can currently reach. Should be synced with player stats.")]
    [SerializeField] private float currentMaxCastDepth = -15f;

    [Tooltip("How many fishes to spawn per 1 unit of depth. Controls the visual density.")]
    [SerializeField] private float fishDensity = 1.5f;

    [Tooltip("Horizontal bounds for spawning fishes (X min, X max).")]
    [SerializeField] private Vector2 horizontalSpawnBounds = new Vector2(-2.5f, 2.5f);

    // Dynamic Object Pool Dictionary: Groups pools by their FishData type
    private Dictionary<FishData, Queue<FishController>> fishPools = new Dictionary<FishData, Queue<FishController>>();
    private List<FishController> activeFishes = new List<FishController>();

    private void OnEnable()
    {
        PlayerController.OnCastCompleted += HandleCastCompleted;
        HookController.OnReturnToSurface += HandleReturnToSurface;
    }

    private void OnDisable()
    {
        PlayerController.OnCastCompleted -= HandleCastCompleted;
        HookController.OnReturnToSurface -= HandleReturnToSurface;
    }

    private void HandleCastCompleted()
    {
        // Calculate the dynamic amount of fishes based on depth to avoid overcrowding
        // Mathf.Abs ensures the depth is positive for calculation, Mathf.Max ensures at least 1 fish spawns.
        int calculatedFishAmount = Mathf.Max(1, Mathf.RoundToInt(Mathf.Abs(currentMaxCastDepth) * fishDensity));

        for (int i = 0; i < calculatedFishAmount; i++)
        {
            float randomDepth = Random.Range(0f, currentMaxCastDepth);

            FishData selectedData = GetRandomFishDataForDepth(randomDepth);
            if (selectedData == null || selectedData.fishPrefab == null) continue;

            // Pass the specific data to get the correct prefab from the correct pool
            FishController fish = GetFishFromPool(selectedData);

            float randomX = Random.Range(horizontalSpawnBounds.x, horizontalSpawnBounds.y);
            float randomDirection = Random.value > 0.5f ? 1f : -1f;

            // Reset and activate the fish
            fish.ResetForSpawn(randomX, randomDepth, this.transform);
            fish.InitializeMovement(randomDirection, selectedData.baseSwimSpeed);
            fish.gameObject.SetActive(true);

            activeFishes.Add(fish);
        }
    }

    private void HandleReturnToSurface()
    {
        foreach (FishController fish in activeFishes)
        {
            if (fish != null)
            {
                fish.gameObject.SetActive(false);

                // Return the fish to its specific pool based on its data
                fishPools[fish.data].Enqueue(fish);
            }
        }

        activeFishes.Clear();
    }

    /// <summary>
    /// Retrieves a specific fish prefab from its designated pool, or instantiates one if empty.
    /// </summary>
    private FishController GetFishFromPool(FishData data)
    {
        // If this fish type doesn't have a pool yet, create one
        if (!fishPools.ContainsKey(data))
        {
            fishPools[data] = new Queue<FishController>();
        }

        // Try to get a pooled object
        if (fishPools[data].Count > 0)
        {
            return fishPools[data].Dequeue();
        }

        // Pool is empty, instantiate the specific prefab assigned in the FishData
        FishController newFish = Instantiate(data.fishPrefab, this.transform);

        // Ensure the instantiated fish knows its own data for when it returns to the pool
        newFish.data = data;

        newFish.gameObject.SetActive(false);
        return newFish;
    }

    private FishData GetRandomFishDataForDepth(float depth)
    {
        List<FishData> validFishes = new List<FishData>();

        foreach (FishData fish in allAvailableFishes)
        {
            if (depth <= fish.minDepth && depth >= fish.maxDepth)
            {
                validFishes.Add(fish);
            }
        }

        if (validFishes.Count == 0) return null;

        return validFishes[Random.Range(0, validFishes.Count)];
    }
}