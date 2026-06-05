using UnityEngine;

[RequireComponent(typeof(LineRenderer))]
public class FishingLineController : MonoBehaviour
{
    [SerializeField] private Transform rodTip;
    [SerializeField] private Transform hook;

    private LineRenderer lineRenderer;
    private Renderer[] childRenderers;

    private void Awake()
    {
        lineRenderer = GetComponent<LineRenderer>();
        lineRenderer.positionCount = 2;

        // Include inactive children to catch components before initial spawn toggles
        childRenderers = GetComponentsInChildren<Renderer>(true);

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
        if (lineRenderer.enabled && rodTip != null && hook != null)
        {
            lineRenderer.SetPosition(0, rodTip.position);
            lineRenderer.SetPosition(1, hook.position);
        }
    }

    private void HandleCastCompleted() => SetVisibility(true);
    private void HandleReturnToSurface() => SetVisibility(false);

    private void SetVisibility(bool isVisible)
    {
        if (childRenderers == null) return;

        foreach (Renderer r in childRenderers)
        {
            if (r != null) r.enabled = isVisible;
        }
    }
}