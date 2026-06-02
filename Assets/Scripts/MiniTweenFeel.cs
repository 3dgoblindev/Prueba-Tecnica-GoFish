using System.Collections;
using UnityEngine;

/// <summary>
/// Compact script to add "Game Feel" (Juice) to any object using native Tweens.
/// </summary>
public class MiniTweenFeel : MonoBehaviour
{
    public enum TweenMode { OneWay, PingPong }
    public enum EaseType { Linear, EaseIn, EaseOut, EaseInOut, PunchElastic }

    [Header("--- Tween Settings ---")]
    [SerializeField] private TweenMode tweenMode = TweenMode.PingPong;
    [SerializeField] private EaseType easeType = EaseType.EaseOut;
    [SerializeField] private float duration = 0.3f;
    [SerializeField] private bool playOnEnable = false;

    [Header("--- Target: Position ---")]
    [SerializeField] private bool animatePosition = false;
    [Tooltip("Relative displacement from its current position.")]
    [SerializeField] private Vector3 positionOffset = new Vector3(0, 1f, 0);

    [Header("--- Target: Rotation ---")]
    [SerializeField] private bool animateRotation = false;
    [Tooltip("Angles to rotate (in degrees) from its current rotation.")]
    [SerializeField] private Vector3 rotationOffset = new Vector3(0, 0, 45f);

    [Header("--- Target: Scale ---")]
    [SerializeField] private bool animateScale = false;
    [Tooltip("Target scale the object will reach.")]
    [SerializeField] private Vector3 targetScale = new Vector3(1.3f, 1.3f, 1.3f);

    // Variables to cache the exact initial states before each animation
    private Vector3 startPosition;
    private Vector3 startRotation;
    private Vector3 startScale;
    private Coroutine tweenCoroutine;

    private void OnEnable()
    {
        if (playOnEnable)
        {
            Play();
        }
    }

    /// <summary>
    /// Executes the visual effect. You can call it from other scripts (e.g., when clicking a button, taking damage, etc.)
    /// </summary>
    [ContextMenu("Test Tween")] // <--- This allows testing it from the editor by right-clicking the component
    public void Play()
    {
        if (tweenCoroutine != null)
        {
            StopCoroutine(tweenCoroutine);
        }

        tweenCoroutine = StartCoroutine(DoTweenRoutine());
    }

    private IEnumerator DoTweenRoutine()
    {
        // Save the initial state RIGHT before starting to avoid misconfigurations if the object moves due to gameplay
        startPosition = transform.localPosition;
        startRotation = transform.localEulerAngles;
        startScale = transform.localScale;

        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float linearPercentage = Mathf.Clamp01(elapsed / duration);

            // 1. Modify time based on the mode (OneWay or PingPong)
            float t = linearPercentage;

            // If it's PingPong and NOT PunchElastic (since punch returns to 0 due to its own math)
            if (tweenMode == TweenMode.PingPong && easeType != EaseType.PunchElastic)
            {
                // Converts the 0->1 progress into a round trip: 0 -> 1 -> 0
                t = linearPercentage < 0.5f ? linearPercentage * 2f : (1f - linearPercentage) * 2f;
            }

            // 2. Apply the mathematical smoothing curve (Ease)
            float evaluatedFactor = EvaluateEase(t, easeType);

            // 3. Apply transformations using LerpUnclamped to allow elastic overshoots
            if (animatePosition)
            {
                transform.localPosition = Vector3.LerpUnclamped(startPosition, startPosition + positionOffset, evaluatedFactor);
            }

            if (animateRotation)
            {
                // We use Vector Lerp on Euler angles for quick, direct-feel rotations
                transform.localEulerAngles = Vector3.LerpUnclamped(startRotation, startRotation + rotationOffset, evaluatedFactor);
            }

            if (animateScale)
            {
                transform.localScale = Vector3.LerpUnclamped(startScale, targetScale, evaluatedFactor);
            }

            yield return null;
        }

        // Ensure everything is perfectly in place at the end of the coroutine if it's OneWay or returned to origin
        RestoreFinalState();
    }

    private float EvaluateEase(float t, EaseType type)
    {
        switch (type)
        {
            case EaseType.Linear:
                return t;
            case EaseType.EaseIn:
                return t * t;
            case EaseType.EaseOut:
                return t * (2f - t);
            case EaseType.EaseInOut:
                return t < 0.5f ? 2f * t * t : 1f - Mathf.Pow(-2f * t + 2f, 2f) / 2f;
            case EaseType.PunchElastic:
                // Mathematical equation simulating an elastic impact: vibrates, damps, and ends at 0.
                // Recommended to use in "OneWay" mode because the curve itself returns to the origin.
                return Mathf.Sin(t * Mathf.PI * 3f) * (1f - t);
            default:
                return t;
        }
    }

    private void RestoreFinalState()
    {
        if (tweenMode == TweenMode.PingPong || easeType == EaseType.PunchElastic)
        {
            if (animatePosition) transform.localPosition = startPosition;
            if (animateRotation) transform.localEulerAngles = startRotation;
            if (animateScale) transform.localScale = startScale;
        }
        else // OneWay
        {
            if (animatePosition) transform.localPosition = startPosition + positionOffset;
            if (animateRotation) transform.localEulerAngles = startRotation + rotationOffset;
            if (animateScale) transform.localScale = targetScale;
        }
    }
}