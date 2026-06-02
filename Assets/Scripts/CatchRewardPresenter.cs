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

    [Header("FX")]
    [SerializeField] private ParticleSystem catchParticlesPrefab;
    [SerializeField] private CoinFlyEffect coinFlyPrefab;
    [SerializeField] private Canvas rootCanvas;

    private HookController hookController;

    private void OnEnable() => HookController.OnCatchReady += HandleCatchReady;
    private void OnDisable() => HookController.OnCatchReady -= HandleCatchReady;
    private void Start() => hookController = FindObjectOfType<HookController>();

    private void HandleCatchReady(List<FishController> fishes)
        => StartCoroutine(PresentAllFish(fishes));

    private IEnumerator PresentAllFish(List<FishController> fishes)
    {
        float singleFishTime = flyToCenterDuration + holdDuration + flyToPlayerDuration;

        for (int i = 0; i < fishes.Count; i++)
        {
            if (fishes[i] != null)
                StartCoroutine(PresentOneFish(fishes[i]));

            if (i < fishes.Count - 1)
                yield return new WaitForSeconds(delayBetweenFish);
        }

        yield return new WaitForSeconds(singleFishTime);
        hookController?.FinalizeCast();
    }

    private IEnumerator PresentOneFish(FishController fish)
    {
        Transform fishT = fish.transform;
        fishT.SetParent(null);

        Vector3 fromPos = fishT.position;
        Vector3 fromScale = fishT.localScale;
        Quaternion fromRot = fishT.rotation;

        Vector3 centerPos = stageCenterWorld.position;
        Vector3 bigScale = fromScale * scaleMultiplier;

        // ── Fase 1: desde caña (pequeño, rotado) → centro (grande, recto) ─────
        float elapsed = 0f;
        while (elapsed < flyToCenterDuration)
        {
            elapsed += Time.deltaTime;
            float e = EaseOutCubic(Mathf.Clamp01(elapsed / flyToCenterDuration));

            fishT.position = Vector3.Lerp(fromPos, centerPos, e);
            fishT.localScale = Vector3.Lerp(fromScale, bigScale, e);
            fishT.rotation = Quaternion.Slerp(fromRot, Quaternion.identity, e);

            yield return null;
        }

        fishT.position = centerPos;
        fishT.localScale = bigScale;
        fishT.rotation = Quaternion.identity;

        SpawnParticles(centerPos);
        SpawnCoinFly(fish, centerPos);

        yield return new WaitForSeconds(holdDuration);

        // ── Fase 2: desde centro (grande, recto) → jugador (cero, desaparece) ──
        Vector3 playerPos = playerTransform.position;
        elapsed = 0f;
        while (elapsed < flyToPlayerDuration)
        {
            elapsed += Time.deltaTime;
            float e = EaseInQuad(Mathf.Clamp01(elapsed / flyToPlayerDuration));

            fishT.position = Vector3.Lerp(centerPos, playerPos, e);
            fishT.localScale = Vector3.Lerp(bigScale, Vector3.zero, e);

            yield return null;
        }

        fishT.gameObject.SetActive(false);
    }

    // ── Eases ─────────────────────────────────────────────────────────────────

    private static float EaseOutCubic(float t) => 1f - Mathf.Pow(1f - t, 3f);
    private static float EaseInQuad(float t) => t * t;

    // ── FX ───────────────────────────────────────────────────────────────────

    private void SpawnParticles(Vector3 worldPos)
    {
        if (catchParticlesPrefab == null) return;
        var ps = Instantiate(catchParticlesPrefab, worldPos, Quaternion.identity);
        Destroy(ps.gameObject, ps.main.duration + ps.main.startLifetime.constantMax);
    }

    private void SpawnCoinFly(FishController fish, Vector3 worldPos)
    {
        if (coinFlyPrefab == null || coinUITarget == null || fish.data == null) return;

        // Instanciar como hijo del Canvas, no en world space
        var coin = Instantiate(coinFlyPrefab, rootCanvas.transform);
        coin.Init(rootCanvas);
        coin.Fly(worldPos, coinUITarget, fish.data.price);
    }
}