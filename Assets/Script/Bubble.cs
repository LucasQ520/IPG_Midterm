using System.Collections;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(Collider))]
[RequireComponent(typeof(Renderer))]
public class Bubble : MonoBehaviour
{
    [Header("Runtime Type")]
    public BubbleType bubbleType = BubbleType.Normal;

    [Header("Movement")]
    public float moveSpeed = 6f;
    public float pushResistance = 1f;
    public int reactorDamage = 1;
    public int scoreValue = 10;

    [Header("Special Effect")]
    public float specialRadius = 2.5f;
    public float specialForce = 8f;

    [Header("Split")]
    public GameObject bubblePrefab;
    public int splitCount = 2;
    public float childScaleMultiplier = 0.65f;

    [Header("Plane Lock")]
    public float fixedY = 0.5f;

    [Header("Arrow Pop Delay")]
    public float arrowPopDelay = 0.12f;

    [Header("Colors")]
    public Color normalColor = new Color(0.6f, 0.9f, 1f, 0.8f);
    public Color volatileColor = new Color(1f, 0.4f, 0.4f, 0.8f);
    public Color implosionColor = new Color(0.7f, 0.4f, 1f, 0.8f);
    public Color heavyColor = new Color(0.7f, 0.7f, 0.7f, 0.9f);
    public Color splitColor = new Color(0.4f, 1f, 0.5f, 0.8f);
    public Color corruptedColor = new Color(0.2f, 0.1f, 0.1f, 0.95f);
    public Color momentumColor = new Color(1f, 0.65f, 0.2f, 0.85f);

    [Header("Audio")]
    public AudioClip popSound;
    public AudioClip containSound;
    public float soundVolume = 0.8f;

