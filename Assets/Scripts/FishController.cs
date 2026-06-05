using UnityEngine;

/// <summary>
/// Controls the autonomous horizontal movement for a specific fish prefab.
/// </summary>
[RequireComponent(typeof(Rigidbody2D))]
public class FishController : MonoBehaviour
{
    [Header("Configuration")]
    [Tooltip("This is assigned automatically by the Spawner, or you can set it in the Prefab.")]
    public FishData data;

    [Header("Juice & Feel")]
    [SerializeField] private MiniTweenFeel catchFeel;

    [SerializeField] private float moveDirection = 1f;

    private float currentSwimSpeed;
    private Rigidbody2D rb;
    private bool isCaught = false;
    private float turnCooldownTimer = 0f;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    /// <summary>
    /// Resets the physical state of the fish when it gets pulled from the Object Pool.
    /// </summary>
    public void ResetForSpawn(float startX, float startY, Transform spawnerTransform)
    {
        isCaught = false;

        // Restore physical properties in case it was previously caught
        GetComponent<Collider2D>().enabled = true;
        rb.isKinematic = false;

        transform.SetParent(spawnerTransform);

        // Reset position aplicando la X y la Y nuevas
        transform.position = new Vector3(startX, startY, transform.position.z);
    }

    /// <summary>
    /// Configures the fish's initial movement parameters.
    /// </summary>
    public void InitializeMovement(float direction, float speed)
    {
        moveDirection = Mathf.Sign(direction);
        currentSwimSpeed = speed;
        UpdateFacingDirection();
    }

    private void FixedUpdate()
    {
        if (isCaught) return;

        if (turnCooldownTimer > 0)
        {
            turnCooldownTimer -= Time.fixedDeltaTime;
        }

        HandleSwimming();
    }

    private void HandleSwimming()
    {
        rb.velocity = new Vector2(currentSwimSpeed * moveDirection, rb.velocity.y);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (isCaught || turnCooldownTimer > 0f) return;

        if (collision.CompareTag("Boundary"))
        {
            TurnAround();
        }
    }

    private void TurnAround()
    {
        moveDirection *= -1f;
        UpdateFacingDirection();
        turnCooldownTimer = 0.5f;
    }

    private void UpdateFacingDirection()
    {
        if (moveDirection > 0)
        {
            transform.rotation = Quaternion.Euler(0f, 0f, 0f);
        }
        else
        {
            transform.rotation = Quaternion.Euler(0f, 180f, 0f);
        }
    }

    public void GetCaught(Transform hookTransform)
    {
        isCaught = true;

        catchFeel.Play();
        Instantiate(data.catchParticlesPrefab, transform.position, Quaternion.identity);

        AudioManager.Instance.PlaySFX(data.catchSound, volume: 1f, pitchMin: 0.85f, pitchMax: 1.15f);

        rb.velocity = Vector2.zero;
        rb.isKinematic = true;
        GetComponent<Collider2D>().enabled = false;

        transform.SetParent(hookTransform);
        transform.localPosition = Vector3.zero;

        float angleOffset = Random.Range(-30f, 30f);
        transform.localRotation = Quaternion.Euler(0, 0, 90f + angleOffset);
    }
}