using UnityEngine;
using TMPro;

/// <summary>
/// Listens to the HookController's depth changes and updates the UI text.
/// Formats the float into a "current/max m" format (e.g., "15/150m").
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
    /// Updates the text component to show current depth vs maximum depth.
    /// </summary>
    private void UpdateText(float currentDepth)
    {
        int roundedDepth = Mathf.RoundToInt(currentDepth);
        int maxDepth = 0;

        // Recuperamos la profundidad máxima del archivo de guardado
        if (SavesManager.Instance != null && SavesManager.Instance.currentData != null)
        {
            // Usamos Mathf.Abs para asegurar que el número se vea positivo en la UI
            maxDepth = Mathf.RoundToInt(Mathf.Abs(SavesManager.Instance.currentData.maxDepth));
        }

        // Actualizamos el texto con el nuevo formato
        depthText.text = $"{roundedDepth}/{maxDepth}m";
    }
}