using UnityEngine;
using System;
using System.Collections.Generic;

/// <summary>
/// Manages the core physics-driven state machine and spatial movement vectors of the fishing hook.
/// Handles spatial boundaries, dynamic multi-catch fish registration routines, and currency rewards serialization.
/// </summary>
[RequireComponent(typeof(Rigidbody2D))]
public class HookController : MonoBehaviour
{
    public enum HookState
    {
        Idle,
        Descending,
        Ascending
    }

    public static event Action OnReturnToSurface;
    public static event Action<float> OnDepthChanged;
    public static event Action<List<FishController>> OnCatchReady;

    /// <summary>
    /// Broadcasts capacity variation data hooks. Parameter 1: Current payload volume. Parameter 2: Maximum asset limit.
    /// </summary>
    public static event Action<int, int> OnCatchCountChanged;

    [Header("Vertical Movement Physics")]
    [Tooltip("Downward vertical step speed applied when passing through the Descending state.")]
    [SerializeField] private float descentSpeed = 3f;

    [Tooltip("Standard upward vertical speed profile applied during active collection runs.")]
    [SerializeField] private float ascentSpeed = 5f;

    [Tooltip("High-velocity upward ascent multiplier applied once inventory volume tolerances are fully saturated.")]
    [SerializeField] private float rapidAscentSpeed = 15f;

    [Tooltip("The safety depth floor limit where the physics trajectory changes to ascending tracking vectors.")]
    [SerializeField] private float maxDepth = -15f;

    [Header("Horizontal Workspace Input")]
    [Tooltip("Linear tracking multiplier calculation matching active screen cursor world positions.")]
    [SerializeField] private float horizontalResponsiveness = 10f;

    [Header("Inventory Capacity Thresholds")]
    [Tooltip("The absolute limit cap tracking concurrent loaded item entities inside the collection collection.")]
    [SerializeField] private int maxFishCapacity = 3;

    [Header("Spatial Rotation (Juice)")]
    [Tooltip("Maximum allowed structural pivot angular bounds applied during lateral steering operations.")]
    [SerializeField] private float maxTiltAngle = 25f;

    [Tooltip("Interpolation speed tracking angular alignment vectors. Higher metrics increase immediate response snaps.")]
    [SerializeField] private float tiltSpeed = 8f;

    // Rigid Architecture Components
    private HookState currentState = HookState.Idle;
    private float startYPosition;
    private Camera mainCamera;
    private Rigidbody2D rb2d;
    private List<FishController> caughtFishes = new List<FishController>();

    private void Awake()
    {
        mainCamera = Camera.main;
        rb2d = GetComponent<Rigidbody2D>();
        startYPosition = transform.position.y;
    }

    private void Start()
    {
        LoadStatsFromSave();

        OnDepthChanged?.Invoke(0f);
        OnCatchCountChanged?.Invoke(0, maxFishCapacity);
    }

    private void OnEnable()
    {
        PlayerController.OnCastCompleted += StartDescending;
    }

    private void OnDisable()
    {
        PlayerController.OnCastCompleted -= StartDescending;
    }

    private void FixedUpdate()
    {
        ProcessStateMachine();
        UpdateDepthUI();
        UpdateHookRotation();
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (currentState != HookState.Ascending || !collision.CompareTag("Fish"))
        {
            return;
        }

        if (caughtFishes.Count >= maxFishCapacity)
        {
            return;
        }

        FishController fish = collision.GetComponent<FishController>();
        if (fish != null && !caughtFishes.Contains(fish))
        {
            fish.GetCaught(transform);
            caughtFishes.Add(fish);

            OnCatchCountChanged?.Invoke(caughtFishes.Count, maxFishCapacity);
        }
    }

    #region State Machine Operations

    /// <summary>
    /// Processes physical velocity profiles based on the active state register.
    /// </summary>
    private void ProcessStateMachine()
    {
        switch (currentState)
        {
            case HookState.Idle:
                rb2d.velocity = Vector2.zero;
                break;

            case HookState.Descending:
                HandleMovement(-descentSpeed);
                CheckDepthLimit();
                break;

            case HookState.Ascending:
                bool isCapacityFull = caughtFishes.Count >= maxFishCapacity;
                float currentAscentSpeed = isCapacityFull ? rapidAscentSpeed : ascentSpeed;

                HandleMovement(currentAscentSpeed);
                CheckSurfaceLimit();
                break;
        }
    }

