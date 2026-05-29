using System;
using UnityEngine;
using UnityEngine.EventSystems;

/// <summary>
/// Handles player input and initiates the fishing sequence.
/// Acts as the main broadcaster for the casting event, remaining decoupled from other systems.
/// </summary>
public class PlayerController : MonoBehaviour
{
    [Header("Dependencies")]
    [Tooltip("Animator handling the fisherman's casting animation.")]
    [SerializeField] private Animator animator;

    // State lock to prevent input spam and animation buffering during the fishing sequence
    private bool isFishing = false;

    // Cache the hashes for performance instead of using string lookups every frame
    private static readonly int ThrowHash = Animator.StringToHash("Throw");
    private static readonly int RecoilHash = Animator.StringToHash("Recoil");

    // Static event using native C# Action for performance and strict typing. 
    // Listeners will subscribe to this to trigger their specific behaviors.
    public static event Action OnCastCompleted;

    private void OnEnable()
    {
        HookController.OnReturnToSurface += PlayRecoilAnimation;
    }

    private void OnDisable()
    {
        HookController.OnReturnToSurface -= PlayRecoilAnimation;
    }

    private void Start()
    {
        if (animator == null)
        {
            animator = GetComponent<Animator>();
        }
    }

    private void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            if (IsPointerOverUI())
            {
                return;
            }

            if (!isFishing)
            {
                ThrowHook();
            }
        }
    }

    private void ThrowHook()
    {
        if (animator != null)
        {
            // Lock the input state immediately upon casting
            isFishing = true;

            // Resetting triggers prevents the Animator from queuing up accidental extra inputs
            animator.ResetTrigger(RecoilHash);
            animator.SetTrigger(ThrowHash);
        }
    }

    private void PlayRecoilAnimation()
    {
        if (animator != null)
        {
            // Reset the throw trigger just in case an edge-case input bypassed the lock
            animator.ResetTrigger(ThrowHash);
            animator.SetTrigger(RecoilHash);
        }
    }

    /// <summary>
    /// Triggered via Animation Event at the exact frame the casting animation finishes extending.
    /// </summary>
    public void OnCastFinished()
    {
        // Safe invocation: checks if there are any active subscribers before broadcasting
        OnCastCompleted?.Invoke();
    }

    /// <summary>
    /// Triggered via Animation Event at the end of the recoil animation.
    /// Unlocks the state machine, allowing the player to cast the line again.
    /// </summary>
    public void OnFishingSequenceEnded()
    {
        isFishing = false;
    }

    private bool IsPointerOverUI()
    {
        // Medida de seguridad
        if (EventSystem.current == null) return false;

        // 1. Comprobar clic de ratón (PC / Editor)
        if (EventSystem.current.IsPointerOverGameObject())
            return true;

        // 2. Comprobar toques en pantalla (Móvil)
        if (Input.touchCount > 0)
        {
            Touch touch = Input.GetTouch(0);
            if (touch.phase == TouchPhase.Began)
            {
                // Le pasamos el ID del dedo al EventSystem
                if (EventSystem.current.IsPointerOverGameObject(touch.fingerId))
                    return true;
            }
        }

        return false;
    }
}