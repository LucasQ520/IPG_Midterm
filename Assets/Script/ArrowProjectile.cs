using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(Collider))]
public class ArrowProjectile : MonoBehaviour
{
    public float speed = 18f;
    public float lifeTime = 2.5f;

    private Rigidbody rb;
    private Vector3 direction;
    private float fixedY;
    private HashSet<Bubble> hitBubbles = new HashSet<Bubble>();

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
    }

    public void Initialize(Vector3 dir, float yLevel)
    {
        direction = dir.normalized;
        fixedY = yLevel;

        Vector3 velocity = direction * speed;
        velocity.y = 0f;
        rb.linearVelocity = velocity;

        Vector3 pos = transform.position;
        pos.y = fixedY;
        transform.position = pos;

        Destroy(gameObject, lifeTime);
    }

    private void FixedUpdate()
    {
        Vector3 pos = transform.position;
        pos.y = fixedY;
        transform.position = pos;

        Vector3 vel = rb.linearVelocity;
        vel.y = 0f;
        rb.linearVelocity = vel;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
            return;

        if (other.CompareTag("Reactor"))
            return;

        Bubble bubble = other.GetComponent<Bubble>();
        if (bubble != null)
        {
            if (!hitBubbles.Contains(bubble))
            {
                hitBubbles.Add(bubble);
                bubble.HitByArrow();
            }

            return;
        }

        if (!other.isTrigger)
        {
            Destroy(gameObject);
        }
    }
}