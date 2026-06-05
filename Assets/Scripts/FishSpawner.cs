using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Manages runtime generation, localized zone distribution filtering, and dictionary-keyed object pooling 
/// for swimming fish entities based on contextual progression depths.
/// </summary>
public class FishSpawner : MonoBehaviour
{
    [Header("Asset Dependencies")]
    [Tooltip("The comprehensive roster array containing all fish definitions available in the system.")]
    [SerializeField] private FishData[] allAvailableFishes;

    [Header("Spatial Boundaries")]
    [Tooltip("The top vertical boundary coordinate threshold where target entities can spawn.")]
    [SerializeField] private float minCastDepth = -1f;

    [Tooltip("The dynamic baseline vertical floor depth limit calculated on system initialization.")]
    [SerializeField] private float currentMaxCastDepth = -15f;

    [Header("Algorithmic Tuners")]
    [Tooltip("The scalar factor multiplied against the total absolute depth to determine maximum entity counts per cast.")]
    [SerializeField] private float fishDensity = 1.5f;

    [Tooltip("Horizontal local coordinate constraints limits for layout distribution (X = Min, Y = Max).")]
    [SerializeField] private Vector2 horizontalSpawnBounds = new Vector2(-2.5f, 2.5f);

    // Context-Keyed Object Pooling Infrastructure
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

    private void Start()
    {
        InitializeMaxCastDepth();
    }

    /// <summary>
    /// Synchronizes spatial spawning ceilings with persistent save profiles.
    /// </summary>
    private void InitializeMaxCastDepth()
    {
        if (SavesManager.Instance?.currentData != null)
        {
            currentMaxCastDepth = SavesManager.Instance.currentData.maxDepth;
        }
    }

    /// <summary>
    /// Event handler triggered on player line cast completion. Evaluates density metrics and populates the vertical volume.
    /// </summary>
    private void HandleCastCompleted()
    {
        int calculatedFishAmount = Mathf.Max(1, Mathf.RoundToInt(Mathf.Abs(currentMaxCastDepth) * fishDensity));
        InitializeMaxCastDepth();

        for (int i = 0; i < calculatedFishAmount; i++)
        {
            float randomDepth = Random.Range(minCastDepth, currentMaxCastDepth);
            FishData selectedData = GetRandomFishDataForDepth(randomDepth);

            if (selectedData == null || selectedData.fishPrefab == null)
            {
                continue;
            }

            FishController fish = GetFishFromPool(selectedData);

            // Compute spatial layouts and directional states
            float randomX = Random.Range(horizontalSpawnBounds.x, horizontalSpawnBounds.y);
            float randomDirection = Random.value > 0.5f ? 1f : -1f;

            // Actor Initialization
            fish.ResetForSpawn(randomX, randomDepth, transform);
            fish.InitializeMovement(randomDirection, selectedData.baseSwimSpeed);
            fish.gameObject.SetActive(true);

            activeFishes.Add(fish);
        }
    }

    /// <summary>
    /// Event handler triggered when the line returns to origin. Disables runtime instances and recycles them to their respective queues.
    /// </summary>
    private void HandleReturnToSurface()
    {
        foreach (FishController fish in activeFishes)
        {
            if (fish == null) continue;

            fish.gameObject.SetActive(false);

            if (fish.data != null && fishPools.ContainsKey(fish.data))
            {
                fishPools[fish.data].Enqueue(fish);
            }
        }

        activeFishes.Clear();
    }

    /// <summary>
    /// Looks up an existing object type pool container or constructs one before spinning up a recycled tracking context.
    /// </summary>
    /// <param name="data">The matching scriptable object asset template acting as the unique structural pool sorting ID.</param>
    /// <returns>A clean, inactive FishController instance linked back to its origin definitions.</returns>
    private FishController GetFishFromPool(FishData data)
    {
        if (!fishPools.ContainsKey(data))
        {
            fishPools[data] = new Queue<FishController>();
        }

        if (fishPools[data].Count > 0)
        {
            return fishPools[data].Dequeue();
        }

        FishController newFish = Instantiate(data.fishPrefab, transform);
        newFish.data = data;
        newFish.gameObject.SetActive(false);

        return newFish;
    }

    /// <summary>
    /// Iterates through the full dataset matrix to filter down a pool of valid choices matching the target space parameters.
    /// </summary>
    /// <param name="depth">The precise vertical evaluation layer coordinate passed by the generation step loop.</param>
    /// <returns>A randomly selected valid scriptable data block profile or null if depth layers evaluate empty.</returns>
    private FishData GetRandomFishDataForDepth(float depth)
    {
        if (allAvailableFishes == null || allAvailableFishes.Length == 0) return null;

        List<FishData> validFishes = new List<FishData>();

        foreach (FishData fish in allAvailableFishes)
        {
            if (fish != null && depth <= fish.minDepth && depth >= fish.maxDepth)
            {
                validFishes.Add(fish);
            }
        }

        if (validFishes.Count == 0) return null;

        return validFishes[Random.Range(0, validFishes.Count)];
    }
}