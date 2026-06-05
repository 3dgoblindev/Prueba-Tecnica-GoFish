using System;
using System.Collections;
using UnityEngine;

/// <summary>
/// A lightweight runtime component used to apply procedural procedural animations (Juice) 
/// to transform spaces using custom mathematical interpolation algorithms.
/// </summary>
public class MiniTweenFeel : MonoBehaviour
{
    public enum TweenMode
    {
        OneWay,
        PingPong
    }

    public enum EaseType
    {
        Linear,
        EaseIn,
        EaseOut,
        EaseInOut,
        PunchElastic
    }

    [Header("Tween Configurations")]
    [SerializeField] private TweenMode tweenMode = TweenMode.PingPong;
    [SerializeField] private EaseType easeType = EaseType.EaseOut;
    [SerializeField] private float duration = 0.3f;
    [SerializeField] private bool playOnEnable = false;

    [Header("Position Modulation")]
    [SerializeField] private bool animatePosition = false;
    [Tooltip("Relative local translation offset vector calculation targeted at completion layout.")]
    [SerializeField] private Vector3 positionOffset = new Vector3(0f, 1f, 0f);

    [Header("Rotation Modulation")]
    [SerializeField] private bool animateRotation = false;
    [Tooltip("Relative local euler structural angular displacement calculated on evaluation loops.")]
    [SerializeField] private Vector3 rotationOffset = new Vector3(0f, 0f, 45f);

    [Header("Scale Modulation")]
    [SerializeField] private bool animateScale = false;
    [Tooltip("Absolute vector parameters applied to local transform scales.")]
    [SerializeField] private Vector3 targetScale = new Vector3(1.3f, 1.3f, 1.3f);

    // Initial Transformation Snapshots
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
    /// Evaluates operational states, overrides running calculations, and executes a fresh interpolation routine cycle.
    /// </summary>
    [ContextMenu("Test Tween")]
    public void Play()
    {
        if (tweenCoroutine != null)
        {
            StopCoroutine(tweenCoroutine);
        }

        tweenCoroutine = StartCoroutine(DoTweenRoutine());
    }

    /// <summary>
    /// Core operational processing loop tracking real-time frames to mutate transformation components.
    /// </summary>
    private IEnumerator DoTweenRoutine()
    {
        CaptureInitialStates();

        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float linearPercentage = Mathf.Clamp01(elapsed / duration);

            float timeFactor = EvaluateTweenProgressMode(linearPercentage);
            float evaluatedFactor = EvaluateEase(timeFactor, easeType);

            ApplyTransformations(evaluatedFactor);

            yield return null;
        }

        RestoreFinalState();
    }

    /// <summary>
    /// Explicitly captures transformation fields before entering dynamic interpolation sequences.
    /// </summary>
    private void CaptureInitialStates()
    {
        startPosition = transform.localPosition;
        startRotation = transform.localEulerAngles;
        startScale = transform.localScale;
    }

    /// <summary>
    /// Translates raw linear timeline progress percentages into contextual trajectory playback sequences.
    /// </summary>
    private float EvaluateTweenProgressMode(float linearPercentage)
    {
        if (tweenMode == TweenMode.PingPong && easeType != EaseType.PunchElastic)
        {
            return linearPercentage < 0.5f ? linearPercentage * 2f : (1f - linearPercentage) * 2f;
        }

        return linearPercentage;
    }

    /// <summary>
    /// Maps normalized time signatures onto explicit curves using closed-form kinematic formulas.
    /// </summary>
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
                return Mathf.Sin(t * Mathf.PI * 3f) * (1f - t);

            default:
                return t;
        }
    }

    /// <summary>
    /// Steps values along the evaluation space using unclamped lerp calculations to preserve kinetic overshoots.
    /// </summary>
    private void ApplyTransformations(float factor)
    {
        if (animatePosition)
        {
            transform.localPosition = Vector3.LerpUnclamped(startPosition, startPosition + positionOffset, factor);
        }

        if (animateRotation)
        {
            transform.localEulerAngles = Vector3.LerpUnclamped(startRotation, startRotation + rotationOffset, factor);
        }

        if (animateScale)
        {
            transform.localScale = Vector3.LerpUnclamped(startScale, targetScale, factor);
        }
    }

    /// <summary>
    /// Enforces rigorous anchoring to alignment thresholds upon loop completion sequences.
    /// </summary>
    private void RestoreFinalState()
    {
        if (tweenMode == TweenMode.PingPong || easeType == EaseType.PunchElastic)
        {
            if (animatePosition) transform.localPosition = startPosition;
            if (animateRotation) transform.localEulerAngles = startRotation;
            if (animateScale) transform.localScale = startScale;
        }
        else
        {
            if (animatePosition) transform.localPosition = startPosition + positionOffset;
            if (animateRotation) transform.localEulerAngles = startRotation + rotationOffset;
            if (animateScale) transform.localScale = targetScale;
        }
    }
}