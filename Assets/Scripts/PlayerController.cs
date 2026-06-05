using System;
using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;

public class PlayerController : MonoBehaviour
{
    public static event Action OnCastCompleted;

    [Header("Dependencies")]
    [SerializeField] private Animator animator;
    [SerializeField] private MiniTweenFeel castFeel;
    [SerializeField] private Camera cam;

    [Header("Camera Zoom")]
    [SerializeField] private float zoomAmount = 0.8f;
    [SerializeField] private float zoomSpeed = 10f;

    [Header("Juice")]
    [SerializeField] private float freezeDuration = 0.10f;

    [Header("Audio")]
    [SerializeField] private AudioClip startCastSound;
    [SerializeField] private AudioClip castSound;
    [SerializeField] private AudioClip castHighlightSound;
    [SerializeField] private AudioClip recoilSound;

    private bool isFishing = false;
    private bool isCharging = false;
    private float baseOrthoSize;

    private static readonly int ThrowHash = Animator.StringToHash("Throw");
    private static readonly int RecoilHash = Animator.StringToHash("Recoil");

    private void OnEnable() => HookController.OnReturnToSurface += HandleReturnToSurface;
    private void OnDisable() => HookController.OnReturnToSurface -= HandleReturnToSurface;

    private void Start()
    {
        if (animator == null) animator = GetComponent<Animator>();
        if (cam == null) cam = Camera.main;

        if (cam != null) baseOrthoSize = cam.orthographicSize;
        else Debug.LogError("[PlayerController] Main camera reference missing!", this);
    }

    private void Update()
    {
        if (IsPointerOverUI()) return;

        HandleInputLoop();

        // Smoothly zoom back out if charging gets canceled or interrupted
        if (!isCharging && cam != null)
        {
            cam.orthographicSize = Mathf.Lerp(cam.orthographicSize, baseOrthoSize, Time.deltaTime * zoomSpeed);
        }
    }

    private void HandleInputLoop()
    {
        if (Input.GetMouseButtonDown(0) && !isFishing) StartCharge();

        if (isCharging)
        {
            TickZoomIn();
            if (Input.GetMouseButtonUp(0)) CommitCast();
        }
    }

    private void StartCharge()
    {
        isCharging = true;
        PlaySFX(startCastSound);
    }

    private void TickZoomIn()
    {
        if (cam == null) return;
        float targetGoal = baseOrthoSize - zoomAmount;
        cam.orthographicSize = Mathf.Lerp(cam.orthographicSize, targetGoal, Time.deltaTime * zoomSpeed);
    }

    private void CommitCast()
    {
        isCharging = false;
        isFishing = true;

        PlaySFX(castSound);
        StartCoroutine(ExecuteZoomOutSnap());

        if (animator != null)
        {
            animator.ResetTrigger(RecoilHash);
            animator.SetTrigger(ThrowHash);
        }
    }

    private IEnumerator ExecuteZoomOutSnap()
    {
        if (cam == null) yield break;

        // Snappy snap-back zoom on release
        while (Mathf.Abs(cam.orthographicSize - baseOrthoSize) > 0.01f)
        {
            cam.orthographicSize = Mathf.Lerp(cam.orthographicSize, baseOrthoSize, Time.deltaTime * (zoomSpeed * 2f));
            yield return null;
        }
        cam.orthographicSize = baseOrthoSize;
    }

    // Animation Event: Triggered via clip at line extension peak
    public void OnCastFinished()
    {
        if (castFeel != null) castFeel.Play();
        StartCoroutine(ExecuteFreezeFrameTransition());
    }

    private IEnumerator ExecuteFreezeFrameTransition()
    {
        Time.timeScale = 0f;
        yield return new WaitForSecondsRealtime(freezeDuration);
        Time.timeScale = 1f;

        OnCastCompleted?.Invoke();
    }

    // Animation Event: Triggered via clip at end of reel/recoil loop
    public void OnFishingSequenceEnded()
    {
        PlaySFX(castHighlightSound);
        if (castFeel != null) castFeel.Play();
        isFishing = false;
    }

    private void HandleReturnToSurface()
    {
        if (animator == null) return;
        animator.ResetTrigger(ThrowHash);
        animator.SetTrigger(RecoilHash);
        PlaySFX(recoilSound);
    }

    private void PlaySFX(AudioClip clip)
    {
        if (clip != null && AudioManager.Instance != null)
        {
            AudioManager.Instance.PlaySFX(clip, volume: 1f, pitchMin: 0.85f, pitchMax: 1.15f);
        }
    }

    private bool IsPointerOverUI()
    {
        if (EventSystem.current == null) return false;

        if (Input.touchCount > 0)
        {
            Touch touch = Input.GetTouch(0);
            if (touch.phase == TouchPhase.Began)
            {
                return EventSystem.current.IsPointerOverGameObject(touch.fingerId);
            }
            return false;
        }
        return EventSystem.current.IsPointerOverGameObject();
    }
}