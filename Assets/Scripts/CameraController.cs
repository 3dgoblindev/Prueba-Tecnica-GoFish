using UnityEngine;

// Smoothly tracks a target along the Y axis using SmoothDamp based on gameplay events.
public class CameraController : MonoBehaviour
{
    [Header("Targeting")]
    [SerializeField] private Transform currentTarget;
    [SerializeField] private Transform hookTarget;
    [SerializeField] private Transform defaultTarget;

    [Header("Target Offsets")]
    [Tooltip("Y offset applied when following the default target.")]
    [SerializeField] private float defaultOffsetY = 2f;
    [Tooltip("Y offset applied when following the hook.")]
    [SerializeField] private float hookOffsetY = -3f;

    [Header("Follow Settings")]
    [SerializeField] private float smoothTime = 0.2f;
    [SerializeField] private float maxY = 0f;
    [SerializeField] private float minY = -50f;

    private Vector3 currentVelocity = Vector3.zero;
    private float currentOffsetY;

    private void Start()
    {
        currentOffsetY = (currentTarget == hookTarget) ? hookOffsetY : defaultOffsetY;
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

    private void LateUpdate()
    {
        if (currentTarget == null) return;

        // Calculate and clamp target Y position
        float targetY = currentTarget.position.y + currentOffsetY;
        float clampedY = Mathf.Clamp(targetY, minY, maxY);

        Vector3 targetPosition = new Vector3(transform.position.x, clampedY, transform.position.z);

        // Smoothly move towards the target position
        transform.position = Vector3.SmoothDamp(
            transform.position,
            targetPosition,
            ref currentVelocity,
            smoothTime
        );
    }
}