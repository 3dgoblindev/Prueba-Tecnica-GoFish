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

    private void OnEnable()
    {
        HookController.OnDepthChanged += UpdateDepth;
        SavesManager.OnDataChanged += UpdateMax;  // nuevo
    }

    private void OnDisable()
    {
        HookController.OnDepthChanged -= UpdateDepth;
        SavesManager.OnDataChanged -= UpdateMax;  // nuevo
    }

    private void Start() => UpdateDepth(0f);

    private void UpdateDepth(float currentDepth)
    {
        int maxDepth = SavesManager.Instance?.currentData != null
            ? Mathf.RoundToInt(Mathf.Abs(SavesManager.Instance.currentData.maxDepth))
            : 0;

        if (depthText != null)
            depthText.text = $"{Mathf.RoundToInt(currentDepth)}/{maxDepth}m";
    }

    private void UpdateMax(SavedData data)
    {
        UpdateDepth(0f);
    }
}