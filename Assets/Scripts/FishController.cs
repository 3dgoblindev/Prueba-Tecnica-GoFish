using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class FishController : MonoBehaviour
{
    public FishData data;

    [Header("Juice")]
    [SerializeField] private MiniTweenFeel catchFeel;

    private float moveDirection = 1f;
    private float currentSwimSpeed;
    private Rigidbody2D rb;
    private Collider2D myCollider;
    private bool isCaught = false;
    private float turnCooldownTimer = 0f;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        myCollider = GetComponent<Collider2D>();
    }

    public void ResetForSpawn(float startX, float startY, Transform spawnerTransform)
    {
        isCaught = false;

        if (myCollider != null) myCollider.enabled = true;
        if (rb != null) rb.isKinematic = false;

        transform.SetParent(spawnerTransform);
        transform.position = new Vector3(startX, startY, transform.position.z + Random.Range(-1f, 1f)); //random z for layering
    }

    public void InitializeMovement(float direction, float speed)
    {
        moveDirection = Mathf.Sign(direction);
        currentSwimSpeed = speed;
        UpdateFacingDirection();
    }

    private void FixedUpdate()
    {
        if (isCaught) return;

        if (turnCooldownTimer > 0f)
        {
            turnCooldownTimer -= Time.fixedDeltaTime;
        }

        rb.velocity = new Vector2(currentSwimSpeed * moveDirection, rb.velocity.y);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (isCaught || turnCooldownTimer > 0f) return;

        if (collision.CompareTag("Boundary"))
        {
            moveDirection *= -1f;
            UpdateFacingDirection();
            turnCooldownTimer = 0.5f; // Prevent rapid back-to-back turning
        }
    }

    private void UpdateFacingDirection()
    {
        // Flip sprite via Y rotation based on move direction
        transform.rotation = Quaternion.Euler(0f, moveDirection > 0f ? 0f : 180f, 0f);
    }

    public void GetCaught(Transform hookTransform)
    {
        isCaught = true;

        FishSpawner spawner = GetComponentInParent<FishSpawner>();
        if (spawner != null)
        {
            spawner.RemoveActiveFish(this); 
        }

        if (catchFeel != null) catchFeel.Play();
        if (data != null && data.catchParticlesPrefab != null)
        {
            Instantiate(data.catchParticlesPrefab, transform.position, Quaternion.identity);
        }

        if (data != null && AudioManager.Instance != null)
        {
            AudioManager.Instance.PlaySFX(data.catchSound, volume: 1f, pitchMin: 0.85f, pitchMax: 1.15f);
        }

        // Disable physics and attach to the hook
        if (rb != null)
        {
            rb.velocity = Vector2.zero;
            rb.isKinematic = true;
        }
        if (myCollider != null) myCollider.enabled = false;

        transform.SetParent(hookTransform);
        transform.localPosition = new Vector3(0,0, Random.Range(-1f, 1f)); //random z for layering

        // Angle the fish slightly downward while hanging on the hook
        float angleOffset = Random.Range(-30f, 30f);
        transform.localRotation = Quaternion.Euler(0f, 0f, 90f + angleOffset);
    }

    public bool IsCaught()
    {
        return isCaught;
    }
}