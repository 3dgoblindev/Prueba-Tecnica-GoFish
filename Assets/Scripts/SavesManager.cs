using System;
using System.IO;
using UnityEngine;

public class SavesManager : MonoBehaviour
{
    public static SavesManager Instance { get; private set; }

    [Header("Current State")]
    public SavedData currentData;

    private string saveFilePath;

    // --- NUEVO EVENTO ---
    // Pasamos el nuevo total de monedas como parámetro para que la UI no tenga ni que buscarlo
    public static event Action<int> OnCoinsChanged;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(this.gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(this.gameObject);

        saveFilePath = Path.Combine(Application.persistentDataPath, "fishing_save.json");
        LoadGame();
    }

    /// <summary>
    /// Adds or subtracts coins, updates the UI, and saves the game automatically.
    /// Use negative numbers to spend coins.
    /// </summary>
    public void AddCoins(int amount)
    {
        currentData.coins += amount;

        // Evitamos que el dinero baje de 0 por seguridad
        if (currentData.coins < 0) currentData.coins = 0;

        // Avisamos a toda la UI (CoinsLabel) de que el dinero ha cambiado
        OnCoinsChanged?.Invoke(currentData.coins);

        // Guardamos el progreso cada vez que la cartera cambia
        SaveGame();
    }

    public void SaveGame()
    {
        string json = JsonUtility.ToJson(currentData, true);
        File.WriteAllText(saveFilePath, json);
        Debug.Log($"Game Saved successfully at: {saveFilePath}");
    }

    public void LoadGame()
    {
        if (File.Exists(saveFilePath))
        {
            string json = File.ReadAllText(saveFilePath);
            currentData = JsonUtility.FromJson<SavedData>(json);
            Debug.Log("Game Loaded successfully.");
        }
        else
        {
            currentData = new SavedData();
        }
    }

    private void OnApplicationQuit()
    {
        SaveGame();
    }
}