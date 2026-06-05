using System.Collections.Generic;
using UnityEngine;

public class WallGenerator : MonoBehaviour
{
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
    [SerializeField] private float randomX = 0.4f;
    [SerializeField] private float randomY = 0.2f;
    [SerializeField] private Vector2 scaleRange = new Vector2(0.8f, 1.2f);

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

    private void Start() => UpdateMaxDepth();

    private void UpdateMaxDepth()
    {
        if (SavesManager.Instance?.currentData != null)
        {
            maxDepth = SavesManager.Instance.currentData.maxDepth;
        }
    }

    private void HandleCastCompleted()
    {
        UpdateMaxDepth();

        float currentDepth = minDepth;
        float targetDepth = maxDepth + maxDepthOffset;

        // Generate left and right side walls downwards
        while (currentDepth >= targetDepth)
        {
            SpawnStone(wallX, currentDepth);
            SpawnStone(-wallX, currentDepth);
            currentDepth -= stoneSpacing;
        }
    }

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

    private void SpawnStone(float baseX, float baseDepth)
    {
        if (stonePrefabs == null || stonePrefabs.Length == 0) return;

        int index = Random.Range(0, stonePrefabs.Length);
        GameObject stone = GetFromPool(index);

        // Apply positions with variance offsets
        float posX = baseX + Random.Range(-randomX, randomX);
        float posY = baseDepth + Random.Range(-randomY, randomY);
        stone.transform.position = new Vector3(posX, posY, 0f);

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