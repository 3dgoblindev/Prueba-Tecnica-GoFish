using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Manages the store UI, button interactions, and purchasing logic.
/// Automatically hides/shows based on the game's state.
/// </summary>
public class StoreManager : MonoBehaviour
{
    [Header("Store UI Elements")]
    [Tooltip("The main panel or parent object of the store that will be toggled on/off.")]
    [SerializeField] private GameObject storePanel;

    [Header("Global Limits")]
    [Tooltip("The absolute maximum depth the player can reach (in positive meters).")]
    [SerializeField] private float absoluteMaxDepth = 500f; // <-- NUEVA VARIABLE

    [Header("Depth Upgrade")]
    [SerializeField] private Button depthButton;
    [SerializeField] private TextMeshProUGUI depthCostText;
    [Tooltip("How much depth is added per upgrade.")]
    [SerializeField] private float depthUpgradeAmount = 5f;

    [Header("Capacity Upgrade")]
    [SerializeField] private Button capacityButton;
    [SerializeField] private TextMeshProUGUI capacityCostText;
    [Tooltip("How many more fish the hook can hold per upgrade.")]
    [SerializeField] private int capacityUpgradeAmount = 1;

    [Header("Dependencies")]
    [Tooltip("Needed to refresh the hook stats immediately after a purchase.")]
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
        // <-- COMPROBACIÓN DE LÍMITE ANTES DE COMPRAR
        float currentDepth = Mathf.Abs(SavesManager.Instance.currentData.maxDepth);
        if (currentDepth >= absoluteMaxDepth)
        {
            Debug.Log("Max depth already reached!");
            return;
        }

        int cost = GetDepthCost();

        if (SavesManager.Instance.currentData.coins >= cost)
        {
            SavesManager.Instance.AddCoins(-cost);
            SavesManager.Instance.currentData.maxDepth -= depthUpgradeAmount;
            SavesManager.Instance.SaveGame();

            if (hookController != null) hookController.RefreshStatsFromSave();

            Debug.Log("Depth Upgrade Purchased!");
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

            Debug.Log("Capacity Upgrade Purchased!");
        }
    }

    private void UpdateStoreUI(int currentCoins)
    {
        if (SavesManager.Instance == null) return;

        // <-- LÓGICA VISUAL DE LÍMITE MÁXIMO
        float currentDepth = Mathf.Abs(SavesManager.Instance.currentData.maxDepth);
        bool isDepthMaxed = currentDepth >= absoluteMaxDepth;

        int depthCost = GetDepthCost();
        int capacityCost = GetCapacityCost();

        // Actualizamos texto de Profundidad
        if (depthCostText != null)
        {
            if (isDepthMaxed)
                depthCostText.text = "Depth Upgrade\nMAX"; // Muestra MAX si se alcanzó el límite
            else
                depthCostText.text = $"Depth Upgrade\n{depthCost} ";
        }

        // Actualizamos texto de Capacidad
        if (capacityCostText != null) capacityCostText.text = $"Catch Upgrade\n{capacityCost} ";

        // Bloqueamos el botón si no hay dinero o si ya está al máximo
        if (depthButton != null)
            depthButton.interactable = !isDepthMaxed && (currentCoins >= depthCost);

        if (capacityButton != null)
            capacityButton.interactable = currentCoins >= capacityCost;
    }
}