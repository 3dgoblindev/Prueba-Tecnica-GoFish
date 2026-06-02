using System;
using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;

public class PlayerController : MonoBehaviour
{
    [Header("Dependencies")]
    [SerializeField] private Animator animator;

    [Header("Juice & Feel")]
    [SerializeField] private MiniTweenFeel castFeel;

    [Header("Charge — Zoom 2D")]
    [Tooltip("La cámara principal (2D orthographic).")]
    [SerializeField] private Camera cam;

    [Tooltip("Cuánto se reduce orthographicSize al cargar (zoom in).")]
    [SerializeField] private float zoomAmount = 0.8f;

    [Tooltip("Velocidad del lerp de zoom.")]
    [SerializeField] private float zoomSpeed = 10f;

    [Header("Freeze Frame")]
    [Tooltip("Segundos que se congela en el peak del lanzamiento.")]
    [SerializeField] private float freezeDuration = 0.10f;

    // ── State ────────────────────────────────────────────────────────────────
    private bool isFishing = false;
    private bool isCharging = false;
    private float baseOrtho;

    // ── Animator hashes ──────────────────────────────────────────────────────
    private static readonly int ThrowHash = Animator.StringToHash("Throw");
    private static readonly int RecoilHash = Animator.StringToHash("Recoil");

    // ── Events ───────────────────────────────────────────────────────────────
    public static event Action OnCastCompleted;

    // ─────────────────────────────────────────────────────────────────────────
    private void OnEnable() => HookController.OnReturnToSurface += PlayRecoilAnimation;
    private void OnDisable() => HookController.OnReturnToSurface -= PlayRecoilAnimation;

    private void Start()
    {
        if (animator == null) animator = GetComponent<Animator>();

        if (cam == null) cam = Camera.main;
        if (cam != null) baseOrtho = cam.orthographicSize;
        else Debug.LogError("[PC] No se encontró cámara — asígnala en el Inspector");
    }

    private void Update()
    {
        if (IsPointerOverUI()) return;

        if (Input.GetMouseButtonDown(0) && !isFishing)
            StartCharge();

        if (isCharging)
        {
            TickZoom();

            if (Input.GetMouseButtonUp(0))
                CommitCast();
        }

        // Zoom out suave si por algún motivo se interrumpe sin CommitCast
        if (!isCharging && cam != null)
        {
            cam.orthographicSize = Mathf.Lerp(
                cam.orthographicSize, baseOrtho, Time.deltaTime * zoomSpeed);
        }
    }

    // ── Charge ───────────────────────────────────────────────────────────────

    private void StartCharge()
    {
        isCharging = true;
        Debug.Log("[PC] ✓ isCharging = true");
    }

    private void TickZoom()
    {
        if (cam == null) return;
        float goal = baseOrtho - zoomAmount;
        cam.orthographicSize = Mathf.Lerp(
            cam.orthographicSize, goal, Time.deltaTime * zoomSpeed);
    }

    // ── Commit ───────────────────────────────────────────────────────────────

    private void CommitCast()
    {
        isCharging = false;
        isFishing = true;
        Debug.Log("[PC] CommitCast → lanzando animación");

        // Zoom out inmediato al soltar (snap feel)
        StartCoroutine(ZoomOut());

        // Lanzar animación ya — el freeze llega en OnCastFinished (Animation Event)
        if (animator != null)
        {
            animator.ResetTrigger(RecoilHash);
            animator.SetTrigger(ThrowHash);
        }
        else Debug.LogError("[PC] animator es NULL");
    }

    private IEnumerator ZoomOut()
    {
        if (cam == null) yield break;
        while (Mathf.Abs(cam.orthographicSize - baseOrtho) > 0.01f)
        {
            cam.orthographicSize = Mathf.Lerp(
                cam.orthographicSize, baseOrtho, Time.deltaTime * (zoomSpeed * 2f));
            yield return null;
        }
        cam.orthographicSize = baseOrtho;
    }

    // ── Animation Events ─────────────────────────────────────────────────────

    /// <summary>
    /// Animation Event — frame del peak del lanzamiento (caña extendida).
    /// Aquí va el freeze: el jugador ya VE la caña estirada → impacto máximo.
    /// </summary>
    public void OnCastFinished()
    {
        Debug.Log("[PC] OnCastFinished → freeze frame + broadcast");
        castFeel?.Play();
        StartCoroutine(FreezeFrame());
    }

    private IEnumerator FreezeFrame()
    {
        Time.timeScale = 0f;
        yield return new WaitForSecondsRealtime(freezeDuration);
        Time.timeScale = 1f;

        // Broadcast DESPUÉS del freeze — el hook empieza a moverse
        // cuando el tiempo ya está restaurado
        OnCastCompleted?.Invoke();
    }

    /// <summary>Animation Event — fin del clip de recoil.</summary>
    public void OnFishingSequenceEnded()
    {
        Debug.Log("[PC] OnFishingSequenceEnded → isFishing = false");
        castFeel?.Play();
        isFishing = false;
    }

    // ── Recoil ───────────────────────────────────────────────────────────────

    private void PlayRecoilAnimation()
    {
        if (animator == null) return;
        animator.ResetTrigger(ThrowHash);
        animator.SetTrigger(RecoilHash);
    }

    // ── UI Guard ─────────────────────────────────────────────────────────────

    private bool IsPointerOverUI()
    {
        if (EventSystem.current == null) return false;

        if (Input.touchCount > 0)
        {
            Touch touch = Input.GetTouch(0);
            if (touch.phase == TouchPhase.Began)
                return EventSystem.current.IsPointerOverGameObject(touch.fingerId);
            return false;
        }

        return EventSystem.current.IsPointerOverGameObject();
    }
}