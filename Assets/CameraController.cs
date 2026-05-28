using UnityEngine;

/// <summary>
/// Handles smooth camera tracking along the Y axis using SmoothDamp.
/// Subscribes to gameplay events to dynamically switch targets.
/// </summary>
public class CameraController : MonoBehaviour
{
    [Header("Targeting")]
    [Tooltip("The active transform the camera is currently following.")]
    [SerializeField] private Transform currentTarget;
    [SerializeField] private Transform hookTarget;
    [SerializeField] private Transform defaultTarget;


    [Header("Follow Settings")]
    [SerializeField] private float smoothTime = 0.2f;
    [SerializeField] private float maxY = 0f;
    [SerializeField] private float minY = -50f;

    private Vector3 currentVelocity = Vector3.zero;

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
        }
    }

    // Camera movement MUST be in LateUpdate to ensure all target physics/movements 
    // have been processed this frame, preventing visual jitter.
    private void LateUpdate()
    {
        if (currentTarget == null) return;

        // Clamp to prevent the camera from showing areas outside the designed level bounds
        float clampedY = Mathf.Clamp(currentTarget.position.y, minY, maxY);
        Vector3 targetPosition = new Vector3(transform.position.x, clampedY, transform.position.z);

        // SmoothDamp acts like a spring, providing a more natural ease-in/ease-out than basic Lerp
        transform.position = Vector3.SmoothDamp(
            transform.position,
            targetPosition,
            ref currentVelocity,
            smoothTime
        );
    }

    private void SwitchToDefault()
    {
        if (defaultTarget != null) currentTarget = defaultTarget;
    }
}