    private void StartDescending()
    {
        currentState = HookState.Descending;
    }

    /// <summary>
    /// Processes input offsets to construct horizontal acceleration vectors and applies final vertical forces.
    /// </summary>
    private void HandleMovement(float verticalSpeed)
    {
        float targetVelocityX = 0f;

        if (Input.GetMouseButton(0))
        {
            Vector3 mousePos = Input.mousePosition;
            mousePos.z = 10f;

            Vector3 targetWorldPos = mainCamera.ScreenToWorldPoint(mousePos);
            float differenceX = targetWorldPos.x - rb2d.position.x;

            targetVelocityX = differenceX * horizontalResponsiveness;
        }

        rb2d.velocity = new Vector2(targetVelocityX, verticalSpeed);
    }

    #endregion

    #region Boundary Checking & Finalization

    private void CheckDepthLimit()
    {
        if (rb2d.position.y <= maxDepth)
        {
            currentState = HookState.Ascending;
        }
    }

    private void CheckSurfaceLimit()
    {
        if (rb2d.position.y < startYPosition) return;

        rb2d.position = new Vector2(0f, startYPosition);
        currentState = HookState.Idle;
        rb2d.velocity = Vector2.zero;
        OnDepthChanged?.Invoke(0f);

        if (caughtFishes.Count > 0)
        {
            OnCatchReady?.Invoke(new List<FishController>(caughtFishes));
        }
        else
        {
            FinalizeCast();
        }
    }

    /// <summary>
    /// Clears data registers, resets interface systems, and dispatches surface feedback loops.
    /// </summary>
    public void FinalizeCast()
    {
        ProcessCatchRewards();
        OnCatchCountChanged?.Invoke(0, maxFishCapacity);
        OnReturnToSurface?.Invoke();
    }

    private void ProcessCatchRewards()
    {
        if (caughtFishes.Count <= 0) return;

        int totalCoinsThisCast = 0;

        foreach (FishController fish in caughtFishes)
        {
            if (fish != null && fish.data != null)
            {
                totalCoinsThisCast += fish.data.price;
            }
        }

        if (SavesManager.Instance != null)
        {
            SavesManager.Instance.AddCoins(totalCoinsThisCast);
        }

        caughtFishes.Clear();
    }

    #endregion

    #region UI & Data Calculations

    private void UpdateDepthUI()
    {
        if (currentState != HookState.Idle)
        {
            float currentDepth = Mathf.Abs(rb2d.position.y - startYPosition);
            OnDepthChanged?.Invoke(currentDepth);
        }
    }

    private void LoadStatsFromSave()
    {
        if (SavesManager.Instance?.currentData != null)
        {
            maxDepth = SavesManager.Instance.currentData.maxDepth;
            maxFishCapacity = SavesManager.Instance.currentData.maxCatch;
        }
    }

    /// <summary>
    /// External trigger interface used to sync core limits post shop purchases.
    /// </summary>
    public void RefreshStatsFromSave()
    {
        LoadStatsFromSave();
        OnCatchCountChanged?.Invoke(caughtFishes.Count, maxFishCapacity);
    }

    /// <summary>
    /// Procedurally shifts orientation based on horizontal drag pressures. Adds a structural upward pitch bias when ascending.
    /// </summary>
    private void UpdateHookRotation()
    {
        if (currentState == HookState.Idle)
        {
            float neutralAngle = Mathf.LerpAngle(rb2d.rotation, 0f, Time.fixedDeltaTime * tiltSpeed);
            rb2d.MoveRotation(neutralAngle);
            return;
        }

        float normalizedX = Mathf.Clamp(rb2d.velocity.x / descentSpeed, -1f, 1f);
        float targetAngle = -normalizedX * maxTiltAngle;

        if (currentState == HookState.Ascending)
        {
            float verticalBias = 8f;
            float directionalSign = Mathf.Sign(targetAngle == 0f ? 0f : targetAngle);
            targetAngle += verticalBias * directionalSign;
        }

        float smoothAngle = Mathf.LerpAngle(rb2d.rotation, targetAngle, Time.fixedDeltaTime * tiltSpeed);
        rb2d.MoveRotation(smoothAngle);
    }

    #endregion
}