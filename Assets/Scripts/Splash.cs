using UnityEngine;

// Handles spawning splash particles and playing audio when entering/exiting the water
public class Splash : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField] private GameObject splashParticlesPrefab;
    [SerializeField] private AudioClip waterSound;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Sea"))
        {
            ExecuteSplash();
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Sea"))
        {
            ExecuteSplash();
        }
    }

    private void ExecuteSplash()
    {
        if (splashParticlesPrefab != null)
        {
            Instantiate(splashParticlesPrefab, transform.position, Quaternion.identity);
        }

        if (waterSound != null && AudioManager.Instance != null)
        {
            AudioManager.Instance.PlaySFX(waterSound, pitchMin: 0.95f, pitchMax: 1.05f);
        }
    }
}