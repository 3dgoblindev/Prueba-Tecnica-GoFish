using System;
using System.Collections;
using UnityEngine;

public class MiniTweenFeel : MonoBehaviour
{
    public enum TweenMode { OneWay, PingPong }
    public enum EaseType { Linear, EaseIn, EaseOut, EaseInOut, PunchElastic }

    [Header("Tween Settings")]
    [SerializeField] private TweenMode tweenMode = TweenMode.PingPong;
    [SerializeField] private EaseType easeType = EaseType.EaseOut;
    [SerializeField] private float duration = 0.3f;
    [SerializeField] private bool playOnEnable = false;

    [Header("Targets")]
    [SerializeField] private bool animatePosition = false;
    [SerializeField] private Vector3 positionOffset = new Vector3(0f, 1f, 0f);
    [Space]
    [SerializeField] private bool animateRotation = false;
    [SerializeField] private Vector3 rotationOffset = new Vector3(0f, 0f, 45f);
    [Space]
    [SerializeField] private bool animateScale = false;
    [SerializeField] private Vector3 targetScale = new Vector3(1.3f, 1.3f, 1.3f);

    private Vector3 startPosition;
    private Vector3 startRotation;
    private Vector3 startScale;
    private Coroutine tweenCoroutine;

    private void OnEnable()
    {
        if (playOnEnable) Play();
    }

    [ContextMenu("Test Tween")]
    public void Play()
    {
        if (tweenCoroutine != null) StopCoroutine(tweenCoroutine);
        tweenCoroutine = StartCoroutine(DoTweenRoutine());
    }

    private IEnumerator DoTweenRoutine()
    {
        // Cache initial values before animation starts
        startPosition = transform.localPosition;
        startRotation = transform.localEulerAngles;
        startScale = transform.localScale;

        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float percent = Mathf.Clamp01(elapsed / duration);

            // Handle PingPong loop mapping
            if (tweenMode == TweenMode.PingPong && easeType != EaseType.PunchElastic)
            {
                percent = percent < 0.5f ? percent * 2f : (1f - percent) * 2f;
            }

            float t = EvaluateEase(percent, easeType);

            // Apply modifications (LerpUnclamped allows overshoot on elastic curves)
            if (animatePosition) transform.localPosition = Vector3.LerpUnclamped(startPosition, startPosition + positionOffset, t);
            if (animateRotation) transform.localEulerAngles = Vector3.LerpUnclamped(startRotation, startRotation + rotationOffset, t);
            if (animateScale) transform.localScale = Vector3.LerpUnclamped(startScale, targetScale, t);

            yield return null;
        }

        ResetOrSnapToFinalState();
    }

    private float EvaluateEase(float t, EaseType type)
    {
        switch (type)
        {
            case EaseType.Linear: return t;
            case EaseType.EaseIn: return t * t;
            case EaseType.EaseOut: return t * (2f - t);
            case EaseType.EaseInOut: return t < 0.5f ? 2f * t * t : 1f - Mathf.Pow(-2f * t + 2f, 2f) / 2f;
            case EaseType.PunchElastic: return Mathf.Sin(t * Mathf.PI * 3f) * (1f - t);
            default: return t;
        }
    }

    private void ResetOrSnapToFinalState()
    {
        if (tweenMode == TweenMode.PingPong || easeType == EaseType.PunchElastic)
        {
            if (animatePosition) transform.localPosition = startPosition;
            if (animateRotation) transform.localEulerAngles = startRotation;
            if (animateScale) transform.localScale = startScale;
        }
        else // OneWay completion snap
        {
            if (animatePosition) transform.localPosition = startPosition + positionOffset;
            if (animateRotation) transform.localEulerAngles = startRotation + rotationOffset;
            if (animateScale) transform.localScale = targetScale;
        }
    }
}