using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;

/// <summary>
/// Manages meta-progression shop operations, processes upgrades for hook depth and catch capacity, 
/// updates UI interactions contextually, and dispatches transaction event hooks.
/// </summary>
public class StoreManager : MonoBehaviour
{
    public static event Action OnPurchaseSuccess;
    public static event Action OnPurchaseError;

    [Header("Store UI Elements")]
    [Tooltip("The main UI panel holding the storefront canvas graphics.")]
    [SerializeField] private GameObject storePanel;

    [Header("Global Limits")]
    [Tooltip("The safety ceiling cap for maximum depth upgrade limits.")]
    [SerializeField] private float absoluteMaxDepth = 500f;

    [Header("Depth Upgrade Configuration")]
    [SerializeField] private Button depthButton;
    [SerializeField] private TextMeshProUGUI depthCostText;
    [Tooltip("The absolute vertical value subtracted from maxDepth per upgrade level.")]
    [SerializeField] private float depthUpgradeAmount = 5f;

    [Header("Capacity Upgrade Configuration")]
    [SerializeField] private Button capacityButton;
    [SerializeField] private TextMeshProUGUI capacityCostText;
    [Tooltip("The raw volume added to max inventory storage capacity per upgrade level.")]
    [SerializeField] private int capacityUpgradeAmount = 1;

    [Header("Dependencies")]
    [Tooltip("Direct runtime reference to the fishing hook engine actor state.")]
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
        InitializeButtons();
        InitializeStoreState();
    }

    /// <summary>
    /// Purges dynamic listeners and binds primary click commands safely.
    /// </summary>
    private void InitializeButtons()
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
    }

    /// <summary>
    /// Forces a refresh on initial frame data if data dependencies are resolved.
    /// </summary>
    private void InitializeStoreState()
    {
        if (SavesManager.Instance?.currentData != null)
        {
            UpdateStoreUI(SavesManager.Instance.currentData.coins);
        }
    }

    private void HideStore()
    {
        if (storePanel != null) storePanel.SetActive(false);
    }

    private void ShowStore()
    {
        if (storePanel != null) storePanel.SetActive(true);
    }

    /// <summary>
    /// Calculates monetary progression price scaling using absolute spatial tracking values.
    /// </summary>
    private int GetDepthCost()
    {
        if (SavesManager.Instance?.currentData == null) return 0;

        float currentDepth = Mathf.Abs(SavesManager.Instance.currentData.maxDepth);
        return Mathf.RoundToInt(currentDepth * 10f);
    }

    /// <summary>
    /// Calculates static inventory capacity pricing based on current storage volumes.
    /// </summary>
    private int GetCapacityCost()
    {
        if (SavesManager.Instance?.currentData == null) return 0;

        int currentCapacity = SavesManager.Instance.currentData.maxCatch;
        return currentCapacity * 50;
    }

    /// <summary>
    /// Core processing transaction for structural hook length metrics.
    /// </summary>
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
            // Deepening subtracts from negative Y space coordinates
            SavesManager.Instance.currentData.maxDepth -= depthUpgradeAmount;

            FinalizeTransaction();
        }
        else
        {
            OnPurchaseError?.Invoke();
        }
    }

    /// <summary>
    /// Core processing transaction for safe hold capacity limits.
    /// </summary>
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

    /// <summary>
    /// Saves operational modifications down to storage layer components and notifies controllers.
    /// </summary>
    private void FinalizeTransaction()
    {
        SavesManager.Instance.SaveGame();

        if (hookController != null)
        {
            hookController.RefreshStatsFromSave();
        }

        OnPurchaseSuccess?.Invoke();
    }

    /// <summary>
    /// Repopulates text matrices and establishes active/inactive validation state on element interactive flags.
    /// </summary>
    /// <param name="currentCoins">The economy current validation threshold pass-value.</param>
    private void UpdateStoreUI(int currentCoins)
    {
        if (SavesManager.Instance?.currentData == null) return;

        float currentDepth = Mathf.Abs(SavesManager.Instance.currentData.maxDepth);
        bool isDepthMaxed = currentDepth >= absoluteMaxDepth;

        int depthCost = GetDepthCost();
        int capacityCost = GetCapacityCost();

        // UI String Allocations
        if (depthCostText != null)
        {
            depthCostText.text = isDepthMaxed ? "Depth Upgrade\nMAX" : $"Depth Upgrade\n{depthCost}";
        }

        if (capacityCostText != null)
        {
            capacityCostText.text = $"Catch Upgrade\n{capacityCost}";
        }

        // Interaction Access Conversions
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