using UnityEngine;

/// <summary>
/// Spawns environmental VFX prefabs and triggers situational sound effects 
/// whenever the actor passes through 2D water trigger boundaries.
/// </summary>
public class Splash : MonoBehaviour
{
    [Header("Visual Effects")]
    [Tooltip("The instantiation prefab template for the water splash particle system.")]
    [SerializeField] private GameObject splashParticlesPrefab;

    [Header("Audio Settings")]
    [Tooltip("Audio clip executed upon cross-boundary trigger intersections.")]
    [SerializeField] private AudioClip waterSound;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Sea"))
        {
            ExecuteSplashEffects();
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Sea"))
        {
            ExecuteSplashEffects();
        }
    }

    /// <summary>
    /// Instantiates particles at the current local pivot space position and routes tracking sound requests to the sound manager.
    /// </summary>
    private void ExecuteSplashEffects()
    {
        // Handle visual effect instantiation safely
        if (splashParticlesPrefab != null)
        {
            Instantiate(splashParticlesPrefab, transform.position, Quaternion.identity);
        }

        // Route one-shot audio properties securely
        if (waterSound != null && AudioManager.Instance != null)
        {
            AudioManager.Instance.PlaySFX(waterSound, pitchMin: 0.95f, pitchMax: 1.05f);
        }
    }
}