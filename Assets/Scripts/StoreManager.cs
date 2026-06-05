using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;

public class StoreManager : MonoBehaviour
{
    public static event Action OnPurchaseSuccess;
    public static event Action OnPurchaseError;

    [Header("UI Elements")]
    [SerializeField] private GameObject storePanel;
    [SerializeField] private Button depthButton;
    [SerializeField] private TextMeshProUGUI depthCostText;
    [SerializeField] private Button capacityButton;
    [SerializeField] private TextMeshProUGUI capacityCostText;

    [Header("Settings")]
    [SerializeField] private float absoluteMaxDepth = 500f;
    [SerializeField] private float depthUpgradeAmount = 5f;
    [SerializeField] private int capacityUpgradeAmount = 1;

    [Header("Dependencies")]
    [SerializeField] private HookController hookController;

    private void OnEnable()
    {
        PlayerController.OnCastCompleted += HideStore;
        HookController.OnReturnToSurface += ShowStore;
        SavesManager.OnCoinsChanged += UpdateStoreUI;
    }

    private void OnDisable()
    {
        PlayerController.OnCastCompleted -= HideStore;
        HookController.OnReturnToSurface -= ShowStore;
        SavesManager.OnCoinsChanged -= UpdateStoreUI;
    }

    private void Start()
    {
        if (depthButton != null)
        {
            depthButton.onClick.RemoveAllListeners();
            depthButton.onClick.AddListener(BuyDepthUpgrade);
        }

        if (capacityButton != null)
        {
            capacityButton.onClick.RemoveAllListeners();
            capacityButton.onClick.AddListener(BuyCapacityUpgrade);
        }

        if (SavesManager.Instance?.currentData != null)
        {
            UpdateStoreUI(SavesManager.Instance.currentData.coins);
        }
    }

    private void HideStore() => storePanel?.SetActive(false);
    private void ShowStore() => storePanel?.SetActive(true);

    private int GetDepthCost()
    {
        if (SavesManager.Instance?.currentData == null) return 0;
        return Mathf.RoundToInt(Mathf.Abs(SavesManager.Instance.currentData.maxDepth) * 10f);
    }

    private int GetCapacityCost()
    {
        if (SavesManager.Instance?.currentData == null) return 0;
        return SavesManager.Instance.currentData.maxCatch * 50;
    }

    private void BuyDepthUpgrade()
    {
        if (SavesManager.Instance?.currentData == null) return;

        float currentDepth = Mathf.Abs(SavesManager.Instance.currentData.maxDepth);
        if (currentDepth >= absoluteMaxDepth)
        {
            OnPurchaseError?.Invoke();
            return;
        }

        int cost = GetDepthCost();
        if (SavesManager.Instance.currentData.coins >= cost)
        {
            SavesManager.Instance.AddCoins(-cost);
            SavesManager.Instance.currentData.maxDepth -= depthUpgradeAmount; // Subtracting moves deeper in Y space
            FinalizeTransaction();
        }
        else
        {
            OnPurchaseError?.Invoke();
        }
    }

    private void BuyCapacityUpgrade()
    {
        if (SavesManager.Instance?.currentData == null) return;

        int cost = GetCapacityCost();
        if (SavesManager.Instance.currentData.coins >= cost)
        {
            SavesManager.Instance.AddCoins(-cost);
            SavesManager.Instance.currentData.maxCatch += capacityUpgradeAmount;
            FinalizeTransaction();
        }
        else
        {
            OnPurchaseError?.Invoke();
        }
    }

    private void FinalizeTransaction()
    {
        SavesManager.Instance.SaveGame();
        if (hookController != null) hookController.RefreshStatsFromSave();
        OnPurchaseSuccess?.Invoke();
    }

    private void UpdateStoreUI(int currentCoins)
    {
        if (SavesManager.Instance?.currentData == null) return;

        float currentDepth = Mathf.Abs(SavesManager.Instance.currentData.maxDepth);
        bool isDepthMaxed = currentDepth >= absoluteMaxDepth;

        int depthCost = GetDepthCost();
        int capacityCost = GetCapacityCost();

        if (depthCostText != null)
        {
            depthCostText.text = isDepthMaxed ? "Depth Upgrade\nMAX" : $"Depth Upgrade\n{depthCost}";
        }

        if (capacityCostText != null)
        {
            capacityCostText.text = $"Catch Upgrade\n{capacityCost}";
        }

        if (depthButton != null)
        {
            depthButton.interactable = !isDepthMaxed && (currentCoins >= depthCost);
        }

        if (capacityButton != null)
        {
            capacityButton.interactable = currentCoins >= capacityCost;
        }
    }
}