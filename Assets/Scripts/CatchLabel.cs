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

    private void OnEnable()
    {
        HookController.OnCatchCountChanged += UpdateText;
        SavesManager.OnDataChanged += UpdateFromData;  
    }

    private void OnDisable()
    {
        HookController.OnCatchCountChanged -= UpdateText;
        SavesManager.OnDataChanged -= UpdateFromData; 
    }

    private void Start()
    {
        if (SavesManager.Instance?.currentData != null)
            UpdateText(0, SavesManager.Instance.currentData.maxCatch);
    }

    private void UpdateFromData(SavedData data)
    {
        UpdateText(0, data.maxCatch);
    }

    private void UpdateText(int currentCatch, int maxCapacity)
    {
        if (catchText != null) catchText.text = $"{currentCatch}/{maxCapacity}";
    }
}