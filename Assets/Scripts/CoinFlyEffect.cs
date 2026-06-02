using System.Collections;
using TMPro;
using UnityEngine;

public class CoinFlyEffect : MonoBehaviour
{
    [SerializeField] private RectTransform coinRect;
    [SerializeField] private TextMeshProUGUI valueText;
    [SerializeField] private float flyDuration = 0.6f;
    [SerializeField] private float arcHeight = 60f; // Altura del salto

    [Header("Ajustes Extra")]
    [Tooltip("Ajuste manual de la posición final (X, Y)")]
    [SerializeField] private Vector2 targetOffset;
    [Tooltip("Escala final al llegar al destino (ej. 0.5 = la mitad de pequeña)")]
    [SerializeField] private float finalScale = 0.5f;

    private Canvas rootCanvas;

    public void Init(Canvas canvas) => rootCanvas = canvas;

    public void Fly(Vector3 worldOrigin, RectTransform uiTarget, int value)
    {
        if (valueText != null)
            valueText.text = $"+{value}";

        StartCoroutine(FlyRoutine(worldOrigin, uiTarget));
    }

    private IEnumerator FlyRoutine(Vector3 worldOrigin, RectTransform uiTarget)
    {
        yield return null;

        Camera cam = Camera.main;
        Camera uiCam = rootCanvas.renderMode == RenderMode.ScreenSpaceOverlay
                           ? null
                           : rootCanvas.worldCamera;

        RectTransform parentRect = coinRect.parent as RectTransform;

        Vector2 screenOrigin = cam.WorldToScreenPoint(worldOrigin);
        Vector2 screenTarget = RectTransformUtility.WorldToScreenPoint(uiCam, uiTarget.position);

        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            parentRect, screenOrigin, uiCam, out Vector2 localOrigin);
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            parentRect, screenTarget, uiCam, out Vector2 localTarget);

        // --- APLICAMOS EL OFFSET MANUAL ---
        localTarget += targetOffset;

        coinRect.anchoredPosition = localOrigin;

        Vector3 startScale = coinRect.localScale;
        Vector3 endScale = startScale * finalScale;

        float t = 0f;
        while (t < 1f)
        {
            t += Time.deltaTime / flyDuration;
            float tc = Mathf.Clamp01(t); // Tiempo lineal de 0 a 1

            // 1. Curva para el movimiento (Ease Out Quad: empieza rápido, frena suavemente)
            float moveEase = 1f - (1f - tc) * (1f - tc);

            // 2. Curva para la escala (Ease In Quad: empieza lento, se achica rápido al final)
            float scaleEase = tc * tc;

            // 3. Altura del arco de la moneda
            float currentHeight = Mathf.Sin(tc * Mathf.PI) * arcHeight;

            // Actualizamos la Posición usando la curva del movimiento + altura
            coinRect.anchoredPosition = Vector2.Lerp(localOrigin, localTarget, moveEase)
                                      + (Vector2.up * currentHeight);

            // Actualizamos la Escala usando la curva de la escala
            coinRect.localScale = Vector3.Lerp(startScale, endScale, scaleEase);

            yield return null;
        }

        Destroy(gameObject);
    }
}