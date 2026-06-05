using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CatchRewardPresenter : MonoBehaviour
{
    [Header("Referencias")]
    [SerializeField] private Transform stageCenterWorld;
    [SerializeField] private Transform playerTransform;
    [SerializeField] private RectTransform coinUITarget;

    [Header("Fase 1 – Pequeño/rotado → Grande/recto en centro")]
    [SerializeField] private float flyToCenterDuration = 0.5f;
    [SerializeField] private float scaleMultiplier = 2.5f;
    [SerializeField] private float holdDuration = 0.4f;

    [Header("Fase 2 – Grande en centro → Pequeño en jugador")]
    [SerializeField] private float flyToPlayerDuration = 0.4f;

    [Header("Stagger")]
    [SerializeField] private float delayBetweenFish = 0.15f;

    [Header("Stagger Speed Scaling")]
    [SerializeField] private float minFishDuration = 0.15f;
    [SerializeField] private float speedIncreasePerFish = 0.15f;

    [Header("FX")]
    [SerializeField] private CoinFlyEffect coinFlyPrefab;
    [SerializeField] private Canvas rootCanvas;

    [Header("Audio")]
    [SerializeField] private AudioClip presentSound;
    [SerializeField] private float presentPitchBase = 0.9f;
    [SerializeField] private float presentPitchStep = 0.08f; // cuánto sube cada pez
    [SerializeField] private float presentPitchMax = 1.8f;

    private HookController hookController;

    private void OnEnable() => HookController.OnCatchReady += HandleCatchReady;
    private void OnDisable() => HookController.OnCatchReady -= HandleCatchReady;
    private void Start() => hookController = FindObjectOfType<HookController>();

    private void HandleCatchReady(List<FishController> fishes)
        => StartCoroutine(PresentAllFish(fishes));

    private IEnumerator PresentAllFish(List<FishController> fishes)
    {
        for (int i = 0; i < fishes.Count; i++)
        {
            if (fishes[i] != null)
            {
                float speedMultiplier = 1f / (1f + i * speedIncreasePerFish);
                float pitch = Mathf.Min(presentPitchBase + i * presentPitchStep, presentPitchMax);
                yield return StartCoroutine(PresentOneFish(fishes[i], speedMultiplier, pitch));
            }

            if (i < fishes.Count - 1)
                yield return new WaitForSeconds(delayBetweenFish * (1f / (1f + i * speedIncreasePerFish)));
        }

        hookController?.FinalizeCast();
    }

    private IEnumerator PresentOneFish(FishController fish, float speedMultiplier = 1f, float pitch = 1f)
    {
        float actualFlyToCenter = Mathf.Max(minFishDuration, flyToCenterDuration * speedMultiplier);
        float actualHold = Mathf.Max(minFishDuration * 0.5f, holdDuration * speedMultiplier);
        float actualFlyToPlayer = Mathf.Max(minFishDuration, flyToPlayerDuration * speedMultiplier);

        Transform fishT = fish.transform;
        fishT.SetParent(null);

        Vector3 fromPos = fishT.position;
        Vector3 fromScale = fishT.localScale;
        Quaternion fromRot = fishT.rotation;

        Vector3 centerPos = stageCenterWorld.position;
        Vector3 bigScale = fromScale * scaleMultiplier;

        // ── Fase 1: desde caña (pequeño, rotado) → centro (grande, recto) ─────
        float elapsed = 0f;
        while (elapsed < actualFlyToCenter)
        {
            elapsed += Time.deltaTime;
            float e = EaseOutCubic(Mathf.Clamp01(elapsed / actualFlyToCenter));

            fishT.position = Vector3.Lerp(fromPos, centerPos, e);
            fishT.localScale = Vector3.Lerp(fromScale, bigScale, e);
            fishT.rotation = Quaternion.Slerp(fromRot, Quaternion.identity, e);

            yield return null;
        }

        fishT.position = centerPos;
        fishT.localScale = bigScale;
        fishT.rotation = Quaternion.identity;

        AudioManager.Instance.PlaySFX(presentSound, volume: 1f, pitch: pitch);


        SpawnCoinFly(fish, centerPos);

        yield return new WaitForSeconds(actualHold);

        // ── Fase 2: desde centro (grande, recto) → jugador (cero, desaparece) ──
        Vector3 playerPos = playerTransform.position;
        elapsed = 0f;
        while (elapsed < actualFlyToPlayer)
        {
            elapsed += Time.deltaTime;
            float e = EaseInQuad(Mathf.Clamp01(elapsed / actualFlyToPlayer));

            fishT.position = Vector3.Lerp(centerPos, playerPos, e);
            fishT.localScale = Vector3.Lerp(bigScale, Vector3.zero, e);

            yield return null;
        }

        SpawnParticles(fish, playerTransform.position);
        AudioManager.Instance.PlaySFX(fish.data.catchSound, volume: 1f, pitchMin: 0.85f, pitchMax: 1.15f);

        fishT.gameObject.SetActive(false);
    }

    // ── Eases ─────────────────────────────────────────────────────────────────

    private static float EaseOutCubic(float t) => 1f - Mathf.Pow(1f - t, 3f);
    private static float EaseInQuad(float t) => t * t;

    // ── FX ───────────────────────────────────────────────────────────────────

    private void SpawnParticles(FishController fish, Vector3 worldPos)
    {
        GameObject catchParticlesPrefab = fish.data?.catchParticlesPrefab;
        if (catchParticlesPrefab == null) return;
        Instantiate(catchParticlesPrefab, worldPos, Quaternion.identity);
    }

    private void SpawnCoinFly(FishController fish, Vector3 worldPos)
    {
        if (coinFlyPrefab == null || coinUITarget == null || fish.data == null) return;

        var coin = Instantiate(coinFlyPrefab, rootCanvas.transform);
        coin.Init(rootCanvas);
        coin.Fly(worldPos, coinUITarget, fish.data.price);
    }
}