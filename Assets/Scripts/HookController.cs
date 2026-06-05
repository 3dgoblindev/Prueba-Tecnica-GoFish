using UnityEngine;
using System;
using System.Collections.Generic;

/// <summary>
/// Controls the movement and state machine of the fishing hook using physics.
/// Handles catching multiple fish on the way up and returning them to the surface.
/// Integrates with SavesManager for economy and upgrades.
/// </summary>
[RequireComponent(typeof(Rigidbody2D))]
public class HookController : MonoBehaviour
{
    // --------------------------------------------------------
    // ENUMS
    // --------------------------------------------------------
    public enum HookState
    {
        Idle,
        Descending,
        Ascending
    }

    // --------------------------------------------------------
    // SERIALIZED FIELDS
    // --------------------------------------------------------
    [Header("Vertical Movement")]
    [Tooltip("Speed at which the hook descends into the water.")]
    [SerializeField] private float descentSpeed = 3f;

    [Tooltip("Standard ascent speed (can be used for specific mechanics).")]
    [SerializeField] private float ascentSpeed = 5f;

    [Tooltip("High-speed ascent velocity used when the hook reaches maximum depth.")]
    [SerializeField] private float rapidAscentSpeed = 15f; // <-- NUEVA VARIABLE DECLARADA

    [Tooltip("Maximum depth the hook can reach before automatically returning.")]
    [SerializeField] private float maxDepth = -15f;

    [Header("Horizontal Movement")]
    [Tooltip("Multiplier for how fast the hook accelerates towards the finger's X position.")]
    [SerializeField] private float horizontalResponsiveness = 10f;

    [Header("Hook Stats")]
    [Tooltip("Maximum number of fish the hook can catch in a single cast.")]
    [SerializeField] private int maxFishCapacity = 3;

    [Header("Hook Rotation")]
    [Tooltip("Ángulo máximo de inclinación lateral (grados).")]
    [SerializeField] private float maxTiltAngle = 25f;

    [Tooltip("Velocidad del lerp de rotación. Más alto = más reactivo.")]
    [SerializeField] private float tiltSpeed = 8f;

    [Header("Audio")]
    [SerializeField] private AudioClip waterSound;

    // --------------------------------------------------------
    // PRIVATE FIELDS
    // --------------------------------------------------------
    private HookState currentState = HookState.Idle;
    private float startY;
    private Camera mainCamera;
    private Rigidbody2D rb;
    private List<FishController> caughtFishes = new List<FishController>();

    // --------------------------------------------------------
    // EVENTS
    // --------------------------------------------------------
    public static event Action OnReturnToSurface;
    public static event Action<float> OnDepthChanged;
    public static event Action<List<FishController>> OnCatchReady;


    /// <summary>
    /// Triggered when a fish is caught. 
    /// Int 1: Current caught count. Int 2: Max capacity.
    /// </summary>
    public static event Action<int, int> OnCatchCountChanged;

    // --------------------------------------------------------
    // UNITY LIFECYCLE
    // --------------------------------------------------------
    private void Awake()
    {
        // Cache references to avoid costly calls during Update/FixedUpdate
        mainCamera = Camera.main;
        rb = GetComponent<Rigidbody2D>();
        startY = transform.position.y;
    }

    private void Start()
    {
        LoadStatsFromSave();

        // Force initial UI update on startup
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
        // Only catch fish during the ascending phase
        if (currentState != HookState.Ascending || !collision.CompareTag("Fish"))
            return;

        // Ensure we don't exceed the capacity limit
        if (caughtFishes.Count >= maxFishCapacity)
            return;

        FishController fish = collision.GetComponent<FishController>();

        // Register the catch if valid and not already caught
        if (fish != null && !caughtFishes.Contains(fish))
        {
            fish.GetCaught(this.transform);
            caughtFishes.Add(fish);

            // Notify UI of the new catch
            OnCatchCountChanged?.Invoke(caughtFishes.Count, maxFishCapacity);
        }
    }

    // --------------------------------------------------------
    // STATE MACHINE & MOVEMENT LOGIC
    // --------------------------------------------------------

    /// <summary>
        /// Handles the core state machine logic for hook movement.
        /// </summary>
    private void ProcessStateMachine()
    {
        switch (currentState)
        {
            case HookState.Idle:
                rb.velocity = Vector2.zero;
                break;

            case HookState.Descending:
                HandleMovement(-descentSpeed);
                CheckDepthLimit();
                break;

            case HookState.Ascending:
                // Evaluamos si el anzuelo ya está lleno
                bool isCapacityFull = caughtFishes.Count >= maxFishCapacity;

                // Asignamos la velocidad dependiendo de si está lleno o no
                float currentAscentSpeed = isCapacityFull ? rapidAscentSpeed : ascentSpeed;

                HandleMovement(currentAscentSpeed);
                CheckSurfaceLimit();
                break;
        }
    }

