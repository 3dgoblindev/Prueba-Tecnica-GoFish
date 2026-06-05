using UnityEngine;

public class StoreSFX : MonoBehaviour
{
    [SerializeField] private AudioClip purchaseSound;
    [SerializeField] private AudioClip errorSound;

    private void OnEnable()
    {
        StoreManager.OnPurchaseSuccess += HandlePurchaseSuccess;
        StoreManager.OnPurchaseError += HandlePurchaseError;
    }

    private void OnDisable()
    {
        StoreManager.OnPurchaseSuccess -= HandlePurchaseSuccess;
        StoreManager.OnPurchaseError -= HandlePurchaseError;
    }

    private void HandlePurchaseSuccess() => PlaySound(purchaseSound);
    private void HandlePurchaseError() => PlaySound(errorSound);

    private void PlaySound(AudioClip clip)
    {
        if (clip == null) return;

        if (AudioManager.Instance != null)
        {
            // Play with micro-pitch variation for juice
            AudioManager.Instance.PlaySFX(clip, pitchMin: 0.95f, pitchMax: 1.05f);
        }
        else
        {
            Debug.LogWarning($"[StoreSFX] Missing AudioManager instance for: {clip.name}", this);
        }
    }
}