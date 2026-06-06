using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[RequireComponent(typeof(Rigidbody2D))]
public class HookController : MonoBehaviour
{
    public enum HookState { Idle, Descending, Ascending }

    public static event Action OnReturnToSurface;
    public static event Action<float> OnDepthChanged;
    public static event Action<List<FishController>> OnCatchReady;
    public static event Action<int, int> OnCatchCountChanged;

    [Header("Movement")]
    [SerializeField] private float descentSpeed = 3f;
    [SerializeField] private float ascentSpeed = 5f;
    [SerializeField] private float rapidAscentSpeed = 15f;
    [SerializeField] private float maxDepth = -15f;
    [SerializeField] private float horizontalResponsiveness = 10f;

    [Header("Capacity")]
    [SerializeField] private int maxFishCapacity = 3;

    [Header("Juice / Rotation")]
    [SerializeField] private float maxTiltAngle = 25f;
    [SerializeField] private float tiltSpeed = 8f;
    [SerializeField] private float freezePerFish = 0.05f;
    [SerializeField] private float freezeLastFish = 0.15f;

    [Header("Audio")]
    [SerializeField] private AudioClip endCatchSound;

    private HookState currentState = HookState.Idle;
    private float startYPosition;
    private Camera mainCamera;
    private Rigidbody2D rb2d;
    private List<FishController> caughtFishes = new List<FishController>();


    [Header("Sprite")]
    [SerializeField] private SpriteRenderer spriteRenderer;

    private void Awake()
    {
        mainCamera = Camera.main;
        rb2d = GetComponent<Rigidbody2D>();
        startYPosition = transform.position.y;
        //spriteRenderer = GetComponent<SpriteRenderer>();
    }

    private void Start()
    {
        LoadStatsFromSave();
        OnDepthChanged?.Invoke(0f);
        OnCatchCountChanged?.Invoke(0, maxFishCapacity);
    }

    private void OnEnable() => PlayerController.OnCastCompleted += StartDescending;
    private void OnDisable() => PlayerController.OnCastCompleted -= StartDescending;

    private void FixedUpdate()
    {
        ProcessStateMachine();
        UpdateDepthUI();
        UpdateHookRotation();
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        // Only collect fish while ascending if capacity isn't full
        if (currentState != HookState.Ascending || !collision.CompareTag("Fish")) return;
        if (caughtFishes.Count >= maxFishCapacity) return;

        FishController fish = collision.GetComponent<FishController>();
        if (fish != null && !caughtFishes.Contains(fish))
        {
            fish.GetCaught(transform);
            caughtFishes.Add(fish);
            OnCatchCountChanged?.Invoke(caughtFishes.Count, maxFishCapacity);
            bool isLast = caughtFishes.Count >= maxFishCapacity;
            float duration = isLast ? freezeLastFish : freezePerFish;
            StartCoroutine(FreezeFrame(duration));
            if (isLast && endCatchSound != null && AudioManager.Instance != null)
                AudioManager.Instance.PlaySFX(endCatchSound, volume: 1f, pitchMin: 0.85f, pitchMax: 1.15f);

        }

    }

    private void ProcessStateMachine()
    {
        switch (currentState)
        {
            case HookState.Idle:
                rb2d.velocity = Vector2.zero;
                break;

            case HookState.Descending:
                HandleMovement(-descentSpeed);
                CheckDepthLimit();
                break;

            case HookState.Ascending:
                // Speed up ascent if basket is completely saturated
                bool isFull = caughtFishes.Count >= maxFishCapacity;
                float speed = isFull ? rapidAscentSpeed : ascentSpeed;

                HandleMovement(speed);
                CheckSurfaceLimit();
                break;
        }
    }

