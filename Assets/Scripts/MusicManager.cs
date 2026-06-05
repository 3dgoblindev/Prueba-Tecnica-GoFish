using UnityEngine;

/// <summary>
/// Simple scene-level initializer that requests the global AudioManager to play a specified background music track on start.
/// </summary>
public class MusicManager : MonoBehaviour
{
    [Header("Audio Configuration")]
    [Tooltip("The background music clip to be played automatically when this scene initializes.")]
    [SerializeField] private AudioClip backgroundMusicClip;

    private void Start()
    {
        InitializeBackgroundMusic();
    }

    /// <summary>
    /// Safely registers the assigned background music track with the persistent AudioManager instance.
    /// </summary>
    private void InitializeBackgroundMusic()
    {
        if (backgroundMusicClip == null) return;

        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlayMusic(backgroundMusicClip);
        }
        else
        {
            Debug.LogWarning($"[{nameof(MusicManager)}] Failed to play background music because AudioManager.Instance is missing from the scene layer.", this);
        }
    }
}