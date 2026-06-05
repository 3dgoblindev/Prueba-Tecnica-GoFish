using UnityEngine;

public class StoreSFX : MonoBehaviour
{
    [SerializeField] private AudioClip purchaseSound;
    [SerializeField] private AudioClip errorSound;

    private void OnEnable()
    {
        StoreManager.OnPurchaseSuccess += PlayPurchase;
        StoreManager.OnPurchaseError += PlayError;
    }

    private void OnDisable()
    {
        StoreManager.OnPurchaseSuccess -= PlayPurchase;
        StoreManager.OnPurchaseError -= PlayError;
    }

    private void PlayPurchase() => AudioManager.Instance.PlaySFX(purchaseSound, pitchMin: 0.95f, pitchMax: 1.05f);
    private void PlayError() => AudioManager.Instance.PlaySFX(errorSound, pitchMin: 0.95f, pitchMax: 1.05f);
}