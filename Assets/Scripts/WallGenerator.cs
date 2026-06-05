using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Generates vertical boundary walls of random stone prefabs at specified X positions down to the maximum depth.
/// Utilizes a dictionary-based object pooling system tied to player cast and return lifecycle events.
/// </summary>
public class WallGenerator : MonoBehaviour
{
    [Header("Dependencies")]
    [Tooltip("Array of stone prefabs used to construct the environment walls.")]
    [SerializeField] private GameObject[] stonePrefabs;

    [Header("Wall Boundary Settings")]
    [Tooltip("The absolute X coordinate position for the left and right walls.")]
    [SerializeField] private float wallX = 5f;

    [Tooltip("The initial Y coordinate depth where wall generation begins.")]
    [SerializeField] private float minDepth = -1f;

    [Tooltip("The fallback maximum depth. This value is typically overwritten by the current save data at runtime.")]
    [SerializeField] private float maxDepth = -20f;

    [Tooltip("Additional padding added to the maximum depth to ensure coverage.")]
    [SerializeField] private float maxDepthOffset = -20f;

    [Tooltip("The vertical distance step interval between individual spawned stones.")]
    [SerializeField] private float stoneSpacing = 1f;

    [Header("Randomization Settings")]
    [Tooltip("If enabled, applies a random Z-axis rotation to each spawned stone.")]
    [SerializeField] private bool randomRotationZ = true;

    [Tooltip("If enabled, applies a random uniform scale within the defined Scale Range.")]
    [SerializeField] private bool randomScale = false;

    [Tooltip("Maximum random horizontal offset applied to the stone's base X position.")]
    [SerializeField] private float randomX = 0.4f;

    [Tooltip("Maximum random vertical offset applied to the stone's base Y position.")]
    [SerializeField] private float randomY = 0.2f;

    [Tooltip("Minimum and maximum boundaries for uniform object scaling (X = Min, Y = Max).")]
    [SerializeField] private Vector2 scaleRange = new Vector2(0.8f, 1.2f);

    // Object Pooling Infrastructure
    private Dictionary<int, Queue<GameObject>> stonePools = new Dictionary<int, Queue<GameObject>>();
    private List<(GameObject go, int prefabIndex)> activeStones = new List<(GameObject, int)>();

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
        InitializeMaxDepth();
    }

    /// <summary>
    /// Synchronizes the generation boundaries with the player's progression data.
    /// </summary>
    private void InitializeMaxDepth()
    {
        if (SavesManager.Instance?.currentData != null)
        {
            maxDepth = SavesManager.Instance.currentData.maxDepth;
        }
    }

    /// <summary>
    /// Event handler triggered when the player's cast is complete. Spawns left and right boundaries down to max depth.
    /// </summary>
    private void HandleCastCompleted()
    {
        InitializeMaxDepth();
        float currentDepth = minDepth;
        float targetDepth = maxDepth + maxDepthOffset;

        while (currentDepth >= targetDepth)
        {
            SpawnStone(wallX, currentDepth);
            SpawnStone(-wallX, currentDepth);
            currentDepth -= stoneSpacing;
        }
    }

    /// <summary>
    /// Event handler triggered when the hook returns to the surface. Clears active instances and recycles them into the pool.
    /// </summary>
    private void HandleReturnToSurface()
    {
        foreach (var (stone, prefabIndex) in activeStones)
        {
            if (stone == null) continue;

            stone.SetActive(false);
            stonePools[prefabIndex].Enqueue(stone);
        }

        activeStones.Clear();
    }

    /// <summary>
    /// Fetches a stone from the pool, applies positions, rotations, and scale modifications, and activates it.
    /// </summary>
    /// <param name="baseX">The base horizontal position axis.</param>
    /// <param name="baseDepth">The base vertical depth axis.</param>
    private void SpawnStone(float baseX, float baseDepth)
    {
        if (stonePrefabs == null || stonePrefabs.Length == 0) return;

        int index = Random.Range(0, stonePrefabs.Length);
        GameObject stone = GetFromPool(index);

        // Position alignment with variance offsets
        float posX = baseX + Random.Range(-randomX, randomX);
        float posY = baseDepth + Random.Range(-randomY, randomY);
        stone.transform.position = new Vector3(posX, posY, 0f);

        // Contextual variance transformations
        if (randomRotationZ)
        {
            stone.transform.rotation = Quaternion.Euler(0f, 0f, Random.Range(0f, 360f));
        }

        if (randomScale)
        {
            float scale = Random.Range(scaleRange.x, scaleRange.y);
            stone.transform.localScale = Vector3.one * scale;
        }

        stone.SetActive(true);
        activeStones.Add((stone, index));
    }

    /// <summary>
    /// Retrieves an available object from the corresponding prefab index pool queue, or instantiates a new one if empty.
    /// </summary>
    /// <param name="prefabIndex">The index matching the required item inside the stonePrefabs collection.</param>
    /// <returns>A recycled or newly instantiated inactive GameObject context.</returns>
    private GameObject GetFromPool(int prefabIndex)
    {
        if (!stonePools.ContainsKey(prefabIndex))
        {
            stonePools[prefabIndex] = new Queue<GameObject>();
        }

        if (stonePools[prefabIndex].Count > 0)
        {
            return stonePools[prefabIndex].Dequeue();
        }

        GameObject newStone = Instantiate(stonePrefabs[prefabIndex], transform);
        newStone.SetActive(false);
        return newStone;
    }
}