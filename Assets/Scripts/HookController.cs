using UnityEngine;
using System;
using System.Collections.Generic;

/// <summary>
/// Controls the movement and state machine of the fishing hook using physics.
/// Handles catching multiple fish on the way up and returning them to the surface.
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
    [Tooltip("Maximum number of fish this hook can carry at once. Upgradable.")]
    [SerializeField] private int maxFishCapacity = 3;

    private HookState currentState = HookState.Idle;
    private float startY;
    private Camera mainCamera;

    // Physics reference
    private Rigidbody2D rb;

    // List to hold all currently hooked fish
    private List<FishController> caughtFishes = new List<FishController>();

    // Event
    public static event Action OnReturnToSurface;

    private void Awake()
    {
        mainCamera = Camera.main;
        rb = GetComponent<Rigidbody2D>();
        startY = transform.position.y;
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
                // Ensure it doesn't drift while idle
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
            // Reset position and stop physics
            rb.position = new Vector2(0, startY);
            currentState = HookState.Idle;
            rb.velocity = Vector2.zero;

            // --- PROCESS ALL CAUGHT FISH ---
            if (caughtFishes.Count > 0)
            {
                int totalCoinsThisCast = 0;

                // Loop through all collected fish
                foreach (FishController fish in caughtFishes)
                {
                    if (fish != null)
                    {
                        Debug.Log($"Collected a {fish.data.fishName} worth {fish.data.price} coins!");
                        totalCoinsThisCast += fish.data.price;

                        // NOTA: No destruimos el objeto aquí. El Spawner se encarga de reciclarlo.
                    }
                }

                Debug.Log($"--- Total earned this cast: {totalCoinsThisCast} coins! ---");
                // TODO: Add 'totalCoinsThisCast' to your GameManager/PlayerInventory

                // Clear the list so the hook is empty for the next cast
                caughtFishes.Clear();
            }

            OnReturnToSurface?.Invoke();
        }
    }

    /// <summary>
    /// Detects collisions with fish while ascending.
    /// </summary>
    private void OnTriggerEnter2D(Collider2D collision)
    {
        // 1. Only catch fish if we are going UP
        // 2. Only check objects tagged as "Fish"
        if (currentState == HookState.Ascending && collision.CompareTag("Fish"))
        {
            // 3. Check if we have reached the maximum capacity
            if (caughtFishes.Count >= maxFishCapacity)
            {
                // Hook is full, ignore this fish
                return;
            }

            FishController fish = collision.GetComponent<FishController>();

            // 4. Ensure we found the component and haven't already caught this exact fish
            if (fish != null && !caughtFishes.Contains(fish))
            {
                // Tell the fish to parent itself to the hook and stop swimming
                fish.GetCaught(this.transform);

                // Add it to our inventory list for this cast
                caughtFishes.Add(fish);
            }
        }
    }
}