using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SimpleEnemy : MonoBehaviour
{
    [Header("Enemy Settings")]
    public float moveSpeed = 3f;
    public float runSpeed = 5f;
    public float chaseRange = 15f;
    public float attackRange = 2.5f;
    public float damage = 10f;
    public float attackCooldown = 2f;
    
    [Header("References")]
    public Transform player;
    public Animator animator;

    [Header("Health (Optional)")]
    public bool useHealthSystem = true;

    // NEW: Add a header and variables for your audio clips
    [Header("Audio")]
    public AudioClip detectionSound; // Sound when the bear first sees the player
    public AudioClip attackSound;     // Sound when the bear attacks
    
    private Rigidbody rb;
    private bool hasKilledPlayer = false;
    private float lastAttackTime = 0f;
    private bool isAttacking = false;
    private EnemyHealth enemyHealth;
    
    // NEW: Private variables for audio management
    private AudioSource audioSource;
    private bool hasDetectedPlayer = false; // Flag to play detection sound only once

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        
        // NEW: Get or add the AudioSource component
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }
        
        // Auto-find animator if not assigned
        if (animator == null)
        {
            animator = GetComponent<Animator>();
            if (animator == null)
            {
                animator = GetComponentInChildren<Animator>();
            }
        }
        
        // Auto-find player if not assigned
        if (player == null)
        {
            GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
            if (playerObj != null)
            {
                player = playerObj.transform;
            }
        }

        // ✅ NEW: Get health component if using health system
        if (useHealthSystem)
        {
            enemyHealth = GetComponent<EnemyHealth>();
            if (enemyHealth == null && useHealthSystem)
            {
                Debug.LogWarning("⚠️ SimpleEnemy: EnemyHealth component not found! Add it or disable useHealthSystem.");
            }
        }

        Debug.Log($"✅ SimpleEnemy initialized with health system: {useHealthSystem}");
    }

    void Update()
    {
        if (player == null || hasKilledPlayer) return;

        // ✅ NEW: Check if player is still alive
        PlayerHealth playerHealth = player.GetComponent<PlayerHealth>();
        if (playerHealth != null && playerHealth.currentHealth <= 0)
        {
            // Player is dead, stop chasing
            StopChasing();
            return;
        }

        float distanceToPlayer = Vector3.Distance(transform.position, player.position);
        
        // Check if player is in chase range
        if (distanceToPlayer <= chaseRange)
        {
            // NEW: Play detection sound the first time the player is detected
            if (!hasDetectedPlayer)
            {
                if (audioSource != null && detectionSound != null)
                {
                    audioSource.PlayOneShot(detectionSound);
                }
                hasDetectedPlayer = true; // Set the flag to true so it doesn't play again
            }
            
            // Check if close enough to attack
            if (distanceToPlayer <= attackRange && Time.time >= lastAttackTime + attackCooldown)
            {
                AttackPlayer();
            }
            else if (!isAttacking)
            {
                ChasePlayer();
            }
        }
        else
        {
            // Player is out of range, reset detection flag so it can be triggered again
            hasDetectedPlayer = false;
            
            // Idle when player is far
            if (animator != null)
            {
                animator.SetBool("IsWalking", false);
                animator.SetBool("IsRunning", false);
            }
        }
    }

    void ChasePlayer()
    {
        // Look at player
        Vector3 direction = (player.position - transform.position).normalized;
        direction.y = 0; // Keep enemy upright
        
        if (direction != Vector3.zero)
        {
            Quaternion lookRotation = Quaternion.LookRotation(direction);
            transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, Time.deltaTime * 8f);
        }
        
        // Move towards player
        Vector3 moveDirection = direction * runSpeed * Time.deltaTime;
        rb.MovePosition(transform.position + moveDirection);
        
        // Set animation to running
        if (animator != null)
        {
            animator.SetBool("IsRunning", true);
            animator.SetBool("IsWalking", false);
        }
    }

    // ✅ NEW: Stop chasing method
    void StopChasing()
    {
        if (animator != null)
        {
            animator.SetBool("IsWalking", false);
            animator.SetBool("IsRunning", false);
        }
    }

    void AttackPlayer()
    {
        if (hasKilledPlayer || isAttacking) return;
        
        isAttacking = true;
        lastAttackTime = Time.time;
        
        // NEW: Play attack sound every time an attack is initiated
        if (audioSource != null && attackSound != null)
        {
            audioSource.PlayOneShot(attackSound);
        }
        
        // Play attack animation
        if (animator != null)
        {
            animator.SetBool("IsRunning", false);
            animator.SetBool("IsWalking", false);
            animator.SetTrigger("Attack");
        }
        
        // Deal damage after a short delay (mid-attack)
        Invoke("DealDamage", 0.5f);
        
        // Reset attacking state
        Invoke("ResetAttack", 1f);
    }

    void DealDamage()
    {
        if (hasKilledPlayer || player == null) return;
        
        // Check if still in range
        float distanceToPlayer = Vector3.Distance(transform.position, player.position);
        if (distanceToPlayer <= attackRange)
        {
            PlayerHealth playerHealth = player.GetComponent<PlayerHealth>();
            if (playerHealth != null)
            {
                playerHealth.TakeDamage(damage);
                if (playerHealth.currentHealth <= 0)
                {
                    hasKilledPlayer = true;
                    Debug.Log("Bear killed the player!");

                    // Reset after a delay so bear can attack again if player respawns
                    Invoke("ResetKillState", 3f);
                }
            }
        }
    }
    
    void ResetKillState()
    {
        hasKilledPlayer = false;
    }

    void ResetAttack()
    {
        isAttacking = false;
    }

    // Optional: Draw debug spheres in scene view
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, chaseRange);
        
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);
    }
}