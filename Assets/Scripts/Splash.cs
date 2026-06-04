using UnityEngine;

public class Splash : MonoBehaviour
{
    [Header("Particles")]
    [SerializeField] private GameObject splashParticlesPrefab;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Sea"))
            Instantiate(splashParticlesPrefab, transform.position, Quaternion.identity);
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Sea"))
            Instantiate(splashParticlesPrefab, transform.position, Quaternion.identity);
    }
}