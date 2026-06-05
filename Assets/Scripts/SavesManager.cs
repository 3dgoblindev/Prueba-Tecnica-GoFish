using System;
using System.IO;
using UnityEngine;

public class SavesManager : MonoBehaviour
{
    public static SavesManager Instance { get; private set; }
    public static event Action<int> OnCoinsChanged;

    public SavedData currentData;
    private string saveFilePath;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        saveFilePath = Path.Combine(Application.persistentDataPath, "fishing_save.json");
        LoadGame();
    }

    private void Update()
    {
#if UNITY_EDITOR
        HandleDebugInput();
#endif
    }

    private void OnApplicationQuit()
    {
        SaveGame();
    }

#if UNITY_EDITOR
    private void HandleDebugInput()
    {
        // Give 1000 coins
        if (Input.GetKeyDown(KeyCode.C))
        {
            Debug.Log("[SavesManager] Debug: Added 1000 coins.");
            AddCoins(1000);
        }

        // Wipe save file
        if (Input.GetKeyDown(KeyCode.X))
        {
            Debug.LogWarning("[SavesManager] Debug: Wiping save file.");

            if (File.Exists(saveFilePath))
            {
                File.Delete(saveFilePath);
            }

            currentData = new SavedData();
            OnCoinsChanged?.Invoke(currentData.coins);
            SaveGame();
        }
    }
#endif

    public void AddCoins(int amount)
    {
        if (currentData == null) return;

        currentData.coins += amount;
        if (currentData.coins < 0) currentData.coins = 0;

        OnCoinsChanged?.Invoke(currentData.coins);
        SaveGame();
    }

    public void SaveGame()
    {
        if (currentData == null) return;

        try
        {
            string json = JsonUtility.ToJson(currentData, true);
            File.WriteAllText(saveFilePath, json);
        }
        catch (Exception e)
        {
            Debug.LogError($"[SavesManager] Failed to save game: {e.Message}");
        }
    }

    public void LoadGame()
    {
        try
        {
            if (File.Exists(saveFilePath))
            {
                string json = File.ReadAllText(saveFilePath);
                currentData = JsonUtility.FromJson<SavedData>(json);

                if (currentData == null) currentData = new SavedData();
            }
            else
            {
                currentData = new SavedData();
            }
        }
        catch (Exception e)
        {
            Debug.LogError($"[SavesManager] Error loading save file, creating new one: {e.Message}");
            currentData = new SavedData();
        }
    }
}