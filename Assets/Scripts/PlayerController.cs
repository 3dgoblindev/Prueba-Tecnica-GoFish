using System;
using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;

/// <summary>
/// Controls player input loops for charging, casting, and reeling states.
/// Handles contextual gameplay camera zooming, game feel freeze frames, and game mechanics event triggers.
/// </summary>
public class PlayerController : MonoBehaviour
{
    public static event Action OnCastCompleted;

    [Header("Dependencies")]
    [SerializeField] private Animator animator;

    [Header("Juice & Feel Settings")]
    [SerializeField] private MiniTweenFeel castFeel;

    [Header("Camera Zoom Configurations")]
    [Tooltip("The main 2D orthographic world camera reference.")]
    [SerializeField] private Camera cam;

    [Tooltip("Target calculation factor subtracted from base orthographic size when charging.")]
    [SerializeField] private float zoomAmount = 0.8f;

    [Tooltip("Interpolation speed factor matching orthographic adjustments.")]
    [SerializeField] private float zoomSpeed = 10f;

    [Header("Freeze Frame Metrics")]
    [Tooltip("Total real-time pause duration processed at the peak point of the cast trajectory animation.")]
    [SerializeField] private float freezeDuration = 0.10f;

    [Header("Audio Configurations")]
    [SerializeField] private AudioClip startCastSound;
    [SerializeField] private AudioClip castSound;
    [SerializeField] private AudioClip castHighlightSound;
    [SerializeField] private AudioClip recoilSound;

    // Internal State Machine Flags
    private bool isFishing = false;
    private bool isCharging = false;
    private float baseOrthoSize;

    // Pre-calculated Animator Hash References
    private static readonly int ThrowHash = Animator.StringToHash("Throw");
    private static readonly int RecoilHash = Animator.StringToHash("Recoil");

    private void OnEnable()
    {
        HookController.OnReturnToSurface += HandleReturnToSurface;
    }

    private void OnDisable()
    {
        HookController.OnReturnToSurface -= HandleReturnToSurface;
    }

    private void Start()
    {
        InitializeComponents();
    }

    private void Update()
    {
        if (IsPointerOverUI()) return;

        HandleInputLoop();
        HandleDynamicInterruptedZoom();
    }

    /// <summary>
    /// Establishes fallback components when references aren't explicitly declared in the Inspector workspace.
    /// </summary>
    private void InitializeComponents()
    {
        if (animator == null)
        {
            animator = GetComponent<Animator>();
        }

        if (cam == null)
        {
            cam = Camera.main;
        }

        if (cam != null)
        {
            baseOrthoSize = cam.orthographicSize;
        }
        else
        {
            Debug.LogError($"[{nameof(PlayerController)}] Main target camera reference missing. Assign a functional component inside the inspector panel.", this);
        }
    }

    /// <summary>
    /// Processes direct user input calls mapping mouse or touch execution pipelines.
    /// </summary>
    private void HandleInputLoop()
    {
        if (Input.GetMouseButtonDown(0) && !isFishing)
        {
            StartCharge();
        }

        if (isCharging)
        {
            TickZoomIn();

            if (Input.GetMouseButtonUp(0))
            {
                CommitCast();
            }
        }
    }

    /// <summary>
    /// Interpolates camera view metrics smooth back to initial parameters if active charging cycles fail to complete cleanly.
    /// </summary>
    private void HandleDynamicInterruptedZoom()
    {
        if (!isCharging && cam != null)
        {
            cam.orthographicSize = Mathf.Lerp(cam.orthographicSize, baseOrthoSize, Time.deltaTime * zoomSpeed);
        }
    }

    private void StartCharge()
    {
        isCharging = true;

        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlaySFX(startCastSound, volume: 1f, pitchMin: 0.85f, pitchMax: 1.15f);
        }
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

        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlaySFX(castSound, volume: 1f, pitchMin: 0.85f, pitchMax: 1.15f);
        }

        StartCoroutine(ExecuteZoomOutSnap());

        if (animator != null)
        {
            animator.ResetTrigger(RecoilHash);
            animator.SetTrigger(ThrowHash);
        }
    }

    /// <summary>
    /// Smoothly steps active fields back to baseline measurements.
    /// </summary>
    private IEnumerator ExecuteZoomOutSnap()
    {
        if (cam == null) yield break;

        while (Mathf.Abs(cam.orthographicSize - baseOrthoSize) > 0.01f)
        {
            cam.orthographicSize = Mathf.Lerp(cam.orthographicSize, baseOrthoSize, Time.deltaTime * (zoomSpeed * 2f));
            yield return null;
        }

        cam.orthographicSize = baseOrthoSize;
    }

    #region Animation Events

    /// <summary>
    /// Animation Event Hook: Triggered at the peak frame sequence when the line fully extends out.
    /// </summary>
    public void OnCastFinished()
    {
        if (castFeel != null)
        {
            castFeel.Play();
        }

        StartCoroutine(ExecuteFreezeFrameTransition());
    }

    /// <summary>
    /// Halts runtime scales instantly to project raw kinetic hit weight feel before notifying downstream actors.
    /// </summary>
    private IEnumerator ExecuteFreezeFrameTransition()
    {
        Time.timeScale = 0f;
        yield return new WaitForSecondsRealtime(freezeDuration);
        Time.timeScale = 1f;

        OnCastCompleted?.Invoke();
    }

    /// <summary>
    /// Animation Event Hook: Triggered at the extreme edge boundary frames finishing recoil loop segments.
    /// </summary>
    public void OnFishingSequenceEnded()
    {
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlaySFX(castHighlightSound, volume: 1f, pitchMin: 0.85f, pitchMax: 1.15f);
        }

        if (castFeel != null)
        {
            castFeel.Play();
        }

        isFishing = false;
    }

    #endregion

    /// <summary>
    /// External listener event execution point mapping back to Hook lifecycle triggers.
    /// </summary>
    private void HandleReturnToSurface()
    {
        if (animator == null) return;

        animator.ResetTrigger(ThrowHash);
        animator.SetTrigger(RecoilHash);

        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlaySFX(recoilSound, volume: 1f, pitchMin: 0.85f, pitchMax: 1.15f);
        }
    }

    /// <summary>
    /// Safely scans touch indexes or hardware positions to filter screen inputs away from functional canvas configurations.
    /// </summary>
    private bool IsPointerOverUI()
    {
        if (EventSystem.current == null) return false;

        if (Input.touchCount > 0)
        {
            Touch activeTouch = Input.GetTouch(0);
            if (activeTouch.phase == TouchPhase.Began)
            {
                return EventSystem.current.IsPointerOverGameObject(activeTouch.fingerId);
            }
            return false;
        }

        return EventSystem.current.IsPointerOverGameObject();
    }
}