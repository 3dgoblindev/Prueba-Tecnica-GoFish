using UnityEngine;

/// <summary>
/// Listens to store purchase transaction events and routes the corresponding audio clips 
/// to the global AudioManager with micro-pitch variations.
/// </summary>
public class StoreSFX : MonoBehaviour
{
    [Header("Audio Clips")]
    [Tooltip("Sound effect played when a transaction completes successfully.")]
    [SerializeField] private AudioClip purchaseSound;

    [Tooltip("Sound effect played when a transaction fails or cannot be processed.")]
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

    /// <summary>
    /// Event handler for successful shop transactions.
    /// </summary>
    private void HandlePurchaseSuccess()
    {
        PlaySound(purchaseSound);
    }

    /// <summary>
    /// Event handler for failed shop transactions.
    /// </summary>
    private void HandlePurchaseError()
    {
        PlaySound(errorSound);
    }

    /// <summary>
    /// Safe wrapper to play one-shot sound effects via the global AudioManager with subtle pitch shifting.
    /// </summary>
    /// <param name="clip">The clip to register with the audio mixer channels.</param>
    private void PlaySound(AudioClip clip)
    {
        if (clip == null) return;

        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlaySFX(clip, pitchMin: 0.95f, pitchMax: 1.05f);
        }
        else
        {
            Debug.LogWarning($"[{nameof(StoreSFX)}] Cannot play sound '{clip.name}' because AudioManager.Instance is missing.", this);
        }
    }
}