using UnityEngine;

/// <summary>
/// Handles smooth camera tracking along the Y axis using SmoothDamp.
/// Subscribes to gameplay events to dynamically switch targets and their respective offsets.
/// </summary>
public class CameraController : MonoBehaviour
{
    [Header("Targeting")]
    [Tooltip("The active transform the camera is currently following.")]
    [SerializeField] private Transform currentTarget;
    [SerializeField] private Transform hookTarget;
    [SerializeField] private Transform defaultTarget;

    [Header("Target Offsets")]
    [Tooltip("Y offset applied when following the default target (e.g., the boat).")]
    [SerializeField] private float defaultOffsetY = 2f;
    [Tooltip("Y offset applied when following the hook. Negative values look further down.")]
    [SerializeField] private float hookOffsetY = -3f;

    [Header("Follow Settings")]
    [SerializeField] private float smoothTime = 0.2f;
    [SerializeField] private float maxY = 0f;
    [SerializeField] private float minY = -50f;

    private Vector3 currentVelocity = Vector3.zero;
    private float currentOffsetY;

    private void Start()
    {
        // Initialize the correct offset based on the starting target
        if (currentTarget == hookTarget)
        {
            currentOffsetY = hookOffsetY;
        }
        else
        {
            currentOffsetY = defaultOffsetY;
        }
    }

    private void OnEnable()
    {
        PlayerController.OnCastCompleted += SwitchToHook;
        HookController.OnReturnToSurface += SwitchToDefault;
    }

    private void OnDisable()
    {
        PlayerController.OnCastCompleted -= SwitchToHook;
        HookController.OnReturnToSurface -= SwitchToDefault;
    }

    private void SwitchToHook()
    {
        if (hookTarget != null)
        {
            currentTarget = hookTarget;
            currentOffsetY = hookOffsetY;
        }
    }

    private void SwitchToDefault()
    {
        if (defaultTarget != null)
        {
            currentTarget = defaultTarget;
            currentOffsetY = defaultOffsetY;
        }
    }

    // Camera movement MUST be in LateUpdate to ensure all target physics/movements 
    // have been processed this frame, preventing visual jitter.
    private void LateUpdate()
    {
        if (currentTarget == null) return;

        // Calculate target Y including the dynamic offset
        float targetY = currentTarget.position.y + currentOffsetY;

        // Clamp to prevent the camera from showing areas outside the designed level bounds
        float clampedY = Mathf.Clamp(targetY, minY, maxY);

        Vector3 targetPosition = new Vector3(transform.position.x, clampedY, transform.position.z);

        // SmoothDamp acts like a spring, providing a more natural ease-in/ease-out than basic Lerp
        transform.position = Vector3.SmoothDamp(
            transform.position,
            targetPosition,
            ref currentVelocity,
            smoothTime
        );
    }
}