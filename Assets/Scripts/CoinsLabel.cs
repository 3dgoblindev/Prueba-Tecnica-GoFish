using UnityEngine;
using TMPro; 

/// <summary>
/// Listens for economy changes and updates the text display automatically.
/// </summary>
[RequireComponent(typeof(TextMeshProUGUI))]
public class CoinsLabel : MonoBehaviour
{
    private TextMeshProUGUI coinText;

    private void Awake()
    {
        coinText = GetComponent<TextMeshProUGUI>();
    }

    private void Start()
    {
        // Al arrancar la escena, le pedimos al SavesManager cuánto dinero tenemos 
        // para poner el contador en su número correcto (por si cargamos una partida guardada).
        if (SavesManager.Instance != null)
        {
            UpdateText(SavesManager.Instance.currentData.coins);
        }
    }

    private void OnEnable()
    {
        // Nos suscribimos al evento del banco
        SavesManager.OnCoinsChanged += UpdateText;
    }

    private void OnDisable()
    {
        // Nos desuscribimos para evitar memory leaks si destruimos el Canvas
        SavesManager.OnCoinsChanged -= UpdateText;
    }

    /// <summary>
    /// Called automatically whenever coins are added or spent.
    /// </summary>
    private void UpdateText(int currentCoins)
    {
        // Formateamos el número (ej: 1500 -> "1,500")
        coinText.text = currentCoins.ToString("N0");
    }
}