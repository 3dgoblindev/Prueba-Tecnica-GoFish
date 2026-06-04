using UnityEngine;

[CreateAssetMenu(fileName = "NewFishData", menuName = "Fishing Game/Fish Data")]
public class FishData : ScriptableObject
{
    public enum FishRarity { Common, Rare, Epic, Legendary }

    [Header("Identity & Prefab")]
    public string fishName = "New Fish";

    [Tooltip("The specific prefab for this fish (contains its unique collider and animations).")]
    public FishController fishPrefab;

    [Header("Economy & Stats")]
    public int price = 10;
    public FishRarity rarity = FishRarity.Common;
    public float baseSwimSpeed = 2f;

    [Header("Spawn Settings")]
    [Tooltip("Minimum depth (closest to surface, e.g., -1f) where this fish can spawn.")]
    public float minDepth = -1f;
    [Tooltip("Maximum depth (deepest point, e.g., -15f) where this fish can spawn.")]
    public float maxDepth = -15f;

    [Header("Juice & Feel")]
    public GameObject catchParticlesPrefab;
}