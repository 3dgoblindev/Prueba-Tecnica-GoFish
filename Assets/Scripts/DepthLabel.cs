using UnityEngine;
using TMPro; // Requerido para TextMeshPro

/// <summary>
/// Listens to the HookController's depth changes and updates the UI text.
/// Formats the float into a clean integer with an 'm' suffix (e.g., "150m").
/// </summary>
[RequireComponent(typeof(TextMeshProUGUI))]
public class DepthLabel : MonoBehaviour
{
    private TextMeshProUGUI depthText;

    private void Awake()
    {
        depthText = GetComponent<TextMeshProUGUI>();
    }

    private void OnEnable()
    {
        HookController.OnDepthChanged += UpdateText;
    }

    private void OnDisable()
    {
        HookController.OnDepthChanged -= UpdateText;
    }

    /// <summary>
    /// Updates the text component. Rounds the depth to avoid flickering decimals.
    /// </summary>
    private void UpdateText(float currentDepth)
    {
        // Usamos Mathf.RoundToInt para un visual más limpio.
        int roundedDepth = Mathf.RoundToInt(currentDepth);
        depthText.text = $"{roundedDepth}m";
    }
}