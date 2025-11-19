using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Fireball : MonoBehaviour
{
    [Header("Fireball Settings")]
    public float speed = 20f;
    public float damage = 34f;
    public float lifetime = 5f;

    [Header("Impact Settings")]
    public GameObject impactEffect;
    public float impactEffectLifetime = 2f;
    public float explosionRadius = 0f;

    [Header("Audio")]
    public AudioClip impactSound;

    private Vector3 direction;
    private Rigidbody rb;
    private bool hasHit = false;
    private AudioSource audioSource;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        if (rb == null)
        {
            rb = gameObject.AddComponent<Rigidbody>();
        }

        rb.useGravity = false;
        rb.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;

        if (GetComponent<Collider>() == null)
        {
            SphereCollider col = gameObject.AddComponent<SphereCollider>();
            col.radius = 0.3f;
            col.isTrigger = true;
        }

        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }

        Destroy(gameObject, lifetime);
    }

    void FixedUpdate()
    {
        if (!hasHit && direction != Vector3.zero)
        {
            rb.velocity = direction * speed;
        }
    }

    public void Initialize(Vector3 shootDirection)
    {
        direction = shootDirection.normalized;

        if (direction != Vector3.zero)
        {
            transform.forward = direction;
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if (hasHit) return;

        // ✅ IGNORE ALL BARRIER TYPES
        if (other.GetComponent<LocationBarrier>() != null)
        {
            Debug.Log("Fireball ignored LocationBarrier");
            return;
        }

        if (other.GetComponent<LocationBarrier_MultiKey>() != null)
        {
            Debug.Log("Fireball ignored LocationBarrier_MultiKey");
            return;
        }

        // 🔥 NEW: Check and ignore the LocationBarrier_DefeatEnemy type
        if (other.GetComponent<LocationBarrier_DefeatEnemy>() != null)
        {
            Debug.Log("Fireball ignored LocationBarrier_DefeatEnemy (Barrier 3 type)");
            return;
        }

        // Don't hit the player who shot it
        if (other.CompareTag("Player"))
        {
            return;
        }

        Debug.Log($"🔥 Fireball hit: {other.gameObject.name} (Tag: {other.tag})");

        // ✅ FIXED: Single, clean damage application for ANY enemy
        if (other.CompareTag("Enemy"))
        {
            // Try EnemyHealth component first (universal approach)
            EnemyHealth enemyHealth = other.GetComponent<EnemyHealth>();

            if (enemyHealth != null)
            {
                // Enemy has health system - use it
                Debug.Log($"💥 Dealing {damage} damage to {other.gameObject.name} via EnemyHealth");
                enemyHealth.TakeDamage(damage);
            }
            else
            {
                // Enemy has no health system - check if it should be instant kill
                SimpleEnemy simpleEnemy = other.GetComponent<SimpleEnemy>();
                GoblinEnemy goblinEnemy = other.GetComponent<GoblinEnemy>();

                if (simpleEnemy != null && !simpleEnemy.useHealthSystem)
                {
                    // SimpleEnemy without health system - instant kill
                    Debug.Log($"💥 Instant kill SimpleEnemy: {other.gameObject.name}");
                    Destroy(other.gameObject);
                }
                else if (goblinEnemy != null && !goblinEnemy.useHealthSystem)
                {
                    // GoblinEnemy without health system - instant kill
                    Debug.Log($"💥 Instant kill GoblinEnemy: {other.gameObject.name}");
                    Destroy(other.gameObject);
                }
                else
                {
                    Debug.LogWarning($"⚠️ Enemy {other.gameObject.name} has no health system and useHealthSystem is not set to false!");
                }
            }
        }

        // Area damage (optional)
        if (explosionRadius > 0)
        {
            DealAreaDamage();
        }

        // Spawn impact effect
        if (impactEffect != null)
        {
            GameObject effect = Instantiate(impactEffect, transform.position, Quaternion.identity);
            Destroy(effect, impactEffectLifetime);
        }

        // Play impact sound
        if (audioSource != null && impactSound != null)
        {
            GameObject soundObj = new GameObject("FireballImpactSound");
            soundObj.transform.position = transform.position;
            AudioSource tempAudio = soundObj.AddComponent<AudioSource>();
            tempAudio.clip = impactSound;
            tempAudio.Play();
            Destroy(soundObj, impactSound.length);
        }

        hasHit = true;
        Destroy(gameObject, 0.1f);
    }

    void DealAreaDamage()
    {
        Collider[] hitColliders = Physics.OverlapSphere(transform.position, explosionRadius);

        foreach (Collider col in hitColliders)
        {
            if (col.CompareTag("Enemy"))
            {
                // Calculate damage falloff based on distance
                float distance = Vector3.Distance(transform.position, col.transform.position);
                float damageFalloff = 1f - (distance / explosionRadius);
                float finalDamage = damage * damageFalloff;

                // Try EnemyHealth first
                EnemyHealth enemyHealth = col.GetComponent<EnemyHealth>();
                if (enemyHealth != null)
                {
                    enemyHealth.TakeDamage(finalDamage);
                    Debug.Log($"💥 Area damage: {finalDamage:F1} to {col.gameObject.name}");
                }
                else
                {
                    // No health system - instant kill if close enough
                    if (damageFalloff > 0.5f) // Only kill if within 50% of explosion radius
                    {
                        Debug.Log($"💥 Area damage instant kill: {col.gameObject.name}");
                        Destroy(col.gameObject);
                    }
                }
            }
        }
    }

    void OnDrawGizmosSelected()
    {
        if (explosionRadius > 0)
        {
            Gizmos.color = new Color(1f, 0.5f, 0f, 0.3f);
            Gizmos.DrawSphere(transform.position, explosionRadius);
        }
    }
}