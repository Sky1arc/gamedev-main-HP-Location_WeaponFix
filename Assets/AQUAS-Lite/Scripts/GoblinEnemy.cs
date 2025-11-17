using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GoblinEnemy : MonoBehaviour
{
    [Header("Enemy Settings")]
    public float moveSpeed = 2.5f;
    public float runSpeed = 4.5f;
    public float chaseRange = 12f;
    public float attackRange = 2f;
    public float damage = 50f;
    public float attackCooldown = 2.5f;
    
    [Header("References")]
    public Transform player;
    public Animator animator;
    
    [Header("Health (Optional)")]
    public bool useHealthSystem = true;
    
    [Header("Ground Check")]
    public LayerMask groundLayer;
    public float groundCheckDistance = 0.1f;
    
    private Rigidbody rb;
    private bool hasKilledPlayer = false;
    private float lastAttackTime = 0f;
    private bool isAttacking = false;
    private EnemyHealth enemyHealth;
    private bool isChasing = false;
    private BoxCollider boxCollider;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        
        // ✅ SETUP: Configure Rigidbody to prevent flying
        if (rb != null)
        {
            rb.constraints = RigidbodyConstraints.FreezeRotationX | 
                             RigidbodyConstraints.FreezeRotationZ;
            rb.useGravity = true;
            rb.isKinematic = false;
            rb.drag = 2f; // Add drag to prevent sliding
            rb.angularDrag = 5f;
        }
        
        // ✅ SETUP: Use Box Collider instead of Capsule
        boxCollider = GetComponent<BoxCollider>();
        if (boxCollider == null)
        {
            // Remove capsule collider if it exists
            CapsuleCollider capsule = GetComponent<CapsuleCollider>();
            if (capsule != null)
            {
                Debug.Log("🔧 Removing CapsuleCollider, adding BoxCollider");
                Destroy(capsule);
            }
            
            // Add box collider
            boxCollider = gameObject.AddComponent<BoxCollider>();
            boxCollider.size = new Vector3(0.6f, 1.8f, 0.6f); // Adjust to your goblin size
            boxCollider.center = new Vector3(0, 0.9f, 0); // Center at goblin's middle
            Debug.Log("✅ BoxCollider added to Goblin");
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
        
        // ✅ FIX: Make sure animator starts in Idle state
        if (animator != null)
        {
            animator.SetBool("IsRunning", false);
            animator.SetBool("IsWalking", false);
            Debug.Log("✅ Animator initialized to Idle state");
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
        
        // Get health component if using health system
        if (useHealthSystem)
        {
            enemyHealth = GetComponent<EnemyHealth>();
            if (enemyHealth == null && useHealthSystem)
            {
                Debug.LogWarning("⚠️ GoblinEnemy: EnemyHealth component not found! Add it or disable useHealthSystem.");
            }
        }
        
        Debug.Log($"✅ GoblinEnemy initialized: Chase Range={chaseRange}, Attack Range={attackRange}");
    }

    void Update()
    {
        if (player == null || hasKilledPlayer) return;
        
        // Check if player is still alive
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
            // Idle when player is far
            StopChasing();
        }
    }

    void ChasePlayer()
    {
        if (!isChasing)
        {
            isChasing = true;
        }
        
        // Look at player
        Vector3 direction = (player.position - transform.position).normalized;
        direction.y = 0; // Keep goblin upright
        
        if (direction != Vector3.zero)
        {
            Quaternion lookRotation = Quaternion.LookRotation(direction);
            transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, Time.deltaTime * 8f);
        }
        
        // Move towards player using Rigidbody velocity (better for Box Collider)
        Vector3 targetVelocity = direction * runSpeed;
        targetVelocity.y = rb.velocity.y; // Preserve vertical velocity (gravity)
        
        rb.velocity = targetVelocity;
        
        // ✅ FIX: Set animation to running ONLY if not already running
        if (animator != null)
        {
            if (!animator.GetBool("IsRunning"))
            {
                animator.SetBool("IsRunning", true);
                Debug.Log("🏃 Goblin started running animation");
            }
        }
    }

    void StopChasing()
    {
        if (isChasing)
        {
            isChasing = false;
            
            // Stop movement
            rb.velocity = new Vector3(0, rb.velocity.y, 0);
            
            // ✅ FIX: Set animation to idle
            if (animator != null)
            {
                animator.SetBool("IsRunning", false);
                animator.SetBool("IsWalking", false);
                Debug.Log("🛑 Goblin stopped running - back to Idle");
            }
        }
    }

    void AttackPlayer()
    {
        if (hasKilledPlayer || isAttacking) return;
        
        isAttacking = true;
        lastAttackTime = Time.time;
        
        // Stop moving during attack
        rb.velocity = new Vector3(0, rb.velocity.y, 0);
        
        // Face the player during attack
        Vector3 direction = (player.position - transform.position).normalized;
        direction.y = 0;
        if (direction != Vector3.zero)
        {
            transform.rotation = Quaternion.LookRotation(direction);
        }
        
        // ✅ FIX: Stop running animation before attack
        if (animator != null)
        {
            animator.SetBool("IsRunning", false);
            animator.SetTrigger("Attack");
            Debug.Log("⚔️ Goblin attacking!");
        }
        
        // Deal damage after a short delay (mid-attack)
        Invoke("DealDamage", 0.5f);
        
        // Reset attacking state
        Invoke("ResetAttack", 1.5f);
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
                
                // Check if player died
                if (playerHealth.currentHealth <= 0)
                {
                    hasKilledPlayer = true;
                    Debug.Log("🗡️ Goblin killed the player!");
                    
                    // Reset after a delay so goblin can attack again if player respawns
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
        Debug.Log("✅ Goblin attack reset - can chase again");
    }

    // Optional: Draw debug spheres in scene view
    void OnDrawGizmosSelected()
    {
        // Chase range (yellow)
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, chaseRange);
        
        // Attack range (red)
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);
        
        // Forward direction
        Gizmos.color = Color.blue;
        Gizmos.DrawRay(transform.position, transform.forward * 3f);
        
        // Box collider visualization
        if (boxCollider != null)
        {
            Gizmos.color = Color.green;
            Gizmos.matrix = transform.localToWorldMatrix;
            Gizmos.DrawWireCube(boxCollider.center, boxCollider.size);
        }
    }
}