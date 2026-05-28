using UnityEngine;

/// <summary>
/// Manages the visual rendering of the fishing line.
/// Subscribes to the casting event to manage its initial visibility dynamically.
/// </summary>
[RequireComponent(typeof(LineRenderer))]
public class FishingLineController : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Transform rodTip;
    [SerializeField] private Transform hook;

    private LineRenderer lineRenderer;
    private Renderer[] childRenderers;

    private void Awake()
    {
        lineRenderer = GetComponent<LineRenderer>();
        lineRenderer.positionCount = 2;

        // Cache all renderers (including children like the hook's SpriteRenderer)
        childRenderers = GetComponentsInChildren<Renderer>();

        // Hide visually instead of disabling GameObjects to maintain initialization lifecycles (Awake/Start)
        SetVisibility(false);
    }

    private void OnEnable()
    {
        // Subscribe to the global cast event
        PlayerController.OnCastCompleted += ShowLine;
        HookController.OnReturnToSurface += HideLine;
    }

    private void OnDisable()
    {
        // Always unsubscribe to prevent memory leaks and null reference exceptions
        PlayerController.OnCastCompleted -= ShowLine;
        HookController.OnReturnToSurface -= HideLine;
    }

    private void Update()
    {
        if (lineRenderer.enabled && rodTip != null && hook != null)
        {
            lineRenderer.SetPosition(0, rodTip.position);
            lineRenderer.SetPosition(1, hook.position);
        }
    }

    private void ShowLine()
    {
        SetVisibility(true);
    }

    private void HideLine()
    {
        SetVisibility(false);
    }

    private void SetVisibility(bool isVisible)
    {
        foreach (Renderer r in childRenderers)
        {
            r.enabled = isVisible;
        }
    }
}