using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;

public class StoreManager : MonoBehaviour
{
    public static event Action OnPurchaseSuccess;
    public static event Action OnPurchaseError;

    [Header("Store UI Elements")]
    [SerializeField] private GameObject storePanel;

    [Header("Global Limits")]
    [SerializeField] private float absoluteMaxDepth = 500f;

    [Header("Depth Upgrade")]
    [SerializeField] private Button depthButton;
    [SerializeField] private TextMeshProUGUI depthCostText;
    [SerializeField] private float depthUpgradeAmount = 5f;

    [Header("Capacity Upgrade")]
    [SerializeField] private Button capacityButton;
    [SerializeField] private TextMeshProUGUI capacityCostText;
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
        depthButton.onClick.RemoveAllListeners();
        depthButton.onClick.AddListener(BuyDepthUpgrade);

        capacityButton.onClick.RemoveAllListeners();
        capacityButton.onClick.AddListener(BuyCapacityUpgrade);

        if (SavesManager.Instance != null)
            UpdateStoreUI(SavesManager.Instance.currentData.coins);
    }

    private void HideStore() { if (storePanel != null) storePanel.SetActive(false); }
    private void ShowStore() { if (storePanel != null) storePanel.SetActive(true); }

    private int GetDepthCost()
    {
        float currentDepth = Mathf.Abs(SavesManager.Instance.currentData.maxDepth);
        return Mathf.RoundToInt(currentDepth * 10f);
    }

    private int GetCapacityCost()
    {
        int currentCapacity = SavesManager.Instance.currentData.maxCatch;
        return currentCapacity * 50;
    }

    private void BuyDepthUpgrade()
    {
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
            SavesManager.Instance.currentData.maxDepth -= depthUpgradeAmount;
            SavesManager.Instance.SaveGame();
            if (hookController != null) hookController.RefreshStatsFromSave();
            OnPurchaseSuccess?.Invoke();
        }
        else
        {
            OnPurchaseError?.Invoke();
        }
    }

    private void BuyCapacityUpgrade()
    {
        int cost = GetCapacityCost();
        if (SavesManager.Instance.currentData.coins >= cost)
        {
            SavesManager.Instance.AddCoins(-cost);
            SavesManager.Instance.currentData.maxCatch += capacityUpgradeAmount;
            SavesManager.Instance.SaveGame();
            if (hookController != null) hookController.RefreshStatsFromSave();
            OnPurchaseSuccess?.Invoke();
        }
        else
        {
            OnPurchaseError?.Invoke();
        }
    }

    private void UpdateStoreUI(int currentCoins)
    {
        if (SavesManager.Instance == null) return;

        float currentDepth = Mathf.Abs(SavesManager.Instance.currentData.maxDepth);
        bool isDepthMaxed = currentDepth >= absoluteMaxDepth;

        int depthCost = GetDepthCost();
        int capacityCost = GetCapacityCost();

        if (depthCostText != null)
            depthCostText.text = isDepthMaxed ? "Depth Upgrade\nMAX" : $"Depth Upgrade\n{depthCost} ";

        if (capacityCostText != null)
            capacityCostText.text = $"Catch Upgrade\n{capacityCost} ";

        if (depthButton != null)
            depthButton.interactable = !isDepthMaxed && (currentCoins >= depthCost);

        if (capacityButton != null)
            capacityButton.interactable = currentCoins >= capacityCost;
    }
}