    private Rigidbody rb;
    private Renderer rend;
    private bool isPopping;
    private bool isContained;
    private bool isArrowMarked;
    private float pushedTimer = 0f;
    private Color baseColor;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        rend = GetComponent<Renderer>();
    }

    private void Start()
    {
        ApplyTypeSettings();
        LockToPlaneImmediate();
    }

    private void FixedUpdate()
    {
        if (GameManager.Instance == null || GameManager.Instance.IsGameOver || isPopping || isContained)
            return;

        if (GameManager.Instance.reactorCenter == null)
            return;

        if (pushedTimer > 0f)
        {
            pushedTimer -= Time.fixedDeltaTime;
            LockToPlane();
            return;
        }

        if (isArrowMarked)
        {
            LockToPlane();
            return;
        }

        Vector3 target = GameManager.Instance.reactorCenter.position;
        Vector3 direction = target - transform.position;
        direction.y = 0f;

        if (direction.sqrMagnitude > 0.01f)
        {
            direction.Normalize();

            Vector3 desiredVelocity = direction * moveSpeed;
            desiredVelocity.y = 0f;

            rb.linearVelocity = desiredVelocity;
        }

        LockToPlane();
    }

    public void SetType(BubbleType newType)
    {
        bubbleType = newType;
        ApplyTypeSettings();
    }

    private void ApplyTypeSettings()
    {
        transform.localScale = Vector3.one * 8f;

        switch (bubbleType)
        {
            case BubbleType.Normal:
                moveSpeed = 7f;
                pushResistance = 1f;
                reactorDamage = 1;
                scoreValue = 10;
                rb.linearDamping = 2.2f;
                SetColor(normalColor);
                break;

            case BubbleType.Volatile:
                moveSpeed = 8f;
                pushResistance = 0.95f;
                reactorDamage = 1;
                scoreValue = 15;
                specialRadius = 100f;
                specialForce = 800f;
                rb.linearDamping = 2.0f;
                SetColor(volatileColor);
                break;

            case BubbleType.Implosion:
                moveSpeed = 7f;
                pushResistance = 1f;
                reactorDamage = 1;
                scoreValue = 20;
                specialRadius = 100f;
                specialForce = 800f;
                rb.linearDamping = 2.4f;
                SetColor(implosionColor);
                break;

            case BubbleType.Heavy:
                moveSpeed = 4f;
                pushResistance = 4f;
                reactorDamage = 1;
                scoreValue = 20;
                rb.linearDamping = 3f;
                transform.localScale = Vector3.one * 12f;
                SetColor(heavyColor);
                break;

            case BubbleType.Split:
                moveSpeed = 6f;
                pushResistance = 1f;
                reactorDamage = 1;
                scoreValue = 18;
                rb.linearDamping = 2.2f;
                SetColor(splitColor);
                break;

            case BubbleType.Corrupted:
                moveSpeed = 14f;
                pushResistance = 0.5f;
                reactorDamage = 3;
                scoreValue = 30;
                rb.linearDamping = 1.8f;
                SetColor(corruptedColor);
                break;

            case BubbleType.Momentum:
                moveSpeed = 7f;
                pushResistance = 0.85f;
                reactorDamage = 1;
                scoreValue = 20;
                rb.linearDamping = 0.8f;
                SetColor(momentumColor);
                break;
        }
    }

    private void SetColor(Color color)
    {
        baseColor = color;
        if (rend != null && rend.material != null)
        {
            rend.material.color = color;
        }
    }

    public void ApplyPush(Vector3 force)
    {
        if (isPopping || isContained) return;

        force.y = 0f;

        float resistance = Mathf.Max(0.1f, pushResistance);

        rb.linearVelocity = Vector3.zero;
        rb.AddForce(force / resistance, ForceMode.Impulse);

        pushedTimer = 0.45f;
    }

    public void Pop(bool giveScore = true)
    {
        if (isPopping || isContained || isArrowMarked) return;
        StartCoroutine(PopRoutine(giveScore));
    }

    public void HitByArrow()
    {
        if (isPopping || isContained || isArrowMarked) return;
        StartCoroutine(ArrowPopRoutine());
    }

    public void Contain()
    {
        if (isPopping || isContained || isArrowMarked) return;
        StartCoroutine(ContainRoutine());
    }

    private IEnumerator ArrowPopRoutine()
    {
        isArrowMarked = true;

        rb.linearVelocity = Vector3.zero;

        float t = 0f;
        float duration = arrowPopDelay;

        Vector3 startScale = transform.localScale;
        Vector3 flashScale = startScale * 1.12f;

        while (t < duration)
        {
            t += Time.unscaledDeltaTime;
            float p = Mathf.Clamp01(t / duration);

            transform.localScale = Vector3.Lerp(startScale, flashScale, p);

            if (rend != null && rend.material != null)
            {
                rend.material.color = Color.Lerp(baseColor, Color.white, p);
            }

            yield return null;
        }

        if (rend != null && rend.material != null)
        {
            rend.material.color = baseColor;
        }

        isArrowMarked = false;
        Pop(false);
    }

    private IEnumerator ContainRoutine()
    {
        isContained = true;

        PlaySound(containSound);

        rb.linearVelocity = Vector3.zero;
        rb.isKinematic = true;

        Vector3 startScale = transform.localScale;
        float t = 0f;
        float duration = 0.18f;

        while (t < duration)
        {
            t += Time.unscaledDeltaTime;
            float p = t / duration;
            transform.localScale = Vector3.Lerp(startScale, Vector3.zero, p);
            yield return null;
        }

        if (GameManager.Instance != null)
        {
            GameManager.Instance.AddScore(scoreValue);
        }

        Destroy(gameObject, 0.2f);
    }

    private IEnumerator PopRoutine(bool giveScore)
    {
        isPopping = true;

        PlaySound(popSound);

        if (giveScore && GameManager.Instance != null)
        {
            GameManager.Instance.AddScore(scoreValue);
        }

        TriggerSpecialEffect();

        rb.linearVelocity = Vector3.zero;
        rb.isKinematic = true;

        Vector3 startScale = transform.localScale;
        float t = 0f;
        float duration = 0.12f;

        while (t < duration)
        {
            t += Time.unscaledDeltaTime;
            float p = t / duration;
            transform.localScale = Vector3.Lerp(startScale, Vector3.zero, p);
            yield return null;
        }

        Destroy(gameObject, 0.2f);
    }

    private void PlaySound(AudioClip clip)
    {
        if (clip != null)
        {
            AudioSource.PlayClipAtPoint(clip, transform.position, soundVolume);
        }
    }
    private void TriggerSpecialEffect()
    {
        Collider[] hits;

        switch (bubbleType)
        {
            case BubbleType.Volatile:
                hits = Physics.OverlapSphere(transform.position, specialRadius);
                foreach (Collider hit in hits)
                {
                    Bubble other = hit.GetComponent<Bubble>();
                    if (other != null && other != this)
                    {
                        Vector3 dir = other.transform.position - transform.position;
                        dir.y = 0f;

                        if (dir.sqrMagnitude > 0.001f)
                        {
                            dir.Normalize();
                            other.ApplyPush(dir * specialForce);
                        }
                    }
                }
                break;

            case BubbleType.Implosion:
                hits = Physics.OverlapSphere(transform.position, specialRadius);
                foreach (Collider hit in hits)
                {
                    Bubble other = hit.GetComponent<Bubble>();
                    if (other != null && other != this)
                    {
                        Vector3 dir = transform.position - other.transform.position;
                        dir.y = 0f;

                        if (dir.sqrMagnitude > 0.001f)
                        {
                            dir.Normalize();
                            other.ApplyPush(dir * specialForce);
                        }
                    }
                }
                break;

            case BubbleType.Split:
                if (bubblePrefab != null)
                {
                    for (int i = 0; i < splitCount; i++)
                    {
                        Vector3 offset = new Vector3(Random.Range(-0.3f, 0.3f), 0f, Random.Range(-0.3f, 0.3f));
                        GameObject child = Instantiate(bubblePrefab, transform.position + offset, Quaternion.identity);
                        Bubble childBubble = child.GetComponent<Bubble>();

                        if (childBubble != null)
                        {
                            childBubble.fixedY = fixedY;
                            childBubble.SetType(BubbleType.Normal);
                            childBubble.transform.localScale = transform.localScale * childScaleMultiplier;

                            Rigidbody childRb = childBubble.GetComponent<Rigidbody>();
                            if (childRb != null)
                            {
                                Vector3 randomDir = new Vector3(Random.Range(-1f, 1f), 0f, Random.Range(-1f, 1f)).normalized;
                                childRb.AddForce(randomDir * 3f, ForceMode.Impulse);
                            }
                        }
                    }
                }
                break;
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (isPopping || isContained || isArrowMarked) return;

        if (other.CompareTag("Reactor"))
        {
            if (GameManager.Instance != null)
            {
                GameManager.Instance.DamageReactor(reactorDamage);
            }

            Destroy(gameObject, 0.2f);
        }
    }

    private void LockToPlane()
    {
        Vector3 pos = transform.position;
        pos.y = fixedY;
        transform.position = pos;

        Vector3 vel = rb.linearVelocity;
        vel.y = 0f;
        rb.linearVelocity = vel;
    }

    private void LockToPlaneImmediate()
    {
        Vector3 pos = transform.position;
        pos.y = fixedY;
        transform.position = pos;
    }

    private void OnDrawGizmosSelected()
    {
        if (bubbleType == BubbleType.Volatile || bubbleType == BubbleType.Implosion)
        {
            Gizmos.color = bubbleType == BubbleType.Volatile ? Color.red : Color.magenta;
            Gizmos.DrawWireSphere(transform.position, specialRadius);
        }
    }
}