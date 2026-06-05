using System;
using System.IO;
using UnityEngine;

/// <summary>
/// Handles persistent local game save states using JSON serialization. 
/// Coordinates global runtime synchronization for economy data and progression parameters.
/// </summary>
public class SavesManager : MonoBehaviour
{
    public static SavesManager Instance { get; private set; }

    public static event Action<int> OnCoinsChanged;

    [Header("Persistent State Data")]
    [Tooltip("The reactive runtime snapshot model containing active user progression stats.")]
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

        InitializeSavePath();
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

    /// <summary>
    /// Configures the direct local platform persistent storage path coordinates.
    /// </summary>
    private void InitializeSavePath()
    {
        saveFilePath = Path.Combine(Application.persistentDataPath, "fishing_save.json");
    }

#if UNITY_EDITOR
    /// <summary>
    /// Sandbox input mappings designed strictly for fast validation checking in the editor workspace.
    /// </summary>
    private void HandleDebugInput()
    {
        // Key [C]: Inject direct soft currency volumes
        if (Input.GetKeyDown(KeyCode.C))
        {
            Debug.Log($"[{nameof(SavesManager)}] Debug Mode: Granting +1000 Coins.");
            AddCoins(1000);
        }

        // Key [X]: Hard wipe operational local progress files
        if (Input.GetKeyDown(KeyCode.X))
        {
            Debug.LogWarning($"[{nameof(SavesManager)}] Debug Mode: Wiping active save profile tracking file data.");

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

    /// <summary>
    /// Safely increments or decrements active balance layers and processes transactional changes down to the disk storage layout.
    /// </summary>
    /// <param name="amount">The variation step size. Negative parameters represent item cost values.</param>
    public void AddCoins(int amount)
    {
        if (currentData == null) return;

        currentData.coins += amount;

        // Balance ceiling safety floor clamping bounds check
        if (currentData.coins < 0)
        {
            currentData.coins = 0;
        }

        OnCoinsChanged?.Invoke(currentData.coins);
        SaveGame();
    }

    /// <summary>
    /// Serializes current structural values to a clean JSON string context and writes it down to the persistent storage layer.
    /// </summary>
    public void SaveGame()
    {
        if (currentData == null) return;

        try
        {
            string json = JsonUtility.ToJson(currentData, true);
            File.WriteAllText(saveFilePath, json);
        }
        catch (Exception exception)
        {
            Debug.LogError($"[{nameof(SavesManager)}] Exception caught trying to serialize state payload data down to storage target file layout: {exception.Message}");
        }
    }

    /// <summary>
    /// Discovers existing files and maps internal keys back to objects, or generates clean baseline templates.
    /// </summary>
    public void LoadGame()
    {
        try
        {
            if (File.Exists(saveFilePath))
            {
                string json = File.ReadAllText(saveFilePath);
                currentData = JsonUtility.FromJson<SavedData>(json);

                // Fallback catch mechanism in case serialization reads empty corrupted documents
                if (currentData == null)
                {
                    currentData = new SavedData();
                }
            }
            else
            {
                currentData = new SavedData();
            }
        }
        catch (Exception exception)
        {
            Debug.LogError($"[{nameof(SavesManager)}] Failed reading fallback structures due to a low-level IO disk initialization error context: {exception.Message}. Reverting back to baseline templates.");
            currentData = new SavedData();
        }
    }
}