    /// <summary>
    /// Initiates the descending state. Triggered via event by the PlayerController.
    /// </summary>
    private void StartDescending()
    {
        currentState = HookState.Descending;
    }

    /// <summary>
    /// Applies vertical velocity and handles horizontal movement based on input.
    /// </summary>
    /// <param name="verticalSpeed">The Y-axis velocity to apply.</param>
    private void HandleMovement(float verticalSpeed)
    {
        float targetVelocityX = 0f;

        // Check for player input to move the hook horizontally
        if (Input.GetMouseButton(0))
        {
            Vector3 mousePos = Input.mousePosition;
            mousePos.z = 10f; // Distance from camera

            Vector3 targetWorldPos = mainCamera.ScreenToWorldPoint(mousePos);
            float differenceX = targetWorldPos.x - rb.position.x;

            targetVelocityX = differenceX * horizontalResponsiveness;
        }

        // Apply calculated velocities to the Rigidbody
        rb.velocity = new Vector2(targetVelocityX, verticalSpeed);
    }

    /// <summary>
    /// Checks if the hook has reached the maximum allowed depth.
    /// If so, switches the state to Ascending.
    /// </summary>
    private void CheckDepthLimit()
    {
        if (rb.position.y <= maxDepth)
        {
            currentState = HookState.Ascending;
        }
    }

    /// <summary>
    /// Checks if the hook has returned to its starting Y position at the surface.
    /// Handles end-of-cast logic, rewards, and state reset.
    /// </summary>
    private void CheckSurfaceLimit()
    {
        if (rb.position.y < startY) return;

        rb.position = new Vector2(0, startY);
        currentState = HookState.Idle;
        rb.velocity = Vector2.zero;
        OnDepthChanged?.Invoke(0f);

        if (caughtFishes.Count > 0)
        {
            // Pasamos la lista al presenter y esperamos a que termine
            OnCatchReady?.Invoke(new List<FishController>(caughtFishes));
            // El presenter llama a FinalizeCast() cuando acaba
        }
        else
        {
            // Sin peces: terminar directo
            FinalizeCast();
        }
    }

    public void FinalizeCast()
    {
        ProcessCatchRewards(); // hace el AddCoins y el caughtFishes.Clear()
        OnCatchCountChanged?.Invoke(0, maxFishCapacity);
        OnReturnToSurface?.Invoke();
    }

    /// <summary>
    /// Calculates the total value of caught fish and updates the economy via SavesManager.
    /// </summary>
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

        // Clear the list for the next cast
        caughtFishes.Clear();
    }

    // --------------------------------------------------------
    // UI & DATA MANAGEMENT
    // --------------------------------------------------------

    /// <summary>
    /// Calculates the absolute distance from the starting point to report current depth.
    /// </summary>
    private void UpdateDepthUI()
    {
        if (currentState != HookState.Idle)
        {
            float currentDepth = Mathf.Abs(rb.position.y - startY);
            OnDepthChanged?.Invoke(currentDepth);
        }
    }

    /// <summary>
    /// Initializes hook stats based on the current save file.
    /// </summary>
    private void LoadStatsFromSave()
    {
        if (SavesManager.Instance != null && SavesManager.Instance.currentData != null)
        {
            maxDepth = SavesManager.Instance.currentData.maxDepth;
            maxFishCapacity = SavesManager.Instance.currentData.maxCatch;
        }
    }

    /// <summary>
    /// Externally callable method to refresh hook stats (e.g., after purchasing an upgrade).
    /// </summary>
    public void RefreshStatsFromSave()
    {
        LoadStatsFromSave();

        // Refresh UI immediately in case the player upgraded capacity mid-game
        OnCatchCountChanged?.Invoke(caughtFishes.Count, maxFishCapacity);
    }

    private void UpdateHookRotation()
    {
        if (currentState == HookState.Idle)
        {
            // Vuelve a neutral cuando está quieto
            float neutralAngle = Mathf.LerpAngle(
                rb.rotation, 0f, Time.fixedDeltaTime * tiltSpeed);
            rb.MoveRotation(neutralAngle);
            return;
        }

        // Tilt lateral basado en velocidad X
        // Dividimos por descentSpeed para normalizar: vel alta → tilt máximo
        float normalizedX = Mathf.Clamp(rb.velocity.x / descentSpeed, -1f, 1f);
        float targetAngle = -normalizedX * maxTiltAngle;

        // Cuando sube, añadimos un ligero cabeceo hacia atrás (se nota más natural)
        if (currentState == HookState.Ascending)
        {
            float verticalBias = 8f; // grados extra hacia "arriba"
            targetAngle += verticalBias * Mathf.Sign(targetAngle == 0 ? 0 : targetAngle);
            // Si va recto hacia arriba sin lateral, cabeceo neutro
        }

        float smoothAngle = Mathf.LerpAngle(
            rb.rotation, targetAngle, Time.fixedDeltaTime * tiltSpeed);

        rb.MoveRotation(smoothAngle);
    }
}