using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class PlayerController : MonoBehaviour
{
    [Header("Movement")]
    public float moveSpeed = 8f;
    public float fixedY = 0.5f;

    [Header("Fan Shockwave")]
    public float shockwaveRadius = 4f;
    public float shockwaveForce = 12f;
    public float shockwaveCooldown = 1.5f;
    [Range(10f, 180f)] public float shockwaveAngle = 70f;
    public KeyCode shockwaveKey = KeyCode.Space;

    [Header("Arrow")]
    public GameObject arrowPrefab;
    public Transform firePoint;
    public float arrowCooldown = 2.2f;

    [Header("Special Burst")]
    public float burstRadius = 5f;
    public float burstForce = 18f;
    public float burstCooldown = 6f;
    public KeyCode burstKey = KeyCode.F;

    [Header("Arena Clamp")]
    public float minX = -8.5f;
    public float maxX = 8.5f;
    public float minZ = -4.5f;
    public float maxZ = 4.5f;

    [Header("Fan Visual")]
    public GameObject shockwaveFanVisualPrefab;

    private Rigidbody rb;
    private Vector3 moveInput;

    private float shockwaveTimer;
    private float arrowTimer;
    private float burstTimer;

    public float ShockwaveTimer => shockwaveTimer;
    public float ArrowTimer => arrowTimer;
    public float BurstTimer => burstTimer;

    public float ShockwaveCooldown => shockwaveCooldown;
    public float ArrowCooldown => arrowCooldown;
    public float BurstCooldown => burstCooldown;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
    }

    private void Update()
    {
        if (GameManager.Instance != null && GameManager.Instance.IsGameOver)
            return;

        float x = Input.GetAxisRaw("Horizontal");
        float z = Input.GetAxisRaw("Vertical");
        moveInput = new Vector3(x, 0f, z).normalized;

        if (shockwaveTimer > 0f) shockwaveTimer -= Time.deltaTime;
        if (arrowTimer > 0f) arrowTimer -= Time.deltaTime;
        if (burstTimer > 0f) burstTimer -= Time.deltaTime;

        if (Input.GetKeyDown(shockwaveKey) && shockwaveTimer <= 0f)
        {
            UseShockwave();
        }

        if (Input.GetMouseButtonDown(0) && arrowTimer <= 0f)
        {
            ShootArrow();
        }

        if (Input.GetKeyDown(burstKey) && burstTimer <= 0f)
        {
            UseRadialBurst();
        }

        if (GameManager.Instance != null && GameManager.Instance.uiManager != null)
        {
            GameManager.Instance.uiManager.UpdateCooldowns(this);
        }
    }

    private void FixedUpdate()
    {
        Vector3 newPosition = rb.position + moveInput * moveSpeed * Time.fixedDeltaTime;
        newPosition.y = fixedY;
        newPosition.x = Mathf.Clamp(newPosition.x, minX, maxX);
        newPosition.z = Mathf.Clamp(newPosition.z, minZ, maxZ);

        rb.MovePosition(newPosition);

        Vector3 pos = transform.position;
        pos.y = fixedY;
        transform.position = pos;
    }

    private void UseShockwave()
    {
        shockwaveTimer = shockwaveCooldown;

        Vector3 forward = GetMouseWorldDirection();
        if (forward == Vector3.zero)
        {
            forward = transform.forward;
        }

        forward.y = 0f;
        forward.Normalize();

        if (forward != Vector3.zero)
        {
            transform.rotation = Quaternion.LookRotation(forward, Vector3.up);
        }

        if (shockwaveFanVisualPrefab != null)
        {
            GameObject visual = Instantiate(shockwaveFanVisualPrefab, transform.position, Quaternion.identity);
            ShockwaveFanVisual fan = visual.GetComponent<ShockwaveFanVisual>();
            if (fan != null)
            {
                fan.Play(forward, shockwaveRadius, shockwaveAngle, fixedY);
            }
        }

        Collider[] hits = Physics.OverlapSphere(transform.position, shockwaveRadius);

        foreach (Collider hit in hits)
        {
            Bubble bubble = hit.GetComponent<Bubble>();
            if (bubble == null) continue;

            Vector3 toBubble = bubble.transform.position - transform.position;
            toBubble.y = 0f;

            float distance = toBubble.magnitude;
            if (distance <= 0.001f) continue;

            Vector3 dirToBubble = toBubble.normalized;
            float angleToBubble = Vector3.Angle(forward, dirToBubble);

            if (angleToBubble <= shockwaveAngle * 0.5f)
            {
                float distanceFactor = 1f - Mathf.Clamp01(distance / shockwaveRadius);
                float finalForce = shockwaveForce * Mathf.Lerp(0.4f, 1f, distanceFactor);
                bubble.ApplyPush(dirToBubble * finalForce);
            }
        }
    }

    private void UseRadialBurst()
    {
        burstTimer = burstCooldown;

        Collider[] hits = Physics.OverlapSphere(transform.position, burstRadius);

        foreach (Collider hit in hits)
        {
            Bubble bubble = hit.GetComponent<Bubble>();
            if (bubble == null) continue;

            Vector3 dir = bubble.transform.position - transform.position;
            dir.y = 0f;

            float distance = dir.magnitude;
            if (distance <= 0.001f) continue;

            dir.Normalize();

            float distanceFactor = 1f - Mathf.Clamp01(distance / burstRadius);
            float finalForce = burstForce * Mathf.Lerp(0.5f, 1f, distanceFactor);

            bubble.ApplyPush(dir * finalForce);
        }
    }

    private void ShootArrow()
    {
        if (arrowPrefab == null || firePoint == null || Camera.main == null) return;

        arrowTimer = arrowCooldown;

        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        Plane groundPlane = new Plane(Vector3.up, new Vector3(0f, fixedY, 0f));

        if (groundPlane.Raycast(ray, out float enter))
        {
            Vector3 targetPoint = ray.GetPoint(enter);
            Vector3 direction = targetPoint - firePoint.position;
            direction.y = 0f;
            direction.Normalize();

            GameObject arrow = Instantiate(arrowPrefab, firePoint.position, Quaternion.identity);

            Collider arrowCollider = arrow.GetComponent<Collider>();
            Collider playerCollider = GetComponent<Collider>();

            if (arrowCollider != null && playerCollider != null)
            {
                Physics.IgnoreCollision(arrowCollider, playerCollider);
            }

            ArrowProjectile projectile = arrow.GetComponent<ArrowProjectile>();

            if (projectile != null)
            {
                projectile.Initialize(direction, fixedY);
            }

            if (direction != Vector3.zero)
            {
                arrow.transform.rotation = Quaternion.LookRotation(direction, Vector3.up);
            }
        }
    }

    private Vector3 GetMouseWorldDirection()
    {
        if (Camera.main == null) return Vector3.zero;

        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        Plane groundPlane = new Plane(Vector3.up, new Vector3(0f, fixedY, 0f));

        if (groundPlane.Raycast(ray, out float enter))
        {
            Vector3 worldPoint = ray.GetPoint(enter);
            Vector3 direction = worldPoint - transform.position;
            direction.y = 0f;
            return direction.normalized;
        }

        return Vector3.zero;
    }
}