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
        // 1. Nos suscribimos a los eventos del ciclo de juego para ocultar/mostrar la tienda
        PlayerController.OnCastCompleted += HideStore;
        HookController.OnReturnToSurface += ShowStore;

        // 2. Nos suscribimos al banco para actualizar los botones si ganamos/gastamos dinero
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
        // --- ASIGNACIÓN DE BOTONES POR CÓDIGO ---
        // Primero limpiamos por si acaso, y luego asignamos la función
        depthButton.onClick.RemoveAllListeners();
        depthButton.onClick.AddListener(BuyDepthUpgrade);

        capacityButton.onClick.RemoveAllListeners();
        capacityButton.onClick.AddListener(BuyCapacityUpgrade);

        // Actualizamos la interfaz por primera vez al arrancar
        // Le pasamos el dinero actual leyendo directamente del SavesManager
        if (SavesManager.Instance != null)
        {
            UpdateStoreUI(SavesManager.Instance.currentData.coins);
        }
    }

    // --- LÓGICA DE OCULTAR/MOSTRAR ---
    private void HideStore()
    {
        if (storePanel != null) storePanel.SetActive(false);
    }

    private void ShowStore()
    {
        if (storePanel != null) storePanel.SetActive(true);
    }

    // --- LÓGICA DE PRECIOS DINÁMICOS ---
    // Estas funciones calculan el precio basándose en el nivel actual. 
    // Puedes cambiar las matemáticas aquí a lo que mejor se ajuste a tu economía.
    private int GetDepthCost()
    {
        float currentDepth = Mathf.Abs(SavesManager.Instance.currentData.maxDepth);
        return Mathf.RoundToInt(currentDepth * 10f); // Ej: 15m * 10 = 150 monedas
    }

    private int GetCapacityCost()
    {
        int currentCapacity = SavesManager.Instance.currentData.maxCatch;
        return currentCapacity * 50; // Ej: 3 peces * 50 = 150 monedas
    }

    // --- LÓGICA DE COMPRA ---
    private void BuyDepthUpgrade()
    {
        int cost = GetDepthCost();

        if (SavesManager.Instance.currentData.coins >= cost)
        {
            // 1. Cobramos el dinero (usamos negativo porque nuestro método suma)
            SavesManager.Instance.AddCoins(-cost);

            // 2. Aplicamos la mejora (restamos porque la profundidad es negativa)
            SavesManager.Instance.currentData.maxDepth -= depthUpgradeAmount;

            // 3. Guardamos la partida
            SavesManager.Instance.SaveGame();

            // 4. Avisamos al anzuelo de que sus stats han cambiado
            if (hookController != null) hookController.RefreshStatsFromSave();

            // Nota: No hace falta llamar a UpdateStoreUI() aquí porque al hacer AddCoins(), 
            // el SavesManager ya lanza el evento OnCoinsChanged, el cual llama a UpdateStoreUI automáticamente.
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

    // --- ACTUALIZACIÓN VISUAL ---
    /// <summary>
    /// Updates the text and interactability of the buttons based on the player's wallet.
    /// Triggered automatically by the SavesManager.OnCoinsChanged event.
    /// </summary>
    private void UpdateStoreUI(int currentCoins)
    {
        if (SavesManager.Instance == null) return;

        int depthCost = GetDepthCost();
        int capacityCost = GetCapacityCost();

        // Actualizamos los textos mostrando el precio
        if (depthCostText != null) depthCostText.text = $"Mejorar Profundidad\n{depthCost} <sprite=0>";
        if (capacityCostText != null) capacityCostText.text = $"Mejorar Capacidad\n{capacityCost} <sprite=0>";

        // Si no tenemos dinero suficiente, el botón se bloquea visual y mecánicamente (se pone gris)
        if (depthButton != null) depthButton.interactable = currentCoins >= depthCost;
        if (capacityButton != null) capacityButton.interactable = currentCoins >= capacityCost;
    }
}