using UnityEngine;
using TMPro;

[RequireComponent(typeof(TextMeshProUGUI))]
public class CatchLabel : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI catchText;

    private void Awake()
    {
        if (catchText == null) catchText = GetComponent<TextMeshProUGUI>();
    }

    private void OnEnable() => HookController.OnCatchCountChanged += UpdateText;
    private void OnDisable() => HookController.OnCatchCountChanged -= UpdateText;

    private void UpdateText(int currentCatch, int maxCapacity)
    {
        if (catchText != null) catchText.text = $"{currentCatch}/{maxCapacity}";
    }
}