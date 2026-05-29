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
    public enum HookState { Idle, Descending, Ascending }

    [Header("Vertical Movement")]
    [SerializeField] private float descentSpeed = 3f;
    [SerializeField] private float ascentSpeed = 5f;
    [SerializeField] private float maxDepth = -15f;

    [Header("Horizontal Movement")]
    [Tooltip("Multiplier for how fast the hook accelerates towards the finger's X position.")]
    [SerializeField] private float horizontalResponsiveness = 10f;

    [Header("Hook Stats")]
    [SerializeField] private int maxFishCapacity = 3;

    private HookState currentState = HookState.Idle;
    private float startY;
    private Camera mainCamera;
    private Rigidbody2D rb;
    private List<FishController> caughtFishes = new List<FishController>();

    // --- NUEVOS EVENTOS PARA LA UI ---
    public static event Action OnReturnToSurface;
    public static event Action<float> OnDepthChanged;
    public static event Action<int, int> OnCatchCountChanged; // Current Caught, Max Capacity

    private void Awake()
    {
        mainCamera = Camera.main;
        rb = GetComponent<Rigidbody2D>();
        startY = transform.position.y;
    }

    private void Start()
    {
        if (SavesManager.Instance != null)
        {
            maxDepth = SavesManager.Instance.currentData.maxDepth;
            maxFishCapacity = SavesManager.Instance.currentData.maxCatch;
        }

        // Forzamos la actualización inicial de la UI al arrancar
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
                HandleMovement(ascentSpeed);
                CheckSurfaceLimit();
                break;
        }

        // Si nos estamos moviendo, avisamos a la UI de la profundidad actual.
        // Calculamos la distancia absoluta desde el punto de inicio para que empiece en 0.
        if (currentState != HookState.Idle)
        {
            float currentDepth = Mathf.Abs(rb.position.y - startY);
            OnDepthChanged?.Invoke(currentDepth);
        }
    }

    private void StartDescending()
    {
        currentState = HookState.Descending;
    }

    private void HandleMovement(float verticalSpeed)
    {
        float targetVelocityX = 0f;

        if (Input.GetMouseButton(0))
        {
            Vector3 mousePos = Input.mousePosition;
            mousePos.z = 10f;
            Vector3 targetWorldPos = mainCamera.ScreenToWorldPoint(mousePos);

            float differenceX = targetWorldPos.x - rb.position.x;
            targetVelocityX = differenceX * horizontalResponsiveness;
        }

        rb.velocity = new Vector2(targetVelocityX, verticalSpeed);
    }

    private void CheckDepthLimit()
    {
        if (rb.position.y <= maxDepth)
        {
            currentState = HookState.Ascending;
        }
    }

    private void CheckSurfaceLimit()
    {
        if (rb.position.y >= startY)
        {
            rb.position = new Vector2(0, startY);
            currentState = HookState.Idle;
            rb.velocity = Vector2.zero;

            // Reseteamos el contador de profundidad en la UI
            OnDepthChanged?.Invoke(0f);

            if (caughtFishes.Count > 0)
            {
                int totalCoinsThisCast = 0;
                foreach (FishController fish in caughtFishes)
                {
                    if (fish != null)
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

            // Reseteamos el contador de peces en la UI
            OnCatchCountChanged?.Invoke(0, maxFishCapacity);
            OnReturnToSurface?.Invoke();
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (currentState == HookState.Ascending && collision.CompareTag("Fish"))
        {
            if (caughtFishes.Count >= maxFishCapacity) return;

            FishController fish = collision.GetComponent<FishController>();

            if (fish != null && !caughtFishes.Contains(fish))
            {
                fish.GetCaught(this.transform);
                caughtFishes.Add(fish);

                // Avisamos a la UI de que hemos pescado uno nuevo
                OnCatchCountChanged?.Invoke(caughtFishes.Count, maxFishCapacity);
            }
        }
    }

    public void RefreshStatsFromSave()
    {
        if (SavesManager.Instance != null)
        {
            maxDepth = SavesManager.Instance.currentData.maxDepth;
            maxFishCapacity = SavesManager.Instance.currentData.maxCatch;

            // Refrescamos la UI por si el jugador acaba de comprar una mejora de capacidad
            OnCatchCountChanged?.Invoke(caughtFishes.Count, maxFishCapacity);
        }
    }
}