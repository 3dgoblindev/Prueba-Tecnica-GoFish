using UnityEngine;

public class MusicManager : MonoBehaviour
{
    [SerializeField] private AudioClip backgroundMusicClip;

    private void Start()
    {
        if (backgroundMusicClip == null) return;

        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlayMusic(backgroundMusicClip);
        }
        else
        {
            Debug.LogWarning($"[{nameof(MusicManager)}] AudioManager.Instance not found in scene.", this);
        }
    }
}