using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Generates walls of random stone prefabs at +X and -X down to maxDepth.
/// Uses object pooling and syncs with cast/return events like FishSpawner.
/// </summary>
public class WallGenerator : MonoBehaviour
{
    [Header("Dependencies")]
    [SerializeField] private GameObject[] stonePrefabs;

    [Header("Wall Settings")]
    [SerializeField] private float wallX = 5f;
    [SerializeField] private float minDepth = -1f;
    [SerializeField] private float maxDepth = -20f;
    [SerializeField] private float maxDepthOffset = -20f;
    [SerializeField] private float stoneSpacing = 1f;

    [Header("Randomization")]
    [SerializeField] private bool randomRotationZ = true;
    [SerializeField] private bool randomScale = false;
    [SerializeField] private float randomY = 0.2f;
    [SerializeField] private float randomX = 0.4f;

    [SerializeField] private Vector2 scaleRange = new Vector2(0.8f, 1.2f);

    // Pool: one queue per prefab index
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

    private void HandleCastCompleted()
    {
        float depth = minDepth;
        while (depth >= maxDepth + maxDepthOffset)
        {
            SpawnStone(wallX, depth);
            SpawnStone(-wallX, depth);
            depth -= stoneSpacing;
        }
    }

    private void HandleReturnToSurface()
    {
        foreach (var (go, prefabIndex) in activeStones)
        {
            if (go == null) continue;
            go.SetActive(false);
            stonePools[prefabIndex].Enqueue(go);
        }
        activeStones.Clear();
    }

    private void SpawnStone(float x, float depth)
    {
        if (stonePrefabs == null || stonePrefabs.Length == 0) return;

        int index = Random.Range(0, stonePrefabs.Length);
        GameObject stone = GetFromPool(index);

        stone.transform.SetParent(transform);
        stone.transform.position = new Vector3(x + Random.Range(-randomX, randomX), depth + Random.Range(-randomY, randomY), 0f);

        if (randomRotationZ)
            stone.transform.rotation = Quaternion.Euler(0f, 0f, Random.Range(0f, 360f));

        if (randomScale)
        {
            float scale = Random.Range(scaleRange.x, scaleRange.y);
            stone.transform.localScale = Vector3.one * scale;
        }

        stone.SetActive(true);
        activeStones.Add((stone, index));
    }

    private GameObject GetFromPool(int prefabIndex)
    {
        if (!stonePools.ContainsKey(prefabIndex))
            stonePools[prefabIndex] = new Queue<GameObject>();

        if (stonePools[prefabIndex].Count > 0)
            return stonePools[prefabIndex].Dequeue();

        GameObject newStone = Instantiate(stonePrefabs[prefabIndex], transform);
        newStone.SetActive(false);
        return newStone;
    }
}