    private void StartDescending()
    {
        currentState = HookState.Descending;
        if (spriteRenderer != null) spriteRenderer.enabled = true; // Se enciende al lanzar
    }
    private void HandleMovement(float verticalSpeed)
    {
        float targetVelocityX = 0f;

        if (Input.GetMouseButton(0))
        {
            Vector3 mousePos = Input.mousePosition;
            mousePos.z = 10f;

            Vector3 targetWorldPos = mainCamera.ScreenToWorldPoint(mousePos);
            float differenceX = targetWorldPos.x - rb2d.position.x;
            targetVelocityX = differenceX * horizontalResponsiveness;
        }

        rb2d.velocity = new Vector2(targetVelocityX, verticalSpeed);
    }

    private void CheckDepthLimit()
    {
        if (rb2d.position.y <= maxDepth) currentState = HookState.Ascending;
    }

    private void CheckSurfaceLimit()
    {
        if (rb2d.position.y < startYPosition) return;

        rb2d.position = new Vector2(0f, startYPosition);
        currentState = HookState.Idle;
        rb2d.velocity = Vector2.zero;

        if (spriteRenderer != null) spriteRenderer.enabled = false;

        OnDepthChanged?.Invoke(0f);

        if (caughtFishes.Count > 0)
        {
            OnCatchReady?.Invoke(new List<FishController>(caughtFishes));
        }
        else
        {
            FinalizeCast();
        }
    }

    public void FinalizeCast()
    {
        ProcessCatchRewards();
        OnCatchCountChanged?.Invoke(0, maxFishCapacity);
        OnReturnToSurface?.Invoke();
    }

    private void ProcessCatchRewards()
    {
        if (caughtFishes.Count <= 0) return;

        int totalCoinsThisCast = 0;
        foreach (FishController fish in caughtFishes)
        {
            if (fish != null && fish.data != null) totalCoinsThisCast += fish.data.price;
        }

        if (SavesManager.Instance != null) SavesManager.Instance.AddCoins(totalCoinsThisCast);
        caughtFishes.Clear();
    }

    private void UpdateDepthUI()
    {
        if (currentState != HookState.Idle)
        {
            float currentDepth = Mathf.Abs(rb2d.position.y - startYPosition);
            OnDepthChanged?.Invoke(currentDepth);
        }
    }

    private void LoadStatsFromSave()
    {
        if (SavesManager.Instance?.currentData != null)
        {
            maxDepth = SavesManager.Instance.currentData.maxDepth;
            maxFishCapacity = SavesManager.Instance.currentData.maxCatch;
        }
    }

    public void RefreshStatsFromSave()
    {
        LoadStatsFromSave();
        OnCatchCountChanged?.Invoke(caughtFishes.Count, maxFishCapacity);
        //OnDepthChanged?.Invoke(Mathf.Abs(rb2d.position.y - startYPosition));
    }

    private void UpdateHookRotation()
    {
        if (currentState == HookState.Idle)
        {
            float neutralAngle = Mathf.LerpAngle(rb2d.rotation, 0f, Time.fixedDeltaTime * tiltSpeed);
            rb2d.MoveRotation(neutralAngle);
            return;
        }

        // Apply visual swing sway based on horizontal speed
        float normalizedX = Mathf.Clamp(rb2d.velocity.x / descentSpeed, -1f, 1f);
        float targetAngle = -normalizedX * maxTiltAngle;

        // Add an extra kick back on the pitch angle if ascending
        if (currentState == HookState.Ascending)
        {
            float verticalBias = 8f;
            float directionalSign = Mathf.Sign(targetAngle == 0f ? 0f : targetAngle);
            targetAngle += verticalBias * directionalSign;
        }

        float smoothAngle = Mathf.LerpAngle(rb2d.rotation, targetAngle, Time.fixedDeltaTime * tiltSpeed);
        rb2d.MoveRotation(smoothAngle);
    }

    private IEnumerator FreezeFrame(float duration)
    {
        Time.timeScale = 0f;
        yield return new WaitForSecondsRealtime(duration);
        Time.timeScale = 1f;
    }
}