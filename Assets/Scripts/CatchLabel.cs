using UnityEngine;
using TMPro;

/// <summary>
/// Listens to the HookController's catch events and updates the UI text.
/// Displays the format Current/Max (e.g., "2/3").
/// </summary>
[RequireComponent(typeof(TextMeshProUGUI))]
public class CatchLabel : MonoBehaviour
{
    private TextMeshProUGUI catchText;

    private void Awake()
    {
        catchText = GetComponent<TextMeshProUGUI>();
    }

    private void OnEnable()
    {
        HookController.OnCatchCountChanged += UpdateText;
    }

    private void OnDisable()
    {
        HookController.OnCatchCountChanged -= UpdateText;
    }

    /// <summary>
    /// Updates the text component with the current capacity ratio.
    /// </summary>
    private void UpdateText(int currentCatch, int maxCapacity)
    {
        catchText.text = $"{currentCatch}/{maxCapacity}";
    }
}