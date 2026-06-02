using System;
using System.IO;
using UnityEngine;

public class SavesManager : MonoBehaviour
{
    public static SavesManager Instance { get; private set; }

    [Header("Current State")]
    public SavedData currentData;

    private string saveFilePath;

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

    private void Update()
    {
#if UNITY_EDITOR
        // DEBUG: Dar 1000 monedas
        if (Input.GetKeyDown(KeyCode.C))
        {
            Debug.Log("[DEBUG] +1000 monedas");
            AddCoins(1000);
        }

        // DEBUG: Borrar partida
        if (Input.GetKeyDown(KeyCode.X))
        {
            Debug.Log("[DEBUG] Partida borrada");

            if (File.Exists(saveFilePath))
            {
                File.Delete(saveFilePath);
            }

            currentData = new SavedData();

            OnCoinsChanged?.Invoke(currentData.coins);
            SaveGame();
        }
#endif
    }

    public void AddCoins(int amount)
    {
        currentData.coins += amount;

        if (currentData.coins < 0)
            currentData.coins = 0;

        OnCoinsChanged?.Invoke(currentData.coins);

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