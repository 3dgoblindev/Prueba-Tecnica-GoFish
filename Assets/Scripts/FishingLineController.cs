using UnityEngine;

/// <summary>
/// Manages the visual coordinate routing and frame updates for the 2D LineRenderer.
/// Dynamically toggles renderer states via global casting lifecycle events to maintain awake execution hierarchies.
/// </summary>
[RequireComponent(typeof(LineRenderer))]
public class FishingLineController : MonoBehaviour
{
    [Header("Transform Attachments")]
    [Tooltip("The starting origin position coordinate point for the line render node array.")]
    [SerializeField] private Transform rodTip;

    [Tooltip("The termination endpoint target anchor for the line render node array.")]
    [SerializeField] private Transform hook;

    private LineRenderer lineRenderer;
    private Renderer[] childRenderers;

    private void Awake()
    {
        InitializeRenderers();
        SetVisibility(false);
    }

    private void OnEnable()
    {
        PlayerController.OnCastCompleted += HandleCastCompleted;
        HookController.OnReturnToSurface += HandleReturnToSurface;
    }

    private void OnDisable()
    {
        PlayerController.OnCastCompleted -= HandleCastCompleted;
        HookController.OnReturnToSurface -= HandleReturnToSurface;
    }

    private void Update()
    {
        RenderLineCoordinates();
    }

    /// <summary>
    /// Caches operational component arrays and configures basic node counts.
    /// </summary>
    private void InitializeRenderers()
    {
        lineRenderer = GetComponent<LineRenderer>();
        lineRenderer.positionCount = 2;

        childRenderers = GetComponentsInChildren<Renderer>(true);
    }

    /// <summary>
    /// Computes spatial positions dynamically across active frames when visibility rendering layers are checked active.
    /// </summary>
    private void RenderLineCoordinates()
    {
        if (lineRenderer.enabled && rodTip != null && hook != null)
        {
            lineRenderer.SetPosition(0, rodTip.position);
            lineRenderer.SetPosition(1, hook.position);
        }
    }

    private void HandleCastCompleted()
    {
        SetVisibility(true);
    }

    private void HandleReturnToSurface()
    {
        SetVisibility(false);
    }

    /// <summary>
    /// Toggles the execution flag status across all cached rendering structural layers.
    /// </summary>
    /// <param name="isVisible">The target visibility status state applied to structural elements.</param>
    private void SetVisibility(bool isVisible)
    {
        if (childRenderers == null) return;

        foreach (Renderer rendererElement in childRenderers)
        {
            if (rendererElement != null)
            {
                rendererElement.enabled = isVisible;
            }
        }
    }
}