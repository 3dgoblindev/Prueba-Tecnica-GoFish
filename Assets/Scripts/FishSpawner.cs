using System.Collections.Generic;
using UnityEngine;

public class FishSpawner : MonoBehaviour
{
    [SerializeField] private FishData[] allAvailableFishes;

    [Header("Settings")]
    [SerializeField] private float minCastDepth = -1f;
    [SerializeField] private float currentMaxCastDepth = -15f;
    [SerializeField] private float fishDensity = 1.5f;
    [SerializeField] private Vector2 horizontalSpawnBounds = new Vector2(-2.5f, 2.5f);

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
        UpdateMaxDepth();
    }

    private void UpdateMaxDepth()
    {
        if (SavesManager.Instance?.currentData != null)
        {
            currentMaxCastDepth = SavesManager.Instance.currentData.maxDepth;
        }
    }

    private void HandleCastCompleted()
    {
        UpdateMaxDepth();

        // Scale spawn count by current maximum depth
        int fishCount = Mathf.Max(1, Mathf.RoundToInt(Mathf.Abs(currentMaxCastDepth) * fishDensity));

        for (int i = 0; i < fishCount; i++)
        {
            float randomDepth = Random.Range(minCastDepth, currentMaxCastDepth);
            FishData selectedData = GetRandomFishDataForDepth(randomDepth);

            if (selectedData == null || selectedData.fishPrefab == null) continue;

            FishController fish = GetFishFromPool(selectedData);

            float randomX = Random.Range(horizontalSpawnBounds.x, horizontalSpawnBounds.y);
            float randomDir = Random.value > 0.5f ? 1f : -1f;

            fish.ResetForSpawn(randomX, randomDepth, transform);
            fish.InitializeMovement(randomDir, selectedData.baseSwimSpeed);
            fish.gameObject.SetActive(true);

            activeFishes.Add(fish);
        }
    }

    private void HandleReturnToSurface()
    {
        foreach (FishController fish in activeFishes)
        {
            if (fish == null) continue;
            fish.gameObject.SetActive(false);

            if (fish.data != null)
            {
                if (!fishPools.ContainsKey(fish.data))
                    fishPools[fish.data] = new Queue<FishController>();

                if (!fishPools[fish.data].Contains(fish))
                    fishPools[fish.data].Enqueue(fish);
            }
        }
        activeFishes.Clear();

        foreach (Transform child in transform)
        {
            FishController orphan = child.GetComponent<FishController>();
            if (orphan == null || orphan.gameObject.activeSelf) continue;
            if (orphan.data == null) continue;

            if (!fishPools.ContainsKey(orphan.data))
                fishPools[orphan.data] = new Queue<FishController>();

            if (!fishPools[orphan.data].Contains(orphan))
                fishPools[orphan.data].Enqueue(orphan);
        }
    }
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

        // Instantiate new if pool is dry
        FishController newFish = Instantiate(data.fishPrefab, transform);
        newFish.data = data;
        newFish.gameObject.SetActive(false);

        return newFish;
    }

    private FishData GetRandomFishDataForDepth(float depth)
    {
        if (allAvailableFishes == null || allAvailableFishes.Length == 0) return null;

        List<FishData> validFishes = new List<FishData>();
        List<float> weights = new List<float>();

        foreach (FishData fish in allAvailableFishes)
        {
            if (fish != null && depth <= fish.minDepth && depth >= fish.maxDepth)
            {
                validFishes.Add(fish);
                weights.Add(GetSpawnWeight(fish.rarity));
            }
        }

        if (validFishes.Count == 0) return null;

        float totalWeight = 0f;
        foreach (float w in weights) totalWeight += w;

        float roll = Random.Range(0f, totalWeight);
        float cumulative = 0f;

        for (int i = 0; i < validFishes.Count; i++)
        {
            cumulative += weights[i];
            if (roll <= cumulative) return validFishes[i];
        }

        return validFishes[validFishes.Count - 1];
    }

    private float GetSpawnWeight(FishData.FishRarity rarity)
    {
        switch (rarity)
        {
            case FishData.FishRarity.Common: return 100f;
            case FishData.FishRarity.Rare: return 50f;
            case FishData.FishRarity.Epic: return 30f;
            case FishData.FishRarity.Legendary: return 10f;
            default: return 100f;
        }
    }

    public void ReturnFishToPool(FishController fish)
    {
        if (fish == null || fish.data == null) return;

        fish.gameObject.SetActive(false);
        fish.transform.SetParent(transform);
        fish.transform.localScale = Vector3.one;

        activeFishes.Remove(fish);

        if (!fishPools.ContainsKey(fish.data))
            fishPools[fish.data] = new Queue<FishController>();

        if (!fishPools[fish.data].Contains(fish))
            fishPools[fish.data].Enqueue(fish);
    }

    public void RemoveActiveFish(FishController fish)
    {
        if (activeFishes.Contains(fish))
        {
            activeFishes.Remove(fish);
        }
    }
}