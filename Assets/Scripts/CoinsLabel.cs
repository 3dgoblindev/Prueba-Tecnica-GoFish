using UnityEngine;
using TMPro;

[RequireComponent(typeof(TextMeshProUGUI))]
public class CoinsLabel : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI coinText;

    private void Awake()
    {
        if (coinText == null) coinText = GetComponent<TextMeshProUGUI>();
    }

    private void Start()
    {
        // Set initial coin count from save data
        if (SavesManager.Instance?.currentData != null)
        {
            UpdateText(SavesManager.Instance.currentData.coins);
        }
    }

    private void OnEnable() => SavesManager.OnCoinsChanged += UpdateText;
    private void OnDisable() => SavesManager.OnCoinsChanged -= UpdateText;

    private void UpdateText(int currentCoins)
    {
        // Standard numeric formatting (e.g., 1000 -> 1,000)
        if (coinText != null) coinText.text = currentCoins.ToString("N0");
    }
}