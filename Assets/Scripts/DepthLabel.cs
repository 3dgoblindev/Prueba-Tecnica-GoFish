using UnityEngine;
using TMPro;

[RequireComponent(typeof(TextMeshProUGUI))]
public class DepthLabel : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI depthText;

    private void Awake()
    {
        if (depthText == null) depthText = GetComponent<TextMeshProUGUI>();
    }

    private void OnEnable() => HookController.OnDepthChanged += UpdateText;
    private void OnDisable() => HookController.OnDepthChanged -= UpdateText;

    private void UpdateText(float currentDepth)
    {
        int maxDepth = 0;

        if (SavesManager.Instance?.currentData != null)
        {
            // Convert negative maxDepth coordinate to positive value for the UI display
            maxDepth = Mathf.RoundToInt(Mathf.Abs(SavesManager.Instance.currentData.maxDepth));
        }

        if (depthText != null)
        {
            depthText.text = $"{Mathf.RoundToInt(currentDepth)}/{maxDepth}m";
        }
    }
}