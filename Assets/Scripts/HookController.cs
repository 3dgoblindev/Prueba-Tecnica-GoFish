using UnityEngine;
using System;

/// <summary>
/// Controls the movement and state machine of the fishing hook using physics.
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

    private HookState currentState = HookState.Idle;
    private float startY;
    private Camera mainCamera;

    // Physics reference
    private Rigidbody2D rb;

    //Event
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

    // Changed from Update to FixedUpdate for reliable physics calculations
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

            // Calculate the distance between current position and target X
            float differenceX = targetWorldPos.x - rb.position.x;

            // Create a proportional velocity towards the target. 
            // The further away the finger is, the faster it moves horizontally to catch up.
            targetVelocityX = differenceX * horizontalResponsiveness;
        }

        // Apply the combined velocity to the Rigidbody2D
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
            // Reset position explicitly to snap exactly to the surface, then stop physics
            rb.position = new Vector2(rb.position.x, startY);
            currentState = HookState.Idle;
            rb.velocity = Vector2.zero;

            OnReturnToSurface?.Invoke();
        }
    }
}