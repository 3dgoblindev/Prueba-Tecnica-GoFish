using UnityEngine;

public class Splash : MonoBehaviour
{
    [Header("Particles")]
    [SerializeField] private GameObject splashParticlesPrefab;

    [Header("Audio")]
    [SerializeField] private AudioClip waterSound;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Sea"))
        {
            Instantiate(splashParticlesPrefab, transform.position, Quaternion.identity);
            AudioManager.Instance.PlaySFX(waterSound, pitchMin: 0.95f, pitchMax: 1.05f);
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Sea"))
        {
            Instantiate(splashParticlesPrefab, transform.position, Quaternion.identity);
            AudioManager.Instance.PlaySFX(waterSound, pitchMin: 0.95f, pitchMax: 1.05f);
        }
    }
}