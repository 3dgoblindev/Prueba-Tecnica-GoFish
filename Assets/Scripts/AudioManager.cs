using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance { get; private set; }

    [Header("Pool")]
    [SerializeField] private int poolSize = 5;

    private AudioSource[] sources;
    private int currentIndex = 1; // 0 reservado para música

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        sources = new AudioSource[poolSize];
        for (int i = 0; i < poolSize; i++)
        {
            sources[i] = gameObject.AddComponent<AudioSource>();
            sources[i].playOnAwake = false;
        }
    }

    // ── SFX: clip único ───────────────────────────────────────────────────────

    public void PlaySFX(AudioClip clip, float volume = 1f, float pitch = 1f)
    {
        if (clip == null) return;
        PlayOnSource(clip, volume, pitch);
    }

    public void PlaySFX(AudioClip clip, float volume = 1f, float pitchMin = 0.9f, float pitchMax = 1.1f)
    {
        if (clip == null) return;
        PlayOnSource(clip, volume, Random.Range(pitchMin, pitchMax));
    }

    // ── SFX: array de clips, elige uno al azar ────────────────────────────────

    public void PlaySFX(AudioClip[] clips, float volume = 1f, float pitch = 1f)
    {
        AudioClip clip = GetRandomClip(clips);
        if (clip == null) return;
        PlayOnSource(clip, volume, pitch);
    }

    public void PlaySFX(AudioClip[] clips, float volume = 1f, float pitchMin = 0.9f, float pitchMax = 1.1f)
    {
        AudioClip clip = GetRandomClip(clips);
        if (clip == null) return;
        PlayOnSource(clip, volume, Random.Range(pitchMin, pitchMax));
    }

    // ── Música ────────────────────────────────────────────────────────────────

    public void PlayMusic(AudioClip clip, float volume = 0.5f)
    {
        sources[0].clip = clip;
        sources[0].volume = volume;
        sources[0].loop = true;
        sources[0].Play();
    }

    public void StopMusic() => sources[0].Stop();
    public void SetMusicVolume(float v) => sources[0].volume = v;

    // ── Internos ──────────────────────────────────────────────────────────────

    private void PlayOnSource(AudioClip clip, float volume, float pitch)
    {
        AudioSource source = sources[currentIndex];
        currentIndex = (currentIndex + 1) % poolSize;
        if (currentIndex == 0) currentIndex = 1; // saltamos el 0 reservado

        source.clip = clip;
        source.volume = volume;
        source.pitch = pitch;
        source.loop = false;
        source.Play();
    }

    private AudioClip GetRandomClip(AudioClip[] clips)
    {
        if (clips == null || clips.Length == 0) return null;
        return clips[Random.Range(0, clips.Length)];
    }
}