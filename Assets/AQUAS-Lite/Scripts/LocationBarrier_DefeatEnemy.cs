using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LocationBarrier_DefeatEnemy : MonoBehaviour
{
    [Header("Barrier Settings")]
    public string barrierName = "Location 3 Barrier";
    public bool isActive = false; // Barrier starts inactive until save point triggers it

    [Header("Enemy Tracking")]
    public List<GameObject> enemiesInArea = new List<GameObject>(); // Assign enemies in inspector
    public bool autoFindEnemies = true; // Automatically find enemies inside barrier
    public string enemyTag = "Enemy"; // Tag to search for

    [Header("Visual Feedback")]
    public string blockedMessage = "Defeat all enemies to unlock the barrier!";
    public string allEnemiesDefeatedMessage = "All enemies defeated! Barrier unlocked!";
    public float messageDisplayTime = 3f;
    public Color barrierGizmoColor = Color.red;

    [Header("Push Back Settings")]
    public float pushBackForce = 15f;
    public float edgeBuffer = 3f;

    private Transform playerTransform;
    private Rigidbody playerRigidbody;
    private bool hasShownMessage = false;
    private float messageTimer = 0f;
    private SphereCollider sphereCollider;
    private Vector3 lastSafePosition;
    private int initialEnemyCount = 0;

    void Start()
    {
        sphereCollider = GetComponent<SphereCollider>();
        if (sphereCollider == null)
        {
            sphereCollider = gameObject.AddComponent<SphereCollider>();
            sphereCollider.radius = 30f;
        }
        sphereCollider.isTrigger = true;

        Debug.Log($"{barrierName} initialized. Radius: {sphereCollider.radius}, Active: {isActive}");
    }

    void Update()
    {
        // Reset message cooldown
        if (hasShownMessage)
        {
            messageTimer += Time.deltaTime;
            if (messageTimer >= 2f)
            {
                hasShownMessage = false;
                messageTimer = 0f;
            }
        }

        // Check if all enemies are defeated
        if (isActive)
        {
            CheckEnemyStatus();
        }

        // Enforce boundary if barrier is ACTIVE and player is being tracked
        if (isActive && playerTransform != null)
        {
            EnforceBoundary();
        }
    }

    void CheckEnemyStatus()
    {
        // Remove null entries (destroyed enemies)
        enemiesInArea.RemoveAll(enemy => enemy == null);

        // If all enemies defeated, unlock barrier
        if (enemiesInArea.Count == 0 && initialEnemyCount > 0)
        {
            Debug.Log($"🎉 All enemies defeated in {barrierName}!");
            UnlockBarrier();
        }
    }

    void EnforceBoundary()
    {
        if (sphereCollider == null || playerTransform == null) return;

        Vector3 playerPos = playerTransform.position;
        Vector3 centerPos = transform.position;

        Vector3 playerPosFlat = new Vector3(playerPos.x, centerPos.y, playerPos.z);
        Vector3 centerPosFlat = new Vector3(centerPos.x, centerPos.y, centerPos.z);

        float distanceFromCenter = Vector3.Distance(playerPosFlat, centerPosFlat);
        float effectiveRadius = sphereCollider.radius * transform.lossyScale.x;
        float maxAllowedDistance = effectiveRadius - edgeBuffer;

        if (distanceFromCenter >= maxAllowedDistance)
        {
            Debug.Log($"⚠️ Player at edge! Distance: {distanceFromCenter:F2} / Max: {maxAllowedDistance:F2}");

            Vector3 directionToCenter = (centerPosFlat - playerPosFlat).normalized;

            if (playerRigidbody != null)
            {
                Vector3 velocity = playerRigidbody.velocity;
                Vector3 velocityFlat = new Vector3(velocity.x, 0, velocity.z);

                if (Vector3.Dot(velocityFlat.normalized, -directionToCenter) > 0.5f)
                {
                    playerRigidbody.velocity = new Vector3(0, velocity.y, 0);
                }

                playerRigidbody.AddForce(directionToCenter * pushBackForce, ForceMode.VelocityChange);
            }
            else
            {
                Vector3 safePosition = centerPos + (-directionToCenter * (maxAllowedDistance - 1f));
                safePosition.y = playerPos.y;
                playerTransform.position = safePosition;
            }

            ShowBlockedMessage();
        }
        else
        {
            lastSafePosition = playerPos;
        }
    }

    void ShowBlockedMessage()
    {
        if (!hasShownMessage)
        {
            GameManager gameManager = FindObjectOfType<GameManager>();
            if (gameManager != null)
            {
                // Show remaining enemy count
                string message = $"{blockedMessage} ({enemiesInArea.Count} remaining)";
                gameManager.ShowMessage(message, messageDisplayTime);
            }
            hasShownMessage = true;
            messageTimer = 0f;
            Debug.Log($"📢 Showed blocked message - {enemiesInArea.Count} enemies remaining");
        }
    }

    // Automatically unlock when all enemies defeated
    void UnlockBarrier()
    {
        isActive = false;
        playerTransform = null;
        playerRigidbody = null;

        Debug.Log($"🔓 {barrierName} UNLOCKED! All enemies defeated.");

        GameManager gameManager = FindObjectOfType<GameManager>();
        if (gameManager != null)
        {
            gameManager.ShowMessage(allEnemiesDefeatedMessage, 4f);
        }
    }

    // Called when save point is activated
    public void LockBarrier()
    {
        isActive = true;

        // Auto-find enemies if enabled
        if (autoFindEnemies)
        {
            FindEnemiesInArea();
        }

        initialEnemyCount = enemiesInArea.Count;
        Debug.Log($"🔒 {barrierName} LOCKED! Must defeat {initialEnemyCount} enemies.");

        // Find and track player
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
        {
            playerTransform = playerObj.transform;
            playerRigidbody = playerObj.GetComponent<Rigidbody>();
            lastSafePosition = playerTransform.position;

            Debug.Log($"✓ Player found and tracked. Rigidbody: {(playerRigidbody != null ? "Yes" : "No")}");
        }
        else
        {
            Debug.LogError("⚠️ Could not find Player! Make sure player is tagged 'Player'");
        }
    }

    // Find all enemies within the barrier sphere
    void FindEnemiesInArea()
    {
        enemiesInArea.Clear();

        float effectiveRadius = sphereCollider.radius * transform.lossyScale.x;
        Collider[] colliders = Physics.OverlapSphere(transform.position, effectiveRadius);

        foreach (Collider col in colliders)
        {
            if (col.CompareTag(enemyTag))
            {
                enemiesInArea.Add(col.gameObject);
                Debug.Log($"✓ Found enemy in area: {col.gameObject.name}");
            }
        }

        Debug.Log($"📊 Total enemies found: {enemiesInArea.Count}");
    }

    // Manual method to add specific enemies
    public void AddEnemy(GameObject enemy)
    {
        if (!enemiesInArea.Contains(enemy))
        {
            enemiesInArea.Add(enemy);
            Debug.Log($"✓ Added enemy to barrier: {enemy.name}");
        }
    }

    // Draw the area boundary in Scene view
    void OnDrawGizmos()
    {
        if (sphereCollider == null) sphereCollider = GetComponent<SphereCollider>();
        if (sphereCollider == null) return;

        Gizmos.color = isActive ? barrierGizmoColor : Color.green;
        Gizmos.matrix = transform.localToWorldMatrix;
        Gizmos.DrawWireSphere(Vector3.zero, sphereCollider.radius);

        Gizmos.color = new Color(1f, 1f, 0f, 0.3f);
        Gizmos.DrawWireSphere(Vector3.zero, sphereCollider.radius - edgeBuffer);
    }

    void OnDrawGizmosSelected()
    {
        if (sphereCollider == null) sphereCollider = GetComponent<SphereCollider>();
        if (sphereCollider == null) return;

        Gizmos.color = isActive ? Color.red : Color.green;
        Gizmos.matrix = transform.localToWorldMatrix;
        Gizmos.DrawWireSphere(Vector3.zero, sphereCollider.radius);

        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(Vector3.zero, sphereCollider.radius - edgeBuffer);

        Gizmos.color = Color.cyan;
        float r = sphereCollider.radius;
        Gizmos.DrawLine(Vector3.zero, new Vector3(r, 0, 0));
        Gizmos.DrawLine(Vector3.zero, new Vector3(-r, 0, 0));
        Gizmos.DrawLine(Vector3.zero, new Vector3(0, 0, r));
        Gizmos.DrawLine(Vector3.zero, new Vector3(0, 0, -r));

        Gizmos.color = Color.magenta;
        Gizmos.DrawSphere(Vector3.zero, 0.5f);

#if UNITY_EDITOR
        GUIStyle style = new GUIStyle();
        style.normal.textColor = isActive ? Color.red : Color.green;
        style.fontSize = 14;
        style.fontStyle = FontStyle.Bold;
        string status = isActive ? $"ACTIVE - {enemiesInArea.Count} enemies" : "INACTIVE";
        UnityEditor.Handles.Label(transform.position + Vector3.up * 3, status, style);
#endif
    }
}